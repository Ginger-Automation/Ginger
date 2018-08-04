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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.UserControls;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Drivers;
using GingerCore.Platforms;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
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

        ApplicationPOMModel mPOM = new ApplicationPOMModel();

        WindowExplorerPage mWindowExplorerPage;
        ObservableList<ElementInfo> mElementsList = new ObservableList<ElementInfo>();
        ObservableList<ElementInfo> mSelectedElementsList = new ObservableList<ElementInfo>();
        Dictionary<string, List<string>> mRequestedElementTypesDict = new Dictionary<string, List<string>>();
        List<string> mRequestedElementTagList = new List<string>();

        AddPOMWizard mWizard;
        IWindowExplorer mWinExplorer;

        public LearnWizardPage(ApplicationPOMModel POM)
        {
            InitializeComponent();

            mPOM = POM;

            mElementsList.CollectionChanged += ElementsListCollectionChanged;

            //SetControlsGridView();
            //xFoundElementsGrid.SetTitleLightStyle = true;
            //xFoundElementsGrid.DataSourceList = mPOM.UIElements;
            //xFoundElementsGrid.btnRefresh.Click += RefreshClick;
            //ElementsGrid.btnAdd.Click += BtnAdd_Click;
        }

        private void ElementsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ElementInfo EI = ((ObservableList<ElementInfo>)sender).Last();

            mWinExplorer.UpdateElementInfoFields(EI);

            EI.Locators = mWinExplorer.GetElementLocators(EI);

            EI.ElementName = GetBestElementName(EI);

            EI.WindowExplorer = mWinExplorer;

            //TODO: Auto decide what is active
            if (!string.IsNullOrEmpty(EI.ElementName))
            {
                //TODO: fix me temp, need to be in IWindowExplorer, or return from eleminfo
                if (EI.ElementType != "BODY" && EI.ElementType != "HTML" && EI.ElementType != "DIV")
                {
                    EI.Active = true;
                }
            }
            else
            {
                //TODO: fix me temp code !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                if (EI.ElementType == "INPUT.TEXT")
                {
                    EI.ElementName = EI.Value + " TextBox";
                    EI.Active = true;
                }
            }


            string elementTagName = string.Empty;
            string elementType = string.Empty;

            if (EI.ElementType.Contains("."))
            {
                elementTagName = EI.ElementType.Substring(0, EI.ElementType.IndexOf("."));
                elementType = EI.ElementType.Substring(EI.ElementType.IndexOf(".") + 1);
            }
            else
            {
                elementTagName = EI.ElementType;
            }

            if (mRequestedElementTagList.Contains(elementTagName))
            {
                List<string> values = null;
                if (mRequestedElementTypesDict.ContainsKey(elementTagName.ToLower()))
                {
                    values = mRequestedElementTypesDict[elementTagName.ToLower()];
                }

                if (values != null && !string.IsNullOrEmpty(elementType))
                {
                    List<string> upperList = values.Select(x => x.ToUpper()).ToList();
                    if (upperList.Contains(elementType))
                        mPOM.MappedUIElements.Add(EI);
                }
                else
                {
                    mPOM.MappedUIElements.Add(EI);
                }

            }
            else
            {
                mPOM.UnMappedUIElements.Add(EI);
            }

        }

        private void InitilizeWindowExplorer()
        {
            ApplicationAgent applicationAgent = new ApplicationAgent();
            applicationAgent.Agent = mWizard.mAgent;
            string targetApplicationName = App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.Key == mPOM.TargetApplicationKey).FirstOrDefault().AppName;
            applicationAgent.AppName = targetApplicationName;
            mWindowExplorerPage = new WindowExplorerPage(applicationAgent,null, WindowExplorerPage.eWindowExplorerPageContext.POMWizard);
            mWindowExplorerPage.WindowControlsGridView.DataSourceList = mPOM.MappedUIElements;
            mWindowExplorerPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            mWindowExplorerPage.Width = 700;
            xWindowExlorerFrame.Content = mWindowExplorerPage;
            
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // ******************************************************************************
            //DO NOT  DELETE Temp commented for moving to GingerCoreCommon
            // ******************************************************************************
            ////TODO: create add page
            //mPOM.UIElements.Add(new ElementInfo() { ElementName = "*New*" });
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            mWizard.IsLearningWasDone = false;
            //xFoundElementsGrid.DataSourceList = mPOM.UIElements;
        }

        //private void SetControlsGridView()
        //{
        //    ////Set the Tool Bar look
           
        //    ////Set the Data Grid columns            
        //    GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
        //    view.GridColsView = new ObservableList<GridColView>();

        //    view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Active), WidthWeight = 2.5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), WidthWeight = 100 });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTitle), WidthWeight = 100 });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Value), WidthWeight = 100 });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementType), WidthWeight = 60 });

        //    view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.X), WidthWeight = 60 });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Y), WidthWeight = 60 });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Width), WidthWeight = 60 });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Height), WidthWeight = 60 });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Path), WidthWeight = 100 });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.XPath), WidthWeight = 150 });

        //    xFoundElementsGrid.SetAllColumnsDefaultView(view);
        //    xFoundElementsGrid.InitViewItems();
        //}


        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    //mWinExplorer = mWizard.WinExplorer;
                    //xFoundElementsGrid.DataSourceList = mWizard.POM.UIElements;
                    break;
                case EventType.Active:
                    if (xWindowExlorerFrame.Content == null)
                        InitilizeWindowExplorer();
                    Learn();
                    break;
                case EventType.LeavingForNextPage:
                    //mPOM.SelectedUIElements = GingerCore.General.ConvertListToObservableList(mPOM.UIElements.Where(x => x.Selected).ToList());

                    break;
            }
        }


        private async void Learn()
        {
            if (!mWizard.IsLearningWasDone)
            {
                mWizard.ProcessStarted();
                mWinExplorer = mWizard.WinExplorer;


                //Dictionary<string, List<string>> filteringCriteriasDict = mWinExplorer.GetFilteringCreteriaDict();

                mRequestedElementTypesDict = SeleniumDriver.GetFilteringCreteriaDict(mWizard.CheckedFilteringCreteriaList);
                mRequestedElementTagList = mRequestedElementTypesDict.Keys.Select(x => x.ToUpper()).ToList();

                //mRequestedElementTypesList = mWizard.CheckedFilteringCreteriaList.Select(x => x.ElementType.ToString()).ToList();

                mWizard.IsLearningWasDone = await GetElementsFromPage();
                mWizard.ProcessEnded();
            }
        }

        private async Task<bool> GetElementsFromPage()
        {
            bool learnSuccess = true;

            try
            {
                await Task.Run(() => GetElementFromPageTask());

                xStopLoadButton.ButtonText = "Stop";
                xStopLoadButton.IsEnabled = true;
                ((DriverBase)mWinExplorer).mStopProcess = false;
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.POMWizardFailedToLearnElement, ex.Message);
                //MessageBox.Show("Error Details:" + Environment.NewLine + ex.Message, "Failed to Parse the WSDL", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, System.Windows.MessageBoxResult.None);
                learnSuccess = false;
            }

            return learnSuccess;
        }

        private void GetElementFromPageTask()
        {
            //ObservableList<UIElementFilter> filters = mWizard.CheckedFilteringCreteriaList;
            mPOM.MappedUIElements.Clear();

            mWinExplorer.GetVisibleControls(new ObservableList<UIElementFilter>(), mElementsList);


            //foreach (ElementInfo EI in list)
            //{
            //    mWinExplorer.UpdateElementInfoFields(EI);

            //    EI.Locators = mWinExplorer.GetElementLocators(EI);

            //    EI.ElementName = GetBestElementName(EI);

            //    EI.WindowExplorer = mWinExplorer;

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

            //    mPOM.UIElements.Add(EI);
            //}

        }

        string GetBestElementName(ElementInfo EI)
        {
            if (string.IsNullOrEmpty(EI.Value)) return null;
            // temp need to be per elem etc... with smart naming for label text box etc...    need to be in the IWindowExplorer        
            return EI.Value + " " + EI.ElementType;
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

        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            xStopLoadButton.ButtonText = "Stopping...";
            xStopLoadButton.IsEnabled = false;
            ((DriverBase)mWinExplorer).mStopProcess = true;
        }

        UnmappedElementsPage mUnmappedElementsPage;

        private void UnmappedElementsButtonClicked(object sender, RoutedEventArgs e)
        {
            if (mUnmappedElementsPage == null)
            {
                mUnmappedElementsPage = new UnmappedElementsPage(mPOM);
            }
            mUnmappedElementsPage.ShowAsWindow();
        }
    }
}
