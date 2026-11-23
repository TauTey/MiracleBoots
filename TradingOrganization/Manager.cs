using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TradingOrganization
{
    /// <summary>
    /// Общее состояние приложения: текущий пользователь, навигация и контекст базы данных.
    /// </summary>
    internal static class Manager
    {
        private static TradingContext _context;

        public static Frame MainFrame { get; set; }

        public static UserSession CurrentUser { get; set; }

        public static bool IsAuthorized
        {
            get { return CurrentUser != null; }
        }

        public static TradingContext Context
        {
            get { return _context ?? (_context = new TradingContext()); }
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        /// <summary>
        /// Переход на страницу с очисткой истории (используется при входе и выходе).
        /// </summary>
        public static void ResetNavigation(Page page)
        {
            if (MainFrame == null)
                return;

            MainFrame.Navigate(page);

            // Запись в журнале навигации появляется уже после завершения перехода,
            // поэтому чистим историю отложенно — когда страница загружена.
            MainFrame.Dispatcher.BeginInvoke(new Action(() =>
            {
                while (MainFrame.CanGoBack)
                    MainFrame.RemoveBackEntry();

                var window = Window.GetWindow(MainFrame) as MainWindow;
                if (window != null)
                    window.RefreshChrome();
            }), DispatcherPriority.Loaded);

            var currentWindow = Window.GetWindow(MainFrame) as MainWindow;
            if (currentWindow != null)
                currentWindow.RefreshChrome();
        }
    }
}
