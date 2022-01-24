#region License
/*
Copyright © 2014-2021 European Support Limited

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
using GingerWPF.WizardLib;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ALM.MapToALMWizard
{
    /// <summary>
    /// Interaction logic for TestSetMappingPage.xaml
    /// </summary>
    public partial class TestSetMappingPage : Page, IWizardPage
    {
        AddMapToALMWizard mWizard;
        Page win;
        bool mIsBusinessFlowMapped = false;
        struct ALMEntitiesDetails
        {
            internal string bfEntityType;
            internal string testLabUploadPath;
            internal string moduleParentId;
            internal string folderCycleId;
        }

        public TestSetMappingPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizard = (AddMapToALMWizard)WizardEventArgs.Wizard;
            switch (WizardEventArgs.EventType)
            {
                case EventType.Active:
                    if (load_frame.Content == null)
                    {
                        LoadInitialTestSetData();
                    }
                    break;
                case EventType.LeavingForNextPage:
                    if(mIsBusinessFlowMapped)
                    {
                        return;
                    }
                    GetSelectedALMTestSetData(WizardEventArgs);
                    break;
            }
        }
        
        #region Binds
        /// <summary>
        /// Bind ALM Test Set Tree.
        /// </summary>
        private void BindTestSet()
        {
                load_frame.Content = GetALMTree();
                mWizard.AddActivitiesGroupsInitialMapping();
        }
        #endregion
        #region Functions
        /// <summary>
        /// 1. Load Test Set tree.
        /// 2. if Business Flow already mapped load mapped Test Set name and id. 
        /// </summary>
        /// <returns>async function</returns>
        private async Task LoadInitialTestSetData()
        {
            mWizard.ProcessStarted();
            try
            {
                // Business Flow Mapped, get mapped test cases and steps to display.
                if (!String.IsNullOrEmpty(mWizard.mapBusinessFlow.ExternalID) && String.IsNullOrEmpty(mWizard.AlmTestSetData.TestSetID))
                {
                    await Task.Run(() => mWizard.SetMappedALMTestSetData()).ConfigureAwait(true);
                    ChangeTestSetPageVisibility();
                    mWizard.UpdateMappedTestCasesCollections();
                    mWizard.RemapTestCasesLists();
                    WizardPage nextPage = mWizard.Pages.Where(p => p.Page is TestCasesMappingPage).FirstOrDefault();
                    (nextPage.Page as TestCasesMappingPage).xUnMapTestCaseGrid.Title = $"ALM '{mWizard.AlmTestSetData.TestSetName}' Test Cases";
                }
                BindTestSet();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed Get Test Set data, Error: {ex.Message}");
            }
            finally
            {
                xTestSetGrid.IsVisibleChanged -= xTestSetGrid_IsVisibleChanged;
                mWizard.ProcessEnded();
            }
        }
        /// <summary>
        /// Get selected Test Set data
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        /// <returns>async function</returns>
        private async Task GetSelectedALMTestSetData(WizardEventArgs WizardEventArgs)
        {
            await Task.Run(() =>
            {
                dynamic SelectedTestSetData = ALMIntegration.Instance.GetSelectedImportTestSetData(win);
                if (SelectedTestSetData is not null)
                {
                    if (SelectedTestSetData.AlreadyImported)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, $"Selected ALM Test Set already mapped to Ginger Business Flow: '{SelectedTestSetData.MappedBusinessFlow}'.\n" +
                            $"Please delete the Business Flow before remapping.\n Business Flow Path: '{SelectedTestSetData.MappedBusinessFlowPath}'");
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }
                    mWizard.SetSelectedTreeTestSetData(SelectedTestSetData);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please select ALM Test Set.");
                    WizardEventArgs.CancelEvent = true;
                    return;
                }
            });
        }
        /// <summary>
        /// GetALMTree:
        /// Get selected alm test sets tree.
        /// </summary>
        /// <returns>ALM Test Sets Tree Page Object</returns>
        private Page GetALMTree()
        {
            try
            {
                win = Dispatcher.Invoke(() => ALMIntegration.Instance.GetALMTestSetsTreePage());
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed get ALM Tree, {System.Reflection.MethodBase.GetCurrentMethod().Name}: {ex.Message}");
            }
            return win;
        }
        // TODO check if needed , for all type of test sets.
        private ALMEntitiesDetails GetALMEntitiesDetails(string getALMTestSetDetails)
        {
            ALMEntitiesDetails aLMEntitiesDetails = new ALMEntitiesDetails();
            string[] getTypeAndId = getALMTestSetDetails.Split('#');
            aLMEntitiesDetails.testLabUploadPath = getTypeAndId[1];
            aLMEntitiesDetails.bfEntityType = getTypeAndId[0];
            aLMEntitiesDetails.moduleParentId = getTypeAndId[2] == null ? string.Empty : getTypeAndId[2];
            aLMEntitiesDetails.folderCycleId = getTypeAndId[3];
            return aLMEntitiesDetails;
        }
        /// <summary>
        /// change to mapped test set visibility
        /// </summary>
        private void ChangeTestSetPageVisibility()
        {
            load_frame.Visibility = Visibility.Collapsed;
            xMappedTestSetBox.Text = $"Test Set Name: {mWizard.AlmTestSetData.TestSetName} , Test Set ID: {mWizard.AlmTestSetData.TestSetID}";
            xMappedTestSetPanel.Visibility = Visibility.Visible;
            mIsBusinessFlowMapped = true;
        }
        #endregion
        #region Events
        private void load_frame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (win is not null)
            {
                win.Width = ActualWidth;
                win.Height = ActualHeight;
            }
        }
        public void xChangeTestSetBtn_Click(object sender, RoutedEventArgs e)
        {
            xMappedTestSetPanel.Visibility = Visibility.Collapsed;
            load_frame.Visibility = Visibility.Visible;
            mIsBusinessFlowMapped = false;
        }

        private void xTestSetGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (load_frame.Content == null)
            {
                mWizard.ProcessStarted();
                mWizard.GetCurrentPage().Page.WizardEvent(new WizardEventArgs(this.mWizard, EventType.Active));
                return;
            }
        }

        #endregion



    }
}
