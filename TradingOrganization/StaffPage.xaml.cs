using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TradingOrganization
{
    /// <summary>
    /// Карточка торговой точки для директора: заведующий, сотрудники и заказы.
    /// </summary>
    public partial class StaffPage : Page
    {
        private enum ViewMode
        {
            Staff,
            Orders
        }

        private readonly int _storeId;
        private Store _point;
        private ViewMode _view = ViewMode.Staff;

        private List<User> _staff = new List<User>();
        private List<Order> _orders = new List<Order>();

        public StaffPage(int storeId)
        {
            InitializeComponent();

            _storeId = storeId;
            LoadPoint();
        }

        private void LoadPoint()
        {
            _point = Manager.Context.Stores
                .Include("Manager")
                .Include("Organization")
                .Include("Employees.Position")
                .Include("Orders.Supplier")
                .FirstOrDefault(t => t.StoreID == _storeId);

            if (_point == null)
            {
                MessageBox.Show("Торговая точка не найдена.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                if (Manager.MainFrame != null && Manager.MainFrame.CanGoBack)
                    Manager.MainFrame.GoBack();
                return;
            }

            TitleText.Text = _point.ТипСЗаглавной;
            SubtitleText.Text = _point.Address + " · " + _point.ОрганизацияНазвание;
            ManagerCard.DataContext = _point.Manager;

            _staff = _point.Employees
                .Where(u => u.Role == Roles.Employee)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();

            _orders = _point.Orders
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .ToList();

            UpdateNavStyles();
            RefreshView();
        }

        private void UpdateNavStyles()
        {
            StaffNavBtn.Style = (Style)FindResource(
                _view == ViewMode.Staff ? "NavButtonActive" : "NavButton");
            OrdersNavBtn.Style = (Style)FindResource(
                _view == ViewMode.Orders ? "NavButtonActive" : "NavButton");
        }

        private void RefreshView()
        {
            StaffList.Visibility = Visibility.Collapsed;
            OrdersList.Visibility = Visibility.Collapsed;

            var search = (SearchBox.Text ?? string.Empty).Trim().ToLower();
            var count = 0;

            if (_view == ViewMode.Staff)
            {
                var filtered = _staff
                    .Where(s => Matches(search, s.ФИО, s.ДолжностьНазвание, s.ВозрастТекст, s.City))
                    .ToList();

                StaffList.ItemsSource = filtered;
                StaffList.Visibility = Visibility.Visible;
                count = filtered.Count;
            }
            else
            {
                var filtered = _orders
                    .Where(o => Matches(search, o.ProductName, o.ПоставщикНазвание,
                        o.ДатаТекст, o.Quantity, o.СуммаТекст))
                    .ToList();

                OrdersList.ItemsSource = filtered;
                OrdersList.Visibility = Visibility.Visible;
                count = filtered.Count;
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

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
                RefreshView();
        }
    }
}
