using System;
using System.Data;
using System.Data.SqlClient;

namespace MOVIE_TICKET_BOOKING
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Data Source=DESKTOP-34IHBTL;Initial Catalog=MovieTicketSystem;Integrated Security=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static decimal GetTotalRevenue(int sellerId)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT SUM((TotalTicketsAvailable - RemainingTickets) * Price) AS Revenue
                    FROM Tickets
                    WHERE SellerID = @SellerID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SellerID", sellerId);
                object result = command.ExecuteScalar();
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        public static int ExecuteScalar(string query, SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    object result = command.ExecuteScalar();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public static void AddTicket(int sellerId, string movieName, string cinemaName, int roomNo, DateTime date, decimal price, int totalTickets)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                // Check if the seller exists
                string checkSellerQuery = "SELECT COUNT(*) FROM Sellers WHERE UserID = @SellerID";
                using (SqlCommand checkSellerCommand = new SqlCommand(checkSellerQuery, connection))
                {
                    checkSellerCommand.Parameters.AddWithValue("@SellerID", sellerId);
                    int sellerExists = (int)checkSellerCommand.ExecuteScalar();

                    if (sellerExists == 0)
                    {
                        throw new Exception("SellerID does not exist in the Sellers table.");
                    }
                }

                string query = "INSERT INTO Tickets (SellerID, MovieName, CinemaName, RoomNumber, Date, Price, TotalTicketsAvailable, RemainingTickets) VALUES (@SellerID, @MovieName, @CinemaName, @RoomNo, @Date, @Price, @TotalTicketsAvailable, @RemainingTickets)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SellerID", sellerId);
                    command.Parameters.AddWithValue("@MovieName", movieName);
                    command.Parameters.AddWithValue("@CinemaName", cinemaName);
                    command.Parameters.AddWithValue("@RoomNo", roomNo);
                    command.Parameters.AddWithValue("@Date", date);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@TotalTicketsAvailable", totalTickets);
                    command.Parameters.AddWithValue("@RemainingTickets", totalTickets);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateTicket(int ticketID, string movieName, string cinemaName, int roomNo, DateTime date, decimal price, int totalTickets)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "UPDATE Tickets SET MovieName = @MovieName, CinemaName = @CinemaName, RoomNumber = @RoomNo, Date = @Date, Price = @Price, TotalTicketsAvailable = @TotalTicketsAvailable, RemainingTickets = @RemainingTickets WHERE TicketID = @TicketID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TicketID", ticketID);
                    command.Parameters.AddWithValue("@MovieName", movieName);
                    command.Parameters.AddWithValue("@CinemaName", cinemaName);
                    command.Parameters.AddWithValue("@RoomNo", roomNo);
                    command.Parameters.AddWithValue("@Date", date);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@TotalTicketsAvailable", totalTickets);
                    command.Parameters.AddWithValue("@RemainingTickets", totalTickets);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteTicket(int ticketID)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM Tickets WHERE TicketID = @TicketID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TicketID", ticketID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static DataRow GetTicketById(int ticketID)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Tickets WHERE TicketID = @TicketID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TicketID", ticketID);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
                    }
                }
            }
        }
    }
}
