using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOVIE_TICKET_BOOKING
{
    public static class SessionManager
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUsername { get; set; }

        public static void Login(int userId, string username)
        {
            CurrentUserId = userId;
            CurrentUsername = username;
        }

        public static void Logout()
        {
            CurrentUserId = 0;
            CurrentUsername = string.Empty;
        }
    }



}
