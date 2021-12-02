using Amdocs.Ginger.Common;
using Ginger.ALM.MappedToALMWizard;
using Ginger.ALM.ZephyrEnt.TreeViewItems;
using Ginger.WizardLib;
using GingerCore;
using GingerCore.ALM.QC;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ginger.ALM.MappedToALMWizard
{
    class AddMappedToALMWizard : WizardBase, INotifyPropertyChanged
    {
        public Context Context;
        public BusinessFlow mapBusinessFlow;
        public string almTestSetDetails { get; set; }
        public ALMTestSet AlmTestSetDetails { get; set; }
        public ObservableList<ALMTestCaseManualMappingConfig> testCasesMappingList = new ObservableList<ALMTestCaseManualMappingConfig>();
        public ObservableList<ALMTSTest> testCasesUnMappedList = new ObservableList<ALMTSTest>();

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public override string Title { get { return String.Format("Map To {0} Wizard.", ALMIntegration.Instance.GetALMType()); } }
        //public void UpdateUnMappedTestCase(ALMTSTest testCase)
        //{
        //    aLMTSTest = testCase;
        //    OnPropertyChanged(nameof(TestCaseName));
        //}
        //public List<ALMTSTest> UnmappedTestCases
        //{
        //    get
        //    {
        //        if (unmappedTestCases is null)
        //        {
        //            unmappedTestCases = new List<ALMTSTest>();
        //        }
        //        return unmappedTestCases;
        //    }
        //}

        public AddMappedToALMWizard(BusinessFlow businessFlow)
        {
            mapBusinessFlow = businessFlow;
            //Context = context;
            //if (parentActivitiesGroup != null)
            //{
            //    ParentActivitiesGroup = parentActivitiesGroup;
            //    ActivitiesGroupPreSet = true;
            //}
            //else
            //{
            //    if (Context.BusinessFlow.ActivitiesGroups.Count > 0)
            //    {
            //       // ParentActivitiesGroup = Context.BusinessFlow.ActivitiesGroups[0];
            //    }
            //    else
            //    {
            //        //ParentActivitiesGroup = Context.BusinessFlow.AddActivitiesGroup();
            //    }
            //}

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "ALM Export Introduction", Page: new WizardIntroPage("/ALM/MappedToALMWizard/MappedToALMIntro.md"));

            AddPage(Name: "Select Test Set", Title: "Select Test Set", SubTitle: String.Format("Map {0} To {1} Test Set", mapBusinessFlow.Name, ALMIntegration.Instance.GetALMType()), Page: new TestSetMappingPage());

            AddPage(Name: "Test Case Mapping", Title: "Test Case Mapping", SubTitle: String.Format("Map Tests Case to {0}", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)), Page: new TestCasesMappingPage(mapBusinessFlow));
            AddPage(Name: "Test Step Mapping", Title: "Test Step Mapping", SubTitle: String.Format("Map Tests Step to {0}", GingerDicser.GetTermResValue(eTermResKey.Activity)), Page: new TestStepMappingPage());
        }

        public override void Finish()
        {
            //Context.BusinessFlow.AddActivity(ActivityToAdd, ParentActivitiesGroup);
        }
        
    }
}
