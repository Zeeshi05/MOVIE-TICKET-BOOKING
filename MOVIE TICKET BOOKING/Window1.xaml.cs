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
using System.Windows.Shapes;

namespace MOVIE_TICKET_BOOKING
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void Seller_Click(object sender, RoutedEventArgs e)
        {
            seller_Login sellerLoginSignup = new seller_Login();
            sellerLoginSignup.Show();
            this.Close();
        }

        private void Buyer_Click(object sender, RoutedEventArgs e)
        {
            buyer_login buyerLoginSignup = new buyer_login();
            buyerLoginSignup.Show();
            this.Close();
        }
    }
}
