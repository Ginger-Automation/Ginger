using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.GlobalSolutionLib
{
    public class GlobalSolutionItem : RepositoryItemBase, INotifyPropertyChanged
    {
        public GlobalSolutionItem(GlobalSolution.eImportItemType ItemType, string ItemExtraInfo, bool Selected, string ItemName, GlobalSolution.eItemDependancyType ItemDependancyType)
        {
            this.ItemType = ItemType;
            this.ItemExtraInfo = ItemExtraInfo;
            this.Selected = Selected;
            this.ItemName = ItemName;
            this.ItemDependancyType = ItemDependancyType;

        }
        private bool mSelected = true;
        public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(nameof(Selected)); } } }
        public GlobalSolution.eImportItemType ItemType { get; set; }
        public override string ItemName { get; set; }
        public string ItemExtraInfo { get; set; }

        public string ItemGUID { get; set; }
        public string ItemParentGUID { get; set; }
        public string ItemSourcePath { get; set; }
        public string ItemTargetPath { get; set; }

        public GlobalSolution.eItemDependancyType ItemDependancyType { get; set; }
        public GlobalSolution.eImportSetting ItemImportSetting { get; set; }

        //public event PropertyChangedEventHandler PropertyChanged;

        //public void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}
    }
}
