#region License
/*
Copyright © 2014-2018 European Support Limited

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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GingerCore;
using GingerCore.GeneralFunctions;
using Ginger.Help;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Drawing;
using Amdocs.Ginger.Common;

namespace Ginger
{
    public static class General
    {
        static HelpWindow mGingerHelpWindow = null;

        public enum RepositoryItemPageViewMode
        {
            Automation = 0, //Item opened from Automate tab and saved item should be App.BusiessFlow
            Standalone = 1, //Allow edit with Save
            SharedReposiotry = 2,  //Item opened from Shared Repository in which the item iteself supposed to be saved to XML
            Child = 3, //Item opened for edit without save
            ChildWithSave = 4, //Item opened as standalone but in save allows to save it original parent
            View = 5 //item should be open for read only

        }

        public static bool isDesignMode()
        {
            //TODO: move this func to General
            bool designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
            return designMode;
        }       

        public static string ConvertSolutionRelativePath(string fileName)
        {
            string s = fileName;
            s= s.ToUpper().Replace(App.UserProfile.Solution.Folder.ToUpper(), @"~\");
            return s;            
        }

        public static string GetFullFilePath(string filename)
        {
            string s = filename;
            if (s.StartsWith(@"~\"))
            {
                s = s.Replace(@"~\", App.UserProfile.Solution.Folder);
            }
            return s;            
        }

        public static string OpenSelectFolderDialog(string Title)
        {
            // We use open file since it is a beter UI than folder selection, the user can type at the address and naviagte quicker - more user friendly
            var OFD = new System.Windows.Forms.OpenFileDialog();
            OFD.FileName = "Folder Selection";
            OFD.Title = Title;
            OFD.Filter = "Folder|Folder";
            OFD.ValidateNames = false;
            OFD.CheckFileExists = false;
            OFD.CheckPathExists = true;
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                string p = OFD.FileName;
                p = p.Replace(System.IO.Path.GetFileName(p), "");
                return p;                
            }
            return null;
        }

        public static void ShowGingerHelpWindow(string SearchString="")
        {
            if (mGingerHelpWindow == null)
                mGingerHelpWindow = new HelpWindow(SearchString);
            else
            {
                mGingerHelpWindow.Close();
                mGingerHelpWindow = new HelpWindow(SearchString);
            }

            mGingerHelpWindow.Show();            
        }

        internal static ImageSource ToBitmapSource(System.Drawing.Bitmap source)
        {
            if (source == null) return null;
            BitmapSource bitSrc = null;
            var hBitmap = source.GetHbitmap();
            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                //TODO:  NativeMethods.DeleteObject(hBitmap);

                //internal static class NativeMethods
                //{
                //[DllImport("gdi32.dll")]
                //[return: MarshalAs(UnmanagedType.Bool)]
                //internal static extern bool DeleteObject(IntPtr hObject);
                //}
            }

            return bitSrc;
        }

        public static string GetItemUniqueName(string item, System.Collections.Generic.List<string> itemList = null, string suffix = "_Copy")
        {
            string originalName = item;
            bool nameUnique = false;
            int counter = 0;
            while (nameUnique == false)
            {
                nameUnique = true;
                foreach (string t in itemList)
                    if (t == item)
                    {
                        nameUnique = false;
                        break;
                    }
                if (nameUnique)
                    break;
                else
                {
                    if (counter == 0)
                    {
                        item = originalName + suffix;
                        counter = 2;
                    }
                    else
                    {
                        item = originalName + suffix + counter;
                        counter++;
                    }
                }
            }
            return item;
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = System.IO.Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, true);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        //TODO: same function and code is copied in several placed - unite all to use the one below
        public static string GetStringBetween(string STR, string FirstString, string LastString = null)
        {
            string str = "";
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2;
            if (LastString != null)
            {
                Pos2 = STR.IndexOf(LastString, Pos1);
            }
            else
            {
                Pos2 = STR.Length;
            }

            if ((Pos2 - Pos1) > 0)
            {
                str = STR.Substring(Pos1, Pos2 - Pos1);
                return str;
            }
            else
            {
                return "";
            }
        }

        public static bool CompareStringsIgnoreCase(string strOne, string strTwo)
        {
            if (strOne == null || strTwo == null)
                return false;
            return strOne.Equals(strTwo, StringComparison.OrdinalIgnoreCase);
        }

        public static Viewbox makeImgFromControl(UIElement control, string count,int typ)
        {
            Viewbox viewbox;
            int xAxis = 0;
            Ellipse e = new Ellipse();
            if (count.Length == 1)
            {
                xAxis = 147;
            }
            else if (count.Length == 2)
            {
                xAxis = 143;
            }
            else if (count.Length == 3)
            {
                xAxis = 141;
            }
            else if (count.Length == 4)
            {
                xAxis = 138;
            }
            viewbox = new Viewbox();
            Grid grd = new Grid();
            var parent = VisualTreeHelper.GetParent(control) as StackPanel;
            if (parent != null)
            {
                parent.Children.Remove(control);
            }                    

            grd.Children.Add(control);
            grd.Children.Add(CreateAnEllipse());          
            TextBlock txt = new TextBlock { Margin = new Thickness(xAxis, 168, 0, 0)};
            if(typ==0)
            {
                App.RunsetBFTextbox = txt;
            }
           else if(typ==1)
            {
                App.RunsetActivityTextbox = txt;
            }
            else
            {
                App.RunsetActionTextbox = txt;
            }        
            grd.Children.Add(txt);        
            viewbox.Child = grd;
            viewbox.Measure(new System.Windows.Size(400, 200));
            viewbox.Arrange(new Rect(0, 0, 400, 200));
            viewbox.UpdateLayout();
            return viewbox;
        }

        public static Ellipse CreateAnEllipse()
        {
            // Create an Ellipse
            Ellipse blueRectangle = new Ellipse();
            blueRectangle.Height = 50;
            blueRectangle.Width = 50;

            // Create a blue and a black Brush
            SolidColorBrush blueBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#d2c5b8"));
            // Set Ellipse's width and color
            blueRectangle.StrokeThickness = 2;
            blueRectangle.Stroke = blueBrush;
            // Fill rectangle with blue color
            blueRectangle.Fill = blueBrush;
            blueRectangle.Margin = new Thickness(0, 35, 0, 0);
            return blueRectangle;
        }

        /// <summary>
        /// Return the Image from Ginger/Images
        /// </summary>
        /// <param name="resourceImageName"></param>
        /// Name of the image in the resource dictionary file
        /// <returns></returns>
        public static BitmapImage GetResourceImage(string resourceImageName)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + resourceImageName));
        }
        
        public static void DoEvents()
        {
            try
            {
                DispatcherFrame frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                    new DispatcherOperationCallback(ExitFrame), frame);
                Dispatcher.PushFrame(frame);
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.WARN, "Error Occurred while doing DoEvents()", ex);
            }
        }

        private static object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }



        public static SolidColorBrush GetStatusBrush(Amdocs.Ginger.CoreNET.Execution.eRunStatus status)
        {
            if (status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
            {
                return App.Current.TryFindResource("$PassedStatusColor") as SolidColorBrush;
            }
            if (status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
            {
                return App.Current.TryFindResource("$FailedStatusColor") as SolidColorBrush;
            }
            if (status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked)
            {
                return App.Current.TryFindResource("$BlockedStatusColor") as SolidColorBrush;
            }
            if (status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
            {
                return App.Current.TryFindResource("$StoppedStatusColor") as SolidColorBrush;
            }
            if (status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped)
            {
                return App.Current.TryFindResource("$SkippedStatusColor") as SolidColorBrush;
            }

            return System.Windows.Media.Brushes.Black;
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }


        /// <summary>
        /// Return image from Images folder
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static System.Windows.Controls.Image GetImage(string imageName, int width = -1, int height = -1)
        {
            //TODO: replace all places where we have pack://application:,,,/Ginger;component/Images/ with below function
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + imageName));
            if (width > 0)
                img.Width = width;
            if (height > 0)
                img.Height = height;
            return img;
        }


        public static BitmapSource GetImageStream(System.Drawing.Image myImage)
        {
            var bitmap = new Bitmap(myImage);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            DeleteObject(bmpPt);
            bitmap.Dispose();
            return bitmapSource;
        }

        public static System.Drawing.Image Base64StringToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                return image;
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static string BitmapToBase64(Bitmap bImage)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] byteImage = ms.ToArray();
                return Convert.ToBase64String(byteImage); //Get Base64
            }
        }

        public static Tuple<int, int> GetImageHeightWidth(string path)
        {
            Tuple<int, int> a;
            using (Stream stream = File.OpenRead(path))
            {
                using (System.Drawing.Image sourceImage = System.Drawing.Image.FromStream(stream, false, false))
                {
                    a = new Tuple<int, int>(sourceImage.Width, sourceImage.Height);
                }
            }
            return a;
        }
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        //For Screenshots
        public static Tuple<int, int> RecalculatingSizeWithKeptRatio(Tuple<int, int> a, int boxWidth, int boxHight)
        {
            //calculate the ratio
            double dbl = (double)a.Item1 / (double)a.Item2;
            if ((int)((double)boxHight * dbl) <= boxWidth)
            {
                return new Tuple<int, int>((int)((double)boxHight * dbl), boxHight);
            }
            else
            {
                return new Tuple<int, int>(boxWidth, (int)((double)boxWidth / dbl));
            }
        }

        //For logos
        public static Tuple<int, int> RecalculatingSizeWithKeptRatio(BitmapSource source, int boxWidth, int boxHight)
        {
            //calculate the ratio
            double dbl = (double)source.Width / (double)source.Height;
            if ((int)((double)boxHight * dbl) <= boxWidth)
            {
                return new Tuple<int, int>((int)((double)boxHight * dbl), boxHight);
            }
            else
            {
                return new Tuple<int, int>(boxWidth, (int)((double)boxWidth / dbl));
            }
        }
    }         
}
