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
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));

            return Convert.ToBase64String(hash);
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