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
    public partial class POMObjectsMappingWizardPage : Page, IWizardPage
    {       
        ObservableList<ElementInfo> mElementsList = new ObservableList<ElementInfo>();
        ObservableList<ElementInfo> mSelectedElementsList = new ObservableList<ElementInfo>();
        Dictionary<string, List<string>> mRequestedElementTypesDict = new Dictionary<string, List<string>>();
        List<string> mRequestedElementTagList = new List<string>();

        AddPOMWizard mWizard;       

        public POMObjectsMappingWizardPage()
        {
            InitializeComponent();                       
        }

        private void ElementsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ElementInfo EI = ((ObservableList<ElementInfo>)sender).Last();

            mWizard.IWindowExplorerDriver.UpdateElementInfoFields(EI);
            EI.Locators = mWizard.IWindowExplorerDriver.GetElementLocators(EI);
            EI.ElementName = GetBestElementName(EI);
            EI.WindowExplorer = mWizard.IWindowExplorerDriver;

            ////TODO: Auto decide what is active
            //if (!string.IsNullOrEmpty(EI.ElementName))
            //{
            //    //TODO: fix me temp, need to be in IWindowExplorer, or return from eleminfo
            //    if (EI.ElementType != "BODY" && EI.ElementType != "HTML" && EI.ElementType != "DIV")
            //    {
            //        EI.Active = true;
            //    }
            //}
            //else
            //{
            //    //TODO: fix me temp code !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //    if (EI.ElementType == "INPUT.TEXT")
            //    {
            //        EI.ElementName = EI.Value + " TextBox";
            //        EI.Active = true;
            //    }
            //}

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
                        mWizard.POM.MappedUIElements.Add(EI);
                }
                else
                {
                    mWizard.POM.MappedUIElements.Add(EI);
                }
            }
            else
            {
                mWizard.POM.UnMappedUIElements.Add(EI);
            }
        }

        private void InitilizePomElementsMappingPage()
        {
            PomAllElementsPage pomAllElementsPage = new PomAllElementsPage(mWizard.POM, mWizard.IWindowExplorerDriver);
            xPomElementsMappingPageFrame.Content = pomAllElementsPage;
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    mElementsList.CollectionChanged += ElementsListCollectionChanged;
                    break;

                case EventType.Active:
                    if (xPomElementsMappingPageFrame.Content == null)
                        InitilizePomElementsMappingPage();
                    Learn();
                    break;
            }
        }


        private async void Learn()
        {
            if (!mWizard.IsLearningWasDone)
            {
                mWizard.ProcessStarted();
                xStopLoadButton.Visibility = Visibility.Visible;
                xReLearnButton.Visibility = Visibility.Collapsed;
               
                mWizard.POM.MappedUIElements.Clear();
                mWizard.POM.UnMappedUIElements.Clear();

                mRequestedElementTypesDict = SeleniumDriver.GetFilteringCreteriaDict(mWizard.CheckedFilteringCreteriaList);
                mRequestedElementTagList = mRequestedElementTypesDict.Keys.Select(x => x.ToUpper()).ToList();

                mWizard.IsLearningWasDone = await GetElementsFromPage();
                xStopLoadButton.Visibility = Visibility.Collapsed;
                xReLearnButton.Visibility = Visibility.Visible;
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
                ((DriverBase)mWizard.Agent.Driver).mStopProcess = false;
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.POMWizardFailedToLearnElement, ex.Message);               
                learnSuccess = false;
            }

            return learnSuccess;
        }

        private void GetElementFromPageTask()
        {
            mWizard.IWindowExplorerDriver.GetVisibleControls(new ObservableList<UIElementFilter>(), mElementsList);
        }

        string GetBestElementName(ElementInfo EI)
        {
            if (string.IsNullOrEmpty(EI.Value)) return null;
            // temp need to be per elem etc... with smart naming for label text box etc...    need to be in the IWindowExplorer        
            return EI.Value + " " + EI.ElementType;
        }

        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            xStopLoadButton.ButtonText = "Stopping...";
            xStopLoadButton.IsEnabled = false;
            ((DriverBase)mWizard.Agent.Driver).mStopProcess = true;
        }

        UnmappedElementsPage mUnmappedElementsPage;       

        private void ReLearnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.POMWizardReLearnWillDeleteAllElements) == MessageBoxResult.Yes)
            {
                Learn();
            }
        }
    }
}
