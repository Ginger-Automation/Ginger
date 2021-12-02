using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ALM.MappedToALMWizard
{
    public class TestCaseGridParam : RepositoryItemBase
    {

        public static partial class Fields
        {
            public static string ActivityGroup = "Activity Group";
            public static string TestCase = "Test Case";
            public static string Description = "Description";
        }

        string mActivityGroup;
        public string ActivityGroup 
        { get { return mActivityGroup; } set { if (mActivityGroup != value) { mActivityGroup = value; OnPropertyChanged(nameof(ActivityGroup)); } } }

        public bool IsPlatformParameter = false;

        public string mTestCase;
        public string TestCase
        {
            get
            {
                return mTestCase;
            }
            set
            {
                if (mTestCase != value)
                {
                    mTestCase = value;
                    OnPropertyChanged(nameof(TestCase));
                }
            }
        }

        public ObservableList<TestCaseGridParam> MultiValues { get; set; } = null;

        string mDescription;
        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(nameof(Description)); } } }

        public override string ItemName
        {
            get
            {
                return this.ActivityGroup;
            }
            set
            {
                this.ActivityGroup = value;
            }
        }

        public string Type { get; set; }
        public List<string> OptionalValues { get; set; }
    }
}