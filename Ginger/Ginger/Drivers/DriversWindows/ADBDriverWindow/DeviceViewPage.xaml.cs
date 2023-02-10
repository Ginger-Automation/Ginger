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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GingerCore.Drivers.AndroidADB;
using GingerCore.Drivers.Common.Devices;
using Ginger;


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


       List<Shape>  mButtons = new List<Shape>();
        
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
            rect = new System.Windows.Shapes.Rectangle();
            rect.Stroke = new SolidColorBrush(Colors.Red);
            rect.StrokeThickness = 2;
            rect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)50, (byte)100, (byte)0, 0));
            rect.Opacity = 50;
            rect.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            rect.VerticalAlignment = System.Windows.VerticalAlignment.Top;
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
            if (mAndroidDeviceConfig.DeviceButtons == null) mAndroidDeviceConfig.DeviceButtons = new List<DeviceButton>();

            List<DeviceButton> list = mAndroidDeviceConfig.DeviceButtons;  // mDeviceConfigFolder GetControllerActions();

            DeviceButtonsGrid.ItemsSource = list;

            foreach(DeviceButton DB in list)
            {
                if (DB.ButtonShape == DeviceButton.eButtonShape.Rectangle)
                {
                    System.Windows.Shapes.Rectangle rect;
                    rect = new System.Windows.Shapes.Rectangle();
                    rect.Stroke = new SolidColorBrush(Colors.Gray);
                    rect.StrokeThickness = 2;
                    rect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)50, (byte)100, (byte)0, 0));                                        
                    rect.Opacity = 50;
                    rect.Tag = DB;
                    rect.ToolTip = DB.ToolTip;
                    rect.MouseLeftButtonUp +=rect_MouseLeftButtonUp;
                    rect.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    rect.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    DeviceScreenShotGrid.Children.Add(rect);

                    mButtons.Add(rect);
                }

                if (DB.ButtonShape == DeviceButton.eButtonShape.Ellipse)
                {
                    System.Windows.Shapes.Ellipse ellipse;
                    ellipse = new System.Windows.Shapes.Ellipse();
                    ellipse.Stroke = new SolidColorBrush(Colors.Gray);
                    ellipse.StrokeThickness = 2;
                    ellipse.Fill = new SolidColorBrush(Colors.Yellow);
                    ellipse.Opacity = 30;
                    ellipse.Tag = DB;
                    ellipse.ToolTip = DB.ToolTip;
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


            if (mRight == 0) mRight = 400;
            if (mBottom == 0) mBottom = 600;

            double LeftOffset = leftMargin + mLeft / scaleFactor;
            double TopOffset = TopMargin + mTop / scaleFactor;

            double RightOffset =((double)image.Width - mRight) / scaleFactor;
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
            if (DeviceScreenShotImageBK.ActualWidth == 0) return;
            foreach (Shape s in mButtons)
            {
                DeviceButton DB = (DeviceButton)s.Tag;
                
                s.Width = DB.Width / scaleFactor;
                s.Height = DB.Height / scaleFactor;

                double left = (DeviceScreenShotGrid.ActualWidth - DeviceScreenShotImageBK.ActualWidth) / 2 + DB.Left / scaleFactor;
                double top =  (DeviceScreenShotGrid.ActualHeight - DeviceScreenShotImageBK.ActualHeight) / 2 + DB.Top / scaleFactor;

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
                DeviceImage.Source = (ImageSource)BI;           
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
            public double Top { get{ return mTop;} }            
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
            System.Windows.Point p = new System.Windows.Point();
            
            p.X = Convert.ToInt16(DeviceImage.Source.Width * point.X / DeviceImage.ActualWidth);
            p.Y = Convert.ToInt16(DeviceImage.Source.Height * point.Y / DeviceImage.ActualHeight);

            return p;
        }

        private System.Windows.Point GetPointFromDeviceCoordinates(System.Windows.Point point)
        {
            System.Windows.Point p = new System.Windows.Point();

            p.X = point.X / scaleFactor; 
            p.Y = point.Y / scaleFactor; 

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

            System.Windows.Point p = GetPointFromDeviceCoordinates(new System.Windows.Point(X, Y));
            
            var image = DeviceImage.Source;

            // since we want to maintain the aspect ratio calculate the ScaleFactor which will be used
            double hscale = (double)image.Width / DeviceImage.ActualWidth;
            double vscale = (double)image.Height / DeviceImage.ActualHeight;
            scaleFactor = Math.Max(hscale, vscale);

            rect.Width = Width / scaleFactor;
            rect.Height = Height / scaleFactor;

            double leftMargin = (DeviceScreenShotGrid.ActualWidth - DeviceImage.ActualWidth) / 2;
            double TopMargin = (DeviceScreenShotGrid.ActualHeight - DeviceImage.ActualHeight) / 2;

            double left = (leftMargin + X / scaleFactor); 
            double top = (TopMargin + Y / scaleFactor);
            
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
                MouseLastPoint = e.GetPosition(DeviceScreenCanvas);
            MouseStartPoint = MouseLastPoint;
        }

        private void DeviceScreenCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // if we swipe then draw a line on the canvas, give user feedback
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line();

                line.Stroke = System.Windows.SystemColors.WindowFrameBrush;
                
                line.StrokeThickness = 3;

                line.X1 = MouseLastPoint.X;
                line.Y1 = MouseLastPoint.Y;
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