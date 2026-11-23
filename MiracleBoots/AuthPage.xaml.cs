using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MiracleBoots
{
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
            Loaded += AuthPage_Loaded;
        }

        private void AuthPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoginBox.Focus();
        }

        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Введите логин пользователя. Поле логина не может быть пустым.",
                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginBox.Focus();
                return;
            }

            try
            {
                var user = MiracleBootsEntities.GetContext().Users
                    .FirstOrDefault(u => u.UserLogin == login);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с логином «" + login + "» не найден.\n" +
                                    "Проверьте правильность ввода. Логин должен совпадать с логином " +
                                    "из списка пользователей системы.",
                        "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoginBox.SelectAll();
                    LoginBox.Focus();
                    return;
                }

                Manager.CurrentUser = user;
                Manager.Cart.Clear();
                Manager.MainFrame.Navigate(new ProductPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось выполнить вход: " + ex.Message,
                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GuestBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.CurrentUser = null;
            Manager.Cart.Clear();
            Manager.MainFrame.Navigate(new ProductPage());
        }
    }
}
