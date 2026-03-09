#region License
/*
Copyright © 2014-2026 European Support Limited

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
using Amdocs.Ginger.Repository;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Ginger.Configurations
{
    public class AccessibilityRuleData : RepositoryItemBase, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private bool mActive = false;
        /// <summary>
        /// Gets or sets the Active associated with the accessibility rule.
        /// </summary>
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

        public string Tags { get { return mTags; } set { if (mTags != value) { mTags = value; OnPropertyChanged(nameof(Tags)); } } }

        private string mImpact;
        public string Impact { get { return mImpact; } set { if (mImpact != value) { mImpact = value; OnPropertyChanged(nameof(Impact)); } } }

        private string mDescription;
        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(nameof(Description)); } } }

        private string mACTRules;
        public string ACTRules { get { return mACTRules; } set { if (mACTRules != value) { mACTRules = value; OnPropertyChanged(nameof(ACTRules)); } } }
        public override string ItemName { get; set; }

        // NEW PROPERTIES:
        public string SuggestedFix { get; set; }
        public string RelatedWCAG { get; set; }

        public ObservableList<AccessibilityRuleData> GetAccessibilityRules(string AccessbiltyString)
        {
            Root data = new()
            {
                accessibilityRules = []
            };
            try
            {
                data = JsonConvert.DeserializeObject<Root>(AccessbiltyString);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to deserialize accessibility rules.", ex);
            }

            return data.accessibilityRules;
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }

    public class Root
    {
        public ObservableList<AccessibilityRuleData> accessibilityRules { get; set; }
    }


}
