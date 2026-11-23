using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TradingOrganization
{
    /// <summary>
    /// Редактирование данных пользователя.
    /// Заведующий может менять себя и сотрудников своей точки, директор — всех.
    /// Если страница открыта из заявки, после сохранения заявка получает статус Applied.
    /// </summary>
    public partial class EditUserPage : Page
    {
        private readonly User _user;
        private readonly ChangeRequest _request;

        public EditUserPage(User user, ChangeRequest request)
        {
            InitializeComponent();

            _user = user;
            _request = request;

            if (_user == null)
            {
                MessageBox.Show("Пользователь не найден.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                GoBack();
                return;
            }

            if (!CanEdit())
            {
                MessageBox.Show("Недостаточно прав для редактирования этих данных.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                GoBack();
                return;
            }

            SubtitleText.Text = _request != null
                ? "Заявка сотрудника: " + _user.ФИО
                : "Профиль: " + _user.ФИО;

            LoadDictionaries();
            FillForm();
        }

        private bool CanEdit()
        {
            var current = Manager.CurrentUser;
            if (current == null)
                return false;

            if (_user.UserID == current.Id)
                return current.Role == UserRole.Manager || current.Role == UserRole.Director;

            if (current.Role == UserRole.Director)
            {
                // Директор управляет пользователями только своей организации,
                // другие директора ему недоступны.
                if (_user.Role == Roles.Director)
                    return false;

                return IsInOrganization(_user, current);
            }

            if (current.Role == UserRole.Manager)
            {
                return _user.Role == Roles.Employee
                       && current.Point != null
                       && _user.StoreID.HasValue
                       && _user.StoreID.Value == current.Point.StoreID;
            }

            return false;
        }

        /// <summary>
        /// Проверяет, что пользователь привязан к организации текущего директора:
        /// работает в её торговой точке или заведует ею.
        /// </summary>
        private static bool IsInOrganization(User user, UserSession current)
        {
            if (current.Organization == null)
                return false;

            var organizationId = current.Organization.OrganizationID;
            var userId = user.UserID;
            var stores = Manager.Context.Stores.Where(s => s.OrganizationID == organizationId);

            if (user.StoreID.HasValue)
            {
                var storeId = user.StoreID.Value;
                return stores.Any(s => s.StoreID == storeId || s.ManagerID == userId);
            }

            return stores.Any(s => s.ManagerID == userId);
        }

        private void LoadDictionaries()
        {
            try
            {
                GenderBox.ItemsSource = new[] { "М", "Ж" };

                PositionBox.ItemsSource = Manager.Context.Positions
                    .OrderBy(p => p.Name)
                    .ToList();

                var current = Manager.CurrentUser;
                var stores = Manager.Context.Stores
                    .Include("Organization")
                    .AsQueryable();

                if (current != null && current.Role == UserRole.Manager && current.Point != null)
                    stores = stores.Where(s => s.StoreID == current.Point.StoreID);

                if (current != null && current.Role == UserRole.Director && current.Organization != null)
                {
                    var organizationId = current.Organization.OrganizationID;
                    stores = stores.Where(s => s.OrganizationID == organizationId);
                }

                StoreBox.ItemsSource = stores
                    .OrderBy(s => s.Address)
                    .ToList();
            }
            catch (Exception ex)
            {
                ShowError("Не удалось загрузить справочники.\n" + ex.Message);
            }
        }

        private void FillForm()
        {
            LastNameBox.Text = _user.LastName;
            FirstNameBox.Text = _user.FirstName;
            PatronymicBox.Text = _user.Patronymic;
            BirthDatePicker.SelectedDate = _user.BirthDate;
            CityBox.Text = _user.City;
            AddressBox.Text = _user.Address;
            LoginBox.Text = _user.Login;
            PasswordBox.Password = _user.Password;

            LoginLabel.Text = _user.RoleValue == UserRole.Director
                ? "Налоговый номер"
                : "Логин";

            GenderBox.SelectedItem = _user.Gender;
            if (GenderBox.SelectedItem == null)
                GenderBox.SelectedIndex = -1;

            var isEmployee = _user.Role == Roles.Employee;
            WorkSection.Visibility = isEmployee ? Visibility.Visible : Visibility.Collapsed;

            if (isEmployee)
            {
                PositionBox.SelectedItem = PositionBox.Items
                    .OfType<Position>()
                    .FirstOrDefault(p => p.PositionID == _user.PositionID);

                StoreBox.SelectedItem = StoreBox.Items
                    .OfType<Store>()
                    .FirstOrDefault(s => s.StoreID == _user.StoreID);
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowError(null);

            var lastName = (LastNameBox.Text ?? string.Empty).Trim();
            var firstName = (FirstNameBox.Text ?? string.Empty).Trim();
            var patronymic = (PatronymicBox.Text ?? string.Empty).Trim();
            var city = (CityBox.Text ?? string.Empty).Trim();
            var address = (AddressBox.Text ?? string.Empty).Trim();
            var login = (LoginBox.Text ?? string.Empty).Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
            {
                ShowError("Укажите фамилию и имя.");
                return;
            }

            if (string.IsNullOrEmpty(login))
            {
                ShowError("Укажите логин.");
                return;
            }

            if (password.Length < 5)
            {
                ShowError("Пароль должен содержать не менее 5 символов.");
                return;
            }

            DateTime? birthDate = BirthDatePicker.SelectedDate.HasValue
                ? BirthDatePicker.SelectedDate.Value.Date
                : (DateTime?)null;

            if (birthDate.HasValue && birthDate.Value > DateTime.Today)
            {
                ShowError("Дата рождения не может быть в будущем.");
                return;
            }

            var isEmployee = _user.Role == Roles.Employee;
            Position position = null;
            Store store = null;

            if (isEmployee)
            {
                position = PositionBox.SelectedItem as Position;
                if (position == null)
                {
                    ShowError("Выберите должность.");
                    return;
                }

                store = StoreBox.SelectedItem as Store;
                if (store == null)
                {
                    ShowError("Выберите торговую точку.");
                    return;
                }
            }

            try
            {
                var context = Manager.Context;

                if (context.Users.Any(u => u.Login == login && u.UserID != _user.UserID))
                {
                    ShowError("Пользователь с таким логином уже существует.");
                    return;
                }

                _user.LastName = lastName;
                _user.FirstName = firstName;
                _user.Patronymic = string.IsNullOrEmpty(patronymic) ? null : patronymic;
                _user.Login = login;
                _user.Password = password;
                _user.BirthDate = birthDate;
                _user.Gender = GenderBox.SelectedItem as string;
                _user.City = string.IsNullOrEmpty(city) ? null : city;
                _user.Address = string.IsNullOrEmpty(address) ? null : address;

                if (isEmployee)
                {
                    _user.PositionID = position.PositionID;
                    _user.StoreID = store.StoreID;
                }

                if (_request != null && _request.Status == RequestStatus.Pending)
                {
                    _request.Status = RequestStatus.Applied;
                    _request.ResolvedAt = DateTime.Now;
                    _request.ManagerID = Manager.CurrentUser.Id;
                    _request.ManagerComment = "Данные изменены заведующим. Проверьте корректировки.";
                }

                context.SaveChanges();

                RefreshSession();

                MessageBox.Show(
                    _request != null
                        ? "Данные сохранены. Сотрудник получил сообщение о необходимости проверить корректировки."
                        : "Данные сохранены.",
                    "Редактирование",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                GoBack();
            }
            catch (Exception ex)
            {
                ShowError("Не удалось сохранить изменения.\n" + ex.Message);
            }
        }

        /// <summary>
        /// Если пользователь изменил собственные данные, обновляем сессию и шапку окна.
        /// </summary>
        private void RefreshSession()
        {
            var current = Manager.CurrentUser;
            if (current == null || current.Id != _user.UserID)
                return;

            var fresh = Manager.Context.Users
                .Include("Store.Organization")
                .Include("ManagedStores.Organization")
                .Include("Organizations")
                .FirstOrDefault(u => u.UserID == _user.UserID);

            if (fresh != null)
                Manager.CurrentUser = UserSession.FromUser(fresh);

            var window = Window.GetWindow(this) as MainWindow;
            if (window != null)
                window.RefreshChrome();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            if (Manager.MainFrame != null && Manager.MainFrame.CanGoBack)
                Manager.MainFrame.GoBack();
            else if (Manager.MainFrame != null)
                Manager.MainFrame.Navigate(new MainPage());
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message ?? string.Empty;
            ErrorText.Visibility = string.IsNullOrEmpty(message)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}
