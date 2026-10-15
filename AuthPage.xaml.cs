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
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            var user = MiracleBootsEntities.GetContext().Users
                .FirstOrDefault(u => u.UserLogin == LoginBox.Text.Trim());

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден");
                return;
            }

            Manager.CurrentUser = user;
            Manager.MainFrame.Navigate(new ProductPage());
        }

        private void GuestBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.CurrentUser = null;
            Manager.MainFrame.Navigate(new ProductPage());
        }
    }
}
