using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MiracleBoots
{
    public partial class ProductViewPage : Page
    {
        private readonly Products _product;
        private readonly List<StockItems> _availableStockItems;

        public ProductViewPage(Products product)
        {
            InitializeComponent();
            _product = product;
            DataContext = product;

            _availableStockItems = product.StockItems
                .Where(si => si.ItemsQuantity > 0)
                .OrderBy(si => si.Sizes.Size)
                .ToList();

            // Полный размерный ряд модели с указанием остатка по каждому размеру.
            var sizeRange = product.StockItems
                .OrderBy(si => si.Sizes.Size)
                .Select(si => si.Sizes.Size.ToString("0.#") + " (" + si.ItemsQuantity + " шт.)");

            SizeRangeText.Text = "Размерный ряд: " + string.Join(", ", sizeRange);

            SizesListBox.ItemsSource = _availableStockItems
                .Select(si => new SizeOption
                {
                    StockItem = si,
                    DisplaySize = "Размер " + si.Sizes.Size.ToString("0.#") + " (остаток: " + si.ItemsQuantity + ")"
                })
                .ToList();

            if (SizesListBox.Items.Count > 0)
                SizesListBox.SelectedIndex = 0;
            else
                MessageBox.Show("Нет доступных размеров для заказа.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void SizesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var option = SizesListBox.SelectedItem as SizeOption;
            SelectedSizeInfoText.Text = option == null
                ? string.Empty
                : "Доступно к заказу: " + option.StockItem.ItemsQuantity + " пар.";
        }

        private void QuantityBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var option = SizesListBox.SelectedItem as SizeOption;
            if (option == null)
            {
                MessageBox.Show("Выберите размер обуви перед добавлением товара в заказ.",
                    "Размер не выбран", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество: целое число больше нуля.",
                    "Некорректное количество", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityBox.Focus();
                QuantityBox.SelectAll();
                return;
            }

            var stockItem = option.StockItem;
            if (quantity > stockItem.ItemsQuantity)
            {
                MessageBox.Show("На складе доступно только " + stockItem.ItemsQuantity +
                                " пар выбранного размера. Уменьшите количество.",
                    "Недостаточно товара", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityBox.Focus();
                QuantityBox.SelectAll();
                return;
            }

            var line = Manager.Cart.FirstOrDefault(c => c.ID_StockItem == stockItem.ID_StockItem);
            if (line != null)
            {
                if (line.Quantity + quantity > stockItem.ItemsQuantity)
                {
                    MessageBox.Show("С учётом уже добавленного в заказ количество превышает остаток на складе. " +
                                    "Доступно к добавлению: " + (stockItem.ItemsQuantity - line.Quantity) + " пар.",
                        "Недостаточно товара", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                line.Quantity += quantity;
            }
            else
            {
                Manager.Cart.Add(new CartItem
                {
                    ID_StockItem = stockItem.ID_StockItem,
                    ID_Product = _product.ID_Product,
                    ProductName = _product.ProductName,
                    Size = stockItem.Sizes.Size,
                    Quantity = quantity,
                    Price = _product.DiscountedPrice
                });
            }

            MessageBox.Show("Товар «" + _product.ProductName + "» (размер " + stockItem.Sizes.Size.ToString("0.#") +
                            ") добавлен в заказ.\nПозиций в заказе: " + Manager.Cart.Count,
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.MainFrame.CanGoBack)
                Manager.MainFrame.GoBack();
            else
                Manager.MainFrame.Navigate(new ProductPage());
        }
    }

    /// <summary>
    /// Вспомогательный класс для отображения размера и остатка в списке.
    /// </summary>
    public class SizeOption
    {
        public StockItems StockItem { get; set; }
        public string DisplaySize { get; set; }
    }
}
