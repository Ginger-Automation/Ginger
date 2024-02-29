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
        [IsSerializedForLocalRepository]
        public bool Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(nameof(Active)); } } }
        [IsSerializedForLocalRepository]
        public string RuleID { get; set; }
        [IsSerializedForLocalRepository]
        public string Tags { get; set; }

        public string Impact { get; set; }

        public string Description { get; set; }

        public string ACTRules { get; set; }
        public override string ItemName { get; set; }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
