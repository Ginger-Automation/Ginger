#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Common;
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
        // Default maximum dimensions for image display
        private const int DEFAULT_MAX_WIDTH = 9999;
        private const int DEFAULT_MAX_HEIGHT = 9999;
        private int maxWidth = DEFAULT_MAX_WIDTH;
        private int maxHeight = DEFAULT_MAX_HEIGHT;

        public Action<object, MouseclickonScrenshot> MouseClickOnScreenshot { get; set; }

        public Action<object, MouseUponScrenshot> MouseUpOnScreenshot { get; set; }

        public Action<object, MouseMoveonScrenshot> MouseMoveOnScreenshot { get; set; }

        public Cursor ImageMouseCursor
        {
            get
            {
                return xMainImage.Cursor;
            }
            set
            {
                xMainImage.Cursor = value;
            }
        }

        public BitmapImage BitmapImage
        {
            get
            {
                return mBitmapImage;
            }
        }

        public ScreenShotViewPage(string Name, BitmapImage bitmapImage)
        {
            InitializeComponent();

            mBitmapImage = bitmapImage;
            mName = Name;

            ShowBitmap();

            xHighLighterRectangle.Visibility = Visibility.Collapsed;
        }

        public ScreenShotViewPage(string Name, BitmapSource bitmapSource, bool IsDisplayName = true, int ImageMaxHeight = DEFAULT_MAX_HEIGHT, int ImageMaxWidth = DEFAULT_MAX_WIDTH)
        {
            InitializeComponent();

            mBitmapSource = bitmapSource;
            mName = Name;
            if (string.IsNullOrEmpty(mName) == false)
            {
                xNameLabel.Content = mName;
            }
            else
            {
                xNameLabel.Content = "Image";
            }
            if (bitmapSource != null)
            {
                xNameLabel.Content = string.Format("{0} ({1})", xNameLabel.Content.ToString(), bitmapSource.PixelHeight + "x" + bitmapSource.PixelWidth);
                // Set maximum dimensions if provided
                if (ImageMaxHeight != DEFAULT_MAX_HEIGHT)
                {
                    maxHeight = ImageMaxHeight;
                }
                if (ImageMaxWidth != DEFAULT_MAX_WIDTH)
                {
                    maxWidth = ImageMaxWidth;
                }
                SetDimensions(height: bitmapSource.PixelHeight, width: bitmapSource.PixelWidth);
                xMainImage.Source = bitmapSource;
            }
            if (IsDisplayName)
            {
                xNameLabel.Visibility = Visibility.Visible;
            }
            else
            {
                xNameLabel.Visibility = Visibility.Collapsed;
            }

            xHighLighterRectangle.Visibility = Visibility.Collapsed;
        }

        public ScreenShotViewPage(string Name, System.Drawing.Bitmap bitmap, double initialZoom = 1)
        {
            InitializeComponent();

            if (bitmap != null)
            {
                mBitmapImage = BitmapToImageSource(bitmap);
                ShowBitmap();
            }
            mName = Name;
            xHighLighterRectangle.Visibility = Visibility.Collapsed;

            if (initialZoom != 1)
            {
                xZoomSlider.Value = initialZoom;
            }
        }

        public ScreenShotViewPage(string Name, String FilePath, double initialZoom = 1)
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

            xHighLighterRectangle.Visibility = Visibility.Collapsed;

            if (initialZoom != 1)
            {
                xZoomSlider.Value = initialZoom;
            }
        }

        private void ClearError()
        {
            xErrorLabel.Visibility = Visibility.Collapsed;
            xMainScrollViewer.Visibility = Visibility.Visible;
        }

        private void ShowError(string err)
        {
            xErrorLabel.Content = err;
            xErrorLabel.Visibility = Visibility.Visible;
            xMainScrollViewer.Visibility = Visibility.Collapsed;
        }

        private BitmapImage GetBimapImageFromFile(String filePath)
        {
            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(filePath);
                image.EndInit();
                return image;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "File extention is not supported.");
                return null;
            }

        }

        private void ShowBitmap()
        {
            if (string.IsNullOrEmpty(mName) == false)
            {
                xNameLabel.Content = mName;
            }
            else
            {
                xNameLabel.Content = "Image";
            }
            if (mBitmapImage != null)
            {
                xNameLabel.Content = string.Format("{0} ({1})", xNameLabel.Content.ToString(), mBitmapImage.PixelHeight + "x" + mBitmapImage.PixelWidth);

                //Change the canvas to match bmp size
                SetDimensions(height: mBitmapImage.PixelHeight, width: mBitmapImage.PixelWidth);
                xMainImage.Source = mBitmapImage;
            }
            else
            {
                ShowError("No Bitmap");
            }
        }

        private bool IsCustomDimensions = false;
        private void SetDimensions(int height, int width)
        {
            try
            {
                // Reset the custom dimensions flag
                IsCustomDimensions = false;
                // Adjusting Viewbox to match the image size within the max limits  
                double aspectRatio = (double)width / height;
                
                // Check if width exceeds maximum and adjust both dimensions proportionally
                if (width > maxWidth)
                {
                    width = maxWidth;
                    height = (int)(width / aspectRatio);
                    IsCustomDimensions = true;
                }

                // After width adjustment, check if height still exceeds maximum
                if (height > maxHeight)
                {
                    height = maxHeight;
                    width = (int)(height * aspectRatio);
                    IsCustomDimensions = true;
                }

                //Change the canvas to match bmp size
                xMainCanvas.Width = width;
                xMainCanvas.Height = height;
                if (IsCustomDimensions)
                {
                    xMainImage.Width = width;
                    xMainImage.Height = height;
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error while setting dimensions for Screenshot Viewer", ex);                
            }
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
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to convert Bitmap to ImageSource", ex);
                    return null;
                }

            }
        }

        public void ShowAsWindow(string Title, bool ShowEnlarge)
        {
            if (!ShowEnlarge)
            {
                xEnlargeButton.Visibility = Visibility.Collapsed;
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
            {
                p = new ScreenShotViewPage(mName, mBitmapImage);
            }
            else if (mBitmapSource != null)
            {
                p = new ScreenShotViewPage(mName, mBitmapSource);
            }

            if (p != null)
            {
                p.ShowAsWindow(mName, false);
            }
        }

        //TODO: the zoom slider is dup with FlowDiagrmaPage - create User control to use for both
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.xMainCanvas == null)
            {
                return;  // will happen only at page load
            }

            // Set the Canvas scale based on ZoomSlider value
            ScaleTransform ST = new ScaleTransform(e.NewValue, e.NewValue);
            // Only apply transform to the image when custom dimensions are set to prevent 
            // unnecessary transformations on images displayed at original size
            if (IsCustomDimensions)
            {
                this.xMainImage.LayoutTransform = ST;
            }
            this.xMainCanvas.LayoutTransform = ST;
            xZoomPercentLabel.Content = (int)(e.NewValue * 100) + "%";
        }

        private void ZoomMinus_Click(object sender, RoutedEventArgs e)
        {
            // We reduce 0.1 and round it nicely to the nearest 10% - so it will go from 57% to 50% instaed of 47%
            xZoomSlider.Value = Math.Round(xZoomSlider.Value * 10 - 1) / 10;
        }

        private void ZoomPlus_Click(object sender, RoutedEventArgs e)
        {
            xZoomSlider.Value = Math.Round(xZoomSlider.Value * 10 + 1) / 10;
        }

        private void MainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MouseClickOnScreenshot != null)
            {
                System.Windows.Point p = Mouse.GetPosition(xMainImage);
                MouseClickOnScreenshot.Invoke(xMainImage, new MouseclickonScrenshot(p.X, p.Y));
            }
        }

        internal void HighLight(int x, int y, int width, int height)
        {
            xHighLighterRectangle.Margin = new Thickness(x + xMainImage.Margin.Left, y + xMainImage.Margin.Top, 0, 0);
            xHighLighterRectangle.Width = width;
            xHighLighterRectangle.Height = height;
            xHighLighterRectangle.Visibility = Visibility.Visible;
        }

        internal void HighLight(Rectangle r)
        {
            HighLight(r.X, r.Y, r.Width, r.Height);
        }

        // We can add controls on top of the screen shot - can be used for Mobile Driver and POM simulator
        public void AddControl(Control c, double X, Double Y)
        {
            c.Margin = new Thickness(X + xMainImage.Margin.Left, Y + xMainImage.Margin.Top, 0, 0);
            xMainCanvas.Children.Add(c);
        }

        public void ClearControls()
        {
            xMainCanvas.Children.Clear();
        }

        private void MainImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (MouseUpOnScreenshot != null)
            {
                System.Windows.Point p = Mouse.GetPosition(xMainImage);
                MouseUpOnScreenshot.Invoke(xMainImage, new MouseUponScrenshot(p.X, p.Y));
            }
        }

        private void MainImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseMoveOnScreenshot != null)
            {
                System.Windows.Point p = Mouse.GetPosition(xMainImage);
                MouseMoveOnScreenshot.Invoke(xMainImage, new MouseMoveonScrenshot(p.X, p.Y));
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
