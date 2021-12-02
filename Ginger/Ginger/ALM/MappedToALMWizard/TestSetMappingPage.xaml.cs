using Ginger.ALM.ZephyrEnt;
using Ginger.ALM.ZephyrEnt.TreeViewItems;
using GingerCore.ALM;
using GingerCore.ALM.QC;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Ginger.ALM.ZephyrEnt.ZephyrEntPlanningExplorerPage;

namespace Ginger.ALM.MappedToALMWizard
{
    /// <summary>
    /// Interaction logic for TestSetMappingPage.xaml
    /// </summary>
    public partial class TestSetMappingPage : Page, IWizardPage
    {
        AddMappedToALMWizard mWizard;
        string getALMTestSetDetails;
        ZephyrEntPlanningExplorerPage win;
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
            //var data = new ZephyrEntPlanningExplorerPage(eExplorerTestPlanningPageUsageType.Import, "");
            //ZephyrEntPlanningExplorerPage win = new ZephyrEntPlanningExplorerPage(eExplorerTestPlanningPageUsageType.Import, "");
            //load_frame.Content = win;
            //getALMTestSetDetails = load_frame.Content.ToString();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizard = (AddMappedToALMWizard)WizardEventArgs.Wizard;
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    //if (mWizard.ActivitiesGroupPreSet == false)
                    //{
                    //    //xGroupComboBox.ItemsSource = mWizard.Context.BusinessFlow.ActivitiesGroups;
                    //    //xGroupComboBox.DisplayMemberPath = nameof(ActivitiesGroup.Name);
                    //    //BindingHandler.ObjFieldBinding(xGroupComboBox, ComboBox.SelectedItemProperty, mWizard, nameof(AddActivityWizard.ParentActivitiesGroup));
                    //}
                    //else
                    //{
                    //    //xGroupPanel.Visibility = Visibility.Collapsed;
                    //}
                    //xRegularType.IsChecked = true;
                    break;
                case EventType.Active:
                    win = new ZephyrEntPlanningExplorerPage(eExplorerTestPlanningPageUsageType.Import, "");
                    load_frame.Content = win;
                    break;
                case EventType.Next:

                    mWizard.almTestSetDetails = getALMTestSetDetails;
                    break;
                case EventType.LeavingForNextPage:
                    if(mWizard.AlmTestSetDetails == null)
                    {
                        mWizard.AlmTestSetDetails = new GingerCore.ALM.QC.ALMTestSet();
                    }
                    mWizard.AlmTestSetDetails.TestSetID = win.CurrentSelectedTestSets[0].Id;
                    mWizard.AlmTestSetDetails.TestSetName = win.CurrentSelectedTestSets[0].Name;
                    mWizard.AlmTestSetDetails.TestSetPath = win.CurrentSelectedTestSets[0].Path;
                    mWizard.AlmTestSetDetails = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).ImportTestSetData(mWizard.AlmTestSetDetails);
                    ALMTestCaseManualMappingConfig aLMTestCaseManualMappingConfig = new ALMTestCaseManualMappingConfig();
                    mWizard.mapBusinessFlow.ActivitiesGroups.ToList().
                        ForEach(ag => mWizard.testCasesMappingList.Add(new ALMTestCaseManualMappingConfig() { activitiesGroup = ag }));
                    mWizard.AlmTestSetDetails.Tests.ToList().
                        ForEach(tc =>
                            {
                                ALMTSTest utc = new ALMTSTest();
                                utc = tc;
                                mWizard.testCasesUnMappedList.Add(utc);
                            });
                    break;
            }
        }
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
    }
}
