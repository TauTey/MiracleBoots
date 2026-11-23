using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace MiracleBoots
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Единый русский формат чисел и дат независимо от региональных
            // настроек компьютера: пробел между разрядами, запятая в дробной части.
            var russianCulture = CultureInfo.GetCultureInfo("ru-RU");
            CultureInfo.CurrentCulture = russianCulture;
            CultureInfo.CurrentUICulture = russianCulture;
            CultureInfo.DefaultThreadCurrentCulture = russianCulture;
            CultureInfo.DefaultThreadCurrentUICulture = russianCulture;
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(russianCulture.IetfLanguageTag)));

            base.OnStartup(e);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// Перехватывает необработанные исключения интерфейса, чтобы приложение
        /// не завершалось аварийно, и показывает пользователю понятное сообщение.
        /// </summary>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ShowError(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
                WriteLog(exception);
        }

        private void ShowError(Exception exception)
        {
            WriteLog(exception);

            MessageBox.Show("Произошла непредвиденная ошибка: " + exception.Message + "\n\n" +
                            "Попробуйте повторить действие. Если ошибка повторится, " +
                            "сообщите администратору и приложите файл журнала:\n" + GetLogPath(),
                "Непредвиденная ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void WriteLog(Exception exception)
        {
            try
            {
                File.AppendAllText(GetLogPath(),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + exception + Environment.NewLine);
            }
            catch
            {
                // Запись журнала не должна приводить к повторному исключению.
            }
        }

        private static string GetLogPath()
        {
            return Path.Combine(Path.GetTempPath(), "MiracleBoots_error.log");
        }
    }
}
