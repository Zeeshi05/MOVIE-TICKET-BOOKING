using System;
using System.Data.SqlClient;
using System.Windows;

namespace MOVIE_TICKET_BOOKING
{
    public partial class buyer_login : Window
    {
        private string connectionString = "Data Source=DESKTOP-34IHBTL;Initial Catalog=MovieTicketSystem;Integrated Security=True";

        public buyer_login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateLogin())
            {
                ExecuteLogin();
            }
        }

        private void btnSignup_Click(object sender, RoutedEventArgs e)
        {
            buyer_signup signupWindow = new buyer_signup();
            signupWindow.Show();
            this.Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Window1 mainWindow = new Window1();
            mainWindow.Show();
            this.Close();
        }

        private bool ValidateLogin()
        {
            string username = tbUsername.Text;
            string password = tbPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void ExecuteLogin()
        {
            string username = tbUsername.Text;
            string password = tbPassword.Password;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT UserID, Password, Username FROM Users WHERE Username = @Username AND Role = 'Buyer'", conn);
                    cmd.Parameters.AddWithValue("@Username", username);

                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        var dbPassword = reader["Password"].ToString();
                        var userId = (int)reader["UserID"];
                        var userName = reader["Username"].ToString();
                        if (Encrypton.HashString(password) == dbPassword)
                        {
                            MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            SessionManager.Login(userId, userName);  // Set session before opening the dashboard
                            buyer_dashboard buyerDashboard = new buyer_dashboard();
                            buyerDashboard.Show();
                            this.Close();
                        }

                        else
                        {
                            MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            tbUsername.Focus();
                        }
                    }
                    else
                    {
                        MessageBox.Show("User not found.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to database: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ClearFields()
        {
            tbUsername.Clear();
            tbPassword.Clear();
        }
    }
}
