using System;
using System.Data.SqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace MOVIE_TICKET_BOOKING
{
    /// <summary>
    /// Interaction logic for buyer_signup.xaml
    /// </summary>
    public partial class buyer_signup : Window
    {
        private string connectionString = "Data Source=DESKTOP-34IHBTL;Initial Catalog=MovieTicketSystem;Integrated Security=True";

        public buyer_signup()
        {
            InitializeComponent();
        }

        private void btnSignup_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSignup())
            {
                ExecuteSignup();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            buyer_login loginWindow = new buyer_login();
            loginWindow.Show();
            this.Close();
        }

        private bool ValidateSignup()
        {
            string username = tbName.Text;
            string email = tbEmail.Text;
            string phoneNumber = tbPhoneNumber.Text;
            string password = tbSignupPassword.Password;
            string confirmPassword = tbConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter your name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbEmail.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\d{10,15}$"))
            {
                MessageBox.Show("Please enter a valid phone number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbPhoneNumber.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbSignupPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Please confirm your password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbConfirmPassword.Focus();
                return false;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbConfirmPassword.Focus();
                return false;
            }

            if (!cbBuyerPolicy.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show("Please agree to the terms and conditions.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                cbBuyerPolicy.Focus();
                return false;
            }

            return true;
        }

        private void ExecuteSignup()
        {
            string username = tbName.Text;
            string email = tbEmail.Text;
            string phoneNumber = tbPhoneNumber.Text;
            string password = tbSignupPassword.Password;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand checkUser = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email OR PhoneNumber = @PhoneNumber", conn);
                    checkUser.Parameters.AddWithValue("@Username", username);
                    checkUser.Parameters.AddWithValue("@Email", email);
                    checkUser.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                    int userExists = (int)checkUser.ExecuteScalar();
                    if (userExists > 0)
                    {
                        MessageBox.Show("An account with this username, email, or phone number already exists.", "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string hashedPassword = Encrypton.HashString(password);

                    SqlCommand insertUser = new SqlCommand("INSERT INTO Users (Username, Password, Email, PhoneNumber, Role) OUTPUT INSERTED.UserID VALUES (@Username, @Password, @Email, @PhoneNumber, 'Buyer')", conn);
                    insertUser.Parameters.AddWithValue("@Username", username);
                    insertUser.Parameters.AddWithValue("@Password", hashedPassword);
                    insertUser.Parameters.AddWithValue("@Email", email);
                    insertUser.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                    int userId = (int)insertUser.ExecuteScalar();

                    MessageBox.Show("Signup successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFields()
        {
            tbName.Clear();
            tbEmail.Clear();
            tbPhoneNumber.Clear();
            tbSignupPassword.Clear();
            tbConfirmPassword.Clear();
            cbBuyerPolicy.IsChecked = false;
        }
    }
}
