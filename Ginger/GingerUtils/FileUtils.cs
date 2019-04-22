using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GingerUtils
{
    public class FileUtils
    {
        public static string RemoveInvalidChars(string fileName)
        {            
            string invalidChars = new string(Path.GetInvalidFileNameChars());

            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c.ToString(), "_");
            }

            return fileName;
        }
    }
}
