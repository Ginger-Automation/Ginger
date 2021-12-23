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
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.ALM.QC;
using GingerWPF.WizardLib;
using System;
using System.Windows;
using System.Windows.Controls;
using static Ginger.ALM.ZephyrEnt.ZephyrEntPlanningExplorerPage;

namespace Ginger.ALM.MapToALMWizard
{
    /// <summary>
    /// Interaction logic for TestSetMappingPage.xaml
    /// </summary>
    public partial class TestSetMappingPage : Page, IWizardPage
    {
        AddMapToALMWizard mWizard;
        Page win;
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
                    // Add ALM Test Set Tree to Page.
                    win = ALMIntegration.Instance.GetALMTestSetPage();
                    load_frame.Content = win;
                    // Add business flow activities groups to test cases mapping list.
                    foreach (ActivitiesGroup ag in mWizard.mapBusinessFlow.ActivitiesGroups)
                    {
                        mWizard.testCasesMappingList.Add(new ALMTestCaseManualMappingConfig() { activitiesGroup = ag });
                    }
                    break;
                case EventType.Active:
                    if(String.IsNullOrEmpty(mWizard.mapBusinessFlow.ExternalID))
                    {
                        mWizard.Pages.MoveNext();
                    }
                    break;
                case EventType.Next:
                    //mWizard.almTestSetDetails = getALMTestSetDetails;
                    break;
                case EventType.LeavingForNextPage:
                    dynamic testSetData = ALMIntegration.Instance.GetSelectedImportTestSetData(win);
                    if (testSetData is not null)
                    {
                        if (mWizard.AlmTestSetDetails.TestSetID == null || mWizard.AlmTestSetDetails.TestSetID != testSetData.Id)
                        {
                            mWizard.testCasesUnMappedList.Clear();
                            mWizard.AlmTestSetDetails.Tests.Clear();
                            // Clear mapped test cases column
                            foreach (ALMTestCaseManualMappingConfig testCaseMapping in mWizard.testCasesMappingList)
                            {
                                testCaseMapping.aLMTSTest = null;
                            }
                            mWizard.AlmTestSetDetails.TestSetID = testSetData.Id;
                            mWizard.AlmTestSetDetails.TestSetName = testSetData.Name;
                            mWizard.AlmTestSetDetails.TestSetPath = testSetData.Path;
                            mWizard.externallID2 = testSetData.TestSetID;
                            mWizard.AlmTestSetDetails = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).ImportTestSetData(mWizard.AlmTestSetDetails);
                            foreach (ALMTSTest testCase in mWizard.AlmTestSetDetails.Tests)
                            {
                                mWizard.testCasesUnMappedList.Add(testCase);
                            }
                        }
                    }
                    else
                    {
                        // Add validation
                    }
                    break;
            }
        }
        #region Binds

        #endregion
        #region Functions
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
        #endregion
        

        
    }
}
