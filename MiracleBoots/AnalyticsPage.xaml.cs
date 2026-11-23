using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MiracleBoots
{
    public partial class AnalyticsPage : Page
    {
        private bool _initializing;
        private bool _validationMessageShown;

        public AnalyticsPage()
        {
            InitializeComponent();

            if (Manager.CurrentUser == null || Manager.CurrentUser.ID_Role != 1)
            {
                MessageBox.Show("Раздел аналитики доступен только администратору.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(new Action(() => Manager.MainFrame.Navigate(new ProductPage())));
                return;
            }

            SetDefaultPeriod();
            Loaded += AnalyticsPage_Loaded;
        }

        /// <summary>
        /// Пересчитывает показатели при повторном открытии страницы,
        /// так как за это время состав заказов мог измениться.
        /// </summary>
        private void AnalyticsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Recalculate();
        }

        private void SetDefaultPeriod()
        {
            DateTime today = DateTime.Today;
            DateTime firstDay = new DateTime(today.Year, today.Month, 1);
            DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);

            _initializing = true;
            StartDatePicker.SelectedDate = firstDay;
            EndDatePicker.SelectedDate = lastDay;
            _initializing = false;

            Recalculate();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Recalculate();
        }

        private void Recalculate()
        {
            if (_initializing)
                return;

            DateTime start;
            DateTime end;

            if (!TryGetPeriod(out start, out end))
            {
                ClearReport();
                ShowValidationMessage();
                return;
            }

            _validationMessageShown = false;
            ClearError();

            try
            {
                var report = BuildReport(start, end);

                TotalOrdersText.Text = report.OrdersCount.ToString();
                TotalSalesText.Text = report.TotalSales.ToString("N2") + " руб.";

                TopModelsGrid.ItemsSource = report.TopModels;
                PeriodInfoText.Text = "Период: " + start.ToString("dd.MM.yyyy") +
                                      " — " + end.ToString("dd.MM.yyyy");

                ExportBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ClearReport();
                SetError("Не удалось рассчитать показатели: " + ex.Message);
                MessageBox.Show("Не удалось рассчитать показатели: " + ex.Message,
                    "Ошибка расчёта", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryGetPeriod(out DateTime start, out DateTime end)
        {
            start = DateTime.MinValue;
            end = DateTime.MinValue;

            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                SetError("Укажите даты начала и окончания периода.");
                return false;
            }

            start = StartDatePicker.SelectedDate.Value.Date;
            end = EndDatePicker.SelectedDate.Value.Date;

            if (start > end)
            {
                SetError("Дата начала периода не может быть позже даты окончания.");
                return false;
            }

            return true;
        }

        private SalesReport BuildReport(DateTime start, DateTime end)
        {
            var context = MiracleBootsEntities.GetContext();

            var ordersInPeriod = context.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate <= end);

            int ordersCount = ordersInPeriod.Count();

            // Сумма продаж считается по итоговым суммам заказов за выбранный период.
            var amounts = ordersInPeriod
                .Select(o => o.TotalAmount)
                .ToList();

            decimal totalSales = amounts.Sum(value => value ?? 0m);

            var items = context.OrderProducts
                .Where(op => op.Orders.OrderDate >= start && op.Orders.OrderDate <= end)
                .Select(op => new
                {
                    op.StockItems.ID_Product,
                    op.StockItems.Products.ProductName,
                    op.StockItems.Products.Manufactures.ManufactureName,
                    op.Quantity
                })
                .ToList();

            var topModels = items
                .GroupBy(x => new { x.ID_Product, x.ProductName, x.ManufactureName })
                .Select(g => new TopModelRow
                {
                    Model = g.Key.ProductName,
                    Manufacturer = g.Key.ManufactureName,
                    Pairs = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(r => r.Pairs)
                .ThenBy(r => r.Model)
                .Take(5)
                .ToList();

            for (int i = 0; i < topModels.Count; i++)
                topModels[i].Rank = i + 1;

            return new SalesReport
            {
                Start = start,
                End = end,
                OrdersCount = ordersCount,
                TotalSales = totalSales,
                TopModels = topModels
            };
        }

        private void ClearReport()
        {
            TotalOrdersText.Text = "—";
            TotalSalesText.Text = "—";
            TopModelsGrid.ItemsSource = null;
            PeriodInfoText.Text = string.Empty;
            ExportBtn.IsEnabled = false;
        }

        private void SetError(string message)
        {
            ErrorText.Text = "Ошибка: " + message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void ClearError()
        {
            ErrorText.Text = string.Empty;
            ErrorText.Visibility = Visibility.Collapsed;
        }

        private void ShowValidationMessage()
        {
            if (_validationMessageShown)
                return;

            _validationMessageShown = true;

            string message;
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
                message = "Укажите даты начала и окончания периода.";
            else
                message = "Дата начала периода не может быть позже даты окончания. " +
                          "Исправьте период, чтобы увидеть отчёт.";

            MessageBox.Show(message, "Некорректный период",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            DateTime start;
            DateTime end;

            if (!TryGetPeriod(out start, out end))
            {
                MessageBox.Show("Исправьте период: дата начала не может быть позже даты окончания.",
                    "Некорректный период", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SalesReport report;
            try
            {
                report = BuildReport(start, end);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подготовить отчёт: " + ex.Message,
                    "Ошибка формирования отчёта", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Сохранение отчёта",
                Filter = "PDF-файл (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName = "Отчет_о_продажах_" + start.ToString("yyyy-MM-dd") + "_" +
                           end.ToString("yyyy-MM-dd") + ".pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                PdfReportExporter.Export(dialog.FileName, report);

                MessageBox.Show("Отчёт успешно сохранён:\n" + dialog.FileName,
                    "Экспорт в PDF", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить PDF-файл: " + ex.Message,
                    "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
