#region License
/*
Copyright © 2014-2023 European Support Limited

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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.GlobalSolutionLib
{
    public class GlobalSolutionItem : INotifyPropertyChanged
    {
        public GlobalSolutionItem(GlobalSolution.eImportItemType ItemType, string ItemFullPath, string ItemExtraInfo, bool Selected, string ItemName, string RequiredFor)
        {
            this.ItemType = ItemType;
            this.ItemFullPath = ItemFullPath;
            this.ItemExtraInfo = ItemExtraInfo;
            this.Selected = Selected;
            this.ItemName = ItemName;
            this.RequiredFor = RequiredFor;
        }
        private bool mSelected = true;
        public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(nameof(Selected)); } } }
        public GlobalSolution.eImportItemType ItemType { get; set; }
        public string ItemName { get; set; }
        public string ItemExtraInfo { get; set; }
        public string ItemFullPath { get; set; }
        public string Comments { get; set; }
        public string ItemNewName { get; set; }
        public string RequiredFor { get; set; }

        public Guid ItemGUID { get; set; }
       

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
