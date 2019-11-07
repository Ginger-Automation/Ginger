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
            return MD5Utility.GetFileMD5(filePath).ToString();

        }
    }
}
