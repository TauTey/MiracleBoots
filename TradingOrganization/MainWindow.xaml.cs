using System;
using System.Windows;
using System.Windows.Controls;

namespace TradingOrganization
{
    /// <summary>
    /// Главное окно: шапка, навигация и контейнер для страниц.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var workArea = SystemParameters.WorkArea;
            if (Width > workArea.Width - 40)
                Width = workArea.Width - 40;
            if (Height > workArea.Height - 40)
                Height = workArea.Height - 40;

            Manager.MainFrame = MainFrame;
            MainFrame.Navigate(new AuthPage());
        }

        private void MainFrame_ContentRendered(object sender, EventArgs e)
        {
            RefreshChrome();
        }

        public void RefreshChrome()
        {
            RefreshHeader();

            // Корневые страницы (вход и главная) не должны предлагать возврат назад,
            // иначе после входа можно вернуться на страницу авторизации.
            var isRootPage = !Manager.IsAuthorized
                             || MainFrame.Content is AuthPage
                             || MainFrame.Content is MainPage;

            BtnBack.Visibility = !isRootPage && MainFrame.CanGoBack
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        public void RefreshHeader()
        {
            var user = Manager.CurrentUser;

            if (user == null)
            {
                UserPanel.Visibility = Visibility.Collapsed;
                HeaderAvatar.Source = null;
                return;
            }

            UserPanel.Visibility = Visibility.Visible;
            HeaderName.Text = user.ФИО;
            HeaderRole.Text = user.РольНазвание;
            HeaderAvatar.Source = PhotoHelper.Load(user.Фото, user.PhotoFolder);
        }

        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is ProfilePage)
                return;

            MainFrame.Navigate(new ProfilePage());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }
    }
}
