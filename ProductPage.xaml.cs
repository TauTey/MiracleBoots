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
            var currentProducts = MiracleBootsEntities.GetContext().Products.ToList();
            CurrentPageList = MiracleBootsEntities.GetContext().Products.ToList();

            var cat = ComboCategory.SelectedItem as Categories;
            if (cat != null && cat.ID_Category != 0)
            {
                currentProducts = currentProducts
                    .Where(p => p.ID_Category == cat.ID_Category).ToList();
            }

            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
            {
                currentProducts = currentProducts
                    .Where(p => p.ProductName.ToLower()
                        .Contains(TBoxSearch.Text.ToLower())).ToList();
            }

            if (RButtonUp.IsChecked == true)
                currentProducts = currentProducts.OrderBy(p => p.ProductCost).ToList();
            if (RButtonDown.IsChecked == true)
                currentProducts = currentProducts.OrderByDescending(p => p.ProductCost).ToList();

            ProductListView.ItemsSource = currentProducts;

            TableList = currentProducts;
            TBCount.Text = "Найдено: " + TableList.Count;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProduct();
        }

        private void ComboCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }
    }
}
