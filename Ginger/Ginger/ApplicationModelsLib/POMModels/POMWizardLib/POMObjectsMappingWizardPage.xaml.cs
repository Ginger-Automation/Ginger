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
using GingerCore;
using GingerCore.Actions.VisualTesting;
using GingerCore.Drivers;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for LearnWizardPage.xaml
    /// </summary>
    public partial class POMObjectsMappingWizardPage : Page, IWizardPage
    {
        public AddPOMWizard mWizard;
        public ObservableList<ElementInfo> mElementsList = new ObservableList<ElementInfo>();                      
        PomAllElementsPage mPomAllElementsPage = null;
        List<eElementType> mSelectedElementTypesList = new List<eElementType>();

        public POMObjectsMappingWizardPage()
        {
            InitializeComponent();                       
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    if (!mWizard.ManualElementConfiguration)
                    {
                        mElementsList.CollectionChanged += ElementsListCollectionChanged;
                        InitilizePomElementsMappingPage();
                    }
                    break;

                case EventType.Active:
                    if (mPomAllElementsPage.mAgent == null)
                    {
                        mPomAllElementsPage.SetAgent(mWizard.Agent);
                    }

                    if (mWizard.ManualElementConfiguration)
                    {
                        xReLearnButton.Visibility = Visibility.Hidden;
                        mPomAllElementsPage.mappedUIElementsPage.MainElementsGrid.ValidationRules.Clear();
                    }
                    else
                    {
                        mPomAllElementsPage.mappedUIElementsPage.MainElementsGrid.ValidationRules.Clear();
                        mPomAllElementsPage.mappedUIElementsPage.MainElementsGrid.ValidationRules.Add(ucGrid.eUcGridValidationRules.CantBeEmpty);

                        xReLearnButton.Visibility = Visibility.Visible;

                        mSelectedElementTypesList = mWizard.AutoMapElementTypesList.Where(x => x.Selected == true).Select(x => x.ElementType).ToList();
                        Learn();
                    }
                    break;

                case EventType.LeavingForNextPage:
                case EventType.Finish:
                    mPomAllElementsPage.FinishEditInAllGrids();
                    if (mPomAllElementsPage != null)
                    {
                        mPomAllElementsPage.StopSpy();
                    }
                    ResetDriverStopProcess();
                    break;
                case EventType.Cancel:
                    if (mPomAllElementsPage != null)
                    {
                        mPomAllElementsPage.StopSpy();
                    }
                    ResetDriverStopProcess();

                    break;
            }
        }

        private void ResetDriverStopProcess()
        {
            if (mWizard.Agent != null && (DriverBase)mWizard.Agent.Driver != null)
            {
                ((DriverBase)mWizard.Agent.Driver).mStopProcess = false;
            }
        }

        private async void Learn()
        {
            if (!mWizard.IsLearningWasDone)
            {
                try
                {
                    mWizard.ProcessStarted();
                    xReLearnButton.Visibility = Visibility.Collapsed;
                    xStopLoadButton.ButtonText = "Stop";
                    xStopLoadButton.IsEnabled = true;
                    ((DriverBase)mWizard.Agent.Driver).mStopProcess = false;
                    xStopLoadButton.Visibility = Visibility.Visible;
                    
                    mWizard.IWindowExplorerDriver.UnHighLightElements();

                    mWizard.ScreenShot = ((IVisualTestingDriver)mWizard.Agent.Driver).GetScreenShot();
                    mWizard.POM.PageURL = ((DriverBase)mWizard.Agent.Driver).GetURL();
                    mWizard.POM.Name = mWizard.IWindowExplorerDriver.GetActiveWindow().Title;
                    mWizard.POM.MappedUIElements.Clear();
                    mWizard.POM.UnMappedUIElements.Clear();
                    
                    await Task.Run(() => mWizard.IWindowExplorerDriver.GetVisibleControls(null, mElementsList, true));

                    mWizard.IsLearningWasDone = true;
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.POMWizardFailedToLearnElement, ex.Message);
                    mWizard.IsLearningWasDone = false;
                }
                finally
                {
                    xStopLoadButton.Visibility = Visibility.Collapsed;
                    xReLearnButton.Visibility = Visibility.Visible;
                    mWizard.ProcessEnded();
                }

            }
        }

        private void ElementsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                ElementInfo EI = ((ObservableList<ElementInfo>)sender).Last();
                List<eLocateBy> mElementLocatorsList = mWizard.AutoMapElementLocatorsList.Select(x => x.LocateBy).ToList();

                List<ElementLocator> orderedLocatorsList = EI.Locators.OrderBy(m => mElementLocatorsList.IndexOf(m.LocateBy)).ToList();
                foreach(ElementLocator elemLoc in orderedLocatorsList)
                {
                    elemLoc.Active = mWizard.AutoMapElementLocatorsList.Where(m => m.LocateBy == elemLoc.LocateBy).FirstOrDefault().Active;
                }
                EI.Locators = new ObservableList<ElementLocator>(orderedLocatorsList);

                UpdateElementInfoName(EI);

                if (mSelectedElementTypesList.Contains(EI.ElementTypeEnum))
                {
                    mWizard.POM.MappedUIElements.Add(EI);
                }
                else
                {
                    mWizard.POM.UnMappedUIElements.Add(EI);
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "POM: Learned Element Info from type was failed to be added to Page Elements", ex);
            }
        }

        /// <summary>
        /// This method is used to update the element name by filtering the specia characters and checking the duplicate names
        /// </summary>
        /// <param name="curElement"></param>
        private void UpdateElementInfoName(ElementInfo curElement)
        {
            try
            {
                if (curElement != null)
                {
                    //remove invalid chars
                    string name = curElement.ElementName.Trim().Replace(".", "").Replace("?", "").Replace("\n", "").Replace("\r", "").Replace("#", "").Replace("!", " ").Replace(",", " ").Replace("   ", "");
                    foreach (char chr in Path.GetInvalidFileNameChars())
                    {
                        name = name.Replace(chr.ToString(), string.Empty);
                    }

                    //set max name length to 60
                    if (name.Length > 60)
                    {
                        name = name.Substring(0, 60);
                    }

                    //make sure name is unique                    
                    name = GetUniqueName(mWizard.POM.MappedUIElements, name);
                    name = GetUniqueName(mWizard.POM.UnMappedUIElements, name);
                    curElement.ElementName = name;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Updating POM Element Name", ex);
            }
        }

        /// <summary>
        /// This method is used to get the uniquename for the element
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetUniqueName(ObservableList<ElementInfo> elements, string name)
        {
            string uname = name;
            try
            {
                if (elements.Where(p => p.ElementName == name).Count() > 0)
                {
                    bool isFound = false;
                    int count = 2;
                    while (!isFound)
                    {
                        string postfix = string.Format("{0}_{1}", name, count);
                        if (elements.Where(p => p.ElementName == postfix).Count() > 0)
                        {
                            count++;
                        }
                        else
                        {
                            uname = postfix;
                            isFound = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Updating POM Element Name", ex);
            }
            return uname;
        }

        private void InitilizePomElementsMappingPage()
        {
            if (mPomAllElementsPage == null)
            {
                mPomAllElementsPage = new PomAllElementsPage(mWizard.POM);
                mPomAllElementsPage.ShowTestAllElementsButton = Visibility.Collapsed;
                mPomAllElementsPage.mappedUIElementsPage.MainElementsGrid.ValidationRules.Add(ucGrid.eUcGridValidationRules.CantBeEmpty);
                xPomElementsMappingPageFrame.Content = mPomAllElementsPage;
            }
        }

        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            xStopLoadButton.ButtonText = "Stopping...";
            xStopLoadButton.IsEnabled = false;
            ((DriverBase)mWizard.Agent.Driver).mStopProcess = true;
        }
         

        private void ReLearnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.POMWizardReLearnWillDeleteAllElements) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                mWizard.IsLearningWasDone = false;
                Learn();
            }
        }
    }
}
