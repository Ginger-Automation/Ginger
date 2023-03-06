#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
