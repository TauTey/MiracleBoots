using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MiracleBoots
{
    public partial class ProductPage : Page
    {
        private List<Products> _allProducts = new List<Products>();

        public ProductPage()
        {
            InitializeComponent();

            if (Manager.CurrentUser != null)
            {
                // ФИО пользователя уже выводится в правом верхнем углу главного окна.
                FIOTB.Visibility = Visibility.Collapsed;
            }
            else
            {
                FIOTB.Text = "Вы вошли как: Гость";
            }

            try
            {
                var categories = MiracleBootsEntities.GetContext().Categories
                    .OrderBy(c => c.CategoryName)
                    .ToList();
                categories.Insert(0, new Categories { ID_Category = 0, CategoryName = "Все категории" });

                ComboCategory.ItemsSource = categories;
                ComboCategory.DisplayMemberPath = "CategoryName";
                ComboCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить список категорий: " + ex.Message,
                    "Ошибка загрузки данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ComboSort.SelectedIndex = 0;
        }

        private void LoadData()
        {
            try
            {
                Products.RefreshDiscountCache();

                var context = MiracleBootsEntities.GetContext();
                _allProducts = context.Products
                    .Include("StockItems.Sizes")
                    .Include("Manufactures")
                    .Include("Categories")
                    .ToList();

                UpdateProduct();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить каталог товаров: " + ex.Message,
                    "Ошибка загрузки данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProduct()
        {
            IEnumerable<Products> query = _allProducts;

            var category = ComboCategory.SelectedItem as Categories;
            if (category != null && category.ID_Category != 0)
                query = query.Where(p => p.ID_Category == category.ID_Category);

            string search = TBoxSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    (!string.IsNullOrWhiteSpace(p.ProductName) &&
                     p.ProductName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(p.ProductDescription) &&
                     p.ProductDescription.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (ComboSort.SelectedIndex == 1)
                query = query.OrderBy(p => p.DiscountedPrice);
            else if (ComboSort.SelectedIndex == 2)
                query = query.OrderByDescending(p => p.DiscountedPrice);

            var result = query.ToList();
            ProductsListView.ItemsSource = result;
            TBCount.Text = "Найдено моделей: " + result.Count;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
                UpdateProduct();
        }

        private void ComboCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                UpdateProduct();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                UpdateProduct();
        }

        private void ProductsListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(ProductsListView, e.OriginalSource as DependencyObject)
                as ListViewItem;

            if (item == null)
                return;

            var product = item.DataContext as Products;
            if (product == null)
                return;

            OpenProduct(product);
        }

        private void OpenProduct(Products product)
        {
            bool hasAvailableSizes = product.StockItems != null &&
                                     product.StockItems.Any(si => si.ItemsQuantity > 0);

            if (!hasAvailableSizes)
            {
                MessageBox.Show("Данного товара нет в наличии. Выберите другую модель.",
                    "Товар отсутствует", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Manager.MainFrame.Navigate(new ProductViewPage(product));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
