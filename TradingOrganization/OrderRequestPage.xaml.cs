using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TradingOrganization
{
    /// <summary>
    /// Заявка заведующего на заказ товара у поставщика.
    /// Шаг 1 — выбор поставщика, шаг 2 — состав заявки.
    /// Заявка уходит директору организации торговой точки.
    /// </summary>
    public partial class OrderRequestPage : Page
    {
        private static readonly string[] UnitOptions = { "шт.", "упак.", "кг", "г", "л", "мл" };

        private readonly UserSession _user;
        private readonly int? _requestId;
        private List<Supplier> _suppliers;
        private OrderRequest _existing;

        public OrderRequestPage()
            : this(0)
        {
        }

        /// <summary>
        /// Режим редактирования: requestId — идентификатор отправленной заявки,
        /// которую заведующий ещё может изменить.
        /// </summary>
        public OrderRequestPage(int requestId)
        {
            InitializeComponent();

            _requestId = requestId > 0 ? requestId : (int?)null;
            _user = Manager.CurrentUser;

            if (_user == null)
            {
                Manager.ResetNavigation(new AuthPage());
                return;
            }

            if (_user.Role != UserRole.Manager)
            {
                MessageBox.Show("Заявки на заказы создают заведующие торговыми точками.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                GoBack();
                return;
            }

            if (_user.Point == null)
            {
                MessageBox.Show("У вас не назначена торговая точка.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                GoBack();
                return;
            }

            Configure();
        }

        private void Configure()
        {
            SubtitleText.Text = "Шаг 1 из 2: выберите поставщика";

            UnitBox.ItemsSource = UnitOptions;
            UnitBox.SelectedIndex = 0;

            DateText.Text = DateTime.Today.ToString("dd.MM.yyyy");
            NextBtn.IsEnabled = false;

            try
            {
                _suppliers = Manager.Context.Suppliers
                    .OrderBy(s => s.Name)
                    .ToList();

                SupplierBox.ItemsSource = _suppliers;
            }
            catch (Exception ex)
            {
                ShowError("Не удалось загрузить список поставщиков.\n" + ex.Message);
            }

            RecipientText.Text = BuildRecipientText();

            if (_requestId.HasValue)
                LoadExistingRequest();
        }

        /// <summary>
        /// Заполнение формы данными ранее отправленной заявки.
        /// </summary>
        private void LoadExistingRequest()
        {
            OrderRequest request;

            try
            {
                request = Manager.Context.OrderRequests
                    .Include("Supplier")
                    .FirstOrDefault(r => r.OrderRequestID == _requestId.Value);
            }
            catch (Exception ex)
            {
                ShowError("Не удалось загрузить заявку.\n" + ex.Message);
                return;
            }

            if (request == null || request.ManagerID != _user.Id)
            {
                MessageBox.Show("Заявка не найдена.",
                    "Заявка на заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                GoBack();
                return;
            }

            if (request.Status != OrderRequestStatus.Pending)
            {
                MessageBox.Show(
                    "Заявку уже рассмотрел директор — изменения недоступны.",
                    "Заявка на заказ", MessageBoxButton.OK, MessageBoxImage.Information);
                GoBack();
                return;
            }

            _existing = request;

            Title = "Изменение заявки на заказ поставщику";
            SubtitleText.Text = "Изменение заявки №" + request.OrderRequestID;
            SendBtn.Content = "Сохранить изменения";
            DateText.Text = request.CreatedAt.ToString("dd.MM.yyyy");

            var supplier = _suppliers != null
                ? _suppliers.FirstOrDefault(s => s.SupplierID == request.SupplierID)
                : null;

            if (supplier == null)
            {
                ShowError("Поставщик, указанный в заявке, больше недоступен. Выберите другого поставщика.");
            }
            else
            {
                SupplierBox.SelectedItem = supplier;
                SupplierNameText.Text = supplier.Name;
                SupplierDetailsText.Text = supplier.ActivityType + ", " + supplier.City +
                                           ", " + supplier.Address;
            }

            ProductBox.Text = request.ProductName;
            QuantityBox.Text = request.Quantity.ToString("0.##");
            PriceBox.Text = request.Price.ToString("0.##");

            var unitIndex = Array.IndexOf(UnitOptions, request.Unit);
            UnitBox.SelectedIndex = unitIndex >= 0 ? unitIndex : 0;

            SupplierStep.Visibility = Visibility.Collapsed;
            FormStep.Visibility = Visibility.Visible;
        }

        private string BuildRecipientText()
        {
            try
            {
                var organization = LoadOrganization();

                if (organization != null && organization.Director != null)
                {
                    return "Получатель: " + organization.Director.ФИО +
                           ", директор организации «" + organization.Name + "».";
                }
            }
            catch
            {
                // ниже вернётся общий текст
            }

            return "Получатель: директор вашей организации.";
        }

        private Organization LoadOrganization()
        {
            var organizationId = _user.Organization != null
                ? _user.Organization.OrganizationID
                : _user.Point.OrganizationID;

            return Manager.Context.Organizations
                .Include("Director")
                .FirstOrDefault(o => o.OrganizationID == organizationId);
        }

        private void SupplierBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var supplier = SupplierBox.SelectedItem as Supplier;

            NextBtn.IsEnabled = supplier != null;

            if (supplier == null)
            {
                SupplierInfoText.Visibility = Visibility.Collapsed;
                return;
            }

            SupplierInfoText.Text = supplier.ActivityType + ", " + supplier.Country +
                                    ", " + supplier.City + ", " + supplier.Address;
            SupplierInfoText.Visibility = Visibility.Visible;
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowError(null);

            var supplier = SupplierBox.SelectedItem as Supplier;

            if (supplier == null)
            {
                ShowError("Выберите поставщика из списка.");
                return;
            }

            SupplierNameText.Text = supplier.Name;
            SupplierDetailsText.Text = supplier.ActivityType + ", " + supplier.City +
                                       ", " + supplier.Address;

            SupplierStep.Visibility = Visibility.Collapsed;
            FormStep.Visibility = Visibility.Visible;
            SubtitleText.Text = "Шаг 2 из 2: составьте заявку";
        }

        private void ChangeSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            FormStep.Visibility = Visibility.Collapsed;
            SupplierStep.Visibility = Visibility.Visible;
            SubtitleText.Text = "Шаг 1 из 2: выберите поставщика";
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowError(null);

            var supplier = SupplierBox.SelectedItem as Supplier;

            if (supplier == null)
            {
                ShowError("Выберите поставщика из списка.");
                return;
            }

            var product = (ProductBox.Text ?? string.Empty).Trim();

            if (product.Length < 2)
            {
                ShowError("Укажите наименование товара.");
                return;
            }

            decimal quantity;
            if (!TryParsePositive(QuantityBox.Text, out quantity))
            {
                ShowError("Укажите количество больше нуля (например: 12 или 2,5).");
                return;
            }

            decimal price;
            if (!TryParsePositive(PriceBox.Text, out price))
            {
                ShowError("Укажите цену больше нуля.");
                return;
            }

            var unit = UnitBox.SelectedItem as string ?? UnitOptions[0];

            if (_existing != null)
            {
                SaveChangesToExisting(supplier, product, quantity, unit, price);
                return;
            }

            try
            {
                var organization = LoadOrganization();

                if (organization == null)
                {
                    ShowError("Не удалось определить организацию вашей торговой точки.");
                    return;
                }

                var request = new OrderRequest
                {
                    StoreID = _user.Point.StoreID,
                    ManagerID = _user.Id,
                    DirectorID = organization.DirectorID,
                    SupplierID = supplier.SupplierID,
                    ProductName = product,
                    Quantity = Math.Round(quantity, 2),
                    Unit = unit,
                    Price = Math.Round(price, 2),
                    Status = OrderRequestStatus.Pending,
                    CreatedAt = DateTime.Now
                };

                Manager.Context.OrderRequests.Add(request);
                Manager.Context.SaveChanges();

                var view = new MainPage(true);
                Manager.ResetNavigation(view);

                MessageBox.Show(
                    "Заявка №" + request.OrderRequestID + " отправлена директору организации.\n" +
                    "Ответ появится в разделе «Заявки на заказы».",
                    "Заявка на заказ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("Не удалось отправить заявку.\n" + ex.Message);
            }
        }

        private void SaveChangesToExisting(Supplier supplier, string product,
                                           decimal quantity, string unit, decimal price)
        {
            try
            {
                var context = Manager.Context;

                var current = context.OrderRequests
                    .FirstOrDefault(r => r.OrderRequestID == _existing.OrderRequestID);

                if (current == null)
                {
                    ShowError("Заявка не найдена.");
                    return;
                }

                context.Entry(current).Reload();

                if (current.Status != OrderRequestStatus.Pending)
                {
                    ShowError("Заявку уже рассмотрел директор — изменения недоступны.");
                    return;
                }

                current.SupplierID = supplier.SupplierID;
                current.ProductName = product;
                current.Quantity = Math.Round(quantity, 2);
                current.Unit = unit;
                current.Price = Math.Round(price, 2);
                context.SaveChanges();

                var view = new MainPage(true);
                Manager.ResetNavigation(view);

                MessageBox.Show(
                    "Изменения по заявке №" + current.OrderRequestID + " сохранены.\n" +
                    "Заявка остаётся на рассмотрении директора.",
                    "Заявка на заказ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("Не удалось сохранить изменения.\n" + ex.Message);
            }
        }

        private static bool TryParsePositive(string text, out decimal value)
        {
            value = 0m;

            var normalized = (text ?? string.Empty).Trim().Replace(" ", string.Empty);

            if (normalized.Length == 0)
                return false;

            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
                return value > 0m;

            return decimal.TryParse(normalized.Replace(',', '.'), NumberStyles.Number,
                CultureInfo.InvariantCulture, out value) && value > 0m;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            Manager.ResetNavigation(new MainPage(true));
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
