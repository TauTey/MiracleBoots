using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MiracleBoots
{
    public partial class OrderDetailsPage : Page
    {
        private readonly int _currentOrderId;
        private Orders _currentOrder;
        private List<OrderProducts> _currentItems;
        private readonly bool _isAdmin;

        // Количества позиций, сохранённые в базе на момент открытия заказа.
        // Нужны для корректного пересчёта остатков: изменение состава заказа
        // затрагивает отслеживаемые сущности до сохранения, поэтому читать
        // исходные значения повторно из контекста нельзя.
        private Dictionary<int, int> _originalQuantities = new Dictionary<int, int>();
        private bool _saved;

        public OrderDetailsPage(int orderId)
        {
            InitializeComponent();

            if (Manager.CurrentUser == null ||
                (Manager.CurrentUser.ID_Role != 1 && Manager.CurrentUser.ID_Role != 2))
            {
                MessageBox.Show("У вас нет прав для просмотра заказов. Раздел доступен администратору и менеджеру.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(new Action(() => Manager.MainFrame.Navigate(new ProductPage())));
                return;
            }

            _currentOrderId = orderId;
            _isAdmin = Manager.CurrentUser.ID_Role == 1;

            // Менеджер работает с заказами только в режиме просмотра:
            // действия редактирования ему недоступны.
            DeletePositionButton.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            SaveButton.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;

            Title = _isAdmin
                ? "Редактирование заказа — ЧудоОбувь"
                : "Просмотр заказа — ЧудоОбувь";

            OrderDatePicker.IsEnabled = _isAdmin;
            ClientCombo.IsEnabled = _isAdmin && _currentOrderId == 0;

            try
            {
                LoadClients();
                LoadOrder();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить заказ: " + ex.Message,
                    "Ошибка загрузки данных", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(new Action(() => Manager.MainFrame.Navigate(new OrdersPage())));
            }
        }

        private void LoadClients()
        {
            var users = MiracleBootsEntities.GetContext().Users
                .OrderBy(u => u.UserSurname)
                .ThenBy(u => u.UserName)
                .ToList();

            ClientCombo.ItemsSource = users;
        }

        private void LoadOrder()
        {
            var context = MiracleBootsEntities.GetContext();

            if (_currentOrderId == 0)
            {
                _currentOrder = new Orders
                {
                    OrderDate = DateTime.Today,
                    ID_User = Manager.CurrentUser.ID_User,
                    TotalAmount = 0
                };

                OrderDatePicker.SelectedDate = _currentOrder.OrderDate;

                var clients = ClientCombo.ItemsSource as List<Users>;
                if (clients != null)
                    ClientCombo.SelectedItem = clients.FirstOrDefault(u => u.ID_User == Manager.CurrentUser.ID_User);

                _currentItems = new List<OrderProducts>();
            }
            else
            {
                _currentOrder = context.Orders
                    .Include("Users")
                    .FirstOrDefault(o => o.ID_Order == _currentOrderId);

                if (_currentOrder == null)
                {
                    MessageBox.Show("Заказ не найден.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Dispatcher.BeginInvoke(new Action(() => Manager.MainFrame.Navigate(new OrdersPage())));
                    return;
                }

                OrderDatePicker.SelectedDate = _currentOrder.OrderDate;

                var clients = ClientCombo.ItemsSource as List<Users>;
                if (clients != null)
                    ClientCombo.SelectedItem = clients.FirstOrDefault(u => u.ID_User == _currentOrder.ID_User);

                _currentItems = context.OrderProducts
                    .Include("StockItems.Products.Manufactures")
                    .Include("StockItems.Sizes")
                    .Where(op => op.ID_Order == _currentOrderId)
                    .ToList();

                _originalQuantities = _currentItems
                    .GroupBy(op => op.ID_StockItem)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
            }

            RefreshItems();
        }

        private void RefreshItems()
        {
            var displayItems = _currentItems.Select(op => new
            {
                OrderProduct = op,
                ProductInfo = op.StockItems.Products.Manufactures.ManufactureName + " — " +
                              op.StockItems.Products.ProductName + " | Размер: " + op.StockItems.Sizes.Size.ToString("0.#"),
                op.Quantity,
                op.PriceAtOrderTime,
                Total = op.Quantity * op.PriceAtOrderTime
            }).ToList();

            OrderItemsGrid.ItemsSource = displayItems;

            decimal total = _currentItems.Sum(op => op.Quantity * op.PriceAtOrderTime);
            TotalAmountText.Text = "Итого: " + total.ToString("N2") + " руб.";
        }

        private void DeletePosition_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin)
            {
                MessageBox.Show("Менеджер может только просматривать заказы.",
                    "Недостаточно прав", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (OrderItemsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите позицию, которую нужно удалить.",
                    "Позиция не выбрана", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var property = OrderItemsGrid.SelectedItem.GetType().GetProperty("OrderProduct");
            var orderProduct = property?.GetValue(OrderItemsGrid.SelectedItem) as OrderProducts;

            if (orderProduct == null)
                return;

            _currentItems.Remove(orderProduct);
            RefreshItems();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin)
            {
                MessageBox.Show("Менеджер может только просматривать заказы.",
                    "Недостаточно прав", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!OrderDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите дату заказа.",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var client = ClientCombo.SelectedItem as Users;
            if (client == null)
            {
                MessageBox.Show("Выберите клиента, для которого оформляется заказ.",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentItems.Count == 0)
            {
                MessageBox.Show("Заказ не может быть пустым. Добавьте хотя бы одну товарную позицию.",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var context = MiracleBootsEntities.GetContext();

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    if (_currentOrderId == 0)
                        SaveNewOrder(context, client);
                    else
                        SaveExistingOrder(context);

                    context.SaveChanges();
                    transaction.Commit();
                    _saved = true;

                    Products.RefreshDiscountCache();

                    MessageBox.Show("Заказ успешно сохранён.",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (Manager.MainFrame.CanGoBack)
                        Manager.MainFrame.GoBack();
                    else
                        Manager.MainFrame.Navigate(new OrdersPage());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Не удалось сохранить заказ: " + ex.Message,
                        "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveNewOrder(MiracleBootsEntities context, Users client)
        {
            var desired = _currentItems
                .GroupBy(i => i.ID_StockItem)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            // Проверяем остатки до изменения данных.
            foreach (var pair in desired)
            {
                var stock = context.StockItems.FirstOrDefault(si => si.ID_StockItem == pair.Key);
                if (stock == null)
                    throw new InvalidOperationException("Товарная позиция не найдена на складе.");

                if (pair.Value > stock.ItemsQuantity)
                {
                    throw new InvalidOperationException(
                        "Товара «" + stock.Products.ProductName + "» (размер " + stock.Sizes.Size.ToString("0.#") +
                        ") осталось только " + stock.ItemsQuantity + " пар.");
                }
            }

            _currentOrder.OrderDate = OrderDatePicker.SelectedDate.Value;
            _currentOrder.ID_User = client.ID_User;
            _currentOrder.TotalAmount = _currentItems.Sum(op => op.Quantity * op.PriceAtOrderTime);

            context.Orders.Add(_currentOrder);
            context.SaveChanges();

            foreach (var item in _currentItems)
            {
                item.ID_Order = _currentOrder.ID_Order;
                context.OrderProducts.Add(item);
            }

            foreach (var pair in desired)
            {
                var stock = context.StockItems.First(si => si.ID_StockItem == pair.Key);
                stock.ItemsQuantity -= pair.Value;
            }
        }

        private void SaveExistingOrder(MiracleBootsEntities context)
        {
            var dbOrder = context.Orders.FirstOrDefault(o => o.ID_Order == _currentOrderId);
            if (dbOrder == null)
                throw new InvalidOperationException("Заказ не найден.");

            var dbItems = context.OrderProducts
                .Where(op => op.ID_Order == _currentOrderId)
                .ToList();

            var desired = _currentItems
                .GroupBy(i => i.ID_StockItem)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            // Исходные количества берём из снимка, сделанного при открытии заказа:
            // отслеживаемые сущности уже могли быть изменены в интерфейсе.
            var original = _originalQuantities;

            // Пересчитываем остатки на складе с учётом изменения состава заказа.
            foreach (int stockItemId in desired.Keys.Union(original.Keys))
            {
                int wanted = desired.ContainsKey(stockItemId) ? desired[stockItemId] : 0;
                int had = original.ContainsKey(stockItemId) ? original[stockItemId] : 0;
                int delta = wanted - had;

                if (delta == 0)
                    continue;

                var stock = context.StockItems.FirstOrDefault(si => si.ID_StockItem == stockItemId);
                if (stock == null)
                    throw new InvalidOperationException("Товарная позиция не найдена на складе.");

                if (delta > 0)
                {
                    if (stock.ItemsQuantity < delta)
                    {
                        throw new InvalidOperationException(
                            "Товара «" + stock.Products.ProductName + "» (размер " + stock.Sizes.Size.ToString("0.#") +
                            ") осталось только " + stock.ItemsQuantity + " пар, а требуется добавить ещё " +
                            delta + ".");
                    }

                    stock.ItemsQuantity -= delta;
                }
                else
                {
                    stock.ItemsQuantity += -delta;
                }
            }

            dbOrder.OrderDate = OrderDatePicker.SelectedDate.Value;
            dbOrder.TotalAmount = _currentItems.Sum(op => op.Quantity * op.PriceAtOrderTime);

            var currentIds = new HashSet<int>(
                _currentItems.Where(i => i.ID_OrderItem != 0).Select(i => i.ID_OrderItem));

            foreach (var dbItem in dbItems)
            {
                if (!currentIds.Contains(dbItem.ID_OrderItem))
                    context.OrderProducts.Remove(dbItem);
            }

            foreach (var item in _currentItems)
            {
                if (item.ID_OrderItem == 0)
                {
                    item.ID_Order = _currentOrderId;
                    context.OrderProducts.Add(item);
                }
                else
                {
                    var dbItem = dbItems.FirstOrDefault(i => i.ID_OrderItem == item.ID_OrderItem);
                    if (dbItem != null)
                    {
                        dbItem.Quantity = item.Quantity;
                        dbItem.PriceAtOrderTime = item.PriceAtOrderTime;
                    }
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.MainFrame.CanGoBack)
                Manager.MainFrame.GoBack();
            else
                Manager.MainFrame.Navigate(new OrdersPage());
        }

        /// <summary>
        /// При уходе со страницы без сохранения возвращает отслеживаемые сущности
        /// к состоянию базы данных, чтобы несохранённые правки не попали в заказ
        /// при следующем сохранении из другого раздела.
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_saved || _currentOrderId == 0)
                return;

            try
            {
                var context = MiracleBootsEntities.GetContext();

                if (_currentOrder != null)
                    context.Entry(_currentOrder).Reload();

                foreach (var item in _currentItems.Where(i => i.ID_OrderItem != 0))
                    context.Entry(item).Reload();
            }
            catch
            {
                // Сброс несохранённых изменений не должен мешать навигации.
            }
        }
    }
}
