using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AdditionsLibrary
{
    public static class HashCode
    {
        public static string GetMD5(string data)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(data);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        public static string ComputeFromBytes(params byte[] data)
        {
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                return Convert.ToBase64String(sha1.ComputeHash(data));
            }
        }

        public static string ComputeFromFile(string path) => ComputeFromBytes(GetBytesFromFile(path));

        public static byte[] GetBytesFromFile(string filePath)
        {
            try
            {
                using (var oFs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var oBr = new BinaryReader(oFs))
                    {
                        return oBr.ReadBytes((int)oFs.Length);
                    }
                }
            }
            catch
            {
                return new byte[] { 0 };
            }
        }
    }
}