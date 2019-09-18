#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Common;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.UserControls;
using GingerWPF.WizardLib;

namespace Ginger.Repository.ItemToRepositoryWizard
{
    /// <summary>
    /// Interaction logic for ItemValidationPage.xaml
    /// </summary>
    public partial class UploadItemsValidationPage : Page, IWizardPage
    {
        public UploadItemToRepositoryWizard UploadItemToRepositoryWizard;
        public ObservableList<ItemValidationBase> lstItemValidation = new ObservableList<ItemValidationBase>();
        public UploadItemsValidationPage()
        {
            InitializeComponent();
            SetItemValidationGridView();
        }

        private void SetItemValidationGridView()
        {
            //# Default View

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ItemValidationBase.Selected), Header = "Accept Resolution", StyleType = GridColView.eGridColStyleType.CheckBox, WidthWeight = 10 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ItemValidationBase.ItemClass), ReadOnly = true, Header = "Item Type", WidthWeight = 10, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ItemValidationBase.ItemName), ReadOnly = true, Header = "Item Name", WidthWeight = 20, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ItemValidationBase.IssueType), ReadOnly = true, Header = "Issue Type", WidthWeight = 10, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ItemValidationBase.IssueDescription), ReadOnly = true, Header = "Issue Description", WidthWeight = 15, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ItemValidationBase.IssueResolution), ReadOnly = true, Header = "Issue Resolution", WidthWeight = 25, AllowSorting = true });


            itemValidationGrid.SetAllColumnsDefaultView(view);
            itemValidationGrid.InitViewItems();
            itemValidationGrid.MarkUnMarkAllActive += SelectUnSelectAll;
        }

        private void SelectUnSelectAll(bool activeStatus)
        {          
            if (ItemValidationBase.mIssuesList.Count > 0)
            {                
                foreach (ItemValidationBase usage in ItemValidationBase.mIssuesList)
                    usage.Selected = activeStatus;
            }
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:            
                    UploadItemToRepositoryWizard = ((UploadItemToRepositoryWizard)WizardEventArgs.Wizard);
                    break;

                case EventType.Active:
            
                    ItemValidationBase.mIssuesList.Clear();
                    int issuesCount = 0;
                    foreach (UploadItemSelection item in UploadItemSelection.mSelectedItems)
                    {
                        if (item.Selected)
                        {
                            issuesCount = ItemValidationBase.mIssuesList.Count;
                            ItemValidationBase.Validate(item);
                        }
                    }

                    if (ItemValidationBase.mIssuesList.Count > 0)
                    {
                        itemValidationGrid.DataSourceList = ItemValidationBase.mIssuesList;
                        itemValidationGrid.Visibility = Visibility.Visible;
                        xLabelMessage.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        itemValidationGrid.Visibility = Visibility.Collapsed;
                        xLabelMessage.Visibility = Visibility.Visible;

                        xLabelMessage.Content = "No Validation Issues Found. Proceed with Item/s Upload";
                    }
                    break;
            }
        }
    }
}
