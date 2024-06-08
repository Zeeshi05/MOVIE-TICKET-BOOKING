using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace MOVIE_TICKET_BOOKING
{
    public partial class buyer_dashboard : Window
    {
        private string connectionString = "Data Source=DESKTOP-34IHBTL;Initial Catalog=MovieTicketSystem;Integrated Security=True";

        public buyer_dashboard()
        {
            InitializeComponent();
            LoadMovieTickets();
            UpdateBuyerName();  // Ensure this is called after the UI is initialized
        }

        private void UpdateBuyerName()
        {
            if (!string.IsNullOrEmpty(SessionManager.CurrentUsername))
            {
                txtBuyerName.Text = SessionManager.CurrentUsername;
            }
            else
            {
                txtBuyerName.Text = "No Buyer Logged In";  // Default text if no user is logged in
            }
        }

        private void LoadMovieTickets()
        {
            DataTable movieTickets = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT TicketID, MovieName, CinemaName, Date, Price, RemainingTickets FROM Tickets";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(movieTickets);
            }
            dataGridMovies.ItemsSource = movieTickets.DefaultView;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = tbSearch.Text;
            DataTable searchResults = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT TicketID, MovieName, CinemaName, Date, Price, RemainingTickets FROM Tickets WHERE MovieName LIKE @SearchTerm";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(searchResults);
            }
            dataGridMovies.ItemsSource = searchResults.DefaultView;
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Assume buyer_login is the login window class name
            buyer_login loginWindow = new buyer_login();
            loginWindow.Show();
            this.Close();
        }

        private void btnBuyNow_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridMovies.SelectedItem != null)
            {
                DataRowView selectedRow = dataGridMovies.SelectedItem as DataRowView;
                int ticketId = Convert.ToInt32(selectedRow["TicketID"]);
                PurchaseTicket(ticketId);
            }
            else
            {
                MessageBox.Show("Please select a ticket to buy.", "Selection Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PurchaseTicket(int ticketId)
        {
            // Confirmation Dialog
            MessageBoxResult result = MessageBox.Show("Are you sure you want to buy this ticket?", "Confirm Purchase", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Start a transaction
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // Check if tickets are available
                        SqlCommand command = new SqlCommand("SELECT RemainingTickets, Price, SellerID FROM Tickets WHERE TicketID = @TicketID", connection, transaction);
                        command.Parameters.AddWithValue("@TicketID", ticketId);
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            int remainingTickets = reader.GetInt32(0);
                            decimal ticketPrice = reader.GetDecimal(1);
                            int sellerId = reader.GetInt32(2);
                            reader.Close();

                            if (remainingTickets > 0)
                            {
                                // Update the remaining tickets
                                command = new SqlCommand("UPDATE Tickets SET RemainingTickets = RemainingTickets - 1 WHERE TicketID = @TicketID", connection, transaction);
                                command.Parameters.AddWithValue("@TicketID", ticketId);
                                command.ExecuteNonQuery();

                                // Commit the transaction
                                transaction.Commit();

                                MessageBox.Show("Ticket purchased successfully!", "Purchase Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Refresh the movie tickets grid
                                LoadMovieTickets();

                                // Refresh the seller dashboard
                                // If a reference to seller dashboard exists, call its methods
                                // sellerDashboard.LoadTickets();
                                // sellerDashboard.CalculateTotalRevenue();
                            }
                            else
                            {
                                reader.Close();
                                MessageBox.Show("No tickets available.", "Purchase Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            reader.Close();
                            MessageBox.Show("Ticket not found.", "Purchase Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Roll back the transaction on error
                        transaction.Rollback();
                        MessageBox.Show("Error purchasing ticket: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

    }
}
