using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.GlobalSolutionLib
{
    public class GlobalSolutionItem : INotifyPropertyChanged
    {
        public GlobalSolutionItem(GlobalSolution.eImportItemType ItemType, string ItemExtraInfo, bool Selected, string ItemName, bool IsDependant)
        {
            this.ItemType = ItemType;
            this.ItemExtraInfo = ItemExtraInfo;
            this.Selected = Selected;
            this.ItemName = ItemName;
            this.IsDependant = IsDependant;

        }
        private bool mSelected = true;
        public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(nameof(Selected)); } } }
        public GlobalSolution.eImportItemType ItemType { get; set; }
        public bool IsDependant { get; set; }
        public string ItemName { get; set; }
        public string ItemExtraInfo { get; set; }
        public string Comments { get; set; }

        public Guid ItemGUID { get; set; }
       

        //public GlobalSolution.eItemDependancyType ItemDependancyType { get; set; }
        public GlobalSolution.eImportSetting ItemImportSetting { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
