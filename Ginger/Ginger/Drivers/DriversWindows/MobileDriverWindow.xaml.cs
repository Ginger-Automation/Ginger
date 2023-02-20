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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using Amdocs.Ginger.CoreNET.Drivers.DriversWindow;
using Amdocs.Ginger.UserControls;
using FontAwesome5;
using Ginger.Agents;
using Ginger.UserControls;
using Ginger.UserControlsLib.TextEditor;
using GingerCore;
using GingerCore.Drivers;
using OpenQA.Selenium;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using System.Text;

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
        bool mIsMetricsClickedOnce = false;



        ObservableList<DeviceInfo> mDeviceDetails = new ObservableList<DeviceInfo>();



        public MobileDriverWindow(DriverBase driver, Agent agent)
        {
            InitializeComponent();

            mDriver = (IMobileDriverWindow)driver;
            mAgent = agent;

            ((DriverBase)mDriver).DriverMessageEvent += MobileDriverWindow_DriverMessageEvent;
            ((DriverBase)mDriver).SpyingElementEvent += CurrentMousePosToSpy;
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

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();


            view.GridColsView.Add(new GridColView() { Field = nameof(DeviceInfo.DetailName), Header = "Name", WidthWeight = 4.5, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeviceInfo.DetailValue), Header = "Value", WidthWeight = 7, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeviceInfo.ExtraInfo), Header = "Extra Info", WidthWeight = 2.5, MaxWidth = 70, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xWindowGrid.Resources["ExtraInfo"] });

            xDeviceDetailsGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshDetailsTable));

            xDeviceDetailsGrid.SetAllColumnsDefaultView(view);
            xDeviceDetailsGrid.InitViewItems();
            xDeviceDetailsGrid.SetTitleLightStyle = true;
        }

        private void SetDeviceMetricsGridView()
        {
            //# Default View

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(DeviceInfo.DetailName), Header = "Name", WidthWeight = 4.5, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeviceInfo.DetailValue), Header = "Value", WidthWeight = 7, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeviceInfo.ExtraInfo), Header = "Extra Info", WidthWeight = 2.6, MaxWidth = 70, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xWindowGrid.Resources["ExtraInfo"] });

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

        public void UpdateRecordingImage(bool ShowRecordIcon)
        {
            Dispatcher.Invoke(() =>
            {
                if (ShowRecordIcon)
                    xRecordingImage.Visibility = Visibility.Visible;
                else
                    xRecordingImage.Visibility = Visibility.Collapsed;
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
                            xMessagePnl.Visibility = Visibility.Collapsed;
                            xDeviceScreenshotCanvas.Visibility = Visibility.Visible;
                            xMessageLbl.Content = "Loading Device Screenshot...";

                            await RefreshDeviceScreenshotAsync();
                            SetOrientationButton();
                            DoContinualDeviceScreenshotRefresh();

                            Dictionary<string, object> mDeviceGeneralInfo;
                            mDeviceGeneralInfo = mDriver.GetDeviceGeneralInfo();

                            SetTitle(mDeviceGeneralInfo);
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

                case DriverBase.eDriverMessageType.RecordingEvent:
                    IsRecording = (sender == null) ? false : (bool)sender;

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

        private async Task<bool> SetDeviceMetricsGridData()
        {
            string mDeviceNetworkInfo = string.Empty;
            Dictionary<string, string> mDeviceMemoryInfo = null;
            Dictionary<string, string> mDeviceCPUInfo = null;

            await Task.Run(() =>
            {
                mDeviceCPUInfo = mDriver.GetDeviceCPUInfo();
                mDeviceMemoryInfo = mDriver.GetDeviceMemoryInfo();
                mDeviceNetworkInfo = mDriver.GetDeviceNetworkInfo().Result;
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
            returnString = returnString.Substring(0, returnString.Length - 2);
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
                mDeviceDetails.Add(new DeviceInfo("Package", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
                mDeviceDetails.Add(new DeviceInfo("Activity", "N/A", DeviceInfo.eDeviceInfoCategory.Detail));
            }
            else
            {
                string activity, package; 
                mActivityAndPackageInfo.TryGetValue("Activity", out activity);
                mActivityAndPackageInfo.TryGetValue("Package", out package);
                mDeviceDetails.Add(new DeviceInfo("Package", package, DeviceInfo.eDeviceInfoCategory.Detail));
                mDeviceDetails.Add(new DeviceInfo("Activity", activity, DeviceInfo.eDeviceInfoCategory.Detail));
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
            SetDeviceButtonsLocation();

            if (mSwipeIsOn)
            {
                SetSwipeButtonsPosition();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetDeviceButtonsLocation();

            if (mSwipeIsOn)
            {
                SetSwipeButtonsPosition();
            }
        }

        DateTime mClickStartTime;
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
            if (mIsMousePressed == true)
            {
                //it's a drag
                mIsItDragAction = true;
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

            if (mIsItDragAction == true)
            {
                //do drag
                mIsItDragAction = false;
                DeviceScreenshotImageMouseDragAsync(mMouseStartPoint, mMouseEndPoint);
            }
            else
            {
                TimeSpan clickDuration = clickEndTime - mClickStartTime;
                if (clickDuration.TotalSeconds > 1)
                {
                    //do long press
                    DeviceScreenshotImageMouseClickAsync(mMouseEndPoint, true);
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
            //System.Windows.Point mMousePoint = e.GetPosition((System.Windows.Controls.Image)sender);
            //DeviceScreenshotImageMouseDragAsync(mMousePoint, new System.Windows.Point(mMousePoint.X, mMousePoint.Y + e.Delta));
            if (e.Delta > 0)
            {
                PerformScreenSwipe(eSwipeSide.Up, 0.25);
            }
            else
            {
                PerformScreenSwipe(eSwipeSide.Down, 0.25);
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
                if (!mIsMetricsClickedOnce)
                {
                    //Setting up the detail and metic grids
                    SetDeviceDetailsGridView();
                    SetDeviceMetricsGridView();

                    mIsMetricsClickedOnce = true;
                }
                //Setting the with of the metrics panel
                SetMeticsPanelView(!mMeticsIsOn);

                if (mMeticsIsOn)
                {

                    await this.Dispatcher.InvokeAsync(async () =>
                    {
                        xMetricsBtn.ButtonStyle = FindResource("$ImageButtonStyle_Pink") as Style;
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
                else
                {
                    xMetricsBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
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
            if (mPinIsOn)
            {
                //undock
                this.Topmost = false;
                xPinBtn.ButtonStyle = FindResource("$ImageButtonStyle_Pink") as Style;
                xPinBtn.ToolTip = "Dock Window";
            }
            else
            {
                //dock
                this.Topmost = true;
                xPinBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                xPinBtn.ToolTip = "Undock Window";
            }

            mPinIsOn = !mPinIsOn;
        }

        private void xOrientationBtn_Click(object sender, RoutedEventArgs e)
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
                if (mDeviceAutoScreenshotRefreshMode == eAutoScreenshotRefreshMode.PostOperation)
                {
                    RefreshDeviceScreenshotAsync();
                }
                SetOrientationButton();
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Operation failed, Error: " + ex.Message);
            }
        }

        bool mSwipeIsOn;
        private void xSwipeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mSwipeIsOn)
            {
                //Turn of Swipe
                xSwipeDown.Visibility = Visibility.Collapsed;
                xSwipeUp.Visibility = Visibility.Collapsed;
                xSwipeRight.Visibility = Visibility.Collapsed;
                xSwipeLeft.Visibility = Visibility.Collapsed;
                xSwipeBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
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
            if (mDriver.GetAppType() == eAppType.Web || mDriver.GetDevicePlatformType() == eDevicePlatformType.iOS)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation not supported for this mobile OS or application type.");
                return;
            }

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
            if (((Window)sender).IsKeyboardFocused)
            {
                mUserClosing = true;

                if (!mSelfClosing)
                {
                    if (Reporter.ToUser(eUserMsgKey.StaticQuestionsMessage, "Close Mobile Agent?") == eUserMsgSelection.Yes)
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
            PerformScreenSwipe(eSwipeSide.Left);
        }

        private void xSwipeRight_Click(object sender, RoutedEventArgs e)
        {
            PerformScreenSwipe(eSwipeSide.Right);
        }

        private void xSwipeUp_Click(object sender, RoutedEventArgs e)
        {
            PerformScreenSwipe(eSwipeSide.Up, 0.25);
        }

        private void xSwipeDown_Click(object sender, RoutedEventArgs e)
        {
            PerformScreenSwipe(eSwipeSide.Down, 0.25);
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

                this.Width = 320;
                this.Height = 650;
                xMessageLbl.Content = "Connecting to Device...";

                //Configurations
                SetConfigurationsPanelView(false);
                //Metrics
                SetMeticsPanelView(false);

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

                xConfigurationsFrame.Visibility = System.Windows.Visibility.Collapsed;
                xConfigurationsCol.Width = new GridLength(0);

                xMeticsCol.Width = new GridLength(0);

                //Main tool bar
                xRefreshButton.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                xPinBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                xConfigurationsBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                xSwipeBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                xPortraiteBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                xLandscapeBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                xMetricsBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                xPinBtn_Click(null, null);

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

                //Device buttons panel
                xHomeBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                xMenuBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                xBackButton.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
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
                //fliping the back icon to fit look on mobile
                xBackButton.xButtonImage.RenderTransformOrigin = new Point(0.5, 0.5);
                ScaleTransform flipTrans = new ScaleTransform();
                flipTrans.ScaleX = -1;
                xBackButton.xButtonImage.RenderTransform = flipTrans;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while designing the Mobile window initial look", ex);
            }
        }

        private void SetDeviceButtonsLocation()
        {
            if (xDeviceScreenshotCanvas != null)
            {
                //correct the device buttons location
                if (xDeviceScreenshotImage != null && xDeviceScreenshotImage.ActualWidth > 0)
                {
                    double emptySpace = ((xDeviceScreenshotCanvas.ActualWidth - xDeviceScreenshotImage.ActualWidth) / 2);
                    Thickness margin = xBackButton.Margin;
                    margin.Right = 10 + emptySpace;
                    xBackButton.Margin = margin;
                    margin = xMenuBtn.Margin;
                    margin.Left = 10 + emptySpace;
                    xMenuBtn.Margin = margin;
                }
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

                xSwipeBtn.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to set the device orientation", ex);
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
                        xRefreshButton.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                        xRefreshButton.ToolTip = "Refreshing...";
                    });
                }

                await Task.Run(() =>
                {
                    //wait before taking screen shot
                    if (waitingTimeInMiliSeconds > 0)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            waitingRatio = int.Parse(xRefreshWaitingRateCombo.Text);
                        });
                        Thread.Sleep(waitingTimeInMiliSeconds * (waitingRatio));
                    }

                    //take screen shot
                    try
                    {
                        byte[] imageByteArray = mDriver.GetScreenshotImage();
                        if (imageByteArray == null || imageByteArray.Length == 0)
                        {
                            Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to update the device screenshot, Error:{0}"));
                            xDeviceScreenshotCanvas.Visibility = Visibility.Collapsed;
                            xMessagePnl.Visibility = Visibility.Visible;
                            xMessageImage.ImageType = eImageType.Image;
                            xMessageLbl.Content = "Failed to get device screenshot";
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
                            Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to update the device screenshot, Error:{0}", ex.Message));

                            this.Dispatcher.Invoke(() =>
                            {
                                xDeviceScreenshotCanvas.Visibility = Visibility.Collapsed;
                                xMessageProcessingImage.Visibility = Visibility.Collapsed;
                                xMessagePnl.Visibility = Visibility.Visible;
                                xMessageImage.ImageType = eImageType.Image;
                                xMessageImage.ImageForeground = new SolidColorBrush(Colors.OrangeRed);
                                xMessageLbl.Content = "Failed to get device screenshot";
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
                        xRefreshButton.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                        xRefreshButton.ToolTip = "Refresh Device Screenshot";
                    });
                }
            }
        }

        private System.Windows.Point GetPointOnMobile(System.Windows.Point pointOnImage)
        {
            try
            {
                System.Windows.Point pointOnMobile = new System.Windows.Point();
                double ratio_X = 1;
                double ratio_Y = 1;

                ratio_X = (xDeviceScreenshotImage.Source.Width / ((GenericAppiumDriver)mDriver).SourceMobileImageWidthConvertFactor) / xDeviceScreenshotImage.ActualWidth;
                ratio_Y = (xDeviceScreenshotImage.Source.Height / ((GenericAppiumDriver)mDriver).SourceMobileImageHeightConvertFactor) / xDeviceScreenshotImage.ActualHeight;

                if (mDriver.GetAppType() == eAppType.Web && mDriver.GetDevicePlatformType() == eDevicePlatformType.Android)
                {
                    pointOnMobile.X = (int)(pointOnImage.X * ratio_X);
                    pointOnMobile.Y = (int)((pointOnImage.Y + xDeviceScreenshotImage.ActualHeight / 9) * ratio_Y);
                    //else
                    //{
                    //    pointOnMobile.X = (int)((pointOnImage.X * ratio_X) / 3);
                    //    pointOnMobile.Y = (int)((pointOnImage.Y * ratio_Y) / 3);
                    //}
                }
                else
                {
                    pointOnMobile.X = (int)(pointOnImage.X * ratio_X);
                    pointOnMobile.Y = (int)(pointOnImage.Y * ratio_Y);
                }

                return pointOnMobile;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get point on device", ex);
                return new Point(0, 0);
            }
        }

        private async void DeviceScreenshotImageMouseClickAsync(System.Windows.Point clickedPoint, bool performLongPress = false)
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
                    await Task.Run(() => { mDriver.PerformLongPress(pointOnMobile_X, pointOnMobile_Y); });
                }
                else
                {
                    await Task.Run(() => { mDriver.PerformTap(pointOnMobile_X, pointOnMobile_Y); });
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

        private async void DeviceScreenshotImageMouseDragAsync(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            try
            {
                startPoint = GetPointOnMobile(startPoint);
                endPoint = GetPointOnMobile(endPoint);

                //Perform drag
                await Task.Run(() =>
                {
                    mDriver.PerformDrag(new System.Drawing.Point((int)startPoint.X, (int)startPoint.Y), new System.Drawing.Point((int)endPoint.X, (int)endPoint.Y));
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
                xConfigurationsFrame.Visibility = System.Windows.Visibility.Visible;
                xConfigurationsSplitter.Visibility = System.Windows.Visibility.Visible;
                xConfigurationsCol.Width = new GridLength(220);
                this.Width = this.Width + Convert.ToDouble(xConfigurationsCol.Width.ToString());
                xConfigurationsBtn.ButtonStyle = FindResource("$ImageButtonStyle_Pink") as Style;
                xConfigurationsBtn.ToolTip = "Hide Configurations";
            }
            else
            {
                xConfigurationsFrame.Visibility = System.Windows.Visibility.Collapsed;
                xConfigurationsSplitter.Visibility = System.Windows.Visibility.Collapsed;
                this.Width = this.Width - xConfigurationsCol.ActualWidth;
                xConfigurationsCol.Width = new GridLength(0);
                xConfigurationsBtn.ButtonStyle = FindResource("$ImageButtonStyle_WhiteSmoke") as Style;
                xConfigurationsBtn.ToolTip = "Show Configurations";
            }
            mConfigIsOn = show;
        }

        private void SetMeticsPanelView(bool show)
        {
            if (show)
            {
                xMeticsCol.Width = new GridLength(350);
                this.Width = this.Width + Convert.ToDouble(xMeticsCol.Width.ToString());
            }
            else
            {
                xMeticsCol.Width = new GridLength(0);
                if (this.Width - xMeticsCol.ActualWidth > 0)
                {
                    this.Width = this.Width - xMeticsCol.ActualWidth;
                }
            }
            mMeticsIsOn = show;
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

        private void PerformScreenSwipe(eSwipeSide swipeSide, double impact = 1)
        {
            try
            {
                mDriver.PerformScreenSwipe(swipeSide, impact);

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
                    DocumentEditorPage docPage = new DocumentEditorPage(tempFilePath, enableEdit: false, UCTextEditorTitle: string.Empty);
                    docPage.Width = 800;
                    docPage.Height = 800;
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
                case "Ginger Agent:":
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AgentEditPage agentEditPage = new AgentEditPage(mAgent, true);
                        agentEditPage.Width = 800;
                        agentEditPage.Height = 800;

                        GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, eWindowShowStyle.Free, mAgent.Name, agentEditPage);
                    });
                    break;
                default:
                    Reporter.ToUser(eUserMsgKey.MobileDriverNotConnected);
                    break;
            }
        }
    }
}
