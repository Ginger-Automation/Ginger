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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Threading;
using GingerCore.Actions;
using System.Xml;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using Amdocs.Ginger.Common;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.Common.Devices;
using GingerCore.Actions.Android;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCore.Drivers.AndroidADB
{
    /// <summary>
    /// Interaction logic for AndroidADBDriverWindow
    /// </summary>
    public partial class AndroidADBDriverWindow : Window
    {
        public AndroidADBDriver mAndroidADBDriver;

        bool mSendKeyboardKeys = false;
        public BusinessFlow mBusinessFlow;        
        XmlNode rectangleXmlNode;
        string pageSourceString = string.Empty;
        DeviceViewPage mDeviceViewPage;
        bool mLiveRefresh = true;

        public AndroidADBDriverWindow(AndroidADBDriver driver, string DeviceConfigFolder)
        {
            InitializeComponent();
            mAndroidADBDriver = driver;        
            mDeviceViewPage = new DeviceViewPage(DeviceConfigFolder);
            DeviceViewFrame.Content = mDeviceViewPage;
            mDeviceViewPage.TouchXY += DeviceViewPage_TouchXY;
            mDeviceViewPage.Swipe += DeviceViewPage_Swipe;
            mDeviceViewPage.ButtonClick += DeviceViewPage_OnButtonClick;

            InitDeviceActions();
        }

        private void DeviceViewPage_Swipe(object sender, DeviceViewPage.SwipeEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            mAndroidADBDriver.SwipeScreen((int)e.X, (int)e.Y, (int)e.XE, (int)e.YE, e.Steps);

            Mouse.OverrideCursor = null;

            if (IsRecording)
            {
                //ActGenElement act = IdentifyClickedElement(e.Left, e.Top);
                //act.GenElementAction = ActGenElement.eGenElementAction.Click;
                //// If not element found use X,Y
                //// ActGenElement act = new ActGenElement() { Description = "Tap X,Y", GenElementAction = ActGenElement.eGenElementAction.Click, Value = e.Left + "," + e.Top };
                //mAndroidADBDriver.BusinessFlow.AddAct(act);
            }
        }

        private void InitDeviceActions()
        {
            List<Act> list = new List<Act>();
            list.Add(new ActGenElement() { Description = "Home" });
            list.Add(new ActGenElement() { Description = "Back" });
            list.Add(new ActGenElement() { Description = "Send Text" });
            list.Add(new ActGenElement() { Description = "Install APK" });
            list.Add(new ActShell() { Description = "Get device API version", Value = "getprop ro.build.version.sdk" });
            
            DeviceActionsGrid.ItemsSource = list;
        }

        private void DeviceViewPage_OnButtonClick(object sender, DeviceViewPage.ButtonEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            DeviceButton DB = e.DeviceButton;

            if (IsRecording)
            {
                ActDeviceButton act = new ActDeviceButton();
                act.Description = DB.SendCommand;
                act.Value = DB.SendCommand;
                mAndroidADBDriver.BusinessFlow.AddAct(act);
            }

            
            if (DB.SendCommand.StartsWith("Press "))
            {
                string key = DB.SendCommand.Replace("Press ", "");
                mAndroidADBDriver.Press(key);                
            }
                // Change to PressKeyCode
            else if (DB.SendCommand.StartsWith("PressKey "))
            {
                string key = DB.SendCommand.Replace("PressKey ", "");
                int KeyCode = int.Parse(key);
                mAndroidADBDriver.PressKeyCode(KeyCode);                
            }
            else
            {
                //Remove from here and create special action
                // we assume it is shell command
                string result = mAndroidADBDriver.ExecuteShellCommand(DB.SendCommand);
            }
            
            Mouse.OverrideCursor = null;
        }

        private void DeviceViewPage_TouchXY(object sender, GingerCore.Drivers.Common.DeviceViewPage.TouchXYEventArgs e)
        {            
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            if (IsRecording)
            {
                ActUIElement act = mAndroidADBDriver.GetActionForClickedElement(e.Left, e.Top);
                mAndroidADBDriver.BusinessFlow.AddAct(act);
            }
          
            mAndroidADBDriver.ClickXY((int)e.Left, (int)e.Top);
            Mouse.OverrideCursor = null;
        }
        
        #region Events
        
        private void DeviceImage_MouseEnter(object sender, MouseEventArgs e)
        {
            // temp remove since we use the text box to send to focused Edit box

            // Put it back with check box flag if user wants it...
            // mSendKeyboardKeys = true;

         
            // TODO: find if we are in Spy mode if yes change the cursor

            //if (InspectorPointBtn.IsChecked == true)
            //    Mouse.OverrideCursor = System.Windows.Input.Cursors.Cross;
            //else
            //    Mouse.OverrideCursor = System.Windows.Input.Cursors.Hand;
        }

        private void DeviceImage_MouseLeave(object sender, MouseEventArgs e)
        {
            mSendKeyboardKeys = false;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage BI = mAndroidADBDriver.GetScreenShotAsBitmapImage();
            if (BI != null)  // it can be null if screen didn't changed
            {
                mDeviceViewPage.UpdateDeviceScreenShot(BI);
            }
        }
     
        private void InspectBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (InspectBtn.IsChecked == true)
            //{
            //    if (RecordBtn.IsChecked == false)
            //        // StorePageSource();
            //    else
            //        RecordBtn.IsChecked = false;
            //    SetInspectorPanelView(true);
            //    //StorePageSource();
            //}
            //else
            //{
            //    SetInspectorPanelView(false);
            //    // SetAttributesActionsView(false);
            //    RemoveElemntRectangle();
            //}
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

        private void inspectorElementTabsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (inspectorElementTabsControl.SelectedItem == DeviceActionsTab)
            {
                // DesignSourceTabContent();
            }

            if (inspectorElementTabsControl.SelectedItem == PageSourceTab)
            {
                 DesignSourceTabContent();
            }
        }

        private void InspectorPointBtn_Click(object sender, RoutedEventArgs e)
        {
            if (InspectorPointBtn.IsChecked == false)
                Mouse.OverrideCursor = null;
        }

        private void sourceXMLRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
           // DesignSourceTabContent();
        }

        private void sourceXMLRadioBtn_Unchecked(object sender, RoutedEventArgs e)
        {
           // DesignSourceTabContent();
        }

        #endregion Events


        #region Functions
        public void DesignWindowInitialLook()
        {
            //try
            //{
            //    if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.AndroidBrowser ||
            //            AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.iOSBrowser)
            //        this.Title = AppiumDriver.DeviceName + " " + AppiumDriver.DriverDeviceType.ToString() + " Browser";
            //    else
            //        this.Title = AppiumDriver.DeviceName + " " + AppiumDriver.DriverDeviceType.ToString();

            //    //don't track actions if asked in agent
            //    if (AppiumDriver.RefreshDeviceScreenShots != null && AppiumDriver.RefreshDeviceScreenShots.Trim().ToUpper() != "YES")
            //        TrackActionsChK.IsChecked = false;

            //    //hide optional columns
            //    this.Width = 300;
            //    ConfigurationsFrame.Visibility = System.Windows.Visibility.Collapsed;
            //    ConfigurationsCol.Width = new GridLength(0);
            //    InspectorFrame.Visibility = System.Windows.Visibility.Collapsed;
            //    InspectorCol.Width = new GridLength(0);
            //    SetAttributesActionsView(false);

            //    //don't allow to record if no business flow
            //    if (BF == null)
            //    {
            //        RecordBtn.Visibility = System.Windows.Visibility.Collapsed;
            //        actionsStckPnl.Visibility = System.Windows.Visibility.Collapsed;
            //    }

            //    //don't allow record in browser mode
            //    if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.AndroidBrowser ||
            //            AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.iOSBrowser)
            //    {
            //        RecordBtn.Visibility = System.Windows.Visibility.Collapsed;
            //        //InspectBtn.Visibility = System.Windows.Visibility.Collapsed;
            //    }

            //    //show device btns according to type
            //    switch (AppiumDriver.DriverPlatformType)
            //    {
            //        case SeleniumAppiumDriver.ePlatformType.Android:
            //            //all buttons as default
            //            break;
            //        case SeleniumAppiumDriver.ePlatformType.iOS:
            //            //only middle btn althogh not supported yet (apple limitation)
            //            backBtn.Visibility = System.Windows.Visibility.Collapsed;
            //            menuBtn.Visibility = System.Windows.Visibility.Collapsed;
            //            break;
            //        case SeleniumAppiumDriver.ePlatformType.AndroidBrowser:
            //            //browser mode- show buttons but disabled
            //            backBtn.IsEnabled = false;
            //            menuBtn.IsEnabled = false;
            //            homeBtn.IsEnabled = false;
            //            break;
            //        case SeleniumAppiumDriver.ePlatformType.iOSBrowser:
            //            //browser mode- show buttons but disabled
            //            backBtn.Visibility = System.Windows.Visibility.Collapsed;
            //            menuBtn.Visibility = System.Windows.Visibility.Collapsed;
            //            homeBtn.IsEnabled = false;
            //            break;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Reporter.ToLog(eLogLevel.ERROR, "Error occured while designing the Mobile window initial look", ex);
            //}
        }

        
        public void StartLiveRefresh()
        {
            // if (LiveRefreshCheckBox.IsChecked == true) return;

            //BitmapImage BI = mAndroidADBDriver.GetScreenShotAsBitmapImage();
            //mDeviceViewPage.UpdateDeviceScreenShot(BI);

            // Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            DateTime dt = DateTime.Now;
            Task t = Task.Factory.StartNew(() =>
            {
                int counter = 0;
                int LastSecond = DateTime.Now.Second;
                // update the screen for the next 60 seconds
                // remove this - or not!!!???
                while (mLiveRefresh)
                {                    
                    DeviceViewFrame.Dispatcher.Invoke(() =>
                    {
                        int Currsec = DateTime.Now.Second;
                        if (Currsec == LastSecond)
                        {
                            counter++;
                        }
                        else
                        {
                            LastSecond = Currsec;
                            int FrameRate = counter;
                            FrameRateLabel.Content = FrameRate;
                            counter = 0;
                        }

                        BitmapImage BI = mAndroidADBDriver.GetScreenShotAsBitmapImage();
                              
                        if (BI != null)  // it can be null if screen didn't changed
                        {
                            mDeviceViewPage.UpdateDeviceScreenShot(BI);
                            Thread.Sleep(5);  //enable user command to go in
                        }
                        else
                        {
                            Thread.Sleep(100);  // no need to update/check the screen more than 10 times per second, if there is no change, so sleep a bit more
                        }
                        General.DoEvents();
                        
                    });
                }

                FrameRateLabel.Dispatcher.Invoke(() =>
                {
                    FrameRateLabel.Content = "0";
                });

            });            
        }

        //public void ShowActionEfect(bool wait = false, Int32 waitingTimeInMiliSeconds = 2000)
        //{
        //    if (TrackActionsChK.IsChecked == true)
        //        LoadMobileScreenImage(wait, waitingTimeInMiliSeconds);
        //}

        //private System.Windows.Point GetPointOnMobile(System.Windows.Point pointOnImage)
        //{
        //    //return new Point(0,0);
        //    System.Windows.Point pointOnMobile = new System.Windows.Point();
        //    double ratio_X = 1;
        //    double ratio_Y = 1;
        //    //switch (mDriver.DriverPlatformType)
        //    //{
        //    //    case SeleniumAppiumDriver.ePlatformType.Android:
        //    //        ratio_X = DeviceImage.Source.Width / DeviceImage.ActualWidth;
        //    //        ratio_Y = DeviceImage.Source.Height / DeviceImage.ActualHeight;
        //    //        break;

        //    //    case SeleniumAppiumDriver.ePlatformType.iOS:
        //    //        ratio_X = (DeviceImage.Source.Width / 2) / DeviceImage.ActualWidth;
        //    //        ratio_Y = (DeviceImage.Source.Height / 2) / DeviceImage.ActualHeight;
        //    //        break;
        //    //}
            
        //        //ratio_X = (DeviceImage.Source.Width / 2) / DeviceImage.ActualWidth;
        //        //ratio_Y = (DeviceImage.Source.Height / 2) / DeviceImage.ActualHeight;

        //    ratio_X = DeviceImage.Source.Width / DeviceImage.ActualWidth;
        //    ratio_Y = DeviceImage.Source.Height / DeviceImage.ActualHeight;
            

        //    pointOnMobile.X = (long)(pointOnImage.X * ratio_X);
        //    pointOnMobile.Y = (long)(pointOnImage.Y * ratio_Y);

        //    return pointOnMobile;
        //}

        //private void DeviceImageMouseClick(System.Windows.Point clickedPoint)
        //{
        //    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        //    //try
        //    //{
        //    //calculate clicked X,Y
        //    // System.Windows.Point pointOnMobile = GetPointOnMobile(clickedPoint);
        //    long pointOnMobile_X = (long)pointOnMobile.X;
        //    long pointOnMobile_Y = (long)pointOnMobile.Y;

        //    mAndroidADBDriver.ExecuteShellCommand("input tap " + pointOnMobile_X + " " + pointOnMobile_Y);

        //    //TODO: wait for end of activitiy

        //    UpdateDeviceScreenShot();
        //    //    if (InspectorPointBtn.IsChecked == false)
        //    //    {
        //    //        //record action
        //    //        if (RecordBtn.IsChecked == true)
        //    //        {
        //    //            RecordAction(pointOnMobile_X, pointOnMobile_Y, ActGenElement.eGenElementAction.Click);
        //    //        }

        //    //        //click the element
        //    //        // AppiumDriver.TapXY(pointOnMobile_X, pointOnMobile_Y);

        //    //        //update the screen
        //    //        GetDeviceScreenShot(true, 1000);
        //    //    }
        //    //    else
        //    //    {
        //    //        ShowSelectedElementDetails(clickedPoint);
        //    //        InspectorPointBtn.IsChecked = false;
        //    Mouse.OverrideCursor = null;
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    Mouse.OverrideCursor = null;
        //    //    Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, ex.Message);
        //    //}
        //}

        //private void StorePageSource()
        //{
        //    //pageSourceXml.LoadXml(mDriver.GetPageSource());            
        //    //if (mDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.AndroidBrowser ||
        //    //    mDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.iOSBrowser)
        //    //{
        //    //    //clear <Head> content because causing XML errors
        //    //    int startIndx = xml.IndexOf("<head>");
        //    //    int endIndx = xml.IndexOf("</head>");
        //    //    xml = xml.Remove(startIndx + 6, endIndx - (startIndx + 6));

        //    //    //remove unneeded nodes
        //    //    RemoveXmlNode(ref xml, "script", 0);
        //    //    RemoveXmlNode(ref xml, "style", 0);
        //    //}
        //    int step = 0;
        //    try
        //    {
        //        step = 1;
        //        // pageSourceString = AppiumDriver.GetPageSource();
        //        pageSourceTextViewer.Text = pageSourceString;
        //        step = 2;
        //        pageSourceXml = new XmlDocument();
        //        pageSourceXml.LoadXml(pageSourceString);
        //        pageSourceXMLViewer.xmlDocument = pageSourceXml;
        //    }
        //    catch (Exception ex)
        //    {
        //        //if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.AndroidBrowser ||
        //        //                AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.iOSBrowser)
        //        //    Reporter.ToLog(eLogLevel.ERROR, "Failed to get mobile page source or convert it to XML format", ex);
        //        //else
        //        //    Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, ex.Message);

        //        if (step == 1)
        //        {
        //            pageSourceXml = null;
        //            pageSourceXMLViewer.xmlDocument = null;
        //            pageSourceTextViewer.Text = string.Empty;
        //        }
        //        else
        //        {
        //            pageSourceXml = null;
        //            pageSourceXMLViewer.xmlDocument = null;
        //        }
        //    }
        //}


        //TODO: fixme
        private void DesignSourceTabContent()
        {
            string XML = mAndroidADBDriver.GetPageSource();

            XmlDocument PageSourceXml = new XmlDocument();
            PageSourceXml.LoadXml(XML);

            pageSourceXMLViewer.xmlDocument = PageSourceXml;
            pageSourceTextViewer.Text = XML;

            if(sourceXMLRadioBtn.IsChecked == true)
            {
                //if (pageSourceXMLViewer == null || pageSourceTextViewer == null)
                //    return;

                //show XML
                try
                {
                    if (pageSourceXMLViewer.xmlDocument != null)
                    {
                        //if (pageSourceXMLViewer.xmlDocument != pageSourceXml)
                        //    pageSourceXMLViewer.xmlDocument = pageSourceXml;
                        pageSourceXMLViewer.Visibility = System.Windows.Visibility.Visible;
                        sourceLbl.Visibility = System.Windows.Visibility.Collapsed;
                        pageSourceTextViewer.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        //failed to load the XML view
                        sourceLbl.Content = "XML View Failure";
                        sourceLbl.Visibility = System.Windows.Visibility.Visible;
                        pageSourceXMLViewer.Visibility = System.Windows.Visibility.Collapsed;
                        pageSourceTextViewer.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                catch(Exception ex)
                {
                    //failed to load the XML view
                    sourceLbl.Content = "XML View Failure";
                    sourceLbl.Visibility = System.Windows.Visibility.Visible;
                    pageSourceXMLViewer.Visibility = System.Windows.Visibility.Collapsed;
                    pageSourceTextViewer.Visibility = System.Windows.Visibility.Collapsed;
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                }
            }
            else
            {
                //show text
                //pageSourceTextViewer.Text = pageSourceString;
                pageSourceTextViewer.Visibility = System.Windows.Visibility.Visible;
                sourceLbl.Visibility = System.Windows.Visibility.Collapsed;
                pageSourceXMLViewer.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void RemoveXmlNode(ref string xml, string nodeName, int searchStartIndx)
        {
            try
            {
                string startNode = "<" + nodeName + ">";
                string endNode = "</" + nodeName + ">";
                int startIndx = xml.IndexOf(startNode, searchStartIndx);
                if (startIndx == -1)
                    return;
                else
                    RemoveXmlNode(ref xml, nodeName, startIndx + 9);//remove the more dipper node
                //remove node
                int endIndx = xml.IndexOf(endNode, startIndx+9);
                if (endIndx != -1 && endIndx > startIndx)
                {
                    xml = xml.Remove(startIndx, endIndx - startIndx + 9);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                return;
            }
        }

        

        private void RecordAction(long pointOnMobile_X, long pointOnMobile_Y, ActGenElement.eGenElementAction actionType)
        {
            //try
            //{
            //    //get the action with element identification
            //    ActGenElement elemntAct = IdentifyClickedElement(pointOnMobile_X, pointOnMobile_Y);
            //    elemntAct.Platform = Platforms.Platform.eType.Mobile;

            //    //add action by type
            //    switch (actionType)
            //    {
            //        case ActGenElement.eGenElementAction.Click:
            //            elemntAct.GenElementAction = actionType;
            //            elemntAct.Description = "Clicking on " + elemntAct.LocateValue;
            //            BF.CurrentActivity.Acts.Add(elemntAct);
            //            if (InspectBtn.IsChecked == true) Reporter.ToUser(eUserMsgKeys.MobileActionWasAdded);
            //            break;

            //        case ActGenElement.eGenElementAction.SetValue:
            //            elemntAct.GenElementAction = actionType;
            //            elemntAct.Description = "Set value to " + elemntAct.LocateValue;
            //            string value = "";
            //            if (InputBoxWindow.OpenDialog("Set Element Value", "Value to Set:", ref value))
            //            {
            //                elemntAct.AddOrUpdateInputParam("Value", value);
            //                BF.CurrentActivity.Acts.Add(elemntAct);
            //                if (InspectBtn.IsChecked == true) Reporter.ToUser(eUserMsgKeys.MobileActionWasAdded);
            //            }
            //            break;

            //        case ActGenElement.eGenElementAction.GetCustomAttribute:
            //            elemntAct.GenElementAction = actionType;
            //            elemntAct.Description = "Get Attribute of " + elemntAct.LocateValue;
            //            string Attribute = "";
            //            if (InputBoxWindow.OpenDialog("Set Element Attribute", "Wanted Attribute Name:", ref Attribute))
            //            {
            //                elemntAct.AddOrUpdateInputParam("Value", Attribute);
            //                BF.CurrentActivity.Acts.Add(elemntAct);
            //                if (InspectBtn.IsChecked == true) Reporter.ToUser(eUserMsgKeys.MobileActionWasAdded);
            //            }
            //            break;

            //        case ActGenElement.eGenElementAction.Visible:
            //            elemntAct.GenElementAction = actionType;
            //            elemntAct.Description = "Validate visibility of " + elemntAct.LocateValue;
            //            BF.CurrentActivity.Acts.Add(elemntAct);
            //            if (InspectBtn.IsChecked == true) Reporter.ToUser(eUserMsgKeys.MobileActionWasAdded);
            //            break;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Reporter.ToLog(eLogLevel.ERROR, "Failed to record the mobile action", ex);
            //}
        }

        //private void AllowRunningAddedAction(Action action)
        //{
        //    if (InspectBtn.IsChecked == true)
        //    {
        //     if((Reporter.ToUser(eUserMsgKeys.MobileActionWasAdded)) == MessageBoxResult.Yes)
        //     { }
        //    }
        //}

        //private void ShowSelectedElementDetails(long pointOnMobile_X, long pointOnMobile_Y)
        private void ShowSelectedElementDetails(System.Windows.Point clickedPoint)
        {
            //try
            //{
            //    //calculate cliked point on mobile
            //    System.Windows.Point pointOnMobile = GetPointOnMobile(clickedPoint);
            //    long pointOnMobile_X = (long)pointOnMobile.X;
            //    long pointOnMobile_Y = (long)pointOnMobile.Y;
            //    inspectedElem_X = pointOnMobile_X;
            //    inspectedElem_Y = pointOnMobile_Y;

            //    //get the clicked element
            //    XmlNode inspectElementNode = FindElementXmlNodeByXY(pointOnMobile_X, pointOnMobile_Y);
            //    if (inspectElementNode != null && inspectElementNode != prevInspectElementNode)
            //    {
            //        //show panl
            //        SetAttributesActionsView(true);

            //        //update the attributes details
            //        elementAttributesDetails.Text = string.Empty;
            //        elementAttributesDetails.Text = inspectElementNode.LocalName + ":" + System.Environment.NewLine;
            //        foreach (XmlAttribute attribute in inspectElementNode.Attributes)
            //            elementAttributesDetails.Text += attribute.Name + "=   '" + attribute.Value + "'" + System.Environment.NewLine;

            //        //mark the element bounds on image
            //        DrawElementRectangle(inspectElementNode);

            //        //TODO: select the node in the xml

            //        prevInspectElementNode = inspectElementNode;
            //    }
            //}
            //catch(Exception ex)
            //{
            //    Reporter.ToUser(eUserMsgKeys.MobileShowElementDetailsFailed, ex.Message);
            //}
        }

        private void DeviceImageMouseSwipe(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            //get coordinates gap
            //double xGap = endPoint.X - startPoint.X;
            //double yGap = endPoint.Y - startPoint.Y;
            //try
            //{
            //    //check which type of swipe
            //    //if (endPoint.X > startPoint.X)
            //    if ((xGap > 0) && (xGap>=5))
            //    {
            //        //swip right
            //        mAndroidADBDriver.SwipeScreen(AndroidADBDriver.eSwipeSide.Right);
            //        if (RecordBtn.IsChecked == true)
            //            BF.CurrentActivity.Acts.Add(new ActMobileDevice() { Active = true, Description = "Swipe Right", MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.SwipeRight, Platform = Platforms.Platform.eType.Mobile });

            //    }
            //    //else if (endPoint.X < startPoint.X)
            //    else if ((xGap < 0) && ((xGap*-1) >= 5))
            //    {
            //        //swip left
            //        mAndroidADBDriver.SwipeScreen(AndroidADBDriver.eSwipeSide.Left);
            //        if (RecordBtn.IsChecked == true)
            //            BF.CurrentActivity.Acts.Add(new ActMobileDevice() { Active = true, Description = "Swipe Left", MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.SwipeLeft, Platform = Platforms.Platform.eType.Mobile });
            //    }
            //    //else if (endPoint.Y > startPoint.Y)
            //    if ((yGap > 0) && (yGap >= 5))
            //    {
            //        //swip down
            //        mAndroidADBDriver.SwipeScreen(AndroidADBDriver.eSwipeSide.Down);
            //        if (RecordBtn.IsChecked == true)
            //            BF.CurrentActivity.Acts.Add(new ActMobileDevice() { Active = true, Description = "Swipe Down", MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.SwipeDown, Platform = Platforms.Platform.eType.Mobile });
            //    }
            //    //else if (endPoint.Y < startPoint.Y)
            //    else if ((yGap < 0) && ((yGap * -1) >= 5))
            //    {
            //        //swip up
            //        mAndroidADBDriver.SwipeScreen(AndroidADBDriver.eSwipeSide.Up);
            //        if (RecordBtn.IsChecked == true)
            //            BF.CurrentActivity.Acts.Add(new ActMobileDevice() { Active = true, Description = "Swipe Up", MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.SwipeUp, Platform = Platforms.Platform.eType.Mobile });
            //    }

            //    //update the screen
            //    UpdateDeviceScreenShot();
            //}
            //catch (Exception ex)
            //{
            //    Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, ex.Message);
            //}
        }

        private void DeviceImageMouseDrag(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            //try
            //{
            //    mAndroidADBDriver.DoDrag(Convert.ToInt32(startPoint.X), Convert.ToInt32(startPoint.Y),
            //                        Convert.ToInt32(endPoint.X), Convert.ToInt32(endPoint.Y));
            //    if (RecordBtn.IsChecked == true)
            //        BF.CurrentActivity.Acts.Add(new ActMobileDevice()
            //        {
            //            Active = true,
            //            Description = "Do Drag XY to XY",
            //            MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.DragXYXY,
            //            Value = Convert.ToInt32(startPoint.X).ToString() + "," + Convert.ToInt32(startPoint.Y).ToString() + "," + Convert.ToInt32(endPoint.X).ToString() + "," + Convert.ToInt32(endPoint.Y).ToString(),
            //            Platform = Platforms.Platform.eType.Mobile
            //        });

            //    //update the screen
            //    LoadMobileScreenImage(true, 300);
            //}
            //catch (Exception ex)
            //{
            //    Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, ex.Message);
            //}
        }

        private void DrawElementRectangle(XmlNode clickedElementNode)
        {
            try
            {
                //remove previous rectangle
                //RemoveElemntRectangle();

                rectangleXmlNode = clickedElementNode;

                string bounds = rectangleXmlNode.Attributes["bounds"].Value;
                bounds = bounds.Replace("[", ",");
                bounds = bounds.Replace("]", ",");
                string[] boundsXY = bounds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (boundsXY.Count() == 4)
                {
                    int element_Start_X = (Convert.ToInt16(boundsXY[0]));
                    int element_Start_Y = (Convert.ToInt16(boundsXY[1]));
                    int element_End_X = (Convert.ToInt16(boundsXY[2]));
                    int element_End_Y = (Convert.ToInt16(boundsXY[3]));

                    mDeviceViewPage.SetHighLighter(element_Start_X, element_Start_Y, element_End_X - element_Start_X, element_End_Y - element_Start_Y);
                    // mDeviceViewPage.SetHighLighter(0, 0, 20, 20);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to draw device element rectangle", ex);
            }
        }

        private void RemoveElemntRectangle()
        {
            //if (DeviceImageCanvas != null)
            //{
            //    if (DeviceImageCanvas.Children.Contains(mHighightRectangle))
            //        DeviceImageCanvas.Children.Remove(mHighightRectangle);
            //}
        }


        //private void SetConfigurationsPanelView(bool show)
        //{
        //    if (show == true)
        //    {
        //        ConfigurationsFrame.Visibility = System.Windows.Visibility.Visible;
        //        configurationsSplitter.Visibility = System.Windows.Visibility.Visible;
        //        //ConfigurationsCol.Width = new GridLength(25, GridUnitType.Star);
        //        ConfigurationsCol.Width = new GridLength(250);
        //        this.Width = this.Width + Convert.ToDouble(ConfigurationsCol.Width.ToString());
        //        //this.Width = this.Width + ConfigurationsCol.ActualWidth;
        //    }
        //    else
        //    {
        //        ConfigurationsFrame.Visibility = System.Windows.Visibility.Collapsed;
        //        configurationsSplitter.Visibility = System.Windows.Visibility.Collapsed;
        //        this.Width = this.Width - ConfigurationsCol.ActualWidth;
        //        ConfigurationsCol.Width = new GridLength(0);
        //    }
        //}

        //private void SetAttributesActionsView(bool show)
        //{
        //    if (show == true)
        //    {
        //        selectElmtLbl.Visibility = System.Windows.Visibility.Collapsed;
        //        attributesStckPnl.Visibility = System.Windows.Visibility.Visible;
        //        if (BF != null)
        //        {
        //            actionsStckPnl.Visibility = System.Windows.Visibility.Visible;
        //            attributesActionsSpliter.Visibility = System.Windows.Visibility.Visible;
        //        }
        //    }
        //    else
        //    {
        //        selectElmtLbl.Visibility = System.Windows.Visibility.Visible;
        //        attributesStckPnl.Visibility = System.Windows.Visibility.Collapsed;
        //        actionsStckPnl.Visibility = System.Windows.Visibility.Collapsed;
        //        attributesActionsSpliter.Visibility = System.Windows.Visibility.Collapsed;
        //        RemoveElemntRectangle();
        //    }
        //}

        

        public void HighLightElement(AndroidElementInfo AEI)
        {
            DrawElementRectangle(AEI.XmlNode);            
        }

        public void StartRecording()
        {
            RecordBtn.IsChecked = true;        
        }

        internal void StopRecording()
        {
            RecordBtn.IsChecked = false;
        }

        bool IsRecording
        {
            get
            {
                if (RecordBtn.IsChecked == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    
        

        #endregion Functions

        

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            //TODO: check e.key and do keyevent for delete backspace etc..., or send text for simple char
            // Create amp and handle key event
            // meanwhile we send the text...
            if (!mSendKeyboardKeys) return;


            string AndroidKey = GetAndroidKey(e.Key);

            
            if (AndroidKey != null)
            {
                if (IsRecording)
                {
                    ActDeviceButton act = new ActDeviceButton();
                    act.Description = "Press Key '" + AndroidKey + "'";
                    act.Value = AndroidKey;
                    mBusinessFlow.AddAct(act);
                }

                // TODO: check if we use UIAutomation PressKey with some keys maps if it is faster then shell command
                string result = mAndroidADBDriver.ExecuteShellCommand("input text " + AndroidKey);
            }
        }

        string GetAndroidKey(Key k)
        {

            //TODO: test/add all keyboard keys
            switch (k)
            {
                case Key.RightCtrl:
                case Key.LeftCtrl:                    
                    return null; // We ignore some keys

                case Key.D0 : return "0";
                case Key.D1 : return "1";
                case Key.D2 : return "2";
                case Key.D3 : return "3";
                case Key.D4 : return "4";
                case Key.D5 : return "5";
                case Key.D6 : return "6";
                case Key.D7 : return "7";
                case Key.D8 : return "8";
                case Key.D9 : return "9";                    
            }
                        
            // if (k == Key.Delete) return "???";

            return k.ToString();

            
        }


        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            string cmd = ADBCommandTextBox.Text;

            if (IsRecording)
            {
                ActShell act = new ActShell();
                act.Description = "Run Shell Command '" + cmd + "'";
                act.Value = cmd;
                mBusinessFlow.AddAct(act);
            }

            Stopwatch st = new Stopwatch();
            st.Start();
            
            string rc = mAndroidADBDriver.ExecuteShellCommand(cmd);
            st.Stop();            
            OutputTextBlock.Text = rc;
            ElapsedLabel.Content = "Elapsed: " + st.ElapsedMilliseconds + " ms";            

            
        }


      

        private void LiveRefreshCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (LiveRefreshCheckBox.IsChecked == true)
            {
                mLiveRefresh = true;
                StartLiveRefresh();
            }         
        }

        private void LiveRefreshCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mLiveRefresh = false;
        }


        internal System.Windows.Point GetMousePositionOnDevice()
        {
            return mDeviceViewPage.GetMousePosition();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            //UpdateDeviceScreenShot();
            //mLiveRefresh = true;
            //LiveRefreshCheckBox.IsChecked = true;            
            // StartLiveRefresh();
        }



        internal void CloseWindow()
        {
            mLiveRefresh = false;
            DeviceViewFrame = null;
            mAndroidADBDriver = null;
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void SendButton_Click(object sender, RoutedEventArgs e)
        //{
        //    mAndroidADBDriver.ExecuteShellCommand("input text " + SendKeysTextBox.Text);

        //    if (IsRecording)
        //    {
        //        ActDeviceButton act = new ActDeviceButton();
        //        act.Description = "Input '" + SendKeysTextBox.Text + "'";
        //        act.Value = SendKeysTextBox.Text;
        //        mBusinessFlow.AddAct(act);

        //    }
        //}

        private void ConfigurationsBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: open device config page
        }

        private void SetValueButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: create an action to get the focusable control from the driver on the device
            List<ElementInfo> list = ((IWindowExplorer)mAndroidADBDriver).GetVisibleControls(null);
            // get from the list only text Edit
            foreach (ElementInfo EI in list)
            {
                if (EI.ElementType == "android.widget.EditText")  // check if it will work on all apps, might need to rethink 
                {
                    ObservableList<ControlProperty> props = EI.GetElementProperties();
                    ControlProperty cp = (from x in props where x.Name == "focused" select x).FirstOrDefault();

                    //FIXME: temp just to try, need to get the best locator for element.
                    ControlProperty cpresourceid = (from x in props where x.Name == "resource-id" select x).FirstOrDefault();

                    if (cp.Value == "true")
                    {
                        // mDeviceViewPage.AddTextBox()
                        ActUIElement a = new ActUIElement();
                        a.ElementLocateBy = eLocateBy.ByResourceID;
                        a.ElementType = eElementType.TextBox;
                        a.ElementAction = ActUIElement.eElementAction.SetText;

                        // Need to set value in both since sending direct to driver
                        // a.ElementLocateValue = cpresourceid.Value;
                        a.GetOrCreateInputParam(ActUIElement.Fields.ElementLocateValue).ValueForDriver = cpresourceid.Value;
                        

                        a.GetOrCreateInputParam(ActUIElement.Fields.Value).ValueForDriver = SetValueTextBox.Text;
                        

                        // a.Value = SendKeysTextBox.Text;
                        mAndroidADBDriver.RunAction(a);


                        if (IsRecording)
                        {
                            ControlProperty desc = (from x in props where x.Name == "content-desc" select x).FirstOrDefault();
                            if (string.IsNullOrEmpty(desc.Value))
                            {
                                desc = cpresourceid;
                            }
                            a.Description = "Set '" + desc.Value + "' value to '" + SetValueTextBox.Text +  "'";
                            a.GetOrCreateInputParam(ActUIElement.Fields.Value).Value = SetValueTextBox.Text;
                            a.GetOrCreateInputParam(ActUIElement.Fields.ElementLocateValue).Value = cpresourceid.Value;
                            
                            mBusinessFlow.AddAct(a);
                        }

                        return;
                    }
                }
            }
            
            

        }

        private void SetValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetValueButton_Click(sender, e);
            }
        }

        internal void LostConenction()
        {            
            Reporter.ToUser(eUserMsgKeys.LostConnection, "Lost connection with the device");
            CloseWindow();
        }
    }
}
