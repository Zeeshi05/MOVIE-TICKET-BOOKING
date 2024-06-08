using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace MOVIE_TICKET_BOOKING
{
    public partial class Seller_Dashboard : Window
    {
        private int _sellerID;
        private string _sellerName;

        public Seller_Dashboard(int sellerID, string sellerName)
        {
            InitializeComponent();
            _sellerID = sellerID;
            _sellerName = sellerName;
            txtSellerName.Text = sellerName;
            LoadSellerData();
        }

        private void LoadSellerData()
        {
            CalculateTotalRevenue();
            LoadTickets();
        }

        public void CalculateTotalRevenue()
        {
            decimal totalRevenue = DatabaseHelper.GetTotalRevenue(_sellerID);
            txtTotalRevenue.Text = totalRevenue.ToString("C");
        }

        public void LoadTickets()
        {
            string query = @"
                SELECT TicketID, MovieName, CinemaName, RoomNumber, Date, Price, TotalTicketsAvailable, 
                RemainingTickets
                FROM Tickets WHERE SellerID = @SellerID";
            SqlParameter[] parameters = { new SqlParameter("@SellerID", _sellerID) };
            DataTable tickets = DatabaseHelper.ExecuteQuery(query, parameters);
            dataGridTickets.ItemsSource = tickets.DefaultView;
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            seller_Login loginWindow = new seller_Login();
            loginWindow.Show();
            this.Close();
        }

        private void btnAddTicket_Click(object sender, RoutedEventArgs e)
        {
            string movieName = tbMovieName.Text;
            string cinemaName = tbCinemaName.Text;

            if (string.IsNullOrWhiteSpace(movieName) || string.IsNullOrWhiteSpace(cinemaName) ||
                string.IsNullOrWhiteSpace(tbRoomNo.Text) || string.IsNullOrWhiteSpace(tbPrice.Text) ||
                string.IsNullOrWhiteSpace(tbTotalTickets.Text) || !dpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please fill in all fields.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(tbRoomNo.Text, out int roomNo))
            {
                MessageBox.Show("Please enter a valid Room Number.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(tbPrice.Text, out decimal price))
            {
                MessageBox.Show("Please enter a valid Price.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(tbTotalTickets.Text, out int totalTickets))
            {
                MessageBox.Show("Please enter a valid Total Tickets.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime date = dpDate.SelectedDate.Value;

            // Validate date is today or in the future
            if (date < DateTime.Today)
            {
                MessageBox.Show("Please select a date that is today or in the future.", "Date Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if the cinema and room are available on the given date
            if (IsCinemaRoomBooked(cinemaName, roomNo, date))
            {
                MessageBox.Show("The selected cinema and room are already booked for another movie on the selected date.", "Booking Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DatabaseHelper.AddTicket(_sellerID, movieName, cinemaName, roomNo, date, price, totalTickets);
            LoadTickets();
            ClearFields();
        }

        private bool IsCinemaRoomBooked(string cinemaName, int roomNo, DateTime date)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM Tickets 
                WHERE CinemaName = @CinemaName AND RoomNumber = @RoomNo AND Date = @Date";
            SqlParameter[] parameters = {
                new SqlParameter("@CinemaName", cinemaName),
                new SqlParameter("@RoomNo", roomNo),
                new SqlParameter("@Date", date)
            };
            return DatabaseHelper.ExecuteScalar(query, parameters) > 0;
        }

        private void btnUpdateTicket_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridTickets.SelectedItem is DataRowView selectedRow)
            {
                int ticketID = Convert.ToInt32(selectedRow["TicketID"]);
                DataRow currentTicket = DatabaseHelper.GetTicketById(ticketID);

                if (currentTicket == null)
                {
                    MessageBox.Show("Selected ticket not found in the database.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string movieName = string.IsNullOrWhiteSpace(tbMovieName.Text) ? currentTicket["MovieName"].ToString() : tbMovieName.Text;
                string cinemaName = string.IsNullOrWhiteSpace(tbCinemaName.Text) ? currentTicket["CinemaName"].ToString() : tbCinemaName.Text;
                int roomNo = string.IsNullOrWhiteSpace(tbRoomNo.Text) ? Convert.ToInt32(currentTicket["RoomNumber"]) : Convert.ToInt32(tbRoomNo.Text);
                DateTime date = !dpDate.SelectedDate.HasValue ? Convert.ToDateTime(currentTicket["Date"]) : dpDate.SelectedDate.Value;
                decimal price = string.IsNullOrWhiteSpace(tbPrice.Text) ? Convert.ToDecimal(currentTicket["Price"]) : Convert.ToDecimal(tbPrice.Text);
                int totalTickets = string.IsNullOrWhiteSpace(tbTotalTickets.Text) ? Convert.ToInt32(currentTicket["TotalTicketsAvailable"]) : Convert.ToInt32(tbTotalTickets.Text);

                // Validate date is today or in the future
                if (date < DateTime.Today)
                {
                    MessageBox.Show("Please select a date that is today or in the future.", "Date Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Check if the cinema and room are available on the given date
                if (IsCinemaRoomBookedForUpdate(cinemaName, roomNo, date, ticketID))
                {
                    MessageBox.Show("The selected cinema and room are already booked for another movie on the selected date.", "Booking Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                DatabaseHelper.UpdateTicket(ticketID, movieName, cinemaName, roomNo, date, price, totalTickets);
                LoadTickets();
                ClearFields();
            }
            else
            {
                MessageBox.Show("Please select a ticket to update.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsCinemaRoomBookedForUpdate(string cinemaName, int roomNo, DateTime date, int ticketID)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM Tickets 
                WHERE CinemaName = @CinemaName AND RoomNumber = @RoomNo AND Date = @Date AND TicketID <> @TicketID";
            SqlParameter[] parameters = {
                new SqlParameter("@CinemaName", cinemaName),
                new SqlParameter("@RoomNo", roomNo),
                new SqlParameter("@Date", date),
                new SqlParameter("@TicketID", ticketID)
            };
            return DatabaseHelper.ExecuteScalar(query, parameters) > 0;
        }

        private void btnDeleteTicket_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridTickets.SelectedItem is DataRowView selectedRow)
            {
                int ticketID = Convert.ToInt32(selectedRow["TicketID"]);
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this ticket?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    DatabaseHelper.DeleteTicket(ticketID);
                    LoadTickets();
                }
            }
            else
            {
                MessageBox.Show("Please select a ticket to delete.", "Deletion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClearFields_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            tbMovieName.Clear();
            tbCinemaName.Clear();
            tbRoomNo.Clear();
            dpDate.SelectedDate = null;
            tbPrice.Clear();
            tbTotalTickets.Clear();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = tbSearch.Text.Trim();

            if (string.IsNullOrEmpty(searchQuery))
            {
                LoadTickets();
            }
            else
            {
                string query = @"
                    SELECT TicketID, MovieName, CinemaName, RoomNumber, Date, Price, TotalTicketsAvailable, 
                    RemainingTickets
                    FROM Tickets 
                    WHERE SellerID = @SellerID AND MovieName LIKE @MovieName";
                SqlParameter[] parameters = {
                    new SqlParameter("@SellerID", _sellerID),
                    new SqlParameter("@MovieName", "%" + searchQuery + "%")
                };
                DataTable tickets = DatabaseHelper.ExecuteQuery(query, parameters);
                dataGridTickets.ItemsSource = tickets.DefaultView;
            }
        }
    }
}
