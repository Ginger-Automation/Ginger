using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using Amdocs.Ginger.CoreNET.Drivers.DriversWindow;
using GingerCore;
using GingerCore.Drivers;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Ginger.Drivers.DriversWindows
{
    /// <summary>
    /// Interaction logic for UpdatedAppiumDriverWindow.xaml
    /// </summary>
    public partial class MobileDriverWindow : Window
    {

        IMobileDriverWindow mDriver;
        Agent mAgent;        
        bool mIsMousePressed = false;
        bool mIsItDragAction = false;
        long mRectangleStartPoint_X;
        long mRectangleStartPoint_Y;
        System.Windows.Point mMouseStartPoint;
        System.Windows.Point mMouseEndPoint;
        public BusinessFlow mBF;
       
        public MobileDriverWindow(IMobileDriverWindow driver, Agent agent)
        {
            InitializeComponent();

            mDriver = driver;
            ((DriverBase)mDriver).DriverMessageEvent += MobileDriverWindow_DriverMessageEvent;
            mAgent = agent;

            DesignWindowInitialLook();
        }

        private void MobileDriverWindow_DriverMessageEvent(object sender, DriverMessageEventArgs e)
        {
            switch(e.DriverMessageType)
            {
                case DriverBase.eDriverMessageType.DriverStatusChanged:
                    if (mDriver.IsDeviceConnected)
                    {
                        LoadDeviceScreenshot();
                    }
                    else
                    {
                        this.Close();
                    }
                    break;

                case DriverBase.eDriverMessageType.ActionPerformed:
                    if (xTrackActionsChK.IsChecked == true)
                    {
                        LoadDeviceScreenshot();
                    }
                    break;
            }
        }
        #region Events

        private void DeviceImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DeviceImageCanvas != null)
            {
                //correct the device buttons location
                if (xDeviceImage != null && xDeviceImage.ActualWidth > 0)
                {
                    double emptySpace = ((DeviceImageCanvas.ActualWidth - xDeviceImage.ActualWidth) / 2);
                    Thickness margin = xBackButton.Margin;
                    margin.Right = 10 + emptySpace;
                    xBackButton.Margin = margin;
                    margin = xMenuBtn.Margin;
                    margin.Left = 10 + emptySpace;
                    xMenuBtn.Margin = margin;
                }
            }
        }

        private void DeviceImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mDriver.GetAppType() == eAppType.Web)
            {
                e.Handled = true;
                return;
            }
            try
            {
                mMouseStartPoint = e.GetPosition((System.Windows.Controls.Image)sender);
            }
            catch
            {
                mMouseStartPoint = e.GetPosition((System.Windows.Shapes.Rectangle)sender);
                //convert to image scale
                mMouseStartPoint.X = mMouseStartPoint.X + mRectangleStartPoint_X;
                mMouseStartPoint.Y = mMouseStartPoint.Y + mRectangleStartPoint_Y;
            }
            mIsMousePressed = true;
        }

        private void DeviceImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (mDriver.GetAppType() == eAppType.Web)
            {
                e.Handled = true;
                return;
            }
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Hand;
            if (mIsMousePressed == true)
            {
                //it's a drag
                mIsItDragAction = true;
            }
        }

        private void DeviceImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mDriver.GetAppType() == eAppType.Web)
            {
                e.Handled = true;
                return;
            }

            try
            {
                mMouseEndPoint = e.GetPosition((System.Windows.Controls.Image)sender);
            }
            catch
            {
                mMouseEndPoint = e.GetPosition((System.Windows.Shapes.Rectangle)sender);
                //convert to image scale
                mMouseEndPoint.X = mMouseEndPoint.X + mRectangleStartPoint_X;
                mMouseEndPoint.Y = mMouseEndPoint.Y + mRectangleStartPoint_Y;
            }
            mIsMousePressed = false;

            if (mIsItDragAction == true)
            {
                //do drag
                mIsItDragAction = false;
                DeviceImageMouseDrag(mMouseStartPoint, mMouseEndPoint);
            }
            else
            {
                //do click
                DeviceImageMouseClick(mMouseEndPoint);
            }
        }

        private void DeviceImage_MouseEnter(object sender, MouseEventArgs e)
        {
            if (mDriver.GetAppType() == eAppType.Web)
            {
                e.Handled = true;
                return;
            }

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Hand;
        }

        private void DeviceImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (mDriver.GetAppType() == eAppType.Web)
            {
                e.Handled = true;
                return;
            }

            Mouse.OverrideCursor = null;
        }

        private void xRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDeviceScreenshot(false);
        }


        private void xBackBtn_Click(object sender, RoutedEventArgs e)
        {
            mDriver.PerformBackButtonPress();
            LoadDeviceScreenshot(true, 300);
        }

        private void xHomeBtn_Click(object sender, RoutedEventArgs e)
        {
            mDriver.PerformHomeButtonPress();
            LoadDeviceScreenshot(true, 300);
        }

        private void xMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            mDriver.PerformHomeButtonPress();
            LoadDeviceScreenshot(true, 300);           
        }

        private void ConfigurationsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigurationsBtn.IsChecked == true)
            {
                SetConfigurationsPanelView(true);
            }
            else
            {
                SetConfigurationsPanelView(false);
            }
        }

        private void PinBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PinBtn.IsChecked == true)
            {
                this.Topmost = true;
            }
            else
            {
                this.Topmost = false;
            }
        }

        private void FilterElementsChK_Checked(object sender, RoutedEventArgs e)
        {
            if (FilterElementsChK.IsChecked == true)
                if (FilterElementsTxtbox != null)
                    FilterElementsTxtbox.IsEnabled = true;
                else
                    if (FilterElementsTxtbox != null)
                    FilterElementsTxtbox.IsEnabled = false;
        }
        #endregion Events


        #region Functions
        public void DesignWindowInitialLook()
        {
            try
            {
                this.Title = string.Format("{0} Device View", mAgent.Name);

                //don't track actions if asked in agent
                if (mDriver.GetAutoRefreshDeviceWindowScreenshot())
                {
                    xTrackActionsChK.IsChecked = false;
                }

                //hide optional columns
                this.Width = 300;
                ConfigurationsFrame.Visibility = System.Windows.Visibility.Collapsed;
                ConfigurationsCol.Width = new GridLength(0);

                //show device buttons according to type
                switch (mDriver.GetDevicePlatformType())
                {
                    case eDevicePlatformType.Android:
                        if (mDriver.GetAppType() == eAppType.Web)
                        {
                            //browser mode- show buttons but disabled
                            xBackButton.IsEnabled = false;
                            xMenuBtn.IsEnabled = false;
                            xHomeBtn.IsEnabled = false;
                        }
                        break;
                    case eDevicePlatformType.iOS:
                        //only middle button 
                        xBackButton.Visibility = Visibility.Collapsed;
                        xMenuBtn.Visibility = Visibility.Collapsed;
                        if (mDriver.GetAppType() == eAppType.Web)
                        {
                            xHomeBtn.IsEnabled = false;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while designing the Mobile window initial look", ex);
            }
        }

        public bool LoadDeviceScreenshot(bool wait = false, Int32 waitingTimeInMiliSeconds = 2000)
        {
            //Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            //wait before taking screen shot
            if (wait)//TODO: remove?
            {
                //Thread.Sleep(waitingTimeInMiliSeconds);
                Thread.Sleep(waitingTimeInMiliSeconds * (int.Parse(RefreshWaitingRateCombo.Text.ToString())));
            }
            //take screen shot
            try
            {
                byte[] imageByteArray = mDriver.GetScreenshotImage();
                if (imageByteArray == null || imageByteArray.Length == 0)
                {
                    Reporter.ToUser(eUserMsgKey.MobileRefreshScreenShotFailed, "Failed to get the screenshot image from the device.");
                    mDriver.IsDeviceConnected = false;
                    return false;
                }
                else
                {
                    var image = new BitmapImage();
                    using (var mem = new MemoryStream(imageByteArray))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad; // here
                        image.StreamSource = mem;
                        image.EndInit();
                    }
                    image.Freeze();
                    xDeviceImage.Source = image;
                    return true;
                }               
            }
            catch (Exception ex)
            {               
                Reporter.ToUser(eUserMsgKey.MobileRefreshScreenShotFailed, string.Format("Failed to update the device screenshot, Error:{0}", ex.Message));
                mDriver.IsDeviceConnected = false;
                return false;
            }
        }

        public void ShowActionEfect(bool wait = false, Int32 waitingTimeInMiliSeconds = 2000)
        {
            if (xTrackActionsChK.IsChecked == true)
            {
                LoadDeviceScreenshot(wait, waitingTimeInMiliSeconds);
            }
        }

        private System.Windows.Point GetPointOnMobile(System.Windows.Point pointOnImage)
        {
            System.Windows.Point pointOnMobile = new System.Windows.Point();
            double ratio_X = 1;
            double ratio_Y = 1;

            if (mDriver.GetDevicePlatformType() == eDevicePlatformType.iOS && mDriver.GetAppType() == eAppType.NativeHybride)
            {
                ratio_X = (xDeviceImage.Source.Width / 2) / xDeviceImage.ActualWidth;
                ratio_Y = (xDeviceImage.Source.Height / 2) / xDeviceImage.ActualHeight;
            }
            else
            {
                ratio_X = xDeviceImage.Source.Width / xDeviceImage.ActualWidth;
                ratio_Y = xDeviceImage.Source.Height / xDeviceImage.ActualHeight;
            }

            //pointOnMobile.X = (long)(pointOnImage.X * ratio_X);
            //pointOnMobile.Y = (long)(pointOnImage.Y * ratio_Y);
            pointOnMobile.X = (int)(pointOnImage.X * ratio_X);
            pointOnMobile.Y = (int)(pointOnImage.Y * ratio_Y);

            return pointOnMobile;
        }

        private void DeviceImageMouseClick(System.Windows.Point clickedPoint)
        {
            try
            {
                //calculate clicked X,Y
                System.Windows.Point pointOnMobile = GetPointOnMobile(clickedPoint);
                long pointOnMobile_X = (long)pointOnMobile.X;
                long pointOnMobile_Y = (long)pointOnMobile.Y;

                //click the element
                mDriver.PerformTap(pointOnMobile_X, pointOnMobile_Y);

                //update the screen
                LoadDeviceScreenshot(true, 1000);

            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, string.Format("Failed to perform tap operation, error: '{0}'", ex.Message));
            }
        }

        private void DeviceImageMouseDrag(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            try
            {
                mDriver.PerformDrag(new System.Drawing.Point((int)startPoint.X, (int)startPoint.Y), new System.Drawing.Point((int)endPoint.X, (int)endPoint.Y));
               
                //update the screen
                LoadDeviceScreenshot(true, 300);
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, string.Format("Failed to perform drag operation, error: '{0}'", ex.Message));
            }
        }

        private void SetConfigurationsPanelView(bool show)
        {
            if (show == true)
            {
                ConfigurationsFrame.Visibility = System.Windows.Visibility.Visible;
                configurationsSplitter.Visibility = System.Windows.Visibility.Visible;
                ConfigurationsCol.Width = new GridLength(250);
                this.Width = this.Width + Convert.ToDouble(ConfigurationsCol.Width.ToString());
            }
            else
            {
                ConfigurationsFrame.Visibility = System.Windows.Visibility.Collapsed;
                configurationsSplitter.Visibility = System.Windows.Visibility.Collapsed;
                this.Width = this.Width - ConfigurationsCol.ActualWidth;
                ConfigurationsCol.Width = new GridLength(0);
            }
        }
        #endregion Functions
    }
}
