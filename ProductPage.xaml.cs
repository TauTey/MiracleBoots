using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MiracleBoots
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        private int _page = 1;
        private int _pageSize = 10;

        List<Products> CurrentPageList = new List<Products>();
        List<Products> TableList;
        public ProductPage()
        {
            InitializeComponent();

            if (Manager.CurrentUser != null)
            {
                FIOTB.Text = "Вы вошли как: " + Manager.CurrentUser.UserSurname + " " +
                             Manager.CurrentUser.UserName + " " +
                             Manager.CurrentUser.UserPatronymic;
            }
            else
            {
                FIOTB.Text = "Вы вошли как: Гость";
            }

            var cats = MiracleBootsEntities.GetContext().Categories.ToList();
            cats.Insert(0, new Categories { ID_Category = 0, CategoryName = "Все категории" });
            ComboCategory.ItemsSource = cats;
            ComboCategory.DisplayMemberPath = "CategoryName";
            ComboCategory.SelectedIndex = 0;

            var currentProducts = MiracleBootsEntities.GetContext().Products.ToList();
            ProductListView.ItemsSource = currentProducts;

            UpdateProduct();
        }
        private void UpdateProduct()
        {
            var context = MiracleBootsEntities.GetContext();
            var currentProducts = context.Products.Include("StockItems.Sizes").ToList();
            var cat = ComboCategory.SelectedItem as Categories;
            if (cat != null && cat.ID_Category != 0)
            {
                currentProducts = currentProducts
                    .Where(p => p.ID_Category == cat.ID_Category).ToList();
            }
            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
            {
                string search = TBoxSearch.Text.ToLower();
                currentProducts = currentProducts.Where(p => (p.ProductName != null && p.ProductName.ToLower().Contains(search)) || (p.ProductDescription != null && p.ProductDescription.ToLower().Contains(search)) || (p.Manufactures != null && p.Manufactures.ManufactureName.ToLower().Contains(search))).ToList();
            }
            if (RButtonUp.IsChecked == true)
                currentProducts = currentProducts.OrderBy(p => p.ProductCost).ToList();
            else if (RButtonDown.IsChecked == true)
                currentProducts = currentProducts.OrderByDescending(p => p.ProductCost).ToList();
            int totalItems = currentProducts.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / _pageSize);
            if (_page > totalPages && totalPages > 0) _page = totalPages;
            if (_page < 1) _page = 1;
            var pagedProducts = currentProducts.Skip((_page - 1) * _pageSize).Take(_pageSize).ToList();
            ProductListView.ItemsSource = pagedProducts;

            TBCount.Text = "Найдено: " + totalItems;
            TBPage.Text = $"Страница {_page} из {Math.Max(1, totalPages)}";
        }
        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _page = 1;
            UpdateProduct();
        }
        private void ComboCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _page = 1;
            UpdateProduct();
        }
        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded) UpdateProduct();
        }
        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded) UpdateProduct();
        }
        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            _page++;
            UpdateProduct();
        }
        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_page > 1)
            {
                _page--;
                UpdateProduct();
            }
        }
    }
}
