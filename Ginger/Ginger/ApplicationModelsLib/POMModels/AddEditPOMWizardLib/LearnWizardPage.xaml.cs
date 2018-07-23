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

using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using GingerWPF.WizardLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for LearnWizardPage.xaml
    /// </summary>
    public partial class LearnWizardPage : Page, IWizardPage
    {
        public LearnWizardPage(ApplicationPOMModel POM)
        {
            InitializeComponent();

            //mPOM = POM;
            //SetControlsGridView();
            //ElementsGrid.DataSourceList = mPOM.UIElements;
            //ElementsGrid.btnRefresh.Click += RefreshClick;
            //ElementsGrid.btnAdd.Click += BtnAdd_Click;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // ******************************************************************************
            //DO NOT  DELETE Temp commented for moving to GingerCoreCommon
            // ******************************************************************************
            ////TODO: create add page
            //mPOM.UIElements.Add(new ElementInfo() { ElementName = "*New*"});
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            //ElementsGrid.DataSourceList = mPOM.UIElements; 
        }

        private void SetControlsGridView()
        {
            ////Set the Tool Bar look
            ////ElementsGrid.ShowAdd = Visibility.Collapsed;
            //ElementsGrid.ShowClearAll = Visibility.Collapsed;
            //ElementsGrid.ShowUpDown = Visibility.Collapsed;
            ////ElementsGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            ////ElementsGrid.AddToolbarTool("@Filter16x16.png", "Filter Elements to show", new RoutedEventHandler(FilterElementButtonClicked));
            ////TODO: enable refresh to do refresh
            //// ControlsGrid.ShowRefresh = System.Windows.Visibility.Collapsed;

            //ElementsGrid.ShowEdit = System.Windows.Visibility.Collapsed;
            //ElementsGrid.ShowDelete = System.Windows.Visibility.Collapsed;

            ////TODO: add button to show all...
            //// ControlsGrid.AddButton("Show All", new RoutedEventHandler(ShowAll));            

            ////Set the Data Grid columns            
            //GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            //view.GridColsView = new ObservableList<GridColView>();

            //// view.GridColsView.Add(new GridColView() { Field = "Image", Header = " ", BindImageCol = "Image", WidthWeight = 2.5, MaxWidth = 20 });
            //view.GridColsView.Add(new GridColView() { Field = ElementInfo.Fields.Active, WidthWeight = 2.5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            //view.GridColsView.Add(new GridColView() { Field = ElementInfo.Fields.ElementName, WidthWeight = 100 });
            //view.GridColsView.Add(new GridColView() { Field = ElementInfo.Fields.ElementTitle, WidthWeight = 100 });
            //view.GridColsView.Add(new GridColView() { Field = ElementInfo.Fields.Value, WidthWeight = 100 });
            //view.GridColsView.Add(new GridColView() { Field = ElementInfo.Fields.ElementType, WidthWeight = 60 });

            //view.GridColsView.Add(new GridColView() { Field = ElementInfo.Fields.X, WidthWeight = 60 });
            //view.GridColsView.Add(new GridColView() { Field = ElementInfo.Fields.Y, WidthWeight = 60 });
            //view.GridColsView.Add(new GridColView() { Field = ElementInfo.Fields.Width, WidthWeight = 60 });
            //view.GridColsView.Add(new GridColView() { Field = ElementInfo.Fields.Height, WidthWeight = 60 });
            //// view.GridColsView.Add(new GridColView() { Field = ElementInfo.Fields.Path, WidthWeight = 100 });
            //// view.GridColsView.Add(new GridColView() { Field = ElementInfo.Fields.XPath, WidthWeight = 150 });

            //ElementsGrid.SetAllColumnsDefaultView(view);
            //ElementsGrid.InitViewItems();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            //switch (WizardEventArgs.EventType)
            //{
            //    case EventType.Init:
            //        mWizard = (AddEditPOMWizard)WizardEventArgs.Wizard;
            //        mWinExplorer = mWizard.WinExplorer;
            //        ElementsGrid.DataSourceList = mWizard.POM.UIElements;
            //        break;
            //    case EventType.Active:                    
            //        mScreenshot = new ScreenShotViewPage(mWizard.POM.Name, mWizard.POM.ScreenShot);
            //        mScreenshot.MouseClickOnScreenshot += MouseClickOnScreenshot_EventHandler;
            //        MainFrame.Content = mScreenshot;
            //        break;
            //}            
        }

        private void MouseClickOnScreenshot_EventHandler(object sender, MouseclickonScrenshot e)
        {
            ////MessageBox.Show("x=" + e.X + ", y=" + e.Y);

            //ObservableList<ElementInfo> filteredlist = new ObservableList<ElementInfo>();

            //foreach (ElementInfo EI in mPOM.UIElements)
            //{
            //    //Check the mouse point is in boundries of element if not filter it out 
            //    if (e.X >= EI.X && e.X <= EI.X + EI.Width && e.Y >= EI.Y && e.Y <= EI.Y + EI.Height)
            //    {
            //        filteredlist.Add(EI);
            //    }
            //}

            //ElementsGrid.DataSourceList = filteredlist;

            //// TODO: return ToolBar unfilter
            //// ElementsGrid.DataSourceList = mDPI.UIElements;
        }

        private void LearnButton_Click(object sender, RoutedEventArgs e)
        {
            //MainProgressBar.Visibility = Visibility.Visible;
            //ProcessingLabel.Visibility = Visibility.Visible;
            //MainProgressBar.Visibility = Visibility.Visible;
            //ProcessingLabel.Refresh();

            //StartAnimation();

            //GingerCore.General.DoEvents();

            //ProcessingLabel.Content = "Getting Screen elements";

            //// mPOM.Name = mWindowExplorerDriver.GetActiveWindow().Title;

            //ObservableList<UIElementFilter> filters = new ObservableList<UIElementFilter>();
            //List<ElementInfo> list = mWinExplorer.GetVisibleControls(filters);

            //MainProgressBar.Value = 0;
            //MainProgressBar.Maximum = list.Count();
            //GingerCore.General.DoEvents();
            //// Convert to observable for the grid
            //mPOM.UIElements.Clear();
            //foreach (ElementInfo EI in list)
            //{
            //    ProcessingLabel.Content = "Getting Element info: " + EI.ElementTitle + " " + EI.ElementType;
            //    ProcessingLabel.Refresh();

            //    MainProgressBar.Value++;
            //    MainProgressBar.Refresh();
            //    GingerCore.General.DoEvents();

            //    //TODO: remove and let Lazy loading handle it
            //    mWinExplorer.UpdateElementInfoFields(EI);
            //    // EI.X = mWindowExplorerDriver.
                
            //    EI.Locators = mWinExplorer.GetElementLocators(EI);

            //    EI.ElementName = GetBestElementName(EI);

            //    EI.WindowExplorer = mWinExplorer;

            //    //EI.Data = EI.GetElementData();
                
            //    //TODO: Auto decide what is active
            //    if (!string.IsNullOrEmpty(EI.ElementName))
            //    {
            //        //TODO: fix me temp, need to be in IWindowExplorer, or return from eleminfo
            //        if (EI.ElementType != "BODY" && EI.ElementType != "HTML" && EI.ElementType != "DIV")
            //        {
            //            EI.Active = true;
            //        }                    
            //    }
            //    else
            //    {
            //        //TODO: fix me temp code !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //        if (EI.ElementType == "INPUT.TEXT")
            //        {
            //            EI.ElementName = EI.Value + " TextBox";
            //            EI.Active = true;
            //        }
            //    }
            //    // EI.GetElementProperties

            //    // EI.ElementName = "?";

            //    mPOM.UIElements.Add(EI);
            //}
            

            ////TODO: check support for IVisualTestingDriver
            //// Bitmap bmp = ((IVisualTestingDriver)mWindowExplorerDriver).GetScreenShot();
            //// mDPI.ScreenShot = bmp;

            ////ScreenShotViewPage p = new ScreenShotViewPage(mWindowExplorerDriver.GetActiveWindow().Title , bmp);
            ////ScreenShotFrame.Content = p;
            //// ShowScreenShot();

            //MainProgressBar.Visibility = Visibility.Collapsed;
            //ProcessingLabel.Visibility = Visibility.Collapsed;
            //MainProgressBar.Visibility = Visibility.Collapsed;

            //StopAnimation();
        }

        string GetBestElementName(ElementInfo EI)
        {
            if (string.IsNullOrEmpty(EI.Value)) return null;
            // temp need to be per elem etc... with smart naming for label text box etc...    need to be in the IWindowExplorer        
            return EI.Value + " " + EI.ElementType;
        }

        private void StopAnimation()
        {
            rectangle1.RenderTransform = null;
        }

        private void StartAnimation()
        {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 360;
            da.Duration = new Duration(TimeSpan.FromSeconds(3));
            da.RepeatBehavior = RepeatBehavior.Forever;
            RotateTransform rt = new RotateTransform();
            rectangle1.RenderTransform = rt;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
            // AnimationLabel
        }

        private void ElementsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            // ******************************************************************************
            //DO NOT  DELETE Temp commented for moving to GingerCoreCommon
            // ******************************************************************************
            //ElementInfo EI = (ElementInfo)mPOM.UIElements.CurrentItem;
            //if (EI != null)
            //{                
            //    if (mWinExplorer != null && EI.ElementObject != null)
            //    {                    
            //        mWinExplorer.HighLightElement(EI);
            //    }
            //    if (mScreenshot != null)
            //    {
            //        mScreenshot.HighLight(EI.X, EI.Y, EI.Width, EI.Height);
            //    }
            //}            

        }
    }
}
