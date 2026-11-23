using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TradingOrganization
{
    /// <summary>
    /// Страница авторизации. Роль определяется автоматически по данным пользователя.
    /// </summary>
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        public AuthPage(string login)
            : this()
        {
            LoginBox.Text = login;
            Loaded += (s, e) => PasswordBox.Focus();
        }

        private void AuthField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                TryLogin();
        }

        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new RegisterPage());
        }

        private void TryLogin()
        {
            ShowError(null);

            var login = LoginBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите логин и пароль.");
                return;
            }

            try
            {
                var user = Manager.Context.Users
                    .Include("Store.Organization")
                    .Include("ManagedStores.Organization")
                    .Include("Organizations")
                    .FirstOrDefault(u => u.Login == login && u.Password == password);

                if (user == null)
                {
                    ShowError("Пользователь с таким логином и паролем не найден.");
                    return;
                }

                Manager.CurrentUser = UserSession.FromUser(user);
                Manager.ResetNavigation(new MainPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось подключиться к базе данных.\n\n" + ex.Message,
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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
