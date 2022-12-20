using System.Security.Cryptography;
using System.Text;

namespace JWT_API.Helpers
{
    public class HashUtil
    {
        public static string GetStringHash(string text,string salt) {

            var _salt = Encoding.UTF8.GetBytes(salt);
            var _text = Encoding.UTF8.GetBytes(text);

            var hmacSHA256 = new HMACSHA256(_salt);
            var saltedHash = hmacSHA256.ComputeHash(_text);

            return Convert.ToBase64String(saltedHash);
        }
    }
}
