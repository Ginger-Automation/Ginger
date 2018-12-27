using System;
using System.Drawing;


namespace Ginger.Utils
{
    public static class BitmapManager
    {
        public static void SaveBitmapToPng(Bitmap bmp, string fileName)
        {                        
            bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);            
        }
        
        public static Bitmap FileToBitmapImage(String path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            Bitmap bmp = (Bitmap)Bitmap.FromFile(path);
            return (bmp);
        }
    }
}
