#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ginger.Actions.UserControls
{
    /// <summary>
    /// Interaction logic for ScreenShotViewPage.xaml
    /// </summary>
    public partial class ScreenShotViewPage : Page
    {
        BitmapImage mBitmapImage;
        BitmapSource mBitmapSource;
        string mName;

        public Action<object, MouseclickonScrenshot> MouseClickOnScreenshot { get; set; }

        public Action<object, MouseUponScrenshot> MouseUpOnScreenshot { get; set; }

        public Action<object, MouseMoveonScrenshot> MouseMoveOnScreenshot { get; set; }

        public ScreenShotViewPage(string Name, BitmapImage bitmapImage)
        {
            InitializeComponent();

            mBitmapImage = bitmapImage;
            mName = Name;
            
            ShowBitmap();

            HighLighterRectangle.Visibility = Visibility.Collapsed;
        }
        
        public ScreenShotViewPage(string Name, BitmapSource bitmapSource)
        {
            InitializeComponent();

            mBitmapSource = bitmapSource;

            if (bitmapSource != null)
            {
                
                SizeLabel.Content = bitmapSource.PixelHeight + "x" + bitmapSource.PixelWidth;

                //Change the canvas to match bmp size
                MainCanvas.Width = bitmapSource.PixelWidth;
                MainCanvas.Height = bitmapSource.PixelHeight;
                MainImage.Source = bitmapSource;
            }

            mName = Name;
            NameLabel.Content = mName;
            HighLighterRectangle.Visibility = Visibility.Collapsed;
        }

        public ScreenShotViewPage(string Name, System.Drawing.Bitmap bitmap)
        {
            InitializeComponent();

            if (bitmap != null)
            {
                mBitmapImage = BitmapToImageSource(bitmap);
                ShowBitmap();
            }
            mName = Name;
            HighLighterRectangle.Visibility = Visibility.Collapsed;
        }

        public ScreenShotViewPage(string Name, String FilePath)
        {
            InitializeComponent();

            string FileName = General.GetFullFilePath(FilePath);

            if (File.Exists(FileName))
            {
                ClearError();                

                mBitmapImage = GetBimapImageFromFile(FileName);
                ShowBitmap();
            }
            else
            {
                ShowError("File not found - " + FileName);
            }
            mName = Name;

            HighLighterRectangle.Visibility = Visibility.Collapsed;
        }

        private void ClearError()
        {
            ErrorLabel.Visibility = Visibility.Collapsed;
            MainScrollViewer.Visibility = Visibility.Visible;
        }

        private void ShowError(string err)
        {
            ErrorLabel.Content = err;
            ErrorLabel.Visibility = Visibility.Visible;
            MainScrollViewer.Visibility = Visibility.Collapsed;
        }

        private BitmapImage GetBimapImageFromFile(String filePath)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(filePath);
            image.EndInit();
            return image;
        }

        private void ShowBitmap()
        {            
            if (mBitmapImage != null)
            {
                SizeLabel.Content = mBitmapImage.PixelHeight + "x" + mBitmapImage.PixelWidth;
                
                //Change the canvas to match bmp size
                MainCanvas.Width = mBitmapImage.PixelWidth; 
                MainCanvas.Height = mBitmapImage.PixelHeight;
                MainImage.Source = mBitmapImage;                
            }
            else
            {
                ShowError("No Bitmap");
            }
            NameLabel.Content = mName;
        }

        //TODO: move to general class
        BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {

                try
                {
                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    return bitmapimage;
                }
                catch
                {
                    return null;
                }

            }
        }

        public void ShowAsWindow(string Title, bool ShowEnlarge)
        {
            if (!ShowEnlarge)
            {
                EnlargeButton.Visibility = Visibility.Collapsed;
            }
            GenericWindow genWin = null;
            this.Height = 600;
            this.Width = 800;
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, eWindowShowStyle.Free, "image viewer - " + mName, this);
        }

        private void EnlargeButton_Click(object sender, RoutedEventArgs e)
        {
            // create a new page to show as full window

            ScreenShotViewPage p = null;
            if (mBitmapImage != null)
                p = new ScreenShotViewPage(mName, mBitmapImage);
            else if (mBitmapSource != null)
                p = new ScreenShotViewPage(mName, mBitmapSource);
            p.ShowAsWindow(mName, false);
        }
       
        //TODO: the zoom slider is dup with FlowDiagrmaPage - create User control to use for both
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.MainCanvas == null) return;  // will happen only at page load

            // Set the Canvas scale based on ZoomSlider value
            ScaleTransform ST = new ScaleTransform(e.NewValue, e.NewValue);
            this.MainCanvas.LayoutTransform = ST;

            ZoomPercentLabel.Content = (int)(e.NewValue * 100) + "%";
        }

        private void ZoomMinus_Click(object sender, RoutedEventArgs e)
        {
            // We reduce 0.1 and round it nicely to the nearest 10% - so it will go from 57% to 50% instaed of 47%
            ZoomSlider.Value = Math.Round(ZoomSlider.Value * 10 - 1) / 10;
        }

        private void ZoomPlus_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value = Math.Round(ZoomSlider.Value * 10 + 1) / 10;
        }

        private void MainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {            
            if (MouseClickOnScreenshot != null)
            {
                System.Windows.Point p = Mouse.GetPosition(MainImage);
                MouseClickOnScreenshot.Invoke(MainImage, new MouseclickonScrenshot(p.X, p.Y));
            }
        }

        internal void HighLight(int x, int y, int width, int height)
        {
            HighLighterRectangle.Margin = new Thickness(x + MainImage.Margin.Left, y + MainImage.Margin.Top ,0,0);
            HighLighterRectangle.Width = width;
            HighLighterRectangle.Height = height;
            HighLighterRectangle.Visibility = Visibility.Visible;
        }

        internal void HighLight(Rectangle r)
        {
            HighLight(r.X, r.Y, r.Width, r.Height);
        }

        // We can add controls on top of the screen shot - can be used for Mobile Driver and POM simulator
        public void AddControl(Control c, double X, Double Y)
        {
            c.Margin = new Thickness(X + MainImage.Margin.Left, Y + MainImage.Margin.Top, 0, 0);
            MainCanvas.Children.Add(c);
        }

        public void ClearControls()
        {
            MainCanvas.Children.Clear();
        }

        private void MainImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
            if (MouseUpOnScreenshot != null)
            {
                System.Windows.Point p = Mouse.GetPosition(MainImage);
                MouseUpOnScreenshot.Invoke(MainImage, new MouseUponScrenshot(p.X, p.Y));
            }
        }

        private void MainImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseMoveOnScreenshot != null)
            {
                System.Windows.Point p = Mouse.GetPosition(MainImage);
                MouseMoveOnScreenshot.Invoke(MainImage, new MouseMoveonScrenshot(p.X, p.Y));
            }
        }
    }

    public class MouseclickonScrenshot : RoutedEventArgs
    {
        private double x;
        private double y;

        public MouseclickonScrenshot(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double X { get { return x; } }
        public double Y { get { return y; } }
    }

    public class MouseUponScrenshot : RoutedEventArgs
    {
        private double x;
        private double y;

        public MouseUponScrenshot(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double X { get { return x; } }
        public double Y { get { return y; } }
    }

    public class MouseMoveonScrenshot : RoutedEventArgs
    {
        private double x;
        private double y;

        public MouseMoveonScrenshot(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double X { get { return x; } }
        public double Y { get { return y; } }
    }
}
