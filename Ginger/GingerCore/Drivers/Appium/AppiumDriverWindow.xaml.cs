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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenQA.Selenium;
using System.Threading;
using GingerCore.Actions;
using System.Xml;
using GingerCore.GeneralLib;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Drivers.Appium
{
    /// <summary>
    /// Interaction logic for AppiumDriverWindow.xaml
    /// </summary>
    public partial class AppiumDriverWindow : Window
    {
        public SeleniumAppiumDriver AppiumDriver;

        bool isMousePressed = false;
        bool isItDragAction = false;
        System.Windows.Point mouseStartPoint;
        System.Windows.Point mouseEndPoint;
        public BusinessFlow BF;

        XmlDocument pageSourceXml = null;
        long inspectedElem_X;
        long inspectedElem_Y;
        System.Windows.Shapes.Rectangle mHighightRectangle;
        long rectangleStartPoint_X;
        long rectangleStartPoint_Y;
        XmlNode rectangleXmlNode;
        XmlNode prevInspectElementNode;
        string pageSourceString = string.Empty;

        public AppiumDriverWindow()
        {
            InitializeComponent();            
        }

        #region Events
        private void DeviceImageCanvas_SizeChange(object sender, SizeChangedEventArgs e)
        {
            if (DeviceImageCanvas != null)
            {
                //correct the rectangle position
                if (DeviceImageCanvas.Children.Contains(mHighightRectangle))
                    DrawElementRectangle(rectangleXmlNode);
            }
        }

        private void DeviceImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DeviceImageCanvas != null)
            {
                //correct the device buttons location
                if (DeviceImage != null && DeviceImage.ActualWidth > 0)
                {
                    double emptySpace = ((DeviceImageCanvas.ActualWidth - DeviceImage.ActualWidth) / 2);
                    Thickness margin = backBtn.Margin;
                    margin.Right = 10 + emptySpace;
                    backBtn.Margin = margin;
                    margin = menuBtn.Margin;
                    margin.Left = 10 + emptySpace;
                    menuBtn.Margin = margin;
                }
            }
        }

        private void DeviceImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {  
            if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser ||
                        AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser)
            {
                e.Handled = true;
                return;
            }

            try
            {
                mouseStartPoint = e.GetPosition((System.Windows.Controls.Image)sender);
            }
            catch
            {
                mouseStartPoint = e.GetPosition((System.Windows.Shapes.Rectangle)sender);
                //convert to image scale
                mouseStartPoint.X = mouseStartPoint.X + rectangleStartPoint_X;
                mouseStartPoint.Y = mouseStartPoint.Y + rectangleStartPoint_Y;
            }
            isMousePressed = true;
        }

        private void DeviceImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser ||
            AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser)
            {
                e.Handled = true;
                return;
            }

            if (InspectorPointBtn.IsChecked == true)
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Cross;
                System.Windows.Point pointOnImage;
                try
                {
                    pointOnImage = e.GetPosition((System.Windows.Controls.Image)sender);
                }
                catch
                {
                    pointOnImage = e.GetPosition((System.Windows.Shapes.Rectangle)sender);
                    //convert to image scale
                    pointOnImage.X = pointOnImage.X + rectangleStartPoint_X;
                    pointOnImage.Y = pointOnImage.Y + rectangleStartPoint_Y;
                }

                //ShowSelectedElementDetails((long)pointOnMobile.X, (long)pointOnMobile.Y);
                ShowSelectedElementDetails(pointOnImage);
            }
            else
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Hand;

            if (isMousePressed == true)
            {
                //it's a drag
                isItDragAction = true;
            }
        }

        private void DeviceImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser ||
            AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser)
            {
                e.Handled = true;
                return;
            }

            try
            {
                mouseEndPoint = e.GetPosition((System.Windows.Controls.Image)sender);
            }
            catch
            {
                mouseEndPoint = e.GetPosition((System.Windows.Shapes.Rectangle)sender);
                //convert to image scale
                mouseEndPoint.X = mouseEndPoint.X + rectangleStartPoint_X;
                mouseEndPoint.Y = mouseEndPoint.Y + rectangleStartPoint_Y;
            }
            isMousePressed = false;
            if (InspectorPointBtn.IsChecked == false)
            {
                if (isItDragAction == true)
                {
                    //do drag
                    isItDragAction = false;
                    DeviceImageMouseDrag(mouseStartPoint, mouseEndPoint);
                }
                else
                {
                    //do click
                    DeviceImageMouseClick(mouseEndPoint);
                }
            }
            else
            {
                //show selected element details
                DeviceImageMouseClick(mouseEndPoint);
            }
        }

        private void DeviceImage_MouseEnter(object sender, MouseEventArgs e)
        {
            if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser ||
            AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser)
            {
                e.Handled = true;
                return;
            }

            if (InspectorPointBtn.IsChecked == true)
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Cross;
            else
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Hand;
        }

        private void DeviceImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser ||
            AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser)
            {
                e.Handled = true;
                return;
            }

            Mouse.OverrideCursor = null;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMobileScreenImage(false);
        }
       

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            AppiumDriver.PressBackBtn();
            //update the screen
            LoadMobileScreenImage(true, 300);
            if (RecordBtn.IsChecked == true)
                BF.CurrentActivity.Acts.Add(new ActMobileDevice() { Active = true, Description = "Click Device Back button", MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.PressBackButton, Platform = GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Mobile });
        }

        private void homeBtn_Click(object sender, RoutedEventArgs e)
        {
            AppiumDriver.PressHomebtn();
            LoadMobileScreenImage(true, 300);
            if (RecordBtn.IsChecked == true)
                BF.CurrentActivity.Acts.Add(new ActMobileDevice() { Active = true, Description = "Click Device Home button", MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.PressHomeButton, Platform = ePlatformType.Mobile });
        }

        private void menuBtn_Click(object sender, RoutedEventArgs e)
        {
            AppiumDriver.PressMenubtn();
            LoadMobileScreenImage(true, 300);
            if (RecordBtn.IsChecked == true)
                BF.CurrentActivity.Acts.Add(new ActMobileDevice() { Active = true, Description = "Click Device Menu button", MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.PressMenuButton, Platform= ePlatformType.Mobile });
        }

        private void RecordBtn_Click(object sender, RoutedEventArgs e)
        {
            //close inspect if open
            if (InspectBtn.IsChecked == true)
            {
                SetInspectorPanelView(false);
                RemoveElemntRectangle();
                InspectBtn.IsChecked = false;
            }

            StorePageSource();
        }

        private void ConfigurationsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigurationsBtn.IsChecked==true)
            {
                SetConfigurationsPanelView(true);
            }
            else
            {
                SetConfigurationsPanelView(false);
            }
        }

        private void InspectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (InspectBtn.IsChecked == true)
            {
                if (RecordBtn.IsChecked == false)
                    StorePageSource();
                else
                    RecordBtn.IsChecked = false;
                SetInspectorPanelView(true);
            }
            else
            {
                SetInspectorPanelView(false);
                SetAttributesActionsView(false);
                RemoveElemntRectangle();
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

        private void inspectorElementTabsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (inspectorElementTabsControl.SelectedIndex == 1)//Source tab
            {
                DesignSourceTabContent();
            }
        }

        private void InspectorPointBtn_Click(object sender, RoutedEventArgs e)
        {
            SetAttributesActionsView(false);
            if (InspectorPointBtn.IsChecked == false)
                Mouse.OverrideCursor = null;
        }

        private void actBtnClick_Click(object sender, RoutedEventArgs e)
        {
            RecordAction(inspectedElem_X, inspectedElem_Y, ActGenElement.eGenElementAction.Click);
        }

        private void actBtnSetValue_Click(object sender, RoutedEventArgs e)
        {
            RecordAction(inspectedElem_X, inspectedElem_Y, ActGenElement.eGenElementAction.SetValue);
        }

        private void actBtnGetElementAttr_Click(object sender, RoutedEventArgs e)
        {
            RecordAction(inspectedElem_X, inspectedElem_Y, ActGenElement.eGenElementAction.GetCustomAttribute);
        }

        private void actBtnValidateVisible_Click(object sender, RoutedEventArgs e)
        {
            RecordAction(inspectedElem_X, inspectedElem_Y, ActGenElement.eGenElementAction.Visible);
        }

        private void sourceXMLRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            DesignSourceTabContent();
        }

        private void sourceXMLRadioBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            DesignSourceTabContent();
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
                if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser ||
                        AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser)
                    this.Title = AppiumDriver.DeviceName + " " + AppiumDriver.DriverDeviceType.ToString() + " Browser";
                else
                    this.Title = AppiumDriver.DeviceName + " " + AppiumDriver.DriverDeviceType.ToString();

                //don't track actions if asked in agent
                if (AppiumDriver.RefreshDeviceScreenShots != null && AppiumDriver.RefreshDeviceScreenShots.Trim().ToUpper() != "YES")
                    TrackActionsChK.IsChecked = false;

                //hide optional columns
                this.Width = 300;
                ConfigurationsFrame.Visibility = System.Windows.Visibility.Collapsed;
                ConfigurationsCol.Width = new GridLength(0);
                InspectorFrame.Visibility = System.Windows.Visibility.Collapsed;
                InspectorCol.Width = new GridLength(0);
                SetAttributesActionsView(false);

                //don't allow to record if no business flow
                if (BF == null)
                {
                    RecordBtn.Visibility = System.Windows.Visibility.Collapsed;
                    actionsStckPnl.Visibility = System.Windows.Visibility.Collapsed;
                }

                //don't allow record in browser mode
                if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser ||
                        AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser)
                {
                    RecordBtn.Visibility = System.Windows.Visibility.Collapsed;
                }

                //show device btns according to type
                switch (AppiumDriver.DriverPlatformType)
                {
                    case SeleniumAppiumDriver.eSeleniumPlatformType.Android:
                        //all buttons as default
                        break;
                    case SeleniumAppiumDriver.eSeleniumPlatformType.iOS:
                        //only middle btn althogh not supported yet (apple limitation)
                        backBtn.Visibility = System.Windows.Visibility.Collapsed;
                        menuBtn.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    case SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser:
                        //browser mode- show buttons but disabled
                        backBtn.IsEnabled = false;
                        menuBtn.IsEnabled = false;
                        homeBtn.IsEnabled = false;
                        break;
                    case SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser:
                        //browser mode- show buttons but disabled
                        backBtn.Visibility = System.Windows.Visibility.Collapsed;
                        menuBtn.Visibility = System.Windows.Visibility.Collapsed;
                        homeBtn.IsEnabled = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error occurred while designing the Mobile window initial look", ex);
            }
        }

        public bool LoadMobileScreenImage(bool wait = false, Int32 waitingTimeInMiliSeconds = 2000)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            //wait before taking screen shot
            if (wait)
            {
                //Thread.Sleep(waitingTimeInMiliSeconds);
                Thread.Sleep(waitingTimeInMiliSeconds * (int.Parse(RefreshWaitingRateCombo.Text.ToString())));
            }

            //take screen shot
            try
            {
                Screenshot SC = AppiumDriver.GetScreenShot();
                UpdateDriverImageFromScreenshot(SC);

                return true;
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                Reporter.ToUser(eUserMsgKeys.MobileRefreshScreenShotFailed, ex.Message);
                AppiumDriver.ConnectedToDevice = false;
                
                return false;               
            }
        }

       public void UpdateDriverImageFromScreenshot(Screenshot SC)
        {
            var image = new BitmapImage();
            using (var ms = new System.IO.MemoryStream(SC.AsByteArray))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
            }
            DeviceImage.Source = image;

            //take the page source if needed
            if (InspectBtn.IsChecked == true || RecordBtn.IsChecked == true)
            {
                StorePageSource();
                if (InspectBtn.IsChecked == true)
                    SetAttributesActionsView(false);
            }

            Mouse.OverrideCursor = null;
        }

        public void ShowActionEfect(bool wait = false, Int32 waitingTimeInMiliSeconds = 2000)
        {
            if (TrackActionsChK.IsChecked == true)
                LoadMobileScreenImage(wait, waitingTimeInMiliSeconds);
        }

        private System.Windows.Point GetPointOnMobile(System.Windows.Point pointOnImage)
        {
            System.Windows.Point pointOnMobile = new System.Windows.Point();
            double ratio_X=1;
            double ratio_Y=1;
            
            if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.iOS &&
                    AppiumDriver.DriverDeviceType == SeleniumAppiumDriver.eDeviceType.Phone)
            {
                ratio_X = (DeviceImage.Source.Width / 2) / DeviceImage.ActualWidth;
                ratio_Y = (DeviceImage.Source.Height / 2) / DeviceImage.ActualHeight;
            }
            else
            {
                ratio_X = DeviceImage.Source.Width / DeviceImage.ActualWidth;
                ratio_Y = DeviceImage.Source.Height / DeviceImage.ActualHeight;
            }

            pointOnMobile.X = (long)(pointOnImage.X * ratio_X);
            pointOnMobile.Y = (long)(pointOnImage.Y * ratio_Y);

            return pointOnMobile;
        }

        private void DeviceImageMouseClick(System.Windows.Point clickedPoint)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            try
            {
                //calculate clicked X,Y
                System.Windows.Point pointOnMobile = GetPointOnMobile(clickedPoint);
                long pointOnMobile_X = (long)pointOnMobile.X;
                long pointOnMobile_Y = (long)pointOnMobile.Y;

                if (InspectorPointBtn.IsChecked == false)
                {
                    //record action
                    if (RecordBtn.IsChecked == true)
                    {
                        RecordAction(pointOnMobile_X, pointOnMobile_Y, ActGenElement.eGenElementAction.Click);
                    }

                    //click the element
                    AppiumDriver.TapXY(pointOnMobile_X, pointOnMobile_Y);

                    //update the screen
                    LoadMobileScreenImage(true, 1000);
                }
                else
                {
                    ShowSelectedElementDetails(clickedPoint);
                    InspectorPointBtn.IsChecked = false;
                    Mouse.OverrideCursor = null;
                }
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, ex.Message);
            }
        }

        private async void StorePageSource()
        {
            LoadingLabel.Visibility = Visibility.Visible;
            inspectorElementTabsControl.Visibility = Visibility.Hidden;
            int step = 0;
            try
            {
                step = 1;
                pageSourceString = await AppiumDriver.GetPageSource();
                pageSourceTextViewer.Text = pageSourceString;
                step = 2;
                pageSourceXml = new XmlDocument();
                pageSourceXml.LoadXml(pageSourceString);
                pageSourceXMLViewer.xmlDocument = pageSourceXml;
            }
            catch (Exception ex)
            {
                if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser ||
                                AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser)
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to get mobile page source or convert it to XML format", ex);
                else
                    Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, ex.Message);

                if (step == 1)
                {
                    pageSourceXml = null;
                    pageSourceXMLViewer.xmlDocument = null;
                    pageSourceTextViewer.Text = string.Empty;
                }
                else
                {
                    pageSourceXml = null;
                    pageSourceXMLViewer.xmlDocument = null;
                }
            }

            LoadingLabel.Visibility = Visibility.Hidden;
            inspectorElementTabsControl.Visibility = Visibility.Visible;
        }

        private void DesignSourceTabContent()
        {
            if(sourceXMLRadioBtn.IsChecked == true)
            {
                if (pageSourceXMLViewer == null || pageSourceTextViewer == null)
                    return;

                //show XML
                try
                {
                    if (pageSourceXMLViewer.xmlDocument != null)
                    {
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
                pageSourceTextViewer.Visibility = System.Windows.Visibility.Visible;
                sourceLbl.Visibility = System.Windows.Visibility.Collapsed;
                pageSourceXMLViewer.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        
        public XmlNode GetElementXmlNodeFromMouse()
        {              
               System.Windows.Point pointOnImage = Mouse.GetPosition(DeviceImage);
               return FindElementXmlNodeByXY((long)pointOnImage.X, (long)pointOnImage.Y);                    
        }


        public XmlNode FindElementXmlNodeByXY(long pointOnMobile_X, long pointOnMobile_Y)
        {
            try
            {
                //get screen elements nodes
                XmlNodeList ElmsNodes;
                 // Do once?
                // if XMLSOurce changed we need to refresh
                //pageSourceString = AppiumDriver.GetPageSource();                                
                //pageSourceXml = new XmlDocument();
                //pageSourceXml.LoadXml(pageSourceString);
                // pageSourceXMLViewer.xmlDocument = pageSourceXml;   

                ElmsNodes = pageSourceXml.SelectNodes("//*");

                ///get the selected element from screen
                if (ElmsNodes != null && ElmsNodes.Count > 0)
                {
                    //move to collection for getting last node which fits to bounds
                    ObservableList<XmlNode> ElmsNodesColc = new ObservableList<XmlNode>();
                    foreach (XmlNode elemNode in ElmsNodes)
                    {
                        //if (mDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.iOS && elemNode.LocalName == "UIAWindow") continue;                        
                        //try { if (mDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.Android && elemNode.Attributes["focusable"].Value == "false") continue; }catch (Exception ex) { }
                        bool skipElement= false;
                        if (FilterElementsChK.IsChecked == true)
                        {
                            string[] filterList = FilterElementsTxtbox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            try
                            {
                                for (int indx = 0; indx < filterList.Length; indx++)
                                    if (elemNode.Name.Contains(filterList[indx].Trim()) ||
                                           elemNode.LocalName.Contains(filterList[indx].Trim()))
                                    {
                                        skipElement = true;
                                        break;
                                    }
                            }
                            catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
                        }

                        if (!skipElement)
                            ElmsNodesColc.Add(elemNode);
                    }

                    Dictionary<XmlNode, long> foundElements = new Dictionary<XmlNode, long>();
                    foreach (XmlNode elementNode in ElmsNodesColc.Reverse())
                    {
                        //get the element location
                        long element_Start_X = -1;
                        long element_Start_Y = -1;
                        long element_Max_X = -1;
                        long element_Max_Y = -1;

                        switch (AppiumDriver.DriverPlatformType)
                        {
                            case SeleniumAppiumDriver.eSeleniumPlatformType.Android:
                                try
                                {
                                    string bounds = elementNode.Attributes["bounds"].Value;
                                    bounds = bounds.Replace("[", ",");
                                    bounds = bounds.Replace("]", ",");
                                    string[] boundsXY = bounds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (boundsXY.Count() == 4)
                                    {
                                        element_Start_X = Convert.ToInt64(boundsXY[0]);
                                        element_Start_Y = Convert.ToInt64(boundsXY[1]);
                                        element_Max_X = Convert.ToInt64(boundsXY[2]);
                                        element_Max_Y = Convert.ToInt64(boundsXY[3]);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    element_Start_X = -1;
                                    element_Start_Y = -1;
                                    element_Max_X = -1;
                                    element_Max_Y = -1;
                                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                                }
                                break;

                            case SeleniumAppiumDriver.eSeleniumPlatformType.iOS:
                                try
                                {
                                    element_Start_X = Convert.ToInt64(elementNode.Attributes["x"].Value);
                                    element_Start_Y = Convert.ToInt64(elementNode.Attributes["y"].Value);
                                    element_Max_X = element_Start_X + Convert.ToInt64(elementNode.Attributes["width"].Value);
                                    element_Max_Y = element_Start_Y + Convert.ToInt64(elementNode.Attributes["height"].Value);
                                }
                                catch(Exception ex)
                                {
                                    element_Start_X = -1;
                                    element_Start_Y = -1;
                                    element_Max_X = -1;
                                    element_Max_Y = -1;
                                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                                }
                                break;
                        }


                        if (((pointOnMobile_X >= element_Start_X) && (pointOnMobile_X <= element_Max_X))
                                   && ((pointOnMobile_Y >= element_Start_Y) && (pointOnMobile_Y <= element_Max_Y)))
                        {
                            //object found                                
                            //return elementNode;
                            foundElements.Add(elementNode, ((element_Max_X - element_Start_X) * (element_Max_Y - element_Start_Y)));
                        }
                    }

                    //getting the smalles node size found
                    XmlNode foundNode = null;
                    long foundNodeSize = 0;
                    if (foundElements.Count > 0)
                    {
                        foundNode = foundElements.Keys.First();
                        foundNodeSize = foundElements.Values.First();
                    }
                    for (int indx = 0; indx < foundElements.Keys.Count; indx++)
                    {
                        if (foundElements.Values.ElementAt(indx) < foundNodeSize)
                        {
                            foundNode = foundElements.Keys.ElementAt(indx);
                            foundNodeSize = foundElements.Values.ElementAt(indx);
                        }
                    }
                    if (foundNode != null)
                        return foundNode;
                }

                return null;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                return null;
            }
        }

        private ActGenElement IdentifyClickedElement(long pointOnMobile_X, long pointOnMobile_Y)
        {
            ActGenElement elemntAct = new ActGenElement() { Active = true };
            try
            {
                //get the clicked element
                XmlNode clickedElementNode = FindElementXmlNodeByXY(pointOnMobile_X, pointOnMobile_Y);
                if (clickedElementNode != null)
                {
                    ////try to identify by priority
                    XmlNodeList validateElms;
                    //ID
                    string element_ID_attrib = string.Empty;
                    string element_ID_value=string.Empty;
                    try
                    {
                        switch(AppiumDriver.DriverPlatformType)
                        {
                            case SeleniumAppiumDriver.eSeleniumPlatformType.Android:
                                element_ID_attrib = "resource-id";
                                break;
                            case SeleniumAppiumDriver.eSeleniumPlatformType.iOS:
                                element_ID_attrib = "name";
                                break;
                        }
                        element_ID_value = clickedElementNode.Attributes[element_ID_attrib].Value;
                    }
                    catch { element_ID_value = string.Empty; }
                    if (!string.IsNullOrEmpty(element_ID_value))
                    {
                        //check if unique 
                        string xpath = "//*[@" + element_ID_attrib + "='" + element_ID_value + "']";
                        validateElms = pageSourceXml.SelectNodes(xpath);
                        if (validateElms != null && validateElms.Count == 1)
                        {
                            if (RecordByXpathChK.IsChecked == true)
                            {
                                elemntAct.LocateBy = eLocateBy.ByXPath;
                                elemntAct.LocateValue = xpath;
                            }
                            else
                            {
                                elemntAct.LocateBy = eLocateBy.ByID;
                                elemntAct.LocateValue = element_ID_value;
                            }
                            return elemntAct;
                        }
                    }
                    //Content Description
                    string element_Desc_attrib = string.Empty;
                    string element_Desc_value=string.Empty;
                    try
                    {
                        switch (AppiumDriver.DriverPlatformType)
                        {
                            case SeleniumAppiumDriver.eSeleniumPlatformType.Android:
                                element_Desc_attrib = "content-desc";
                                break;
                            case SeleniumAppiumDriver.eSeleniumPlatformType.iOS:
                                element_Desc_attrib = "hint";
                                break;
                        }
                        element_Desc_value = clickedElementNode.Attributes[element_Desc_attrib].Value;
                    }
                    catch { element_Desc_value = string.Empty; }
                    if (!string.IsNullOrEmpty(element_Desc_value))
                    {
                        string xpath = "//*[@" + element_Desc_attrib + "='" + element_Desc_value + "']";
                        validateElms = pageSourceXml.SelectNodes(xpath);
                        //check if unique
                        if (validateElms != null && validateElms.Count == 1)
                        {
                            //name is no more supported by appium , so we'll always record by xpath instead of name
                            elemntAct.LocateBy = eLocateBy.ByXPath;
                            elemntAct.LocateValue = xpath;
                            return elemntAct;
                        }
                    }
                    //Text
                    string element_Text_attrib = string.Empty;
                    string element_Text_value=string.Empty;
                    try
                    {
                        switch (AppiumDriver.DriverPlatformType)
                        {
                            case SeleniumAppiumDriver.eSeleniumPlatformType.Android:
                                element_Text_attrib = "text";
                                break;
                            case SeleniumAppiumDriver.eSeleniumPlatformType.iOS:
                                element_Text_attrib = "label";
                                break;
                        }
                        element_Text_value = clickedElementNode.Attributes[element_Text_attrib].Value;
                    }
                    catch { element_Text_value = string.Empty; }
                    if (!string.IsNullOrEmpty(element_Text_value))
                    {
                        //check if unique
                        string xpath = "//*[@" + element_Text_attrib + "='" + element_Text_value + "']";
                        validateElms = pageSourceXml.SelectNodes(xpath);
                        if (validateElms != null && validateElms.Count == 1)
                        {
                            //name is no more supported by appium , so we'll always record by xpath instead of name
                            elemntAct.LocateBy = eLocateBy.ByXPath;
                            elemntAct.LocateValue = xpath;
                       
                            return elemntAct;
                        }
                    }
                    //Xpath
                    string element_Xpath;
                    //get element class
                    string element_Class;
                    element_Class = clickedElementNode.LocalName;//TO DO: validate!!
                    if (element_Desc_value != element_Text_value)
                        element_Xpath = "*//" + element_Class +
                                            "[@" + element_ID_attrib + "='" + element_ID_value + "' and " +
                                            "@" + element_Text_attrib + "='" + element_Text_value + "' and " +
                                            "@" + element_Desc_attrib + "='" + element_Desc_value + "']";
                    else
                        element_Xpath = "*//" + element_Class +
                                            "[@" + element_ID_attrib + "='" + element_ID_value + "' and " +
                                            "@" + element_Text_attrib + "='" + element_Text_value + "']";
                    if (!string.IsNullOrEmpty(element_Xpath))
                    {
                        //check if unique:
                        validateElms = pageSourceXml.SelectNodes(element_Xpath);
                        if (validateElms != null && validateElms.Count != 1)
                        {
                            //add index
                            string element_Xpath_Orig = element_Xpath;
                            switch (AppiumDriver.DriverPlatformType)
                            {
                                case SeleniumAppiumDriver.eSeleniumPlatformType.Android:
                                    string element_Index;
                                    try
                                    {
                                        element_Index = clickedElementNode.Attributes["index"].Value;
                                    }
                                    catch { element_Index = string.Empty; }
                                    string element_Instance;
                                    try
                                    {
                                        element_Instance = clickedElementNode.Attributes["instance"].Value;
                                    }
                                    catch { element_Instance = string.Empty; }
                                    if (!string.IsNullOrEmpty(element_Index) || !string.IsNullOrEmpty(element_Instance))
                                        element_Xpath = element_Xpath.Replace("']", "' and @index='" + element_Index + "' and @instance='" + element_Instance + "']");
                                    break;

                                case SeleniumAppiumDriver.eSeleniumPlatformType.iOS:
                                    string element_path_attrib="path";
                                    string element_path_value = string.Empty;
                                    try
                                    {
                                        element_path_value = clickedElementNode.Attributes[element_path_attrib].Value;
                                    }
                                    catch { element_path_value = string.Empty; }
                                    if (!string.IsNullOrEmpty(element_path_value))
                                        element_Xpath = element_Xpath.Replace("']", "' and @" + element_path_attrib + "='" + element_path_value + "']");
                                    break;
                            }

                            if (element_Xpath_Orig != element_Xpath)
                            {
                                //check if unique
                                validateElms = pageSourceXml.SelectNodes(element_Xpath);
                                if (validateElms != null && validateElms.Count == 1)
                                {
                                    elemntAct.LocateBy = eLocateBy.ByXPath;
                                    elemntAct.LocateValue = element_Xpath;
                                    return elemntAct;
                                }
                            }
                        }
                        else
                        {
                            elemntAct.LocateBy = eLocateBy.ByXPath;
                            elemntAct.LocateValue = element_Xpath;
                            return elemntAct;
                        }
                    }
                }

                //identify by X,Y
                elemntAct.LocateBy = eLocateBy.ByXY;
                elemntAct.LocateValue = pointOnMobile_X + "," + pointOnMobile_Y;
                return elemntAct;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to identify the mobile element", ex);
                elemntAct.LocateBy = eLocateBy.ByXY;
                elemntAct.LocateValue = pointOnMobile_X + "," + pointOnMobile_Y;
                return elemntAct;
            }
        }

        private void RecordAction(long pointOnMobile_X, long pointOnMobile_Y, ActGenElement.eGenElementAction actionType)
        {
            try
            {
                //get the action with element identification
                ActGenElement elemntAct = IdentifyClickedElement(pointOnMobile_X, pointOnMobile_Y);
                elemntAct.Platform = ePlatformType.Mobile;

                //add action by type
                switch (actionType)
                {
                    case ActGenElement.eGenElementAction.Click:
                        elemntAct.GenElementAction = actionType;
                        elemntAct.Description = "Clicking on " + elemntAct.LocateValue;
                        BF.CurrentActivity.Acts.Add(elemntAct);
                        if (InspectBtn.IsChecked == true) Reporter.ToUser(eUserMsgKeys.MobileActionWasAdded);
                        break;

                    case ActGenElement.eGenElementAction.SetValue:
                        elemntAct.GenElementAction = actionType;
                        elemntAct.Description = "Set value to " + elemntAct.LocateValue;
                        string value = "";
                        if (InputBoxWindow.OpenDialog("Set Element Value", "Value to Set:", ref value))
                        {
                            elemntAct.AddOrUpdateInputParamValue("Value", value);
                            BF.CurrentActivity.Acts.Add(elemntAct);
                            if (InspectBtn.IsChecked == true) Reporter.ToUser(eUserMsgKeys.MobileActionWasAdded);
                        }
                        break;

                    case ActGenElement.eGenElementAction.GetCustomAttribute:
                        elemntAct.GenElementAction = actionType;
                        elemntAct.Description = "Get Attribute of " + elemntAct.LocateValue;
                        string Attribute = "";
                        if (InputBoxWindow.OpenDialog("Set Element Attribute", "Wanted Attribute Name:", ref Attribute))
                        {
                            elemntAct.AddOrUpdateInputParamValue("Value", Attribute);
                            BF.CurrentActivity.Acts.Add(elemntAct);
                            if (InspectBtn.IsChecked == true) Reporter.ToUser(eUserMsgKeys.MobileActionWasAdded);
                        }
                        break;

                    case ActGenElement.eGenElementAction.Visible:
                        elemntAct.GenElementAction = actionType;
                        elemntAct.Description = "Validate visibility of " + elemntAct.LocateValue;
                        BF.CurrentActivity.Acts.Add(elemntAct);
                        if (InspectBtn.IsChecked == true) Reporter.ToUser(eUserMsgKeys.MobileActionWasAdded);
                        break;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to record the mobile action", ex);
            }
        }
        
        private void ShowSelectedElementDetails(System.Windows.Point clickedPoint)
        {
            try
            {
                //calculate cliked point on mobile
                System.Windows.Point pointOnMobile = GetPointOnMobile(clickedPoint);
                long pointOnMobile_X = (long)pointOnMobile.X;
                long pointOnMobile_Y = (long)pointOnMobile.Y;
                inspectedElem_X = pointOnMobile_X;
                inspectedElem_Y = pointOnMobile_Y;

                //get the clicked element
                XmlNode inspectElementNode = FindElementXmlNodeByXY(pointOnMobile_X, pointOnMobile_Y);
                if (inspectElementNode != null && inspectElementNode != prevInspectElementNode)
                {
                    //show panl
                    SetAttributesActionsView(true);

                    //update the attributes details
                    elementAttributesDetails.Text = string.Empty;
                    elementAttributesDetails.Text = inspectElementNode.LocalName + ":" + System.Environment.NewLine;
                    foreach (XmlAttribute attribute in inspectElementNode.Attributes)
                        elementAttributesDetails.Text += attribute.Name + "=   '" + attribute.Value + "'" + System.Environment.NewLine;

                    //mark the element bounds on image
                    DrawElementRectangle(inspectElementNode);

                    //TODO: select the node in the xml

                    prevInspectElementNode = inspectElementNode;
                }
            }
            catch(Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.MobileShowElementDetailsFailed, ex.Message);
            }
        }

        private void DeviceImageMouseDrag(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            try
            {
                AppiumDriver.DoDrag(Convert.ToInt32(startPoint.X), Convert.ToInt32(startPoint.Y),
                                    Convert.ToInt32(endPoint.X), Convert.ToInt32(endPoint.Y));
                if (RecordBtn.IsChecked == true)
                    BF.CurrentActivity.Acts.Add(new ActMobileDevice()
                    {
                        Active = true,
                        Description = "Do Drag XY to XY",
                        MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.DragXYXY,
                        Value = Convert.ToInt32(startPoint.X).ToString() + "," + Convert.ToInt32(startPoint.Y).ToString() + "," + Convert.ToInt32(endPoint.X).ToString() + "," + Convert.ToInt32(endPoint.Y).ToString(),
                        Platform = ePlatformType.Mobile
                    });

                //update the screen
                LoadMobileScreenImage(true, 300);
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, ex.Message);
            }
        }

        private void DrawElementRectangle(XmlNode clickedElementNode)
        {
            try
            {
                //remove previous rectangle
                RemoveElemntRectangle();

                rectangleXmlNode = clickedElementNode;

                //get the element location
                double ratio_X = 0;
                double ratio_Y = 0;
                double element_Start_X = 0;
                double element_Start_Y = 0;
                double element_Max_X = 0;
                double element_Max_Y = 0;

                switch (AppiumDriver.DriverPlatformType)
                {
                    case SeleniumAppiumDriver.eSeleniumPlatformType.Android:
                        ratio_X = DeviceImage.Source.Width / DeviceImage.ActualWidth;
                        ratio_Y = DeviceImage.Source.Height / DeviceImage.ActualHeight;
                        string bounds = rectangleXmlNode.Attributes["bounds"].Value;
                        bounds = bounds.Replace("[", ",");
                        bounds = bounds.Replace("]", ",");
                        string[] boundsXY = bounds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (boundsXY.Count() == 4)
                        {
                            element_Start_X = (Convert.ToInt64(boundsXY[0])) / ratio_X;
                            rectangleStartPoint_X = (long)element_Start_X;
                            element_Start_Y = (Convert.ToInt64(boundsXY[1])) / ratio_Y;
                            rectangleStartPoint_Y = (long)element_Start_Y;
                            element_Max_X = (Convert.ToInt64(boundsXY[2])) / ratio_X;
                            element_Max_Y = (Convert.ToInt64(boundsXY[3])) / ratio_Y;
                        }
                        break;
                    case SeleniumAppiumDriver.eSeleniumPlatformType.iOS:
                        if (AppiumDriver.DriverDeviceType == SeleniumAppiumDriver.eDeviceType.Phone)
                        {
                            ratio_X = (DeviceImage.Source.Width / 2) / DeviceImage.ActualWidth;
                            ratio_Y = (DeviceImage.Source.Height / 2) / DeviceImage.ActualHeight;
                        }
                        else
                        {
                            ratio_X = DeviceImage.Source.Width / DeviceImage.ActualWidth;
                            ratio_Y = DeviceImage.Source.Height / DeviceImage.ActualHeight;
                        }
                        element_Start_X = (Convert.ToInt64(rectangleXmlNode.Attributes["x"].Value)) / ratio_X;
                        rectangleStartPoint_X = (long)element_Start_X;
                        element_Start_Y = (Convert.ToInt64(rectangleXmlNode.Attributes["y"].Value)) / ratio_Y;
                        rectangleStartPoint_Y = (long)element_Start_Y;
                        element_Max_X = element_Start_X + ((Convert.ToInt64(rectangleXmlNode.Attributes["width"].Value)) / ratio_X);
                        element_Max_Y = element_Start_Y + ((Convert.ToInt64(rectangleXmlNode.Attributes["height"].Value)) / ratio_Y);
                        break;
                }

                //draw the rectangle
                mHighightRectangle = new System.Windows.Shapes.Rectangle();
                mHighightRectangle.SetValue(Canvas.LeftProperty, element_Start_X + ((DeviceImageCanvas.ActualWidth - DeviceImage.ActualWidth) / 2));
                mHighightRectangle.SetValue(Canvas.TopProperty, element_Start_Y + ((DeviceImageCanvas.ActualHeight - DeviceImage.ActualHeight) / 2));
                mHighightRectangle.Width = (element_Max_X - element_Start_X);
                mHighightRectangle.Height = (element_Max_Y - element_Start_Y);
                mHighightRectangle.Fill = new SolidColorBrush() { Color = Colors.Red, Opacity = 0.5f };
                DeviceImageCanvas.Children.Add(mHighightRectangle);

                //bind events to device image events
                mHighightRectangle.MouseMove += DeviceImage_MouseMove;
                mHighightRectangle.MouseLeftButtonDown += DeviceImage_MouseLeftButtonDown;
                mHighightRectangle.MouseLeftButtonUp += DeviceImage_MouseLeftButtonUp;
                mHighightRectangle.MouseEnter += DeviceImage_MouseEnter;
                mHighightRectangle.MouseLeave += DeviceImage_MouseLeave;
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to draw mobile element rectangle", ex);
            }
        }

        private void RemoveElemntRectangle()
        {
            if (DeviceImageCanvas != null)
            {
                if (DeviceImageCanvas.Children.Contains(mHighightRectangle))
                    DeviceImageCanvas.Children.Remove(mHighightRectangle);
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

        private void SetAttributesActionsView(bool show)
        {
            if (show == true)
            {
                selectElmtLbl.Visibility = System.Windows.Visibility.Collapsed;
                attributesStckPnl.Visibility = System.Windows.Visibility.Visible;
                if (BF != null)
                {
                    actionsStckPnl.Visibility = System.Windows.Visibility.Visible;
                    attributesActionsSpliter.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                selectElmtLbl.Visibility = System.Windows.Visibility.Visible;
                attributesStckPnl.Visibility = System.Windows.Visibility.Collapsed;
                actionsStckPnl.Visibility = System.Windows.Visibility.Collapsed;
                attributesActionsSpliter.Visibility = System.Windows.Visibility.Collapsed;
                RemoveElemntRectangle();
            }
        }

        private void SetInspectorPanelView(bool show)
        {
            if (show == true)
            {
                InspectorFrame.Visibility = System.Windows.Visibility.Visible;
                inspectroSplitter.Visibility = System.Windows.Visibility.Visible;
                InspectorCol.Width = new GridLength(250);
                this.Width = this.Width + Convert.ToDouble(InspectorCol.Width.ToString());

                //allow only XML view for browser mode
                if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser ||
                        AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser)
                {
                    elementAttributesTab.Visibility = System.Windows.Visibility.Collapsed;
                    InspectorPointBtn.Visibility = System.Windows.Visibility.Collapsed;
                    inspectorElementTabsControl.SelectedIndex = 1;//Source tab
                }
            }
            else
            {
                InspectorFrame.Visibility = System.Windows.Visibility.Collapsed;
                inspectroSplitter.Visibility = System.Windows.Visibility.Collapsed;
                this.Width = this.Width - InspectorCol.ActualWidth;
                InspectorCol.Width = new GridLength(0);
            }
        }

        public void HighLightElement(AppiumElementInfo AEI)
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

        #endregion Functions
    }
}
