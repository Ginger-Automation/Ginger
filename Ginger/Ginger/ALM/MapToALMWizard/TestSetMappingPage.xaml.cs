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
                case EventType.Init:
                    BindTestSet();
                    break;
                case EventType.Active:
                    // Business Flow Mapped, get mapped test cases and steps to display.
                    if (!String.IsNullOrEmpty(mWizard.mapBusinessFlow.ExternalID) && String.IsNullOrEmpty(mWizard.AlmTestSetData.TestSetID))
                    {
                        mWizard.SetMappedALMTestSetData();
                        ChangeTestSetPageVisibility();
                        mWizard.UpdateMappedTestCasesCollections();
                        mWizard.RemapTestCasesLists();
                        WizardPage nextPage = mWizard.Pages.Where(p => p.Page is TestCasesMappingPage).FirstOrDefault();
                        (nextPage.Page as TestCasesMappingPage).xUnMapTestCaseGrid.Title = $"ALM '{mWizard.AlmTestSetData.TestSetName}' Test Cases";
                    }
                    break;
                case EventType.LeavingForNextPage:
                    if(mIsBusinessFlowMapped)
                    {
                        return;
                    }
                    dynamic SelectedTestSetData = ALMIntegration.Instance.GetSelectedImportTestSetData(win);
                    if (SelectedTestSetData is not null)
                    {
                        mWizard.SetSelectedTreeTestSetData(SelectedTestSetData);
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please select ALM Test Set.");
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }
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
        /// GetALMTree:
        /// Get selected alm test sets tree.
        /// </summary>
        /// <returns>ALM Test Sets Tree Page Object</returns>
        private Page GetALMTree()
        {
            try
            {
                win = ALMIntegration.Instance.GetALMTestSetsTreePage();
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
            xMappedTestSetBox.Text = mWizard.AlmTestSetData.TestSetName;
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
        #endregion



    }
}
