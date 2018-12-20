using System.Drawing;


namespace Ginger.Utils
{
    public static class BitmapManager
    {
        public static void SaveBitmapToPng(Bitmap bmp, string fileName)
        {                        
            bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);            
        }
    }
}
