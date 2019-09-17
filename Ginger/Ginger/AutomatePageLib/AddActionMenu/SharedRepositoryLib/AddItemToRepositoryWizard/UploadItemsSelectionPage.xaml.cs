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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.UserControls;
using GingerCore.GeneralLib;
using GingerWPF.WizardLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Repository.ItemToRepositoryWizard
{
    /// <summary>
    /// Interaction logic for ItemSelectionPage.xaml
    /// </summary>
    public partial class UploadItemsSelectionPage : Page, IWizardPage
    {
        public UploadItemToRepositoryWizard UploadItemToRepositoryWizard;
        public UploadItemsValidationPage itemValidate;
      
        public UploadItemsSelectionPage(ObservableList<UploadItemSelection> items)
        {
            InitializeComponent();
            SetSelectedItemsGridView();
            itemSelectionGrid.DataSourceList = items;       
        }

        private void SetSelectedItemsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(UploadItemSelection.Selected), StyleType = GridColView.eGridColStyleType.CheckBox, WidthWeight = 10 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(UploadItemSelection.ItemName), Header = "Item To Upload", WidthWeight = 15, ReadOnly = true });
            
            List<ComboEnumItem> itemUploadTypeList = GingerCore.General.GetEnumValuesForCombo(typeof(UploadItemSelection.eItemUploadType));
            GridColView GCWUploadType = new GridColView()
            {
                Field = nameof(UploadItemSelection.ItemUploadType),
                Header = "Upload Type",
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(UploadItemSelection.UploadTypeList), nameof(UploadItemSelection.ItemUploadType), false, false, nameof(UploadItemSelection.IsExistingItemParent), true),
                
                WidthWeight = 15
            };

            defView.GridColsView.Add(GCWUploadType);
            defView.GridColsView.Add(new GridColView() { Field = nameof(UploadItemSelection.ExistingItemName), Header = "Existing Item", WidthWeight = 15, ReadOnly = true });

            GridColView GCW = new GridColView()
            {
                Field = nameof(UploadItemSelection.SelectedItemPart),
                Header = "Part to Upload",
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(UploadItemSelection.PartToUpload), nameof(UploadItemSelection.SelectedItemPart), false, false, nameof(UploadItemSelection.IsOverrite), true),
                WidthWeight = 15
            };

            defView.GridColsView.Add(GCW);
            defView.GridColsView.Add(new GridColView() { Field = nameof(UploadItemSelection.Comment), StyleType = GridColView.eGridColStyleType.Text, Header = "Comment", WidthWeight = 30, ReadOnly = true });
            itemSelectionGrid.SetAllColumnsDefaultView(defView);
            itemSelectionGrid.InitViewItems();
            itemSelectionGrid.btnMarkAll.Visibility = Visibility.Visible;
            itemSelectionGrid.MarkUnMarkAllActive += SelectUnSelectAll;
            itemSelectionGrid.AddToolbarTool("@DropDownList_16x16.png", "Set Same Selected Part to All", new RoutedEventHandler(SetSamePartToAll));
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    UploadItemToRepositoryWizard = ((UploadItemToRepositoryWizard)WizardEventArgs.Wizard);
                    break;
            }
            //UploadItemToRepositoryWizard = ((UploadItemToRepositoryWizard)WizardEventArgs.Wizard);
            //UploadItemToRepositoryWizard.FinishEnabled = false;

            //if (WizardEventArgs.EventType == EventType.Prev)
            //{
            //}
            //else if (WizardEventArgs.EventType == EventType.Next)
            //{
            //}
            //else if (WizardEventArgs.EventType == EventType.Validate)
            //{
            //    if (UploadItemSelection.mSelectedItems.Count == 0)
            //        WizardEventArgs.AddError("Select atleast 1 item to process");
            //}
            //else if (WizardEventArgs.EventType == EventType.Cancel)
            //{
            //}
            //else if (WizardEventArgs.EventType == EventType.Active)
            //{
            //    UploadItemToRepositoryWizard.NextEnabled = true;
            //}
        }

        private void SetSamePartToAll(object sender, RoutedEventArgs e)
        {
            if (itemSelectionGrid.CurrentItem != null)
            {
                UploadItemSelection a = (UploadItemSelection)itemSelectionGrid.CurrentItem;
                foreach (UploadItemSelection itm in UploadItemSelection.mSelectedItems)
                {
                    if (itm.ItemUploadType != UploadItemSelection.eItemUploadType.New)
                    {
                        itm.SelectedItemPart = a.SelectedItemPart;
                    }
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

        private void SelectUnSelectAll(bool activeStatus)
        {           
            if (UploadItemSelection.mSelectedItems.Count > 0)
            {                
                foreach (UploadItemSelection usage in UploadItemSelection.mSelectedItems)
                    usage.Selected = activeStatus;
            }
        }
    }
}