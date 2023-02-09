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

using GingerTestHelper;
using ImageMagick;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GingerTest
{
    public class VisualCompare
    {
        private void TakeVisualScreenShot(Visual visual, string FileName)
        {
            int width=0;
            int height=0;
            if (visual is Window)
            {                
                width = (int)((Window)visual).Width;                
                height = (int)((Window)visual).Height;

                System.Windows.Point p = Mouse.GetPosition((Window)visual);
                //((Window)visual).IsMouseOver
                ((Window)visual).ReleaseMouseCapture();
                //Mouse.set

            }
            else if (visual is Page)
            {
                width = (int)((Page)visual).ActualWidth;
                height = (int)((Page)visual).ActualHeight;
            }
            // TODO: handle other types or throw 

            // Remove the mouse so it will not impact
            

            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using (Stream stream = File.Create(FileName)) encoder.Save(stream);
        }
       

        //We do visual compare using ImageMagick
        public bool IsVisualEquel(Visual visual, string VisualID)
        {
            string tempScreenFolder = TestResources.GetTestTempFolder("VisualCompareScreens");
            if (!System.IO.Directory.Exists(tempScreenFolder))
            {
                System.IO.Directory.CreateDirectory(tempScreenFolder);
            }
            string FileName = Path.Combine(tempScreenFolder, VisualID + ".png");
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
            TakeVisualScreenShot(visual, FileName);
            string BaselineFileName = TestResources.GetTestResourcesFile(@"VisualCompareScreens\" + VisualID + ".png");
            string ResultFileName = Path.Combine(tempScreenFolder, VisualID + "_Diff.png");
            if (File.Exists(ResultFileName))
            {
                File.Delete(ResultFileName);
            }
            // Copy also baseline to output temp folder
            if (System.IO.File.Exists(BaselineFileName))
            {
                string baseLinefileName = Path.Combine(tempScreenFolder, VisualID + ".Baseline.png");
                File.Copy(BaselineFileName, baseLinefileName, true);
                bool Diff = IsBitmapEquel(BaselineFileName, FileName, ResultFileName);
                return Diff;
            }
            else
            {
                throw new Exception("No baseline file for compare, missing: " + BaselineFileName);
            }
        }

        public static bool IsBitmapEquel(string BaseImageFileName, string TargetImageFileName, string ResultFileName)
        {
            MagickImage magickBaseImg = new MagickImage(BaseImageFileName);
            MagickImage magickTargetImg = new MagickImage(TargetImageFileName);

            var diffImg = new MagickImage();

            double percentageDifference;
            ErrorMetric EM = ErrorMetric.Fuzz;   // Fuzz !?

            percentageDifference = magickBaseImg.Compare(magickTargetImg, EM, diffImg, Channels.Red);
            percentageDifference = percentageDifference * 100;
            percentageDifference = Math.Round(percentageDifference, 2);

            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
            Bitmap ImgToSave = (Bitmap)tc.ConvertFrom(diffImg.ToByteArray());
            ImgToSave.Save(ResultFileName);

            if (percentageDifference == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
