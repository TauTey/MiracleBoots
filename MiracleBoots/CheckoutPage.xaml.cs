using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MiracleBoots
{
    public partial class CheckoutPage : Page
    {
        public CheckoutPage()
        {
            InitializeComponent();
            RefreshCart();

            if (Manager.CurrentUser == null)
            {
                MessageBox.Show("Для оформления заказа требуется авторизация.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                CheckoutBtn.IsEnabled = false;
            }
        }

        private void RefreshCart()
        {
            CartGrid.ItemsSource = null;
            CartGrid.ItemsSource = Manager.Cart;

            decimal total = Manager.Cart.Sum(c => c.Total);
            TotalText.Text = "Итого: " + total.ToString("N2") + " руб.";
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (CartGrid.SelectedItem is CartItem item)
            {
                Manager.Cart.Remove(item);
                RefreshCart();
            }
            else
            {
                MessageBox.Show("Выберите позицию, которую нужно удалить из заказа.",
                    "Позиция не выбрана", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.CurrentUser == null)
            {
                MessageBox.Show("Для оформления заказа требуется авторизация.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Manager.Cart.Count == 0)
            {
                MessageBox.Show("Заказ пуст. Добавьте товары из каталога перед оформлением.",
                    "Пустой заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Подтвердить оформление заказа на сумму " +
                                          Manager.Cart.Sum(c => c.Total).ToString("N2") + " руб.?",
                "Подтверждение заказа", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            var context = MiracleBootsEntities.GetContext();

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // Повторная проверка остатков перед сохранением, чтобы не продать отсутствующий товар.
                    foreach (var cartItem in Manager.Cart)
                    {
                        var stock = context.StockItems.FirstOrDefault(si => si.ID_StockItem == cartItem.ID_StockItem);
                        if (stock == null)
                            throw new InvalidOperationException("Товарная позиция не найдена на складе.");

                        if (cartItem.Quantity > stock.ItemsQuantity)
                        {
                            throw new InvalidOperationException(
                                "Товара «" + cartItem.ProductName + "» (размер " + cartItem.Size +
                                ") осталось только " + stock.ItemsQuantity + " пар. " +
                                "Измените количество в заказе.");
                        }
                    }

                    var order = new Orders
                    {
                        OrderDate = DateTime.Today,
                        ID_User = Manager.CurrentUser.ID_User,
                        TotalAmount = Manager.Cart.Sum(c => c.Total)
                    };

                    context.Orders.Add(order);
                    context.SaveChanges();

                    foreach (var cartItem in Manager.Cart)
                    {
                        context.OrderProducts.Add(new OrderProducts
                        {
                            ID_Order = order.ID_Order,
                            ID_StockItem = cartItem.ID_StockItem,
                            Quantity = cartItem.Quantity,
                            PriceAtOrderTime = cartItem.Price
                        });

                        var stock = context.StockItems.First(si => si.ID_StockItem == cartItem.ID_StockItem);
                        stock.ItemsQuantity -= cartItem.Quantity;
                    }

                    context.SaveChanges();
                    transaction.Commit();

                    Manager.Cart.Clear();
                    RefreshCart();
                    Products.RefreshDiscountCache();

                    MessageBox.Show("Заказ №" + order.ID_Order + " успешно оформлен.",
                        "Заказ оформлен", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Открываем каталог заново, чтобы сразу показать актуальные остатки после заказа.
                    Manager.MainFrame.Navigate(new ProductPage());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Не удалось оформить заказ: " + ex.Message,
                        "Ошибка оформления заказа", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
