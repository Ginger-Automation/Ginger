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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using Amdocs.Ginger.CoreNET.Drivers.DriversWindow;
using Amdocs.Ginger.UserControls;
using Ginger.Agents;
using Ginger.UserControls;
using Ginger.UserControlsLib.TextEditor;
using GingerCore;
using GingerCore.Drivers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        bool mIsMousePressed;
        bool mIsItDragAction;
        bool mSelfClosing;
        bool mUserClosing;
        long mRectangleStartPoint_X;
        long mRectangleStartPoint_Y;
        System.Windows.Point mMouseStartPoint;
        System.Windows.Point mMouseEndPoint;
        public BusinessFlow mBF;
        eAutoScreenshotRefreshMode mDeviceAutoScreenshotRefreshMode;
        bool mWindowIsOpen = true;
        bool IsRecording = false;

       
        ObservableList <DeviceInfo> mDeviceDetails = [];

        public MobileDriverWindow(DriverBase driver, Agent agent)
        {
            InitializeComponent();
            
            mDriver = (IMobileDriverWindow)driver;
            mAgent = agent;

            ((DriverBase)mDriver).DriverMessageEvent += MobileDriverWindow_DriverMessageEvent;
            ((DriverBase)mDriver).SpyingElementEvent += CurrentMousePosToSpy;

            //Setting up the detail and metic grids
            SetDeviceDetailsGridView();
            SetDeviceMetricsGridView();
        }
       
        private async void RefreshDetailsTable(object sender, RoutedEventArgs e)
        {
            await this.Dispatcher.InvokeAsync(async () =>
            {
                xDeviceDetailsGrid.Visibility = Visibility.Collapsed;
                xDetailsLoadingPnl.Visibility = Visibility.Visible;
                await SetDeviceDetailsGridData();
                xDeviceDetailsGrid.Visibility = Visibility.Visible;
                xDetailsLoadingPnl.Visibility = Visibility.Collapsed;
            });

        }
        private async void RefreshMetricsTable(object sender, RoutedEventArgs e)
        {
            await this.Dispatcher.InvokeAsync(async () =>
            {
                xDeviceMetricsGrid.Visibility = Visibility.Collapsed;
                xMetricsLoadingPnl.Visibility = Visibility.Visible;
                await SetDeviceMetricsGridData();
                xDeviceMetricsGrid.Visibility = Visibility.Visible;
                xMetricsLoadingPnl.Visibility = Visibility.Collapsed;
            });

        }

        private void SetDeviceDetailsGridView()
        {
            //# Default View

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(DeviceInfo.DetailName), Header = "Name", WidthWeight = 4.5, ReadOnly = true },
                new GridColView() { Field = nameof(DeviceInfo.DetailValue), Header = "Value", WidthWeight = 7, ReadOnly = true },
                new GridColView() { Field = nameof(DeviceInfo.ExtraInfo), Header = "Extra Info", WidthWeight = 2.5, MaxWidth = 70, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xWindowGrid.Resources["ExtraInfo"] },
            ]
            };

            xDeviceDetailsGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshDetailsTable));

            xDeviceDetailsGrid.SetAllColumnsDefaultView(view);
            xDeviceDetailsGrid.InitViewItems();
            xDeviceDetailsGrid.SetTitleLightStyle = true;
        }

        private void SetDeviceMetricsGridView()
        {
            //# Default View

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(DeviceInfo.DetailName), Header = "Name", WidthWeight = 4.5, ReadOnly = true },
                new GridColView() { Field = nameof(DeviceInfo.DetailValue), Header = "Value", WidthWeight = 7, ReadOnly = true },
                new GridColView() { Field = nameof(DeviceInfo.ExtraInfo), Header = "Extra Info", WidthWeight = 2.6, MaxWidth = 70, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xWindowGrid.Resources["ExtraInfo"] },
            ]
            };

            xDeviceMetricsGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshMetricsTable));

            xDeviceMetricsGrid.SetAllColumnsDefaultView(view);
            xDeviceMetricsGrid.InitViewItems();
            xDeviceMetricsGrid.SetTitleLightStyle = true;
        }

        private object CurrentMousePosToSpy()
        {
            Point mousePos = new Point(-1, -1);

            Dispatcher.Invoke(() => mousePos = Mouse.GetPosition(xDeviceScreenshotImage));

            System.Drawing.Point pointOnAppScreen = new System.Drawing.Point(-1, -1);

            this.Dispatcher.Invoke(() =>
                pointOnAppScreen = ((DriverBase)mDriver).GetPointOnAppWindow(new System.Drawing.Point((int)mousePos.X, (int)mousePos.Y),
                    xDeviceScreenshotImage.Source.Width, xDeviceScreenshotImage.Source.Height, xDeviceScreenshotImage.ActualWidth, xDeviceScreenshotImage.ActualHeight)
            );

            return pointOnAppScreen;
        }

        private void HighlightElementEvent(Amdocs.Ginger.Common.UIElement.ElementInfo elementInfo)
        {
            System.Drawing.Point ElementStartPoint = new System.Drawing.Point(elementInfo.X, elementInfo.Y);
            System.Drawing.Point ElementMaxPoint = new System.Drawing.Point(elementInfo.X + elementInfo.Width, elementInfo.Y + elementInfo.Height);

            this.Dispatcher.Invoke(() =>
                    DrawRectangle(ElementStartPoint, ElementMaxPoint, elementInfo)
            );
        }

        private void UnHighlightElementEvent()
        {
            this.Dispatcher.Invoke(() => RemoveRectangle());
        }

        private void DrawRectangle(System.Drawing.Point ElementStartPoint, System.Drawing.Point ElementMaxPoint, Amdocs.Ginger.Common.UIElement.ElementInfo elementInfo)
        {
            ((DriverBase)mDriver).SetRectangleProperties(ref ElementStartPoint, ref ElementMaxPoint, xDeviceScreenshotImage.Source.Width, xDeviceScreenshotImage.Source.Height,
                xDeviceScreenshotImage.ActualWidth, xDeviceScreenshotImage.ActualHeight, elementInfo);

            xHighlighterBorder.SetValue(Canvas.LeftProperty, ElementStartPoint.X + ((xDeviceScreenshotCanvas.ActualWidth - xDeviceScreenshotImage.ActualWidth) / 2));
            xHighlighterBorder.SetValue(Canvas.TopProperty, ElementStartPoint.Y + ((xDeviceScreenshotCanvas.ActualHeight - xDeviceScreenshotImage.ActualHeight) / 2));
            xHighlighterBorder.Margin = new Thickness(0);
            xHighlighterBorder.Width = (ElementMaxPoint.X - ElementStartPoint.X);
            int calcHeight = (ElementMaxPoint.Y - ElementStartPoint.Y);
            if (calcHeight < 0)
            {
                calcHeight = 0 - calcHeight;
            }
            xHighlighterBorder.Height = calcHeight;
            xHighlighterBorder.Visibility = Visibility.Visible;
        }

        private void RemoveRectangle()
        {
            xHighlighterBorder.Width = 0;
            xHighlighterBorder.Height = 0;
            xHighlighterBorder.Visibility = Visibility.Collapsed;
        }
        public void UpdateRotateIcon()
        {
            Dispatcher.Invoke(() =>
            {
                SetOrientationButton();
            });
        }
        public void UpdateRecordingImage(bool ShowRecordIcon)
        {
            Dispatcher.Invoke(() =>
            {
                if (ShowRecordIcon)
                {
                    xRecordingImage.Visibility = Visibility.Visible;
                }
                else
                {
                    xRecordingImage.Visibility = Visibility.Collapsed;
                }
            });
        }

        #region Events
        private async void MobileDriverWindow_DriverMessageEvent(object sender, DriverMessageEventArgs e)
        {
            switch (e.DriverMessageType)
            {
                case DriverBase.eDriverMessageType.DriverStatusChanged:
                    if (mDriver.IsDeviceConnected)
                    {
                        await this.Dispatcher.InvokeAsync(async () =>
                        {
                            xMessageLbl.Content = "Loading Device Screenshot...";
                            xDeviceSectionMainPnl.Background = new SolidColorBrush(Colors.Transparent);
                            xMessagePnl.Visibility = Visibility.Collapsed;

                            xDeviceScreenshotCanvas.Visibility = Visibility.Visible;
                            await RefreshDeviceScreenshotAsync();

                            SetOrientationButton();
                            xSwipeBtn.Visibility = Visibility.Visible;
                            xCordBtn.Visibility = Visibility.Visible;
                            if (mDriver.IsUftLabDevice)
                            {
                                xExternalViewBtn.Visibility = Visibility.Visible;
                            }
                            DoContinualDeviceScreenshotRefresh();
                            Dictionary<string, object> mDeviceGeneralInfo;
                            mDeviceGeneralInfo = mDriver.GetDeviceGeneralInfo();
                            SetTitle(mDeviceGeneralInfo);
                            AlloworDisableControls(true);
                            xPinBtn_Click(null, null);
                            AdjustWindowSize(eImageChangeType.DoNotChange, true);
                        });

                    }
                    else
                    {
                        if (!mSelfClosing)
                        {
                            DoSelfClose();
                        }
                    }
                    break;

                case DriverBase.eDriverMessageType.ActionPerformed:
                    if (mDeviceAutoScreenshotRefreshMode == eAutoScreenshotRefreshMode.PostOperation)
                    {
                        await RefreshDeviceScreenshotAsync(100);
                    }
                    break;

                case DriverBase.eDriverMessageType.RotateEvent:                    
                    UpdateRotateIcon(); 
                    break;

                case DriverBase.eDriverMessageType.RecordingEvent:
                    IsRecording = sender != null && (bool)sender;
                    UpdateRecordingImage(IsRecording);
                    break;

                case DriverBase.eDriverMessageType.HighlightElement:
                    if (sender is Amdocs.Ginger.Common.UIElement.ElementInfo)
                    {
                        HighlightElementEvent(sender as Amdocs.Ginger.Common.UIElement.ElementInfo);
                    }
                    break;

                case DriverBase.eDriverMessageType.UnHighlightElement:
                    UnHighlightElementEvent();
                    break;
            }
        }

        private void SetTitle(Dictionary<string, object> mDeviceGeneralInfo)
        {
            try
            {
                object value, platformVersion;
                if (mDeviceGeneralInfo.Count > 0)
                {
                    if (mDeviceGeneralInfo.TryGetValue("manufacturer", out value) || mDeviceGeneralInfo.TryGetValue("model", out value))
                    {
                        if ((string)value != "iPhone")
                        {
                            this.Title = " Android";
                        }
                        else
                        {
                            this.Title = " iOS";
                        }
                    }

                    if (mDeviceGeneralInfo.TryGetValue("platformVersion", out platformVersion))
                    {
                        this.Title += " [" + (string)platformVersion + "] ";
                    }

                    if (mDeviceGeneralInfo.TryGetValue("manufacturer", out value))
                    {
                        this.Title += (string)value;
                    }

                    if (mDeviceGeneralInfo.TryGetValue("model", out value))
                    {
                        this.Title += " " + (string)value;
                    }
                    if (!string.IsNullOrEmpty(mDriver.GetDeviceUDID()))
                    {
                        this.Title += " [" + mDriver.GetDeviceUDID() + "]";
                    }
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Failed to Set full Title, created title:" + this.Title, ex);
            }
        }

        private async Task<bool> SetDeviceMetricsGridData()
        {
            string mDeviceNetworkInfo = string.Empty;
            Dictionary<string, string> mDeviceMemoryInfo = null;
            Dictionary<string, string> mDeviceCPUInfo = null;

            await Task.Run(() =>
            {
                try
                {
                    mDeviceCPUInfo = mDriver.GetDeviceCPUInfo();
                    mDeviceMemoryInfo = mDriver.GetDeviceMemoryInfo();
                    mDeviceNetworkInfo = mDriver.GetDeviceNetworkInfo()?.Result;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to Get Device Info", ex);
                }
            });


            mDeviceDetails = new ObservableList<DeviceInfo>(mDeviceDetails.Where(x => x.Category == DeviceInfo.eDeviceInfoCategory.Detail).ToList());

            #region setting device metrics grid
            if (mDeviceCPUInfo != null && mDeviceCPUInfo.Count > 0)
            {
                double userCpuUsage = 0, kernelCpuUsage = 0;
                double.TryParse(mDeviceCPUInfo["user"], out userCpuUsage);
                userCpuUsage = Math.Round(userCpuUsage, 2);
                double.TryParse(mDeviceCPUInfo["kernel"], out kernelCpuUsage);
                kernelCpuUsage = Math.Round(kernelCpuUsage, 2);

                string cpuUsage = (userCpuUsage + kernelCpuUsage).ToString() + " %";
                mDeviceDetails.Add(new DeviceInfo("App CPU Usage:", cpuUsage, DeviceInfo.eDeviceInfoCategory.Metric, DictionaryToString(mDeviceCPUInfo)));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("App CPU Usage:", "N/A", DeviceInfo.eDeviceInfoCategory.Metric));
            }

            if (mDeviceMemoryInfo != null && mDeviceMemoryInfo.Count > 0)
            {
                double totalPss = 0;
                double.TryParse(mDeviceMemoryInfo["totalPss"], out totalPss);
                totalPss = Math.Round(totalPss / 1024, 2);
                string ramUsage = (totalPss).ToString() + " Mb";
                mDeviceDetails.Add(new DeviceInfo("App RAM Usage:", ramUsage, DeviceInfo.eDeviceInfoCategory.Metric, DictionaryToString(mDeviceMemoryInfo)));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("App RAM Usage:", "N/A", DeviceInfo.eDeviceInfoCategory.Metric));
            }

            if (!string.IsNullOrEmpty(mDeviceNetworkInfo))
            {
                mDeviceDetails.Add(new DeviceInfo("Networks:", "Press info to view", DeviceInfo.eDeviceInfoCategory.Metric, mDeviceNetworkInfo));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("Networks:", "N/A", DeviceInfo.eDeviceInfoCategory.Metric));
            }
            #endregion
            xDeviceMetricsGrid.DataSourceList = new ObservableList<DeviceInfo>(mDeviceDetails.Where(x => x.Category == DeviceInfo.eDeviceInfoCategory.Metric).ToList());
            return false;
        }

        private string DictionaryToString(Dictionary<string, string> dict)
        {
            string returnString = string.Empty;
            foreach (KeyValuePair<string, string> entry in dict)
            {
                returnString = new StringBuilder(returnString + entry.Key + ": " + entry.Value + ", ").ToString();
            }
            returnString = returnString[..^2];
            return returnString;
        }

        private async Task<bool> SetDeviceDetailsGridData()
        {

            bool isPhysicalDevice = false;
            Dictionary<string, object> mDeviceGeneralInfo = null;
            Dictionary<string, string> mDeviceBatteryInfo = null;
            Dictionary<string, string> mActivityAndPackageInfo = null;

            await Task.Run(() =>
            {
                try
                {
                    mDeviceGeneralInfo = mDriver.GetDeviceGeneralInfo();
                    if (mDriver.GetDevicePlatformType() == eDevicePlatformType.iOS)
                    {
                        object value;
                        mDeviceGeneralInfo.TryGetValue("isSimulator", out value);
                        if (value is bool)
                        {
                            isPhysicalDevice = !(bool)value;
                        }
                    }
                    else
                    {
                        isPhysicalDevice = mDriver.IsRealDeviceAsync().Result;
                    }

                    mDeviceBatteryInfo = mDriver.GetDeviceBatteryInfo();
                    mActivityAndPackageInfo = mDriver.GetDeviceActivityAndPackage();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to Get Device Details", ex);
                }
            });

            mDeviceDetails = new ObservableList<DeviceInfo>(mDeviceDetails.Where(x => x.Category == DeviceInfo.eDeviceInfoCategory.Metric).ToList());

            #region setting device details grid
            object value, apiVersion, platformVersion;
            string battery;

            if (!string.IsNullOrEmpty(mDriver.GetDeviceUDID()))
            {
                mDeviceDetails.Add(new DeviceInfo("Device ID:", mDriver.GetDeviceUDID(), DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("Device ID:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
            }

            //Check if device is real device or emulator
            if (isPhysicalDevice)
            {
                mDeviceDetails.Add(new DeviceInfo("Physical/emulator:", "Physical", DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("Physical/emulator:", "Emulator", DeviceInfo.eDeviceInfoCategory.Detail));
            }

            if (mDriver.GetDevicePlatformType() == eDevicePlatformType.iOS)
            {
                mDeviceDetails.Add(new DeviceInfo("Manufacture:", "Apple", DeviceInfo.eDeviceInfoCategory.Detail));
                mDeviceDetails.Add(new DeviceInfo("Brand:", "Apple", DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                if ((mDeviceGeneralInfo.TryGetValue("manufacturer", out value) && !string.IsNullOrEmpty((string)value)))
                {
                    mDeviceDetails.Add(new DeviceInfo("Manufacture:", (string)value, DeviceInfo.eDeviceInfoCategory.Detail));
                }
                else
                {
                    mDeviceDetails.Add(new DeviceInfo("Manufacture:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));

                }

                if (mDeviceGeneralInfo.TryGetValue("brand", out value) && !string.IsNullOrEmpty((string)value))
                {
                    mDeviceDetails.Add(new DeviceInfo("Brand:", (string)value, DeviceInfo.eDeviceInfoCategory.Detail));
                }
                else
                {
                    mDeviceDetails.Add(new DeviceInfo("Brand:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
                }
            }



            if (mDeviceGeneralInfo.TryGetValue("model", out value) && !string.IsNullOrEmpty((string)value))
            {
                mDeviceDetails.Add(new DeviceInfo("Model:", (string)value, DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("Model:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
            }

            if (mDriver.GetDevicePlatformType() == eDevicePlatformType.iOS)
            {
                mDeviceDetails.Add(new DeviceInfo("OS Type:", "iOS", DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                if (mDeviceGeneralInfo.TryGetValue("manufacturer", out value) && !string.IsNullOrEmpty((string)value))
                {
                    if ((string)value != "apple")
                    {
                        mDeviceDetails.Add(new DeviceInfo("OS Type:", "Android", DeviceInfo.eDeviceInfoCategory.Detail));
                    }
                    else
                    {
                        mDeviceDetails.Add(new DeviceInfo("OS Type:", "iOS", DeviceInfo.eDeviceInfoCategory.Detail));
                    }
                }
                else
                {
                    mDeviceDetails.Add(new DeviceInfo("OS Type:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
                }
            }


            if (mDeviceGeneralInfo.TryGetValue("platformVersion", out platformVersion))
            {
                string version;
                version = (string)platformVersion;
                if (mDeviceGeneralInfo.TryGetValue("apiVersion", out apiVersion))
                {
                    version += " [" + (string)apiVersion + "]";
                    mDeviceDetails.Add(new DeviceInfo("OS Version:", version, DeviceInfo.eDeviceInfoCategory.Detail));
                }
                if (string.IsNullOrEmpty((string)platformVersion) || string.IsNullOrEmpty((string)apiVersion))
                {
                    mDeviceDetails.Add(new DeviceInfo("OS Version:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
                }
            }


            if (mDriver.GetDevicePlatformType() == eDevicePlatformType.iOS)
            {
                mDeviceDetails.Add(new DeviceInfo("Package:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
                mDeviceDetails.Add(new DeviceInfo("Activity:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                string activity, package;
                mActivityAndPackageInfo.TryGetValue("Activity", out activity);
                mActivityAndPackageInfo.TryGetValue("Package", out package);
                mDeviceDetails.Add(new DeviceInfo("Package:", package, DeviceInfo.eDeviceInfoCategory.Detail, package));
                mDeviceDetails.Add(new DeviceInfo("Activity:", activity, DeviceInfo.eDeviceInfoCategory.Detail, activity));
            }


            if (mDriver.GetDevicePlatformType() == eDevicePlatformType.iOS && mDeviceGeneralInfo.TryGetValue("currentLocale", out value))
            {
                mDeviceDetails.Add(new DeviceInfo("Language:", ((string)value).Replace("_", "/"), DeviceInfo.eDeviceInfoCategory.Detail));

            }
            else
            {
                if (mDeviceGeneralInfo.TryGetValue("locale", out value) && !string.IsNullOrEmpty((string)value))
                {
                    mDeviceDetails.Add(new DeviceInfo("Language:", ((string)value).Replace("_", "/"), DeviceInfo.eDeviceInfoCategory.Detail));
                }
                else
                {
                    mDeviceDetails.Add(new DeviceInfo("Language:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
                }
            }



            if (mDeviceGeneralInfo.TryGetValue("timeZone", out value) && !string.IsNullOrEmpty((string)value))
            {
                mDeviceDetails.Add(new DeviceInfo("Time zone:", (string)value, DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("Time zone:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
            }

            if (mDeviceGeneralInfo.TryGetValue("realDisplaySize", out value) && !string.IsNullOrEmpty((string)value))
            {
                mDeviceDetails.Add(new DeviceInfo("Screen resolution:", (string)value, DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("Screen resolution:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
            }

            if (mDeviceGeneralInfo.TryGetValue("displayDensity", out value) && !string.IsNullOrEmpty(value.ToString()))
            {
                mDeviceDetails.Add(new DeviceInfo("Display density:", value.ToString(), DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("Display density:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
            }

            if (mDeviceGeneralInfo.TryGetValue("carrierName", out value) && !string.IsNullOrEmpty((string)value))
            {
                mDeviceDetails.Add(new DeviceInfo("Carrier name:", (string)value, DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("Carrier name:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
            }

            if (mDeviceGeneralInfo.TryGetValue("bluetooth", out value) && ((Dictionary<string, object>)value).Count > 0)
            {
                mDeviceDetails.Add(new DeviceInfo("Bluetooth:", ((Dictionary<string, object>)value)["state"].ToString(), DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("Bluetooth:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
            }


            if (mDeviceBatteryInfo.TryGetValue("power", out battery))
            {
                mDeviceDetails.Add(new DeviceInfo("Device Battery:", battery + "%", DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("Battery:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
            }


            if (string.IsNullOrEmpty(mAgent.Name))
            {
                mDeviceDetails.Add(new DeviceInfo("Ginger Agent:", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                mDeviceDetails.Add(new DeviceInfo("Ginger Agent:", mAgent.Name, DeviceInfo.eDeviceInfoCategory.Detail, "Has extra info"));
            }
            #endregion

            xDeviceDetailsGrid.DataSourceList = new ObservableList<DeviceInfo>(mDeviceDetails.Where(x => x.Category == DeviceInfo.eDeviceInfoCategory.Detail));
            return false;
        }

        private void xDeviceScreenshotImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (mSwipeIsOn)
            {
                SetSwipeButtonsPosition();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (mSwipeIsOn)
            {
                SetSwipeButtonsPosition();
            }
        }

        DateTime mClickStartTime;
        DateTime mMoveStartTime;
        private void xDeviceScreenshotImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mClickStartTime = DateTime.Now;
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

        private void xDeviceScreenshotImage_MouseMove(object sender, MouseEventArgs e)
        {
            mMoveStartTime = DateTime.Now;
            if (mIsMousePressed == true)
            {
                //it's a drag
                mIsItDragAction = true;
            }

            if (mCordIsOn)
            {
                Point mousePoint;
                try
                {
                    mousePoint = e.GetPosition((System.Windows.Controls.Image)sender);
                }
                catch
                {
                    mousePoint = e.GetPosition((System.Windows.Shapes.Rectangle)sender);
                    //convert to image scale
                    mousePoint.X = mMouseEndPoint.X + mRectangleStartPoint_X;
                    mousePoint.Y = mMouseEndPoint.Y + mRectangleStartPoint_Y;
                }
                Point pointOnMobile = GetPointOnMobile(mousePoint);
                xXcord.Content = "X: " + (long)pointOnMobile.X;
                xYcord.Content = "Y: " + (long)pointOnMobile.Y;
            }
        }


        private void xDeviceScreenshotImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DateTime clickEndTime = DateTime.Now;
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

            TimeSpan pressDuration;

            if (mIsItDragAction == true)
            {
                //do drag
                mIsItDragAction = false;
                pressDuration = mMoveStartTime - mClickStartTime;
                TimeSpan dragDuration = clickEndTime - mMoveStartTime;
                DeviceScreenshotImageMouseDragAsync(mMouseStartPoint, mMouseEndPoint, pressDuration, dragDuration);
            }
            else
            {
                pressDuration = clickEndTime - mClickStartTime;
                if (pressDuration.TotalSeconds > 1)
                {
                    //do long press
                    DeviceScreenshotImageMouseClickAsync(mMouseEndPoint, true, pressDuration);
                }
                else
                {
                    //do click
                    DeviceScreenshotImageMouseClickAsync(mMouseEndPoint);
                }
            }
        }

        private void xDeviceScreenshotImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                PerformScreenSwipe(eSwipeSide.Up, 0.25, TimeSpan.FromMilliseconds(200));
            }
            else
            {
                PerformScreenSwipe(eSwipeSide.Down, 0.25, TimeSpan.FromMilliseconds(200));
            }
        }

        private void xRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshDeviceScreenshotAsync();
        }

        bool mConfigIsOn = false;
        private void xConfigurationsBtn_Click(object sender, RoutedEventArgs e)
        {
            SetConfigurationsPanelView(!mConfigIsOn);
        }

        bool mMeticsIsOn = false;
        private async void xMetricsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mDriver.IsDeviceConnected)
            {
                //Setting the tabs
                if (!mMeticsIsOn)
                {
                    SetTabsColumnView(eTabsViewMode.DetailsAndMetrics);
                }
                else
                {
                    SetTabsColumnView(eTabsViewMode.None);
                }

                if (mMeticsIsOn)
                {
                    await this.Dispatcher.InvokeAsync(async () =>
                    {
                        //Loading the device details and metrics
                        xDeviceDetailsGrid.Visibility = Visibility.Collapsed;
                        xDeviceMetricsGrid.Visibility = Visibility.Collapsed;

                        xDetailsLoadingPnl.Visibility = Visibility.Visible;
                        xMetricsLoadingPnl.Visibility = Visibility.Visible;

                        await SetDeviceDetailsGridData();
                        xDeviceDetailsGrid.Visibility = Visibility.Visible;
                        xDetailsLoadingPnl.Visibility = Visibility.Collapsed;

                        await SetDeviceMetricsGridData();
                        xDeviceMetricsGrid.Visibility = Visibility.Visible;
                        xMetricsLoadingPnl.Visibility = Visibility.Collapsed;
                    });
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.MobileDriverNotConnected);
            }
        }


        bool mPinIsOn = false;
        private void xPinBtn_Click(object sender, RoutedEventArgs e)
        {
            mPinIsOn = !mPinIsOn;

            if (mPinIsOn)
            {
                //dock
                this.Topmost = true;
                xPinBtn.ButtonStyle = FindResource("$ImageButtonStyle_Pink") as Style;
                xPinBtn.ToolTip = "Undock Window";
            }
            else
            {
                //undock
                this.Topmost = false;
                xPinBtn.ButtonStyle = FindResource("$ImageButtonStyle") as Style;
                xPinBtn.ToolTip = "Dock Window";
            }
        }

        private async void xOrientationBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mDriver.GetOrientation() == eDeviceOrientation.Landscape)
                {
                    mDriver.SwitchToPortrait();
                }
                else
                {
                    mDriver.SwitchToLandscape();
                }
                SetOrientationButton();
                await RefreshDeviceScreenshotAsync();
                AdjustWindowSize(eImageChangeType.DoNotChange, true);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Change device orientation failed, Error: " + ex.Message);
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Seems like changing orientation is not possible in current device state, make sure the loaded application supports it.");
            }
        }

        bool mSwipeIsOn;
        private void xSwipeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mSwipeIsOn)
            {
                //Turn off Swipe
                xSwipeDown.Visibility = Visibility.Collapsed;
                xSwipeUp.Visibility = Visibility.Collapsed;
                xSwipeRight.Visibility = Visibility.Collapsed;
                xSwipeLeft.Visibility = Visibility.Collapsed;
                xSwipeBtn.ButtonStyle = FindResource("$ImageButtonStyle") as Style;
                xSwipeBtn.ToolTip = "Perform Swipe";
            }
            else
            {
                //Allow Swipe
                SetSwipeButtonsPosition();
                xSwipeBtn.ButtonStyle = FindResource("$ImageButtonStyle_Pink") as Style;
                xSwipeBtn.ToolTip = "Hide Swipe Buttons";
            }

            mSwipeIsOn = !mSwipeIsOn;
        }

        private void SetSwipeButtonsPosition()
        {
            xSwipeDown.Visibility = Visibility.Visible;
            xSwipeDown.SetValue(Canvas.LeftProperty, xDeviceScreenshotCanvas.ActualWidth / 2 - 15);
            xSwipeUp.Visibility = Visibility.Visible;
            xSwipeUp.SetValue(Canvas.LeftProperty, xDeviceScreenshotCanvas.ActualWidth / 2 - 15);
            xSwipeRight.Visibility = Visibility.Visible;
            xSwipeRight.SetValue(Canvas.TopProperty, xDeviceScreenshotCanvas.ActualHeight / 2 - 30);
            xSwipeLeft.Visibility = Visibility.Visible;
            xSwipeLeft.SetValue(Canvas.TopProperty, xDeviceScreenshotCanvas.ActualHeight / 2 - 30);
        }

        bool mCordIsOn;
        private void xCordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mCordIsOn)
            {
                xCordsStack.Visibility = Visibility.Collapsed;
                xCordBtn.ButtonStyle = FindResource("$ImageButtonStyle") as Style;
                xCordBtn.ToolTip = "Show Mouse Coordinates";
            }
            else
            {
                //Allow Swipe
                xCordsStack.Visibility = Visibility.Visible;
                xCordBtn.ButtonStyle = FindResource("$ImageButtonStyle_Pink") as Style;
                xCordBtn.ToolTip = "Hide Mouse Coordinates"; ;
            }

            mCordIsOn = !mCordIsOn;
        }

        private void xBackBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mDriver.PerformBackButtonPress();
                if (mDeviceAutoScreenshotRefreshMode == eAutoScreenshotRefreshMode.PostOperation)
                {
                    RefreshDeviceScreenshotAsync(100);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Operation failed, Error: " + ex.Message);
            }
        }

        private void xHomeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mDriver.GetAppType() == eAppType.Web)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation not supported for this mobile OS or application type.");
                return;
            }

            try
            {
                mDriver.PerformHomeButtonPress();
                if (mDeviceAutoScreenshotRefreshMode == eAutoScreenshotRefreshMode.PostOperation)
                {
                    RefreshDeviceScreenshotAsync(100);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Operation failed, Error: " + ex.Message);
            }
        }

        private void xMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mDriver.GetAppType() == eAppType.Web)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation not supported for this mobile OS or application type.");
                return;
            }

            try
            {
                mDriver.PerformMenuButtonPress();
                if (mDeviceAutoScreenshotRefreshMode == eAutoScreenshotRefreshMode.PostOperation)
                {
                    RefreshDeviceScreenshotAsync(100);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Operation failed, Error: " + ex.Message);
            }
        }

        private void xVolumUpPnl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mDriver.GetAppType() == eAppType.Web)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation not supported for this mobile OS or application type.");
                return;
            }
            try
            {
                mDriver.PerformVolumeButtonPress(eVolumeOperation.Up);
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Operation failed, Error: " + ex.Message);
            }
        }

        private void xVolumDownPnl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mDriver.GetAppType() == eAppType.Web)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation not supported for this mobile OS or application type.");
                return;
            }
            try
            {
                mDriver.PerformVolumeButtonPress(eVolumeOperation.Down);
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Operation failed, Error: " + ex.Message);
            }
        }

        bool lockDone;
        private void xLockPnl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!lockDone)
                {
                    mDriver.PerformLockButtonPress(eLockOperation.Lock);
                    lockDone = true;
                }
                else
                {
                    mDriver.PerformLockButtonPress(eLockOperation.UnLock);
                    lockDone = false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Operation failed, Error: " + ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mUserClosing = true;

            if (!mSelfClosing)
            {
                try
                {
                    mAgent.AgentOperations.Close();
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to close Agent, Error: " + ex.Message);
                }
            }

            mWindowIsOpen = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitWindowLook();
        }

        private void xTrackActionsChK_Unchecked(object sender, RoutedEventArgs e)
        {
            mDeviceAutoScreenshotRefreshMode = eAutoScreenshotRefreshMode.Live;
            if (this.IsLoaded)
            {
                xRefreshButton.Visibility = Visibility.Collapsed;
            }
        }

        private void xContinualRdBtn_Checked(object sender, RoutedEventArgs e)
        {
            mDeviceAutoScreenshotRefreshMode = eAutoScreenshotRefreshMode.Live;
            if (this.IsLoaded)
            {
                xRefreshWaitingRatePnl.Visibility = Visibility.Visible;
                xRefreshButton.Visibility = Visibility.Collapsed;
            }
        }

        private void xPostOperationRdBtn_Checked(object sender, RoutedEventArgs e)
        {
            mDeviceAutoScreenshotRefreshMode = eAutoScreenshotRefreshMode.PostOperation;
            if (this.IsLoaded)
            {
                xRefreshWaitingRatePnl.Visibility = Visibility.Visible;
                xRefreshButton.Visibility = Visibility.Visible;
            }
        }

        private void xDisabledRdBtn_Checked(object sender, RoutedEventArgs e)
        {
            mDeviceAutoScreenshotRefreshMode = eAutoScreenshotRefreshMode.Disabled;
            if (this.IsLoaded)
            {
                xRefreshWaitingRatePnl.Visibility = Visibility.Collapsed;
                xRefreshButton.Visibility = Visibility.Visible;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (mDriver.GetAppType() == eAppType.Web)
                {
                    Task.Run(() =>
                    {
                        string key = e.Key.ToString();

                        if (key.Contains("NumPad"))
                        {
                            key = key.Replace("NumPad", "");
                        }

                        if (e.Key == Key.Back)
                        {
                            mDriver.PerformSendKey("\b");
                        }
                        else if (key.Length == 2 && key.Contains("D"))
                        {
                            mDriver.PerformSendKey(key.TrimStart('D'));
                        }
                        else if (!(key.Contains("Ctrl") || key.Contains("Shift")))
                        {
                            mDriver.PerformSendKey(key);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Failed to perform send key to mobile device", ex);
            }
        }

        private void xSwipeLeft_Click(object sender, RoutedEventArgs e)
        {
            PerformScreenSwipe(eSwipeSide.Left, 1, TimeSpan.FromMilliseconds(200));
        }

        private void xSwipeRight_Click(object sender, RoutedEventArgs e)
        {
            PerformScreenSwipe(eSwipeSide.Right, 1, TimeSpan.FromMilliseconds(200));
        }

        private void xSwipeUp_Click(object sender, RoutedEventArgs e)
        {
            PerformScreenSwipe(eSwipeSide.Up, 1, TimeSpan.FromMilliseconds(200));
        }

        private void xSwipeUp_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PerformScreenSwipe(eSwipeSide.Up, 1.5, TimeSpan.FromMilliseconds(200));
        }

        private void xSwipeDown_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PerformScreenSwipe(eSwipeSide.Down, 1.5, TimeSpan.FromMilliseconds(200));
        }

        private void xSwipeDown_Click(object sender, RoutedEventArgs e)
        {
            PerformScreenSwipe(eSwipeSide.Down, 1, TimeSpan.FromMilliseconds(200));
        }

        private void xDeviceSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mDriver.OpenDeviceSettings();
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Operation failed, Error: " + ex.Message);
            }
        }

        private void xClearHighlightsBtn_Click(object sender, RoutedEventArgs e)
        {
            UnHighlightElementEvent();
        }
        #endregion Events


        #region Functions
        public void InitWindowLook()
        {
            try
            {
                ImageMakerControl image = new ImageMakerControl();
                //General
                if (mDriver.GetDevicePlatformType() == eDevicePlatformType.Android)
                {
                    this.Icon = ImageMakerControl.GetImageSource(eImageType.Android);
                }
                else
                {
                    this.Icon = ImageMakerControl.GetImageSource(eImageType.Ios);
                }
                xMessageLbl.Content = "Connecting to Device...";

                //Configurations & Metrics
                SetTabsColumnView(eTabsViewMode.None);

                //set refresh mode by what configured on driver      
                mDeviceAutoScreenshotRefreshMode = mDriver.DeviceAutoScreenshotRefreshMode;
                switch (mDeviceAutoScreenshotRefreshMode)
                {
                    case eAutoScreenshotRefreshMode.Live:
                        xLiveRdBtn.IsChecked = true;
                        break;
                    case eAutoScreenshotRefreshMode.PostOperation:
                        xPostOperationRdBtn.IsChecked = true;
                        break;
                    case eAutoScreenshotRefreshMode.Disabled:
                        xDisabledRdBtn.IsChecked = true;
                        break;
                }

                xTabsCol.Width = new GridLength(0);

                //Loading Pnl
                xDeviceScreenshotCanvas.Visibility = Visibility.Collapsed;
                xMessagePnl.Visibility = Visibility.Visible;
                if (mDriver.GetDevicePlatformType() == eDevicePlatformType.Android)
                {
                    xMessageImage.ImageType = eImageType.AndroidWhite;
                }
                else
                {
                    xMessageImage.ImageType = eImageType.IosWhite;
                }

                //Control bar
                AlloworDisableControls(false);
                switch (mDriver.GetDevicePlatformType())
                {
                    case eDevicePlatformType.Android:
                        break;
                    case eDevicePlatformType.iOS:
                        //only middle button 
                        xBackButton.Visibility = Visibility.Collapsed;
                        xMenuBtn.Visibility = Visibility.Collapsed;
                        break;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while designing the Mobile window initial look", ex);
            }
        }

        private void AlloworDisableControls(bool toAllow)
        {
            xExternalViewBtn.IsEnabled = toAllow;
            xPinBtn.IsEnabled = toAllow;
            xRefreshButton.IsEnabled = toAllow;
            xCordBtn.IsEnabled = toAllow;
            xSwipeBtn.IsEnabled = toAllow;
            xClearHighlightBtn.IsEnabled = toAllow;
            xPortraiteBtn.IsEnabled = toAllow;
            xLandscapeBtn.IsEnabled = toAllow;
            xZoomInBtn.IsEnabled = toAllow;
            xZoomOutBtn.IsEnabled = toAllow;
            xConfigurationsBtn.IsEnabled = toAllow;

            xMetricsBtn.IsEnabled = toAllow;
            xDeviceSettingsBtn.IsEnabled = toAllow;
            xBackButton.IsEnabled = toAllow;
            xHomeBtn.IsEnabled = toAllow;
            xMenuBtn.IsEnabled = toAllow;

            if (mDriver.GetAppType() == eAppType.Web)// not supported for Web
            {
                xMetricsBtn.Visibility = Visibility.Collapsed;
                xDeviceSettingsBtn.Visibility = Visibility.Collapsed;
                xBackButton.Visibility = Visibility.Collapsed;
                xHomeBtn.Visibility = Visibility.Collapsed;
                xMenuBtn.Visibility = Visibility.Collapsed;
            }

            if (!toAllow)
            {
                //set LightGray brush from hex 
                xDeviceWindowControlsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dddddd"));
                xDeviceControlsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dddddd"));
            }
            else
            {
                //set white
                xDeviceWindowControlsBorder.Background = new SolidColorBrush(Colors.White);
                xDeviceControlsBorder.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void SetOrientationButton()
        {
            try
            {
                eDeviceOrientation currentOrientation = mDriver.GetOrientation();
                if (currentOrientation == eDeviceOrientation.Landscape)
                {
                    xPortraiteBtn.Visibility = Visibility.Visible;
                    xLandscapeBtn.Visibility = Visibility.Collapsed;
                }
                else
                {
                    xPortraiteBtn.Visibility = Visibility.Collapsed;
                    xLandscapeBtn.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to set the device orientation, seems like the connection to the device is not valid.", ex);
            }
        }

       
        private void DoContinualDeviceScreenshotRefresh()
        {
            Task.Run(() =>
            {
                while (mWindowIsOpen && mDriver != null && mDriver.IsDeviceConnected)
                {
                    if (mDeviceAutoScreenshotRefreshMode == eAutoScreenshotRefreshMode.Live)
                    {
                        RefreshDeviceScreenshotAsync(200).Wait();
                    }
                }
            });
        }

        bool clearedHighlights = false;
        private async Task<bool> RefreshDeviceScreenshotAsync(int waitingTimeInMiliSeconds = 0)
        {
            try
            {
                if (!xDeviceScreenshotImage.IsVisible)
                {
                    return false;
                }

                if (!clearedHighlights) //bool is for clearing only once in 2 refresh for allowing user to see the highlighted area
                {
                    UnHighlightElementEvent();
                    clearedHighlights = true;
                }
                else
                {
                    clearedHighlights = false;
                }

                int waitingRatio = 1;
                if (mDeviceAutoScreenshotRefreshMode != eAutoScreenshotRefreshMode.Live)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        xRefreshButton.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing;
                        xRefreshButton.ToolTip = "Refreshing...";
                    });
                }

                await Task.Run(() =>
                {
                    try
                    {
                        //wait before taking screen shot
                        if (waitingTimeInMiliSeconds > 0)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                try
                                {
                                    waitingRatio = int.Parse(xRefreshWaitingRateCombo.Text);
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "Failed to Parse Waiting Ratio", ex);
                                }
                            });
                            Thread.Sleep(waitingTimeInMiliSeconds * (waitingRatio));
                        }

                        //take screen shot

                        byte[] imageByteArray = mDriver.GetScreenshotImage();
                        if (imageByteArray == null || imageByteArray.Length == 0)
                        {
                            Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to update the device screenshot, Error:{0}"));

                            xDeviceScreenshotCanvas.Visibility = Visibility.Collapsed;
                            xDeviceSectionMainPnl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));
                            xMessagePnl.Visibility = Visibility.Visible;
                            xMessageImage.ImageType = eImageType.Image;
                            xMessageLbl.Content = "Failed to retrieve device screenshot " + Environment.NewLine + "due to a lost or failed connection." + Environment.NewLine + "Check the log for details.";
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
                            this.Dispatcher.Invoke(() =>
                            {
                                xDeviceScreenshotCanvas.Visibility = Visibility.Visible;
                                xMessagePnl.Visibility = Visibility.Collapsed;
                                xDeviceScreenshotImage.Source = image;
                            });
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {

                        if (!mDriver.IsDeviceConnected)
                        {
                            if (!mSelfClosing)
                            {
                                DoSelfClose();
                            }
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.WARN, "Failed to update the device screenshot, seems like the connection to the device is not valid.", ex);

                            this.Dispatcher.Invoke(() =>
                            {
                                xDeviceScreenshotCanvas.Visibility = Visibility.Collapsed;
                                xMessageProcessingImage.Visibility = Visibility.Collapsed;
                                xDeviceSectionMainPnl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));
                                xMessagePnl.Visibility = Visibility.Visible;
                                xMessageImage.ImageType = eImageType.Image;
                                xMessageImage.ImageForeground = new SolidColorBrush(Colors.OrangeRed);
                                xMessageLbl.Content = "Failed to retrieve device screenshot " + Environment.NewLine + "due to a lost or failed connection." + Environment.NewLine + "Check the log for details.";
                            });

                            if (mDeviceAutoScreenshotRefreshMode == eAutoScreenshotRefreshMode.Live)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    xPostOperationRdBtn.IsChecked = true;
                                });
                            }
                        }

                        return false;
                    }
                });

                return false;
            }
            finally
            {
                if (mDeviceAutoScreenshotRefreshMode != eAutoScreenshotRefreshMode.Live)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        xRefreshButton.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Refresh;
                        xRefreshButton.ToolTip = "Refresh Device Screenshot";
                    });
                }
            }
        }

        private System.Windows.Point GetPointOnMobile(System.Windows.Point pointOnImage)
        {
            try
            {
                var point = ((DriverBase)mDriver).GetPointOnAppWindow(new System.Drawing.Point((int)pointOnImage.X, (int)pointOnImage.Y),
                  xDeviceScreenshotImage.Source.Width, xDeviceScreenshotImage.Source.Height, xDeviceScreenshotImage.ActualWidth, xDeviceScreenshotImage.ActualHeight);
                System.Windows.Point pointOnMobile = new System.Windows.Point(point.X, point.Y);
                return pointOnMobile;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get point on device", ex);
                return new Point(0, 0);
            }
        }

        private async void DeviceScreenshotImageMouseClickAsync(System.Windows.Point clickedPoint, bool performLongPress = false, TimeSpan? clickDuration = null)
        {
            try
            {
                //calculate clicked X,Y
                System.Windows.Point pointOnMobile = GetPointOnMobile(clickedPoint);
                long pointOnMobile_X = (long)pointOnMobile.X;
                long pointOnMobile_Y = (long)pointOnMobile.Y;

                //click the element
                if (performLongPress)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            mDriver.PerformLongPress(pointOnMobile_X, pointOnMobile_Y, clickDuration);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to Perform Long Press", ex);
                        }
                    });
                }
                else
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            mDriver.PerformTap(pointOnMobile_X, pointOnMobile_Y);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to Perform Tap", ex);
                        }
                    });
                }

                //update the screen
                if (mDeviceAutoScreenshotRefreshMode == eAutoScreenshotRefreshMode.PostOperation)
                {
                    RefreshDeviceScreenshotAsync(100);
                }
                UnHighlightElementEvent();
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("Failed to perform tap operation, error: '{0}'", ex.Message));
            }
        }

        private async void DeviceScreenshotImageMouseDragAsync(System.Windows.Point startPoint, System.Windows.Point endPoint, TimeSpan pressDuration, TimeSpan dragDuration)
        {
            try
            {
                startPoint = GetPointOnMobile(startPoint);
                endPoint = GetPointOnMobile(endPoint);

                //Perform drag
                await Task.Run(() =>
                {
                    try
                    {
                        mDriver.PerformDrag(new System.Drawing.Point((int)startPoint.X, (int)startPoint.Y), new System.Drawing.Point((int)endPoint.X, (int)endPoint.Y), pressDuration, dragDuration);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to Perform Drag", ex);
                    }
                });

                //update the screen
                if (mDeviceAutoScreenshotRefreshMode == eAutoScreenshotRefreshMode.PostOperation)
                {
                    RefreshDeviceScreenshotAsync(100);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("Failed to perform drag operation, error: '{0}'", ex.Message));
            }
        }

        private void SetConfigurationsPanelView(bool show)
        {
            if (show == true)
            {
                SetTabsColumnView(eTabsViewMode.Configurations);
            }
            else
            {
                SetTabsColumnView(eTabsViewMode.None);
            }
            mConfigIsOn = show;
        }

        enum eTabsViewMode { None, DetailsAndMetrics, Configurations }
        private void SetTabsColumnView(eTabsViewMode mode)
        {
            switch (mode)
            {
                case eTabsViewMode.DetailsAndMetrics:
                    this.Width = this.Width - xTabsCol.ActualWidth;
                    xTabsCol.Width = new GridLength(350);
                    this.Width = this.Width + Convert.ToDouble(xTabsCol.Width.ToString());

                    xDeviceDetailsAndMetricsTabs.Visibility = Visibility.Visible;
                    xWindowConfigurationsTabs.Visibility = Visibility.Collapsed;

                    xMetricsBtn.ButtonStyle = FindResource("$ImageButtonStyle_Pink") as Style;
                    xMetricsBtn.ToolTip = "Hide Device Details & Metrics";
                    mMeticsIsOn = true;
                    xConfigurationsBtn.ButtonStyle = FindResource("$ImageButtonStyle") as Style;
                    xConfigurationsBtn.ToolTip = "Show Window Configurations";
                    mConfigIsOn = false;
                    break;

                case eTabsViewMode.Configurations:
                    this.Width = this.Width - xTabsCol.ActualWidth;
                    xTabsCol.Width = new GridLength(270);
                    this.Width = this.Width + Convert.ToDouble(xTabsCol.Width.ToString());

                    xDeviceDetailsAndMetricsTabs.Visibility = Visibility.Collapsed;
                    xWindowConfigurationsTabs.Visibility = Visibility.Visible;

                    xMetricsBtn.ButtonStyle = FindResource("$ImageButtonStyle") as Style;
                    xMetricsBtn.ToolTip = "Show Device Details & Metrics";
                    mMeticsIsOn = false;
                    xConfigurationsBtn.ButtonStyle = FindResource("$ImageButtonStyle_Pink") as Style;
                    xConfigurationsBtn.ToolTip = "Hide Window Configurations";
                    mConfigIsOn = true;
                    break;

                case eTabsViewMode.None:
                default:
                    if (this.Width - xTabsCol.ActualWidth > 0)
                    {
                        this.Width = this.Width - xTabsCol.ActualWidth;
                    }
                    xTabsCol.Width = new GridLength(0);

                    xDeviceDetailsAndMetricsTabs.Visibility = Visibility.Collapsed;
                    xWindowConfigurationsTabs.Visibility = Visibility.Collapsed;

                    xMetricsBtn.ButtonStyle = FindResource("$ImageButtonStyle") as Style;
                    xMetricsBtn.ToolTip = "Show Device Details & Metrics";
                    mMeticsIsOn = false;
                    xConfigurationsBtn.ButtonStyle = FindResource("$ImageButtonStyle") as Style;
                    xConfigurationsBtn.ToolTip = "Show Window Configurations";
                    mConfigIsOn = false;
                    break;
            }
        }

        private void DoSelfClose()
        {
            mSelfClosing = true;
            if (!mUserClosing)
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
            }
        }

        private void PerformScreenSwipe(eSwipeSide swipeSide, double swipeScale, TimeSpan swipeDuration)
        {
            try
            {
                mDriver.PerformScreenSwipe(swipeSide, swipeScale, swipeDuration);

                if (mDeviceAutoScreenshotRefreshMode == eAutoScreenshotRefreshMode.PostOperation)
                {
                    RefreshDeviceScreenshotAsync();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Operation failed, Error: " + ex.Message);
            }
        }
        #endregion Functions

        GenericWindow genWin;

        private void OpenPopUpWindow(string content, string title)
        {
            string tempFilePath = GingerCoreNET.GeneralLib.General.CreateTempTextFile(content);
            if (System.IO.File.Exists(tempFilePath))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DocumentEditorPage docPage = new DocumentEditorPage(tempFilePath, enableEdit: false, UCTextEditorTitle: string.Empty)
                    {
                        Width = 800,
                        Height = 800
                    };
                    GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, eWindowShowStyle.Free, title, docPage);

                });

                System.IO.File.Delete(tempFilePath);
                return;
            }
        }



        private void xExtraInfoBtn_ExtraInfo_Click(object sender, RoutedEventArgs e)
        {
            DeviceInfo deviceInfo = (DeviceInfo)((ucButton)sender).DataContext;
            switch (deviceInfo.DetailName)
            {
                case "App RAM Usage:":
                    OpenPopUpWindow(deviceInfo.ExtraInfo.Replace(", ", Environment.NewLine), "Full device's Memory Information");
                    break;
                case "Networks:":
                    OpenPopUpWindow(deviceInfo.ExtraInfo, "Full device's Network Information");
                    break;
                case "App CPU Usage:":
                    OpenPopUpWindow(deviceInfo.ExtraInfo.Replace(", ", Environment.NewLine), "Full device's CPU Information");
                    break;
                case "Activity:":
                    OpenPopUpWindow(deviceInfo.ExtraInfo.Replace(", ", Environment.NewLine), "Application Activity");
                    break;
                case "Package:":
                    OpenPopUpWindow(deviceInfo.ExtraInfo.Replace(", ", Environment.NewLine), "Application Package");
                    break;
                case "Ginger Agent:":
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AgentEditPage agentEditPage = new AgentEditPage(mAgent, true)
                        {
                            Width = 800,
                            Height = 800
                        };

                        GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, eWindowShowStyle.Free, mAgent.Name, agentEditPage);
                    });
                    break;
                default:
                    Reporter.ToUser(eUserMsgKey.MobileDriverNotConnected);
                    break;
            }
        }

        int mCordsStackZIndex = 1;
        private void xCordsStack_MouseEnter(object sender, MouseEventArgs e)
        {
            if (mCordsStackZIndex == 1)
            {
                mCordsStackZIndex = 100;
            }
            else
            {
                mCordsStackZIndex = 1;
            }
            Canvas.SetLeft(xCordsStack, mCordsStackZIndex);
        }

        private void xExternalViewBtn_Click(object sender, RoutedEventArgs e)
        {
            mDriver.OpenDeviceExternalView();
        }

        enum eImageChangeType { Increase, Decrease, DoNotChange }
        private void xZoomInBtn_Click(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize(eImageChangeType.Increase, false);
        }

        private void xZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize(eImageChangeType.Decrease, false);
        }

        private double mZoomSize = 0.25;
        private void AdjustWindowSize(eImageChangeType operationType, bool resetCanvasSize)
        {
            if (xDeviceScreenshotImage.Source != null)
            {
                double imageSourceHightWidthRatio = xDeviceScreenshotImage.Source.Height / xDeviceScreenshotImage.Source.Width;
                if (resetCanvasSize)
                {
                    mZoomSize = 0.25;
                    xDeviceScreenshotCanvas.Width = xDeviceScreenshotImage.Source.Width * mZoomSize;
                    while (xDeviceScreenshotCanvas.Width < 250)
                    {
                        mZoomSize *= 1.05;
                        xDeviceScreenshotCanvas.Width = xDeviceScreenshotImage.Source.Width * mZoomSize;
                    }
                }
                double previousCanasWidth = xDeviceScreenshotCanvas.ActualWidth;
                double previousCanasHeight = xDeviceScreenshotCanvas.ActualHeight;
                double targetWidthRatio = xDeviceScreenshotImage.Source.Width / xDeviceScreenshotCanvas.Width;

                //Update canvas size
                xDeviceScreenshotCanvas.Width = (xDeviceScreenshotImage.Source.Width / targetWidthRatio);
                switch (operationType)
                {
                    case eImageChangeType.Increase:
                        xDeviceScreenshotCanvas.Width = xDeviceScreenshotCanvas.Width * 1.15;
                        break;

                    case eImageChangeType.Decrease:
                        xDeviceScreenshotCanvas.Width = xDeviceScreenshotCanvas.Width * 0.85;
                        break;
                }
                mZoomSize = xDeviceScreenshotCanvas.Width / xDeviceScreenshotImage.Source.Width;
                xDeviceScreenshotCanvas.Height = xDeviceScreenshotCanvas.Width * imageSourceHightWidthRatio;

                //Update window size
                this.Width = this.Width + (xDeviceScreenshotCanvas.Width - previousCanasWidth);
                this.Height = xDeviceScreenshotCanvas.Height + 100;

                if (mZoomSize >= 1)
                {
                    xZoomInBtn.IsEnabled = false;
                }
                else
                {
                    xZoomInBtn.IsEnabled = true;
                }
                if (mZoomSize <= 0.2)
                {
                    xZoomOutBtn.IsEnabled = false;
                }
                else
                {
                    xZoomOutBtn.IsEnabled = true;
                }
                int roundedNumber = (int)Math.Round(mZoomSize * 100);
                xZoomSizeLbl.Content = roundedNumber.ToString() + "%";
            }
        }
    }
}
