using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TradingOrganization
{
    /// <summary>
    /// Регистрация нового сотрудника. Роль всегда "сотрудник", без выбора роли.
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();

            GenderBox.ItemsSource = new[] { "М", "Ж" };
            GenderBox.SelectedIndex = 0;

            LoadDictionaries();
        }

        private void LoadDictionaries()
        {
            try
            {
                PositionBox.ItemsSource = Manager.Context.Positions
                    .OrderBy(p => p.Name)
                    .ToList();

                StoreBox.ItemsSource = Manager.Context.Stores
                    .Include("Organization")
                    .OrderBy(s => s.Address)
                    .ToList();
            }
            catch (Exception ex)
            {
                ShowError("Не удалось загрузить справочники.\n" + ex.Message);
            }
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowError(null);

            var lastName = (LastNameBox.Text ?? string.Empty).Trim();
            var firstName = (FirstNameBox.Text ?? string.Empty).Trim();
            var patronymic = (PatronymicBox.Text ?? string.Empty).Trim();
            var city = (CityBox.Text ?? string.Empty).Trim();
            var address = (AddressBox.Text ?? string.Empty).Trim();
            var login = (LoginBox.Text ?? string.Empty).Trim();
            var password = PasswordBox.Password;
            var passwordAgain = PasswordAgainBox.Password;

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

            if (password != passwordAgain)
            {
                ShowError("Пароли не совпадают.");
                return;
            }

            if (!BirthDatePicker.SelectedDate.HasValue)
            {
                ShowError("Укажите дату рождения.");
                return;
            }

            var birthDate = BirthDatePicker.SelectedDate.Value.Date;
            if (birthDate > DateTime.Today)
            {
                ShowError("Дата рождения не может быть в будущем.");
                return;
            }

            if (PositionBox.SelectedItem == null)
            {
                ShowError("Выберите должность.");
                return;
            }

            var store = StoreBox.SelectedItem as Store;
            if (store == null)
            {
                ShowError("Выберите торговую точку.");
                return;
            }

            try
            {
                var context = Manager.Context;

                if (context.Users.Any(u => u.Login == login))
                {
                    ShowError("Пользователь с таким логином уже существует.");
                    return;
                }

                var user = new User
                {
                    LastName = lastName,
                    FirstName = firstName,
                    Patronymic = string.IsNullOrEmpty(patronymic) ? null : patronymic,
                    Login = login,
                    Password = password,
                    BirthDate = birthDate,
                    Gender = GenderBox.SelectedItem as string,
                    City = string.IsNullOrEmpty(city) ? null : city,
                    Address = string.IsNullOrEmpty(address) ? null : address,
                    Role = Roles.Employee,
                    PositionID = ((Position)PositionBox.SelectedItem).PositionID,
                    StoreID = store.StoreID
                };

                context.Users.Add(user);
                context.SaveChanges();

                MessageBox.Show(
                    "Регистрация завершена.\nТеперь войдите в систему под своим логином.",
                    "Регистрация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Manager.ResetNavigation(new AuthPage(login));
            }
            catch (Exception ex)
            {
                ShowError("Не удалось сохранить пользователя.\n" + ex.Message);
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.MainFrame != null && Manager.MainFrame.CanGoBack)
                Manager.MainFrame.GoBack();
            else
                Manager.ResetNavigation(new AuthPage());
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
