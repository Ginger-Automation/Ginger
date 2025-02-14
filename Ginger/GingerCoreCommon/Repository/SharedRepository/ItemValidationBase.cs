#region License
/*
Copyright © 2014-2025 European Support Limited

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
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;

namespace Ginger.Repository.ItemToRepositoryWizard
{
    public class ItemValidationBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public RepositoryItemBase UsageItem { get; set; }

        public static ObservableList<ItemValidationBase> mIssuesList = [];

        private bool mSelected;
        public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(nameof(Selected)); } } }
        public string ItemName { get; set; }
        public string ItemClass { get; set; }
        public string IssueDescription { get; set; }
        public string IssueResolution { get; set; }
        public string ItemNewName { get; set; }
        public List<String> missingVariablesList { get; set; }

        public enum eIssueType
        {
            DuplicateName,
            MissingVariables
        }

        public eIssueType mIssueType;

        public string IssueType
        {
            get
            {
                return mIssueType.ToString();
            }
            set
            {
                mIssueType = (eIssueType)Enum.Parse(typeof(eIssueType), value);
            }
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }



        public static ItemValidationBase CreateNewIssue(RepositoryItemBase rItem)
        {
            ItemValidationBase ITB = new ItemValidationBase
            {
                UsageItem = rItem,
                ItemName = rItem.ItemName,
                // TODO: remove me and use RepositoryItemBase
                //ITB.ItemClass = RepositoryItem.GetShortType(rItem.GetType());
                ItemClass = rItem.GetItemType()
            };

            return ITB;
        }

        public static List<ItemValidationBase> GetAllIssuesForItem(UploadItemSelection item)
        {
            return mIssuesList.Where(x => x.UsageItem.Guid == item.UsageItem.Guid).ToList();
        }
    }
}
