using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace GingerUtils.Encryption
{
   public class MD5Utility
    {

        public static byte[] GetFileMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            
                    return md5.ComputeHash(stream);
            }
            
        }

        public static string GetFileMD5string(string filePath)
        {
            byte[] md5 = MD5Utility.GetFileMD5(filePath);
            return GetMD5string(md5);            
        }


        public static string GetMD5string(byte[] md5)
        {            
            StringBuilder hex = new StringBuilder(md5.Length * 2);
            foreach (byte b in md5)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}
