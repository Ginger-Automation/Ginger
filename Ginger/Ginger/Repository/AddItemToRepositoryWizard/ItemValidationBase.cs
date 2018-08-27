#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Ginger.Properties;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using GingerCore;
using Amdocs.Ginger.Common;
using GingerCore.Activities;
using GingerCore.Variables;
using GingerCore.Actions;
using System.Linq;
using Ginger.Repository.AddItemToRepositoryWizard;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.Repository.ItemToRepositoryWizard
{
    public class ItemValidationBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static class Fields
        {
            public static string Selected = "Selected";
            public static string ItemName = "ItemName";
            public static string ItemGUID = "ItemGUID";
            public static string IssueDescription = "IssueDescription";
            public static string IssueResolution = "IssueResolution";
            public static string Details = "Details";
            public static string IssueType = "IssueType";
            public static string ItemClass = "ItemClass";
            public static string Impact = "Impact";
        }
        public RepositoryItemBase UsageItem { get; set; }

        public static ObservableList<ItemValidationBase> mIssuesList = new ObservableList<ItemValidationBase>();

        private bool mSelected;
        public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(Fields.Selected); } } }
        public string ItemName { get; set; }
        public string ItemClass { get; set; }
        public string IssueDescription { get; set; }
        public string IssueResolution{ get; set; }
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
                mIssueType =(eIssueType)Enum.Parse(typeof(eIssueType),value);
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

        public static void Validate(UploadItemSelection selectedItem)
        {  
            bool isDuplicateFound = CheckForItemWithDuplicateName(selectedItem);
            if (isDuplicateFound)
            {
                    ItemValidationBase VA = CreateNewIssue((RepositoryItem)selectedItem.UsageItem);
                    VA.IssueDescription = "Item with same name already exists";
                    VA.mIssueType = eIssueType.DuplicateName;
                    VA.ItemNewName = GetUniqueItemName(selectedItem);
                    VA.IssueResolution = "Item will be uploaded with new name: " + VA.ItemNewName;
                    VA.Selected = true;
                    mIssuesList.Add(VA);
            }
            switch (selectedItem.UsageItem.GetType().Name)
            {
                case "Activity":
                    ValidateActivity.Validate((Activity)selectedItem.UsageItem);
                    break;
            }
        }

        public static bool CheckForItemWithDuplicateName(UploadItemSelection selectedItem)
        {
            List<RepositoryItemBase> existingRepoItems = new List<RepositoryItemBase>();
            ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            ObservableList<Act> SharedActions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
            ObservableList<VariableBase> variables= WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
            if (selectedItem.UsageItem is ActivitiesGroup) existingRepoItems = App.LocalRepository.GetSolutionRepoActivitiesGroups().Cast<RepositoryItemBase>().ToList();            
            else if (selectedItem.UsageItem is Activity) existingRepoItems = activities.Cast<RepositoryItemBase>().ToList();
            else if (selectedItem.UsageItem is Act) existingRepoItems = SharedActions.Cast<RepositoryItemBase>().ToList();
            
            else if (selectedItem.UsageItem is VariableBase) existingRepoItems = variables.Cast<RepositoryItemBase>().ToList();
            
            if (selectedItem.ItemUploadType == UploadItemSelection.eItemUploadType.Overwrite)
            {
                existingRepoItems.Remove(selectedItem.ExistingItem);
            }

            foreach (object o in existingRepoItems)
            {       
                if (((RepositoryItem)o).GetNameForFileName() == selectedItem.ItemName)
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetUniqueItemName(UploadItemSelection duplicateItem)
        {
            List<string> existingRepoItems = new List<string>();
            ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            ObservableList<Act> actions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
            ObservableList<VariableBase> variables = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();

            if (duplicateItem.UsageItem is ActivitiesGroup) existingRepoItems = App.LocalRepository.GetSolutionRepoActivitiesGroups().Select(x => x.ItemName).ToList();            
            else if (duplicateItem.UsageItem is Activity) existingRepoItems = activities.Select(x => x.ItemName).ToList(); 
            else if (duplicateItem.UsageItem is Act) existingRepoItems = actions.Select(x => x.ItemName).ToList(); 
            else if (duplicateItem.UsageItem is VariableBase) existingRepoItems = variables.Select(x => x.ItemName).ToList();

            string newItemName = duplicateItem.ItemName;
           
            while (true)
            {
                newItemName += "_copy";

                if (!existingRepoItems.Contains(newItemName))
                {
                    return newItemName;
                }
            }
            //TODO - find better way to get unique name
        }

        public static ItemValidationBase CreateNewIssue(RepositoryItemBase rItem)
        {
            ItemValidationBase ITB = new ItemValidationBase();
            ITB.UsageItem = rItem;
            ITB.ItemName = rItem.ItemName;
            ITB.ItemClass = RepositoryItem.GetShortType(rItem.GetType());          
            return ITB;
        }

        public static List<ItemValidationBase> GetAllIssuesForItem(UploadItemSelection item)
        {
            return mIssuesList.Where(x => x.UsageItem.Guid == item.UsageItem.Guid).ToList();
        }
    }
}
