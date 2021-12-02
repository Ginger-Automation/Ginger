using GingerCore.Activities;
using GingerCore.ALM.QC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ALM.MappedToALMWizard
{
    public enum TestCaseStepsMappingError { Invalid, Error }
    public class ALMTestCaseManualMappingConfig : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public ActivitiesGroup activitiesGroup = new ActivitiesGroup();
        public ALMTSTest aLMTSTest;
        public List<ALMTSTestStep> aLMTSTestSteps = new List<ALMTSTestStep>();

        public bool TestCaseStepsMappingStatus = false;
        public TestCaseStepsMappingError testCaseStepsMappingError;
        public string TestCaseName
        {
            get
            {
                if(aLMTSTest == null)
                {
                    return "";
                }
                return aLMTSTest.TestName;
            }
        }
        public string ActivityGroupName
        {
            get
            {
                return activitiesGroup.Name;
            }
        }

        public void UpdateMappedTestCase(ALMTSTest testCase)
        {
            aLMTSTest = testCase;
            OnPropertyChanged(nameof(TestCaseName));
        }

    }
}
