using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TradingOrganization
{
    /// <summary>
    /// Главная страница: списки, доступные текущей роли пользователя.
    /// </summary>
    public partial class MainPage : Page
    {
        private enum ViewMode
        {
            Staff,
            Orders,
            Points,
            Users,
            Requests,
            OrderRequests
        }

        private readonly UserSession _user;
        private readonly bool _openOrderRequests;
        private ViewMode _view;

        private List<User> _staff = new List<User>();
        private List<Order> _orders = new List<Order>();
        private List<Store> _points = new List<Store>();
        private List<User> _users = new List<User>();
        private List<ChangeRequest> _requests = new List<ChangeRequest>();
        private List<OrderRequest> _orderRequests = new List<OrderRequest>();

        public MainPage()
            : this(false)
        {
        }

        /// <summary>
        /// Главная страница. openOrderRequests — сразу открыть список заявок на заказы.
        /// </summary>
        public MainPage(bool openOrderRequests)
        {
            InitializeComponent();

            _openOrderRequests = openOrderRequests;

            _user = Manager.CurrentUser;
            if (_user == null)
            {
                Manager.ResetNavigation(new AuthPage());
                return;
            }

            ConfigureForRole();

            if (_openOrderRequests && _user.Role == UserRole.Manager)
            {
                _view = ViewMode.OrderRequests;
                UpdateNavStyles();
            }

            Loaded += (s, e) =>
            {
                LoadData();
                RefreshView();
                LoadLetter();
            };
        }

        private void ConfigureForRole()
        {
            StaffNavBtn.Visibility = Visibility.Collapsed;
            OrdersNavBtn.Visibility = Visibility.Collapsed;
            PointsNavBtn.Visibility = Visibility.Collapsed;
            UsersNavBtn.Visibility = Visibility.Collapsed;
            RequestsNavBtn.Visibility = Visibility.Collapsed;
            OrderRequestsNavBtn.Visibility = Visibility.Collapsed;
            NewOrderRequestBtn.Visibility = Visibility.Collapsed;

            switch (_user.Role)
            {
                case UserRole.Employee:
                    TitleText.Text = "Заказы поставщикам";
                    SubtitleText.Text = "Моя торговая точка: " + _user.ТочкаОписание;
                    OrdersNavBtn.Visibility = Visibility.Visible;
                    _view = ViewMode.Orders;
                    break;

                case UserRole.Manager:
                    TitleText.Text = "Моя торговая точка";
                    SubtitleText.Text = _user.ТочкаОписание;
                    StaffNavBtn.Visibility = Visibility.Visible;
                    OrdersNavBtn.Visibility = Visibility.Visible;
                    RequestsNavBtn.Visibility = Visibility.Visible;
                    OrderRequestsNavBtn.Visibility = Visibility.Visible;
                    NewOrderRequestBtn.Visibility = Visibility.Visible;
                    _view = ViewMode.Staff;
                    break;

                case UserRole.Director:
                    TitleText.Text = "Торговые точки";
                    SubtitleText.Text = "Организация: " + _user.ОрганизацияНазвание;
                    PointsNavBtn.Visibility = Visibility.Visible;
                    UsersNavBtn.Visibility = Visibility.Visible;
                    OrderRequestsNavBtn.Visibility = Visibility.Visible;
                    _view = ViewMode.Points;
                    break;
            }

            UpdateNavStyles();
        }

        private void LoadData()
        {
            var context = Manager.Context;

            switch (_user.Role)
            {
                case UserRole.Employee:
                    if (_user.Point != null)
                        _orders = LoadOrders(_user.Point.StoreID);
                    break;

                case UserRole.Manager:
                    if (_user.Point != null)
                    {
                        var storeId = _user.Point.StoreID;

                        _staff = context.Users
                            .Include("Position")
                            .Where(u => u.StoreID == storeId && u.Role == Roles.Employee)
                            .OrderBy(u => u.LastName)
                            .ThenBy(u => u.FirstName)
                            .ToList();

                        _orders = LoadOrders(storeId);

                        _requests = context.ChangeRequests
                            .Include("User")
                            .Where(r => r.ManagerID == _user.Id)
                            .OrderByDescending(r => r.CreatedAt)
                            .ToList();

                        _orderRequests = context.OrderRequests
                            .Include("Supplier")
                            .Include("Store")
                            .Include("Manager")
                            .Where(r => r.ManagerID == _user.Id)
                            .OrderByDescending(r => r.CreatedAt)
                            .ToList();
                    }
                    break;

                case UserRole.Director:
                    if (_user.Organization != null)
                    {
                        var organizationId = _user.Organization.OrganizationID;

                        _points = context.Stores
                            .Include("Manager")
                            .Include("Employees")
                            .Include("Orders")
                            .Include("Organization")
                            .Where(s => s.OrganizationID == organizationId)
                            .OrderBy(s => s.Address)
                            .ToList();

                        // Директор работает только с пользователями своей организации:
                        // сотрудники её точек, её заведующие и он сам.
                        // Другие директора не показываются — они не должны знать друг о друге.
                        _users = context.Users
                            .Include("Position")
                            .Include("Store.Organization")
                            .Where(u =>
                                u.UserID == _user.Id ||
                                u.Store.OrganizationID == organizationId ||
                                u.ManagedStores.Any(s => s.OrganizationID == organizationId))
                            .OrderBy(u => u.Role)
                            .ThenBy(u => u.LastName)
                            .ThenBy(u => u.FirstName)
                            .ToList();
                    }

                    _orderRequests = context.OrderRequests
                        .Include("Supplier")
                        .Include("Store")
                        .Include("Manager")
                        .Where(r => r.DirectorID == _user.Id)
                        .OrderByDescending(r => r.CreatedAt)
                        .ToList();
                    break;
            }
        }

        private List<Order> LoadOrders(int storeId)
        {
            return Manager.Context.Orders
                .Include("Supplier")
                .Where(o => o.StoreID == storeId)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .ToList();
        }

        private void LoadLetter()
        {
            LetterBanner.Visibility = Visibility.Collapsed;

            if (_user.Role != UserRole.Employee)
                return;

            var letter = Manager.Context.ChangeRequests
                .Where(r => r.UserID == _user.Id &&
                            (r.Status == RequestStatus.Pending ||
                             r.Status == RequestStatus.Applied ||
                             r.Status == RequestStatus.Rejected))
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            if (letter == null)
                return;

            switch (letter.Status)
            {
                case RequestStatus.Pending:
                    LetterText.Text = "Ваш запрос на изменение данных отправлен заведующему. Ожидайте ответа.";
                    LetterActionBtn.Content = "Открыть";
                    break;

                case RequestStatus.Applied:
                    LetterText.Text = "Данные были изменены. Проверьте, правильно ли внесены корректировки.";
                    LetterActionBtn.Content = "Проверить";
                    break;

                default:
                    LetterText.Text = "Заведующий отклонил ваш запрос. Можно составить новый.";
                    LetterActionBtn.Content = "Открыть";
                    break;
            }

            LetterBanner.Visibility = Visibility.Visible;
        }

        private void UpdateNavStyles()
        {
            StaffNavBtn.Style = (Style)FindResource(
                _view == ViewMode.Staff ? "NavButtonActive" : "NavButton");
            OrdersNavBtn.Style = (Style)FindResource(
                _view == ViewMode.Orders ? "NavButtonActive" : "NavButton");
            PointsNavBtn.Style = (Style)FindResource(
                _view == ViewMode.Points ? "NavButtonActive" : "NavButton");
            UsersNavBtn.Style = (Style)FindResource(
                _view == ViewMode.Users ? "NavButtonActive" : "NavButton");
            RequestsNavBtn.Style = (Style)FindResource(
                _view == ViewMode.Requests ? "NavButtonActive" : "NavButton");
            OrderRequestsNavBtn.Style = (Style)FindResource(
                _view == ViewMode.OrderRequests ? "NavButtonActive" : "NavButton");
        }

        private void RefreshView()
        {
            StaffList.Visibility = Visibility.Collapsed;
            OrdersList.Visibility = Visibility.Collapsed;
            PointsList.Visibility = Visibility.Collapsed;
            UsersList.Visibility = Visibility.Collapsed;
            RequestsList.Visibility = Visibility.Collapsed;
            OrderRequestsList.Visibility = Visibility.Collapsed;

            var search = (SearchBox.Text ?? string.Empty).Trim().ToLower();
            var count = 0;

            switch (_view)
            {
                case ViewMode.Staff:
                {
                    var filtered = _staff
                        .Where(s => Matches(search, s.ФИО, s.ДолжностьНазвание, s.ВозрастТекст, s.City))
                        .ToList();

                    StaffList.ItemsSource = filtered;
                    StaffList.Visibility = Visibility.Visible;
                    count = filtered.Count;
                    break;
                }

                case ViewMode.Orders:
                {
                    var filtered = _orders
                        .Where(o => Matches(search, o.ProductName, o.ПоставщикНазвание,
                            o.ДатаТекст, o.Quantity, o.СуммаТекст))
                        .ToList();

                    OrdersList.ItemsSource = filtered;
                    OrdersList.Visibility = Visibility.Visible;
                    count = filtered.Count;
                    break;
                }

                case ViewMode.Points:
                {
                    var filtered = _points
                        .Where(p => Matches(search, p.ТипСЗаглавной, p.Address, p.ЗаведующийФИО,
                            p.ОрганизацияНазвание))
                        .ToList();

                    PointsList.ItemsSource = filtered;
                    PointsList.Visibility = Visibility.Visible;
                    count = filtered.Count;
                    break;
                }

                case ViewMode.Users:
                {
                    var filtered = _users
                        .Where(u => Matches(search, u.ФИО, u.РольНазвание, u.Логин,
                            u.ДолжностьНазвание, u.ТочкаОписание))
                        .ToList();

                    UsersList.ItemsSource = filtered;
                    UsersList.Visibility = Visibility.Visible;
                    count = filtered.Count;
                    break;
                }

                case ViewMode.Requests:
                {
                    var filtered = _requests
                        .Where(r => Matches(search, r.User.ФИО, r.Message, r.СтатусНазвание))
                        .ToList();

                    RequestsList.ItemsSource = filtered;
                    RequestsList.Visibility = Visibility.Visible;
                    count = filtered.Count;
                    break;
                }

                case ViewMode.OrderRequests:
                {
                    var filtered = _orderRequests
                        .Where(r => Matches(search, r.ПоставщикНазвание, r.ProductName,
                            r.СтатусНазвание, r.ТочкаОписание, r.ЗаведующийФИО,
                            r.КоличествоТекст, r.ЦенаТекст))
                        .ToList();

                    OrderRequestsList.ItemTemplate = (DataTemplate)FindResource(
                        _user.Role == UserRole.Director
                            ? "OrderRequestReviewTemplate"
                            : "OrderRequestTemplate");
                    OrderRequestsList.ItemsSource = filtered;
                    OrderRequestsList.Visibility = Visibility.Visible;
                    count = filtered.Count;
                    break;
                }
            }

            CountText.Text = "Найдено: " + count;
            EmptyText.Text = string.IsNullOrEmpty(search) ? "Данных нет" : "Ничего не найдено";
            EmptyText.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static bool Matches(string search, params string[] values)
        {
            if (string.IsNullOrEmpty(search))
                return true;

            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value) &&
                    value.ToLower().Contains(search))
                    return true;
            }

            return false;
        }

        private void StaffNavBtn_Click(object sender, RoutedEventArgs e)
        {
            _view = ViewMode.Staff;
            UpdateNavStyles();
            RefreshView();
        }

        private void OrdersNavBtn_Click(object sender, RoutedEventArgs e)
        {
            _view = ViewMode.Orders;
            UpdateNavStyles();
            RefreshView();
        }

        private void PointsNavBtn_Click(object sender, RoutedEventArgs e)
        {
            _view = ViewMode.Points;
            UpdateNavStyles();
            RefreshView();
        }

        private void UsersNavBtn_Click(object sender, RoutedEventArgs e)
        {
            _view = ViewMode.Users;
            UpdateNavStyles();
            RefreshView();
        }

        private void RequestsNavBtn_Click(object sender, RoutedEventArgs e)
        {
            _view = ViewMode.Requests;
            UpdateNavStyles();
            RefreshView();
        }

        private void OrderRequestsNavBtn_Click(object sender, RoutedEventArgs e)
        {
            _view = ViewMode.OrderRequests;
            UpdateNavStyles();
            RefreshView();
        }

        private void NewOrderRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.MainFrame != null)
                Manager.MainFrame.Navigate(new OrderRequestPage());
        }

        private void OrderRequestApprove_Click(object sender, RoutedEventArgs e)
        {
            var request = (sender as FrameworkElement)?.DataContext as OrderRequest;

            if (request == null || request.Status != OrderRequestStatus.Pending)
                return;

            if (_user.Role != UserRole.Director)
                return;

            var result = MessageBox.Show(
                "Подтвердить заявку на заказ?\n\n" +
                "Поставщик: " + request.ПоставщикНазвание + "\n" +
                "Товар: " + request.ProductName + "\n" +
                "Количество: " + request.КоличествоТекст + "\n" +
                "Сумма: " + request.ЦенаТекст + "\n\n" +
                "Будет создан заказ для торговой точки: " + request.ТочкаОписание + ".",
                "Подтверждение заявки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var context = Manager.Context;

                var order = new Order
                {
                    StoreID = request.StoreID,
                    SupplierID = request.SupplierID,
                    ProductName = request.ProductName,
                    Quantity = request.КоличествоТекст,
                    Price = request.Price,
                    OrderDate = DateTime.Now
                };

                context.Orders.Add(order);
                context.SaveChanges();

                request.Status = OrderRequestStatus.Approved;
                request.ResolvedAt = DateTime.Now;
                request.OrderID = order.OrderID;
                request.DirectorComment = "Заявка подтверждена. Создан заказ №" + order.OrderID + ".";
                context.SaveChanges();

                LoadData();
                RefreshView();

                MessageBox.Show(
                    "Заявка подтверждена, создан заказ №" + order.OrderID + ".",
                    "Заявка на заказ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подтвердить заявку.\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrderRequestReject_Click(object sender, RoutedEventArgs e)
        {
            var request = (sender as FrameworkElement)?.DataContext as OrderRequest;

            if (request == null || request.Status != OrderRequestStatus.Pending)
                return;

            if (_user.Role != UserRole.Director)
                return;

            var dialog = new CommentDialog(
                "Заявка №" + request.OrderRequestID + ": " + request.ПоставщикНазвание +
                ", " + request.ProductName + " — " + request.КоличествоТекст + ".")
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                request.Status = OrderRequestStatus.Rejected;
                request.ResolvedAt = DateTime.Now;
                request.DirectorComment = dialog.Comment;
                Manager.Context.SaveChanges();

                LoadData();
                RefreshView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось отклонить заявку.\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrderRequestEdit_Click(object sender, RoutedEventArgs e)
        {
            var request = (sender as FrameworkElement)?.DataContext as OrderRequest;

            if (request == null)
                return;

            if (_user.Role != UserRole.Manager)
                return;

            if (request.Status != OrderRequestStatus.Pending)
            {
                MessageBox.Show(
                    "Редактировать можно только заявку, которая ещё ожидает решения директора.",
                    "Заявка на заказ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                LoadData();
                RefreshView();
                return;
            }

            if (Manager.MainFrame != null)
                Manager.MainFrame.Navigate(new OrderRequestPage(request.OrderRequestID));
        }

        private void OrderRequestCancel_Click(object sender, RoutedEventArgs e)
        {
            var request = (sender as FrameworkElement)?.DataContext as OrderRequest;

            if (request == null || request.Status != OrderRequestStatus.Approved)
                return;

            if (_user.Role != UserRole.Director)
                return;

            var dialog = new CommentDialog(
                "Заявка №" + request.OrderRequestID + ": " + request.ПоставщикНазвание +
                ", " + request.ProductName + " — " + request.КоличествоТекст +
                " для точки «" + request.ТочкаОписание + "».",
                "Отмена заявки",
                "Отменить заявку",
                "Например: «Поставщик не может выполнить заказ в срок».",
                "Укажите причину отмены (не менее 5 символов).")
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var context = Manager.Context;
                var orderId = request.OrderID;

                request.Status = OrderRequestStatus.Cancelled;
                request.ResolvedAt = DateTime.Now;
                request.DirectorComment = dialog.Comment;
                request.OrderID = null;
                context.SaveChanges();

                if (orderId.HasValue)
                {
                    var order = context.Orders
                        .FirstOrDefault(o => o.OrderID == orderId.Value);

                    if (order != null)
                    {
                        context.Orders.Remove(order);
                        context.SaveChanges();
                    }
                }

                LoadData();
                RefreshView();

                MessageBox.Show(
                    "Заявка №" + request.OrderRequestID + " отменена." +
                    (orderId.HasValue
                        ? "\nСозданный по ней заказ №" + orderId.Value + " удалён."
                        : string.Empty),
                    "Заявка на заказ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось отменить заявку.\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
                RefreshView();
        }

        private void PointsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var point = PointsList.SelectedItem as Store;
            if (point != null)
                Manager.MainFrame.Navigate(new StaffPage(point.StoreID));
        }

        private void StaffEdit_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as FrameworkElement)?.DataContext as User;
            if (user != null)
                Manager.MainFrame.Navigate(new EditUserPage(user, null));
        }

        private void UserEdit_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as FrameworkElement)?.DataContext as User;
            if (user != null)
                Manager.MainFrame.Navigate(new EditUserPage(user, null));
        }

        private void RequestEdit_Click(object sender, RoutedEventArgs e)
        {
            var request = (sender as FrameworkElement)?.DataContext as ChangeRequest;
            if (request == null)
                return;

            Manager.MainFrame.Navigate(new EditUserPage(request.User, request));
        }

        private void RequestReject_Click(object sender, RoutedEventArgs e)
        {
            var request = (sender as FrameworkElement)?.DataContext as ChangeRequest;
            if (request == null)
                return;

            var result = MessageBox.Show(
                "Отклонить заявку сотрудника " + request.User.ФИО + "?",
                "Заявка на изменение данных",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                request.Status = RequestStatus.Rejected;
                request.ResolvedAt = DateTime.Now;
                request.ManagerID = _user.Id;
                request.ManagerComment = "Заявка отклонена заведующим.";
                Manager.Context.SaveChanges();

                LoadData();
                RefreshView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось отклонить заявку.\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LetterActionBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProfilePage());
        }
    }
}
