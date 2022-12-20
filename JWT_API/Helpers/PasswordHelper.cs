using System.Security.Cryptography;
using System.Text;

namespace JWT_API.Helpers
{
    public class PasswordHelper
    {
        public static string GetStringHash(string password,string salt) {

            var _salt = Encoding.UTF8.GetBytes(salt);
            var _password = Encoding.UTF8.GetBytes(password);

            var hmacSHA256 = new HMACSHA256(_salt);
            var saltedHash = hmacSHA256.ComputeHash(_password);

            return Convert.ToBase64String(saltedHash);
        }
    }
}
