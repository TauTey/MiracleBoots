using System;
using System.Windows;
using System.Windows.Controls;

namespace MiracleBoots
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Manager.MainFrame = MainFrame;
            MainFrame.Navigated += MainFrame_Navigated;
            MainFrame.Navigate(new AuthPage());
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var page = e.Content as Page;

            UpdateNavigation(page);

            if (page != null && !string.IsNullOrWhiteSpace(page.Title))
                Title = page.Title;
        }

        /// <summary>
        /// Обновляет сведения о пользователе и доступность разделов меню
        /// в зависимости от роли и открытой страницы.
        /// </summary>
        private void UpdateNavigation(Page page)
        {
            bool onAuthPage = page is AuthPage;
            bool isAuthorized = Manager.CurrentUser != null;
            int roleId = isAuthorized ? Manager.CurrentUser.ID_Role : 0;

            if (isAuthorized && !onAuthPage)
            {
                string patronymic = string.IsNullOrWhiteSpace(Manager.CurrentUser.UserPatronymic)
                    ? string.Empty
                    : " " + Manager.CurrentUser.UserPatronymic;

                UserInfoTextBlock.Text = Manager.CurrentUser.UserSurname + " " +
                                         Manager.CurrentUser.UserName + patronymic;
                UserInfoTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                UserInfoTextBlock.Text = string.Empty;
                UserInfoTextBlock.Visibility = Visibility.Collapsed;
            }

            // На странице входа навигация по разделам недоступна.
            CartBtn.Visibility = onAuthPage ? Visibility.Collapsed : Visibility.Visible;
            CatalogBtn.Visibility = onAuthPage ? Visibility.Collapsed : Visibility.Visible;
            OrdersBtn.Visibility = !onAuthPage && (roleId == 1 || roleId == 2)
                ? Visibility.Visible
                : Visibility.Collapsed;
            AnalyticsBtn.Visibility = !onAuthPage && roleId == 1
                ? Visibility.Visible
                : Visibility.Collapsed;

            BtnBack.Visibility = !onAuthPage && MainFrame.CanGoBack
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        private void CatalogBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProductPage());
        }

        private void OrdersBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrdersPage());
        }

        private void AnalyticsBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AnalyticsPage());
        }

        private void CartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.CurrentUser == null)
            {
                MessageBox.Show("Для оформления заказа необходимо авторизоваться.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainFrame.Navigate(new CheckoutPage());
        }
    }
}
