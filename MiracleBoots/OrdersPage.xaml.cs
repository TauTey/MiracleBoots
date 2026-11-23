using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MiracleBoots
{
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();

            if (Manager.CurrentUser == null ||
                (Manager.CurrentUser.ID_Role != 1 && Manager.CurrentUser.ID_Role != 2))
            {
                MessageBox.Show("У вас нет прав для работы с заказами. Раздел доступен администратору и менеджеру.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.BeginInvoke(new Action(() => Manager.MainFrame.Navigate(new ProductPage())));
                return;
            }

            // Менеджеру доступен только просмотр списка заказов.
            bool isAdmin = Manager.CurrentUser.ID_Role == 1;
            DeleteOrderButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            Title = isAdmin
                ? "Управление заказами — ЧудоОбувь"
                : "Просмотр заказов — ЧудоОбувь";
        }

        private void LoadOrders()
        {
            try
            {
                var context = MiracleBootsEntities.GetContext();
                var orders = context.Orders
                    .Include("Users")
                    .OrderByDescending(o => o.OrderDate)
                    .ThenByDescending(o => o.ID_Order)
                    .ToList();

                var displayList = orders.Select(o => new
                {
                    o.ID_Order,
                    o.OrderDate,
                    UserFullName = (o.Users.UserSurname + " " + o.Users.UserName + " " +
                                    (o.Users.UserPatronymic ?? string.Empty)).Trim(),
                    o.TotalAmount
                }).ToList();

                OrdersGrid.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить список заказов: " + ex.Message,
                    "Ошибка загрузки данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetSelectedOrderId()
        {
            if (OrdersGrid.SelectedItem == null)
                return 0;

            var property = OrdersGrid.SelectedItem.GetType().GetProperty("ID_Order");
            return property == null ? 0 : (int)property.GetValue(OrdersGrid.SelectedItem);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        /// <summary>
        /// Обновляет список при возврате на страницу, чтобы отобразить
        /// изменения, сделанные в составе заказа.
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void OpenOrder_Click(object sender, RoutedEventArgs e)
        {
            int orderId = GetSelectedOrderId();
            if (orderId == 0)
            {
                MessageBox.Show("Выберите заказ в списке.",
                    "Заказ не выбран", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Manager.MainFrame.Navigate(new OrderDetailsPage(orderId));
        }

        private void OrdersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenOrder_Click(sender, e);
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.CurrentUser == null || Manager.CurrentUser.ID_Role != 1)
            {
                MessageBox.Show("Удалять заказы может только администратор.",
                    "Недостаточно прав", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int orderId = GetSelectedOrderId();
            if (orderId == 0)
            {
                MessageBox.Show("Выберите заказ, который нужно удалить.",
                    "Заказ не выбран", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Вы действительно хотите удалить заказ №" + orderId + "?\n" +
                                          "Товары из заказа будут возвращены на склад.",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            var context = MiracleBootsEntities.GetContext();

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var order = context.Orders.FirstOrDefault(o => o.ID_Order == orderId);
                    if (order == null)
                    {
                        MessageBox.Show("Заказ не найден.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var items = context.OrderProducts
                        .Where(op => op.ID_Order == orderId)
                        .ToList();

                    // Возвращаем количество товара на склад перед удалением заказа.
                    foreach (var item in items)
                    {
                        var stock = context.StockItems
                            .FirstOrDefault(si => si.ID_StockItem == item.ID_StockItem);
                        if (stock != null)
                            stock.ItemsQuantity += item.Quantity;
                    }

                    context.OrderProducts.RemoveRange(items);
                    context.Orders.Remove(order);
                    context.SaveChanges();
                    transaction.Commit();

                    MessageBox.Show("Заказ удалён, товары возвращены на склад.",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Не удалось удалить заказ: " + ex.Message,
                        "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
