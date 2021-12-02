using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ALM.MappedToALMWizard
{
    public class TestStepGridParam : RepositoryItemBase
    {

        public static partial class Fields
        {
            public static string Activity = "Activity";
            public static string TestStep = "Test Step";
            public static string Description = "Description";
        }

        string mActivity;
        public string Activity { get { return mActivity; } set { if (mActivity != value) { mActivity = value; OnPropertyChanged(nameof(Activity)); } } }

        public bool IsPlatformParameter = false;

        public string mTestStep;
        public string TestStep
        {
            get
            {
                return mTestStep;
            }
            set
            {
                if (mTestStep != value)
                {
                    mTestStep = value;
                    OnPropertyChanged(nameof(TestStep));
                }
            }
        }

        public ObservableList<TestStepGridParam> MultiValues { get; set; } = null;

        string mDescription;
        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(nameof(Description)); } } }

        public override string ItemName
        {
            get
            {
                return this.Activity;
            }
            set
            {
                this.Activity = value;
            }
        }

        public string Type { get; set; }
        public List<string> OptionalValues { get; set; }
    }
}
