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

using Ginger;
using GingerCore.Drivers.Common.Devices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace GingerCore.Drivers.Common
{
    /// <summary>
    /// Interaction logic for DeviceViewPage.xaml
    /// </summary>
    public partial class DeviceViewPage : Page
    {
        DeviceConfig mAndroidDeviceConfig;
        string mDeviceConfigFolder;

        public DeviceConfig AndroidDeviceConfig { get { return mAndroidDeviceConfig; } }

        double scaleFactor = 1;

        //Events

        public event TouchXYEventHandler TouchXY;
        public event SwipeEventHandler Swipe;
        public event ButtonEventHandler ButtonClick;


        List<Shape> mButtons = [];

        System.Windows.Shapes.Rectangle rect;

        System.Windows.Point MouseStartPoint = new System.Windows.Point();
        System.Windows.Point MouseLastPoint = new System.Windows.Point();


        public DeviceViewPage(string DeviceConfigFolder)
        {
            mDeviceConfigFolder = DeviceConfigFolder;
            mAndroidDeviceConfig = DeviceConfig.LoadFromDeviceFolder(mDeviceConfigFolder);

            Init();
        }

        public DeviceViewPage(DeviceConfig DC)
        {
            mAndroidDeviceConfig = DC;
            Init();
        }

        private void Init()
        {
            InitializeComponent();

            DeviceNameLabel.Content = mAndroidDeviceConfig.Name;

            InitDeviceView();
        }

        private void InitDeviceView()
        {
            string ImageFile = mDeviceConfigFolder + mAndroidDeviceConfig.DeviceImage;
            if (!string.IsNullOrEmpty(mDeviceConfigFolder))
            {
                DeviceScreenShotImageBK.Source = new BitmapImage(new Uri(ImageFile));
                string ScreenFile = mDeviceConfigFolder + "screen.png";
                DeviceImage.Source = new BitmapImage(new Uri(ScreenFile));
            }
            else
            {
                // put some default pics
                //string img = "pack://application:,,,/Ginger;component/Images/RemoteAgentScreen.jpg";             
                string img = @"/Images/RemoteAgentScreen.jpg";
                BitmapSource Bs = new BitmapImage(new Uri(img, UriKind.RelativeOrAbsolute));
                DeviceScreenShotImageBK.Source = Bs;

                //string ScreenFile = "pack://application:,,,/Ginger;component/Images/RemoteAgentTempScreen.jpg";
                string ScreenFile = @"/Images/RemoteAgentTempScreen.jpg";
                DeviceImage.Source = new BitmapImage(new Uri(ScreenFile, UriKind.RelativeOrAbsolute));
            }
            InitDeviceButtons();
            InitHighLighter();
            InitControllerView();
        }

        //TODO: move to general
        BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
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
        }

        private void InitHighLighter()
        {
            rect = new System.Windows.Shapes.Rectangle
            {
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 100, 0, 0)),
                Opacity = 50,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Top
            };
            DeviceScreenShotGrid.Children.Add(rect);
        }

        private void InitControllerView()
        {
            if (mAndroidDeviceConfig.DeviceControllers == null || mAndroidDeviceConfig.DeviceControllers.Count == 0)
            {
                DeviceControllerGridColumn.Width = new GridLength(0);
            }
        }

        private void InitDeviceButtons()
        {
            if (mAndroidDeviceConfig.DeviceButtons == null)
            {
                mAndroidDeviceConfig.DeviceButtons = [];
            }

            List<DeviceButton> list = mAndroidDeviceConfig.DeviceButtons;  // mDeviceConfigFolder GetControllerActions();

            DeviceButtonsGrid.ItemsSource = list;

            foreach (DeviceButton DB in list)
            {
                if (DB.ButtonShape == DeviceButton.eButtonShape.Rectangle)
                {
                    System.Windows.Shapes.Rectangle rect;
                    rect = new System.Windows.Shapes.Rectangle
                    {
                        Stroke = new SolidColorBrush(Colors.Gray),
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 100, 0, 0)),
                        Opacity = 50,
                        Tag = DB,
                        ToolTip = DB.ToolTip
                    };
                    rect.MouseLeftButtonUp += rect_MouseLeftButtonUp;
                    rect.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    rect.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    DeviceScreenShotGrid.Children.Add(rect);

                    mButtons.Add(rect);
                }

                if (DB.ButtonShape == DeviceButton.eButtonShape.Ellipse)
                {
                    System.Windows.Shapes.Ellipse ellipse;
                    ellipse = new System.Windows.Shapes.Ellipse
                    {
                        Stroke = new SolidColorBrush(Colors.Gray),
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Colors.Yellow),
                        Opacity = 30,
                        Tag = DB,
                        ToolTip = DB.ToolTip
                    };
                    ellipse.MouseLeftButtonUp += rect_MouseLeftButtonUp;
                    ellipse.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    ellipse.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    DeviceScreenShotGrid.Children.Add(ellipse);

                    mButtons.Add(ellipse);
                }

                ResizeDeviceButtons();
                //TODO: else handle other type of buttons
            }
        }

        private void rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Shapes.Shape button = (System.Windows.Shapes.Shape)sender;
            DeviceButton DB = (DeviceButton)button.Tag;
            OnButtonClick(DB);
        }


        private void ControllerActionsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DeviceButton CA = (DeviceButton)ControllerActionsGrid.SelectedItem;
        }

        private void DeviceScreenShotGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // First, compute the horizontal and vertical scaling factors individually:

            var image = DeviceScreenShotImageBK.Source;

            // since we want to maintain the aspect ratio calculate the ScaleFactor which will be used
            double hscale = (double)image.Width / e.NewSize.Width;
            double vscale = (double)image.Height / e.NewSize.Height;
            scaleFactor = Math.Max(hscale, vscale);

            // Calculate where the image will located calc left & Top Margin as it goes uniform and centered
            double leftMargin = (e.NewSize.Width - DeviceScreenShotImageBK.ActualWidth) / 2;
            double TopMargin = (e.NewSize.Height - DeviceScreenShotImageBK.ActualHeight) / 2;

            // Device screen location in original bitmap coordinates, like pbrush shows
            double mLeft = mAndroidDeviceConfig.DeviceImageScreenLeft;
            double mTop = mAndroidDeviceConfig.DeviceImageScreenTop;
            double mRight = mAndroidDeviceConfig.DeviceImageScreenRight;
            double mBottom = mAndroidDeviceConfig.DeviceImageScreenBottom;


            if (mRight == 0)
            {
                mRight = 400;
            }

            if (mBottom == 0)
            {
                mBottom = 600;
            }

            double LeftOffset = leftMargin + mLeft / scaleFactor;
            double TopOffset = TopMargin + mTop / scaleFactor;

            double RightOffset = ((double)image.Width - mRight) / scaleFactor;
            double BottomOffset = TopMargin + ((double)image.Height - mBottom) / scaleFactor;

            DeviceImage.Width = (mRight - mLeft) / scaleFactor;
            DeviceImage.Height = (mBottom - mTop) / scaleFactor;

            // Create new margin where we want to device bitmap to be            
            DeviceImage.Margin = new Thickness(LeftOffset, TopOffset, RightOffset, BottomOffset);

            // realign the Canvas to be on top of the image
            DeviceScreenCanvas.Width = DeviceImage.Width;
            DeviceScreenCanvas.Height = DeviceImage.Height;
            DeviceScreenCanvas.Margin = DeviceImage.Margin;

            ResizeDeviceButtons();

            ScaleLabel.Content = "Scale: " + scaleFactor.ToString("0.##");
        }

        private void ResizeDeviceButtons()
        {
            if (DeviceScreenCanvas == null || DeviceImage == null || DeviceScreenShotImageBK == null)
            {
                return;
            }

            // if we don't have a valid scale or image sizes yet bail out
            if (scaleFactor <= 0 || DeviceScreenShotImageBK.ActualWidth == 0)
            {
                return;
            }

            // DeviceScreenCanvas.Margin is set to DeviceImage.Margin in SizeChanged handler,
            // so it reflects the actual device image placement relative to the grid.
            double canvasLeft = DeviceScreenCanvas.Margin.Left;
            double canvasTop = DeviceScreenCanvas.Margin.Top;

            foreach (Shape s in mButtons)
            {
                if (s?.Tag == null) continue;

                DeviceButton DB = (DeviceButton)s.Tag;

                // size the visual element according to the computed scale factor
                s.Width = DB.Width / scaleFactor;
                s.Height = DB.Height / scaleFactor;

                // compute position relative to the DeviceScreenCanvas (which overlays the actual device image)
                double left = canvasLeft + DB.Left / scaleFactor;
                double top = canvasTop + DB.Top / scaleFactor;

                // set margin to place the shape inside the grid at the correct absolute location
                s.Margin = new Thickness(left, top, 0, 0);
            }
        }

        public void UpdateDeviceScreenShot(Bitmap bmp)
        {
            //this.Dispatcher.Invoke(() =>
            //{
            //    DeviceImage.Source = General.ToBitmapSource(bmp);
            //});
        }

        public void UpdateDeviceScreenShot(BitmapImage BI)
        {
            DeviceImage.Dispatcher.Invoke(() =>
            {
                // works but give pther thread own
                DeviceImage.Source = BI;
            });
        }

        public class TouchXYEventArgs : EventArgs
        {
            double mLeft = -1;
            double mTop = -1;

            public TouchXYEventArgs(double Left, double Top)
            {
                mLeft = Left;
                mTop = Top;
            }

            public double Left { get { return mLeft; } }
            public double Top { get { return mTop; } }
        }

        public class SwipeEventArgs : EventArgs
        {
            double mX = -1;
            double mY = -1;
            double mXE = -1;
            double mYE = -1;
            int mSteps = 1;

            public SwipeEventArgs(double X, double Y, double XE, double YE, int steps)
            {
                mX = X;
                mY = Y;
                mXE = XE;
                mYE = YE;
                mSteps = steps;
            }

            public double X { get { return mX; } }
            public double Y { get { return mY; } }
            public double XE { get { return mXE; } }
            public double YE { get { return mYE; } }
            public int Steps { get { return mSteps; } }
        }


        public delegate void TouchXYEventHandler(object sender, TouchXYEventArgs e);

        public delegate void SwipeEventHandler(object sender, SwipeEventArgs e);

        public void OnTouchXY(double Left, double Top)
        {
            TouchXYEventHandler handler = TouchXY;
            if (handler != null)
            {
                handler(this, new TouchXYEventArgs(Left, Top));
            }
        }

        private void DeviceControllerGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        //--------------- Button Event

        public class ButtonEventArgs : EventArgs
        {
            DeviceButton mDB;

            public ButtonEventArgs(DeviceButton db)
            {
                mDB = db;
            }

            public DeviceButton DeviceButton { get { return mDB; } }
        }

        public delegate void ButtonEventHandler(object sender, ButtonEventArgs e);

        public void OnButtonClick(DeviceButton DB)
        {
            ButtonEventHandler handler = ButtonClick;
            if (handler != null)
            {
                handler(this, new ButtonEventArgs(DB));
            }
        }

        public void RedrawDevice()
        {
            InitDeviceView();
        }

       
        private System.Windows.Point GetDevicePoint(System.Windows.Point point)
        {
            // Map a point relative to the Image control (DeviceImage) to source image pixel coordinates.
            // Use BitmapSource.PixelWidth/PixelHeight to get the true screenshot pixel size (fixes TV scaling issues).
            var bitmap = DeviceImage.Source as System.Windows.Media.Imaging.BitmapSource;
            if (bitmap == null || DeviceImage.ActualWidth <= 0 || DeviceImage.ActualHeight <= 0)
            {
                return new System.Windows.Point(0, 0);
            }

            double srcPixelWidth = bitmap.PixelWidth;
            double srcPixelHeight = bitmap.PixelHeight;
            double controlWidth = DeviceImage.ActualWidth;
            double controlHeight = DeviceImage.ActualHeight;

            if (srcPixelWidth <= 0 || srcPixelHeight <= 0)
            {
                return new System.Windows.Point(0, 0);
            }

            // Compute based on Image.Stretch
            double imageX, imageY;
            switch (DeviceImage.Stretch)
            {
                case Stretch.Fill:
                    {
                        // image stretched non-uniformly to fill control
                        double scaleX = srcPixelWidth / controlWidth;
                        double scaleY = srcPixelHeight / controlHeight;
                        imageX = point.X * scaleX;
                        imageY = point.Y * scaleY;
                        break;
                    }

                case Stretch.UniformToFill:
                    {
                        double scale = Math.Max(controlWidth / srcPixelWidth, controlHeight / srcPixelHeight);
                        double displayedWidth = srcPixelWidth * scale;
                        double displayedHeight = srcPixelHeight * scale;
                        double offsetX = (controlWidth - displayedWidth) / 2.0;
                        double offsetY = (controlHeight - displayedHeight) / 2.0;
                        double relX = point.X - offsetX;
                        double relY = point.Y - offsetY;
                        relX = Math.Max(0, Math.Min(relX, displayedWidth - 1));
                        relY = Math.Max(0, Math.Min(relY, displayedHeight - 1));
                        imageX = relX / scale;
                        imageY = relY / scale;
                        break;
                    }

                case Stretch.None:
                    {
                        // No scaling: control shows image at native (or clipped) size
                        imageX = point.X;
                        imageY = point.Y;
                        break;
                    }

                case Stretch.Uniform:
                default:
                    {
                        double scale = Math.Min(controlWidth / srcPixelWidth, controlHeight / srcPixelHeight);
                        double displayedWidth = srcPixelWidth * scale;
                        double displayedHeight = srcPixelHeight * scale;
                        double offsetX = (controlWidth - displayedWidth) / 2.0;
                        double offsetY = (controlHeight - displayedHeight) / 2.0;
                        double relX = point.X - offsetX;
                        double relY = point.Y - offsetY;
                        relX = Math.Max(0, Math.Min(relX, displayedWidth - 1));
                        relY = Math.Max(0, Math.Min(relY, displayedHeight - 1));
                        imageX = relX / scale;
                        imageY = relY / scale;
                        break;
                    }
            }

            // Clamp to source bounds and return (image source pixels)
            imageX = Math.Max(0, Math.Min(imageX, srcPixelWidth - 1));
            imageY = Math.Max(0, Math.Min(imageY, srcPixelHeight - 1));

            return new System.Windows.Point(imageX, imageY);
        }



        private System.Windows.Point GetPointFromDeviceCoordinates(System.Windows.Point point)
        {
            System.Windows.Point p = new System.Windows.Point
            {
                X = point.X / scaleFactor,
                Y = point.Y / scaleFactor
            };

            return p;
        }

        private void DeviceImage_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = GetDevicePoint(e.GetPosition(DeviceImage));
            string s = p.X + "," + p.Y;
            MouseLcoationLabel.Content = s;
        }

        public void SetHighLighter(int X, int Y, int Width, int Height)
        {
            // We set the highlighter in the DeviceScreenShotImageBK - on the screen shot

            if (DeviceScreenCanvas == null || DeviceImage == null || DeviceScreenShotImageBK == null)
            {
                return;
            }

            // Calculate scale factor from the actual DeviceImage source (if available) and its control size
            var image = DeviceImage.Source;
            if (image == null || DeviceImage.ActualWidth <= 0 || DeviceImage.ActualHeight <= 0)
            {
                return;
            }

            double hscale = (double)image.Width / DeviceImage.ActualWidth;
            double vscale = (double)image.Height / DeviceImage.ActualHeight;
            scaleFactor = Math.Max(hscale, vscale);
            if (scaleFactor <= 0) scaleFactor = 1;

            rect.Width = Width / scaleFactor;
            rect.Height = Height / scaleFactor;

            double canvasLeft = DeviceScreenCanvas.Margin.Left;
            double canvasTop = DeviceScreenCanvas.Margin.Top;

            double left = canvasLeft + X / scaleFactor;
            double top = canvasTop + Y / scaleFactor;

            rect.Margin = new Thickness(left, top, 0, 0);
        }

        public System.Windows.Point GetMousePosition()
        {
            System.Windows.Point p = new System.Windows.Point();
            this.Dispatcher.Invoke(() =>
            {
                p = Mouse.GetPosition(DeviceImage);
                p.X = p.X * scaleFactor;
                p.Y = p.Y * scaleFactor;
            });
            return p;
        }

        private void DeviceImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void DeviceImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        public void OnSwipe(double X, double Y, double XE, double YE, int steps)
        {
            SwipeEventHandler handler = Swipe;
            if (handler != null)
            {
                handler(this, new SwipeEventArgs(X, Y, XE, YE, steps));
            }
        }

        private void DeviceScreenCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Hand;
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                MouseLastPoint = e.GetPosition(DeviceScreenCanvas);
            }

            MouseStartPoint = MouseLastPoint;
        }

        private void DeviceScreenCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // if we swipe then draw a line on the canvas, give user feedback
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line
                {
                    Stroke = System.Windows.SystemColors.WindowFrameBrush,

                    StrokeThickness = 3,

                    X1 = MouseLastPoint.X,
                    Y1 = MouseLastPoint.Y
                };
                MouseLastPoint = e.GetPosition(DeviceScreenCanvas);
                line.X2 = MouseLastPoint.X;
                line.Y2 = MouseLastPoint.Y;

                DeviceScreenCanvas.Children.Add(line);
            }

            MouseLastPoint = e.GetPosition(DeviceScreenCanvas);

            System.Windows.Point p = GetDevicePoint(MouseLastPoint);
            string s = p.X + "," + p.Y;
            MouseLcoationLabel.Content = s;
        }

        private void DeviceScreenCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = null;

            // check if the mouse down was close then do touch, otherwise we do swipe
            if (Math.Abs(MouseStartPoint.X - MouseLastPoint.X) < 10 && Math.Abs(MouseStartPoint.Y - MouseLastPoint.Y) < 10)
            {
                System.Windows.Point p = GetDevicePoint(MouseLastPoint);
                OnTouchXY(p.X, p.Y);
                DeviceScreenCanvas.Children.Clear();
            }
            else
            {
                int steps = 30;  // each step takes 5ms - so for 100 it will take half a second
                System.Windows.Point p1 = GetDevicePoint(MouseStartPoint);
                System.Windows.Point p2 = GetDevicePoint(MouseLastPoint);
                OnSwipe(p1.X, p1.Y, p2.X, p2.Y, steps);
                DeviceScreenCanvas.Children.Clear();
            }
        }

        public void ShowAsWindow()
        {
            GenericWindow genWin = null;
            this.Height = 400;
            this.Width = 400;
            GingerCore.General.LoadGenericWindow(ref genWin, null, eWindowShowStyle.Free, "Device View", this);
        }
    }
}