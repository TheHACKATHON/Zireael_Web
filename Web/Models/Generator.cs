using System;
using System.Security.Cryptography;
using System.Text;

namespace ASP_Articles.Models
{
    public class Generator
    {
        private static string BytesToHexString(byte[] bytes)
        {
            var hexString = new StringBuilder(64);

            foreach (var b in bytes)
            {
                hexString.Append($"{b:X2}");
            }

            return hexString.ToString();
        }

        public static string String(int size)
        {
            using (var generator = RandomNumberGenerator.Create())
            {
                size = (int) Math.Ceiling(size / 2.0);
                var salt = new byte[size];
                generator.GetBytes(salt);
                return BytesToHexString(salt);
            }
        }
    }
}