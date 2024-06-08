using System;
using System.Data.SqlClient;
using System.Windows;

namespace MOVIE_TICKET_BOOKING
{
    public partial class seller_Login : Window
    {
        private string connectionString = "Data Source=DESKTOP-34IHBTL;Initial Catalog=MovieTicketSystem;Integrated Security=True";

        public seller_Login()
        {
            InitializeComponent();
        }

        public void ClearForm()
        {
            tbUsername.Clear();
            tbPassword.Clear();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text;
            string password = tbPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Query to retrieve user information based on username and role
                    string query = "SELECT UserID, Password, Username FROM Users WHERE Username = @Username AND Role = 'Seller'";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", username);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string hashedPasswordFromDatabase = reader["Password"] != DBNull.Value ? reader["Password"].ToString() : string.Empty;
                        int sellerID = reader["UserID"] != DBNull.Value ? Convert.ToInt32(reader["UserID"]) : 0;
                        string sellerName = reader["Username"] != DBNull.Value ? reader["Username"].ToString() : string.Empty;

                        // Hash the password entered by the user
                        string hashedPasswordEntered = Encrypton.HashString(password);

                        // Compare the hashed passwords
                        if (hashedPasswordEntered == hashedPasswordFromDatabase)
                        {
                            // Successful login
                            MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearForm();

                            // Navigate to Seller Dashboard
                            Seller_Dashboard seller_Dashboard = new Seller_Dashboard(sellerID, sellerName);
                            seller_Dashboard.Show();
                            this.Close();
                        }
                        else
                        {
                            // Failed login
                            MessageBox.Show("Invalid username or password. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        // User not found
                        MessageBox.Show("User with the provided username does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSignup_Click(object sender, RoutedEventArgs e)
        {
            seller_signup signupWindow = new seller_signup();
            signupWindow.Show();
            this.Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Window1 mainWindow = new Window1();
            mainWindow.Show();
            this.Close();
        }
    }
}
