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
using Ginger.WizardLib;
using GingerCore;
using GingerCore.ALM.QC;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ginger.ALM.MapToALMWizard
{
    /// <summary>
    /// AddMapToALMWizard
    /// Wizard for manual mapping Alm Test Set to Ginger Business Flow.
    /// User will map the test objects and will get link between Alm & Ginger
    /// For Extra oerations.
    /// </summary>
    class AddMapToALMWizard : WizardBase, INotifyPropertyChanged
    {
        public BusinessFlow mapBusinessFlow { get; private set; }
        public ALMTestSet almTestSetDetails;
        public string externallID2 = "";
        public ALMTestSet AlmTestSetDetails
        {
            get
            {
                if(almTestSetDetails == null)
                {
                    almTestSetDetails = new ALMTestSet();
                }
                return almTestSetDetails;
            }
            set
            {
                almTestSetDetails = value;
            }
        }
        public ObservableList<ALMTestCaseManualMappingConfig> testCasesMappingList = new ObservableList<ALMTestCaseManualMappingConfig>();
        
        public ObservableList<ALMTSTest> testCasesUnMappedList = new ObservableList<ALMTSTest>();
        public Dictionary<String, ObservableList<ALMTSTestStep>> testStepsUnMappedList = new Dictionary<string, ObservableList<ALMTSTestStep>>();
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
        
        public AddMapToALMWizard(BusinessFlow businessFlow)
        {
            ALMIntegration.Instance.GetDefaultAlmConfig();
            mapBusinessFlow = businessFlow;
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Map To ALM Introduction", Page: new WizardIntroPage("/ALM/MapToALMWizard/MappedToALMIntro.md"));
            AddPage(Name: "Test Set Mapping", Title: "Test Set Mapping", SubTitle: GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, $"Select matching ALM Test Set to Ginger ‘{mapBusinessFlow.Name}’") , Page: new TestSetMappingPage());
            AddPage(Name: "Test Cases Mapping", Title: "Test Cases Mapping", SubTitle: GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup, "Select matching ALM Test Case foreach Ginger"), Page: new TestCasesMappingPage(mapBusinessFlow));
            AddPage(Name: "Test Steps Mapping", Title: "Test Steps Mapping", SubTitle: GingerDicser.GetTermResValue(eTermResKey.Activity, $"Select matching ALM Steps foreach Ginger"), Page: new TestStepMappingPage());
        }
        /// <summary>
        /// Finish():
        /// Mapping results after Mapping finished.
        /// </summary>
        public override void Finish()
        {
            try 
            {
                // Map Test Set
                mapBusinessFlow.ExternalID = AlmTestSetDetails.TestSetID;
                mapBusinessFlow.ExternalID2 = externallID2;

                // Map Test Case
                foreach (ALMTestCaseManualMappingConfig mappedTestCase in testCasesMappingList)
                {
                    if (mappedTestCase.aLMTSTest is not null && mappedTestCase.aLMTSTest.TestName is not null)
                    {
                        mappedTestCase.activitiesGroup.ExternalID = mappedTestCase.aLMTSTest.TestID;
                        mappedTestCase.activitiesGroup.ExternalID2 = mappedTestCase.aLMTSTest.LinkedTestID;
                        // Map Test Step
                        foreach (ALMTestStepManualMappingConfig mappedTestStep in mappedTestCase.testStepsMappingList)
                        {
                            if (mappedTestStep.almTestStep != null)
                            {
                                mappedTestStep.activity.ExternalID = mappedTestStep.almTestStep.StepID;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Test Set Id's Failed mapping to ");
            }
        }
    }
}
