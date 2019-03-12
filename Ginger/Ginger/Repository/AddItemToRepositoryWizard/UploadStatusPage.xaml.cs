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

using System.Windows.Controls;
using Amdocs.Ginger.Common;
using Ginger.Repository.ItemToRepositoryWizard;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Variables;
using GingerWPF.WizardLib;

namespace Ginger.Repository.AddItemToRepositoryWizard
{
    /// <summary>
    /// Interaction logic for UploadStatusPage.xaml
    /// </summary>
    public partial class UploadStatusPage : Page, IWizardPage
    {
        public UploadStatusPage()
        {
            InitializeComponent();
            SetSelectedItemsGridView();
            itemStatusGrid.DataSourceList = UploadItemSelection.mSelectedItems;            
        }

        private void SetSelectedItemsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(UploadItemSelection.Selected), StyleType = GridColView.eGridColStyleType.CheckBox, WidthWeight = 10, ReadOnly=true});
            defView.GridColsView.Add(new GridColView() { Field = nameof(UploadItemSelection.ItemName), Header = "Item Name", WidthWeight = 25, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(UploadItemSelection.ItemUploadType), Header = "Item Upload Type", WidthWeight = 20, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(UploadItemSelection.SelectedItemPart), Header="Part To Upload", WidthWeight=10 ,ReadOnly=true});
            defView.GridColsView.Add(new GridColView() { Field = nameof(UploadItemSelection.ItemUploadStatus), Header = "Status", WidthWeight = 10, ReadOnly = true });
           
            defView.GridColsView.Add(new GridColView() { Field = nameof(UploadItemSelection.Comment), StyleType = GridColView.eGridColStyleType.Text, Header = "Comment", WidthWeight = 20, ReadOnly = true });
            itemStatusGrid.SetAllColumnsDefaultView(defView);
            itemStatusGrid.InitViewItems();     
        }

        public UploadItemToRepositoryWizard UploadItemToRepositoryWizard;
        void IWizardPage.WizardEvent(WizardEventArgs WizardEventArgs)
        {
            if (WizardEventArgs.EventType == EventType.Init)
            {
                UploadItemToRepositoryWizard = ((UploadItemToRepositoryWizard)WizardEventArgs.Wizard);
            }
            else if (WizardEventArgs.EventType == EventType.Active)
            {
                foreach (UploadItemSelection selectedItem in UploadItemSelection.mSelectedItems)
                {
                    if (selectedItem.Selected )
                    {
                        if (selectedItem.ItemUploadStatus != UploadItemSelection.eItemUploadStatus.Uploaded)
                        {
                          (new SharedRepositoryOperations()).UploadItemToRepository(UploadItemToRepositoryWizard.Context, selectedItem);
                        }
                    }
                    else 
                        selectedItem.ItemUploadStatus = UploadItemSelection.eItemUploadStatus.Skipped;
                }

                // UploadItemToRepositoryWizard.PrevVisible = false;
                // UploadItemToRepositoryWizard.NextVisible = false;
                // UploadItemToRepositoryWizard.mWizardWindow.CancelButton.Visibility = System.Windows.Visibility.Collapsed; /// WHY FIXME
                // UploadItemToRepositoryWizard.FinishEnabled = true;
            }
        }
    }
}
