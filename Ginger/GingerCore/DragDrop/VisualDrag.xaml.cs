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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GingerCore.DragDrop 
{
    public class dragParams { public bool exitFlag = false; }
    /// <summary>
    /// Interaction logic for ActionHelper.xaml
    /// </summary>
    public partial class VisualDrag : Window
    {
        #region DragLocationWindow Bounds Region
        // Drag window location and animation based on Bounds of window. example if too close to right hand of screen then drag will be on left
        /// <summary>
        /// Position to lock for change size in horizontal way (x)
        /// </summary>
        public static readonly DependencyProperty LocationLockHorizontalProperty = DependencyProperty.Register("LocationLockHorizontal", typeof(AlignmentX), typeof(Window));
        /// <summary>
        /// Position to lock for change size in vertical way (y)
        /// </summary>
        public static readonly DependencyProperty LocationLockVerticalProperty = DependencyProperty.Register("LocationLockVertical", typeof(AlignmentY), typeof(Window));
        /// <summary>
        /// Location to lock for change size.
        /// </summary>
        public AlignmentX LocationLockHorizontal
        {
            get { return (AlignmentX)GetValue(LocationLockHorizontalProperty); }
            set { SetValue(LocationLockHorizontalProperty, value); }
        }
        /// <summary>
        /// Location to lock for change size.
        /// </summary>
        public AlignmentY LocationLockVertical
        {
            get { return (AlignmentY)GetValue(LocationLockVerticalProperty); }
            set { SetValue(LocationLockVerticalProperty, value); }
        }
        #endregion DragLocationWindow Bounds

        #region MouseTracker Region
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            Console.WriteLine("Mouse-X:" + w32Mouse.X.ToString() + "Y:" + w32Mouse.Y.ToString());
            return new Point(w32Mouse.X, w32Mouse.Y);
        }
        private async void bindmouse()
        {
            while (!exitFlag)
            {
                Point p = GetMousePosition();
                if (this.Dispatcher.CheckAccess() && this != null)
                {
                    this.Left = p.X;
                    this.Top = p.Y;
                }
                else if (this != null)
                {
                    await this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.Top = p.Y + 2;
                        this.Left = p.X + 2;
                    }));

                }
            }
            await Dispatcher.BeginInvoke((Action)delegate
            {
                this.Close();
            }); 
        }
        #endregion mousetracker

        public VisualDrag()
        {
            InitializeComponent();
            exitFlag = false;
            string img = "pack://application:,,,/Ginger;component/Images/@Variable_32x32.png";
            if (dragType == "Activity") { img = "pack://application:,,,/Ginger;component/Images/@ViewFiles_32x32.png"; }
            if (dragType == "VariableBase") { img = "pack://application:,,,/Ginger;component/Images/@Variable_32x32.png"; dragText = dragText.Replace("VariableBase Item", "Variable Item"); }
            if (dragType == "Act") { img = "pack://application:,,,/Ginger;component/Images/@Action_32x32.png"; dragText = dragText.Replace("Act Item", "Action Item"); }
            SetStringWidth();
            
            tbDragText.Text = dragText;

            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource =
                new BitmapImage(new Uri(img));
            DragImg.Fill = imgBrush;
        }

        public static int newHeight { get; set; }
        public static int newWidth { get; set; }
        public int duration { get; set; }
        public static string dragType {get;set;}
        public static string dragText { get; set; }
        public static bool exitFlag { get; set; }

        /// <summary>
        /// Initializes Thread for Drag Window
        /// </summary>
        /// <param name="xParameters"></param>
        public static void DragDropWindow(object xParameters)
        {
            GingerCore.DragDrop.dragParams p = xParameters as GingerCore.DragDrop.dragParams;
            // Create our context, and install it:
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherSynchronizationContext(
                    Dispatcher.CurrentDispatcher));

            GingerCore.DragDrop.VisualDrag DragDropWindow = new GingerCore.DragDrop.VisualDrag();

            // When the window closes, shut down the dispatcher
            DragDropWindow.Closed += (s, tp) =>
               Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

            DragDropWindow.Show();
            // Start the Dispatcher Processing
            System.Windows.Threading.Dispatcher.Run();
        }

        /// <summary>
        /// invokes the resizing animation of window
        /// </summary>
        /// <param name="changeWidth"></param>
        /// <param name="changeHeight"></param>
        /// <param name="durationMilisec"></param>
        public void AnimateResize(double changeWidth = 0d, double changeHeight = 0d, double durationMilisec = 200.0)
        {
            Storyboard sb = new Storyboard { Duration = new Duration(TimeSpan.FromMilliseconds(durationMilisec)) }; 

            DoubleAnimationUsingKeyFrames daw;
            DoubleAnimationUsingKeyFrames dah;

            // animate window width

            // animate window height
            if (changeHeight != 0.0)
            {
                dah = new DoubleAnimationUsingKeyFrames();
                dah.KeyFrames.Add(new EasingDoubleKeyFrame(this.ActualHeight, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 00))));
                dah.KeyFrames.Add(new EasingDoubleKeyFrame(this.ActualHeight, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec / 2))));
                dah.KeyFrames.Add(new EasingDoubleKeyFrame(this.ActualHeight + changeHeight, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec))));
                dah.Duration = new Duration(TimeSpan.FromMilliseconds(durationMilisec));
                dah.AccelerationRatio = 0.4;
                dah.DecelerationRatio = 0.6;
                Storyboard.SetTarget(dah, this);
                Storyboard.SetTargetProperty(dah, new PropertyPath(Window.HeightProperty));
                sb.Children.Add(dah); 
            }

            if (changeWidth != 0.0)
            {
                daw = new DoubleAnimationUsingKeyFrames();
                daw.KeyFrames.Add(new EasingDoubleKeyFrame(this.ActualWidth, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 00))));
                daw.KeyFrames.Add(new EasingDoubleKeyFrame(this.ActualWidth + changeWidth, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec / 2))));
                daw.Duration = new Duration(TimeSpan.FromMilliseconds(durationMilisec / 2));
                daw.AccelerationRatio = 0.4;
                daw.DecelerationRatio = 0.6;
                this.BeginAnimation(Window.WidthProperty, daw);
                Storyboard.SetTarget(daw, this);
                Storyboard.SetTargetProperty(daw, new PropertyPath(Window.WidthProperty));
                sb.Children.Add(daw);
            }

            DoubleAnimationUsingKeyFrames dax;
            DoubleAnimationUsingKeyFrames day;

            // animate window move in horizontal way
            if (LocationLockHorizontal == AlignmentX.Center || LocationLockHorizontal == AlignmentX.Right)
            {
                dax = new DoubleAnimationUsingKeyFrames();
                dax.KeyFrames.Add(new EasingDoubleKeyFrame(this.Left, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 00))));
                switch (LocationLockHorizontal)
                {
                    case AlignmentX.Center:
                        dax.KeyFrames.Add(new EasingDoubleKeyFrame(this.Left - changeWidth / 2.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec / 2))));
                        break;
                    case AlignmentX.Right:
                        dax.KeyFrames.Add(new EasingDoubleKeyFrame(this.Left - changeWidth, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec / 2))));
                        break;
                }
                dax.Duration = new Duration(TimeSpan.FromMilliseconds(durationMilisec));

                dax.AccelerationRatio = 0.4; dax.DecelerationRatio = 0.6;

                Storyboard.SetTarget(dax, this);
                Storyboard.SetTargetProperty(dax, new PropertyPath(Window.LeftProperty));
                sb.Children.Add(dax); 
            }

            // animate window move vertical 
            if (LocationLockVertical == AlignmentY.Center || LocationLockVertical == AlignmentY.Bottom)
            {
                day = new DoubleAnimationUsingKeyFrames();
                day.KeyFrames.Add(new EasingDoubleKeyFrame(this.Top, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 00))));
                day.KeyFrames.Add(new EasingDoubleKeyFrame(this.Top, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec / 2))));

                switch (LocationLockVertical)
                {
                    case AlignmentY.Center:
                        day.KeyFrames.Add(new EasingDoubleKeyFrame(this.Top - changeWidth / 2.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec))));
                        break;
                    case AlignmentY.Bottom:
                        day.KeyFrames.Add(new EasingDoubleKeyFrame(this.Top - changeHeight, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec))));
                        break;
                }
                day.Duration = new Duration(TimeSpan.FromMilliseconds(durationMilisec));
                day.AccelerationRatio = 0.4; day.DecelerationRatio = 0.6;

                Storyboard.SetTarget(day, this);
                Storyboard.SetTargetProperty(day, new PropertyPath(Window.TopProperty));
                sb.Children.Add(day);
            }
            
            sb.Begin();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.AnimateResize(newWidth - 38 ,newHeight - 38, 1000.0);
            //run mouse binding on separate thread
            Parallel.Invoke(()=>{bindmouse();});
        }

        private void VisualDragWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        private void SetStringWidth()
        {
            var formattedText = new FormattedText(
                dragText,
                System.Globalization.CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(this.tbDragText.FontFamily, this.tbDragText.FontStyle, this.tbDragText.FontWeight, this.tbDragText.FontStretch),
                this.tbDragText.FontSize, Brushes.White);
            newWidth = (int)formattedText.Width + 55;
            newHeight = (int)formattedText.Height + 25;
        }
    }
}