using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Repository;

namespace Ginger.Configurations
{
    public class AccessibilityRuleData : RepositoryItemBase, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private bool mActive = false;
        /// <summary>
        /// Gets or sets the Active associated with the accessibility rule.
        /// </summary>
        [IsSerializedForLocalRepository]
        public bool Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(nameof(Active)); } } }

        private string mRuleID;
        /// <summary>
        /// Gets or sets the Rule Id associated with the accessibility rule.
        /// </summary>
        [IsSerializedForLocalRepository]
        public string RuleID { get { return mRuleID; } set { if (mRuleID != value) { mRuleID = value; OnPropertyChanged(nameof(RuleID)); } } }

        private string mTags;
        /// <summary>
        /// Gets or sets the tags associated with the accessibility rule.
        /// </summary>
        [IsSerializedForLocalRepository]
        public string Tags { get { return mTags; } set { if (mTags != value) { mTags = value; OnPropertyChanged(nameof(Tags)); } } }

        private string mImpact;
        public string Impact { get { return mImpact; } set { if (mImpact != value) { mImpact = value; OnPropertyChanged(nameof(Impact)); } } }

        private string mDescription;
        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(nameof(Description)); } } }

        private string mACTRules;
        public string ACTRules { get { return mACTRules; } set { if (mACTRules != value) { mACTRules = value; OnPropertyChanged(nameof(ACTRules)); } } }
        public override string ItemName { get; set; }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
