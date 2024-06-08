using System;
using System.Data.SqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace MOVIE_TICKET_BOOKING
{
    /// <summary>
    /// Interaction logic for seller_signup.xaml
    /// </summary>
    public partial class seller_signup : Window
    {
        private string connectionString = "Data Source=DESKTOP-34IHBTL;Initial Catalog=MovieTicketSystem;Integrated Security=True";

        public seller_signup()
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
            seller_Login loginWindow = new seller_Login();
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
            string cnic = tbCnic.Text;

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

            if (string.IsNullOrWhiteSpace(cnic) || !Regex.IsMatch(cnic, @"^\d{5}-\d{7}-\d$"))
            {
                MessageBox.Show("Please enter a valid CNIC.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbCnic.Focus();
                return false;
            }

            if (!cbSellerPolicy.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show("Please agree to the terms and conditions.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                cbSellerPolicy.Focus();
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
            string cnic = tbCnic.Text;
            string companyName = tbCompanyName.Text;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Check if the username already exists for the seller role
                    SqlCommand checkUser = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username AND Role = 'Seller'", conn);
                    checkUser.Parameters.AddWithValue("@Username", username);

                    int userExists = (int)checkUser.ExecuteScalar();
                    if (userExists > 0)
                    {
                        MessageBox.Show("An account with this username already exists as a seller.", "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        tbName.Focus();
                        return;
                    }

                    // Check if the email, phone number, or CNIC already exists
                    checkUser = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email OR PhoneNumber = @PhoneNumber", conn);
                    checkUser.Parameters.AddWithValue("@Email", email);
                    checkUser.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                    userExists = (int)checkUser.ExecuteScalar();
                    if (userExists > 0)
                    {
                        MessageBox.Show("An account with this email or phone number already exists.", "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    checkUser = new SqlCommand("SELECT COUNT(*) FROM Sellers WHERE CNIC = @CNIC", conn);
                    checkUser.Parameters.AddWithValue("@CNIC", cnic);

                    userExists = (int)checkUser.ExecuteScalar();
                    if (userExists > 0)
                    {
                        MessageBox.Show("An account with this CNIC already exists.", "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        tbCnic.Focus();
                        return;
                    }

                    string hashedPassword = Encrypton.HashString(password);

                    SqlCommand insertUser = new SqlCommand("INSERT INTO Users (Username, Password, Email, PhoneNumber, Role) OUTPUT INSERTED.UserID VALUES (@Username, @Password, @Email, @PhoneNumber, 'Seller')", conn);
                    insertUser.Parameters.AddWithValue("@Username", username);
                    insertUser.Parameters.AddWithValue("@Password", hashedPassword);
                    insertUser.Parameters.AddWithValue("@Email", email);
                    insertUser.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                    int userId = (int)insertUser.ExecuteScalar();

                    SqlCommand insertSeller = new SqlCommand("INSERT INTO Sellers (UserID, Name, CompanyName, CNIC) VALUES (@UserID, @Name, @CompanyName, @CNIC)", conn);
                    insertSeller.Parameters.AddWithValue("@UserID", userId);
                    insertSeller.Parameters.AddWithValue("@Name", username);
                    insertSeller.Parameters.AddWithValue("@CompanyName", companyName);
                    insertSeller.Parameters.AddWithValue("@CNIC", cnic);

                    insertSeller.ExecuteNonQuery();

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
            tbCnic.Clear();
            tbCompanyName.Clear();
            cbSellerPolicy.IsChecked = false;
        }
    }
}