using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOVIE_TICKET_BOOKING
{
    public class MovieTicket
    {
        public string CompanyName { get; set; }
        public string MovieName { get; set; }
        public string CinemaName { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int TotalTicketsAvailable { get; set; }
        public string SellerName { get; set; }
    }

}
