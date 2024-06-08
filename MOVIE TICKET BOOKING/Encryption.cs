using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MOVIE_TICKET_BOOKING
{
    public class Encrypton
    {
        public static string HashString(string passwordString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(passwordString))
                sb.Append(b.ToString("X3"));
            return sb.ToString();
        }
        public static byte[] GetHash(string passwordString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(passwordString));
        }
    }
}
