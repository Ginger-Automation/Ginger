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

using Amdocs.Ginger.Common;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.UserControls;
using GingerWPF.WizardLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using Amdocs.Ginger.UserControls;

namespace Ginger.Repository.ItemToRepositoryWizard
{
    /// <summary>
    /// Interaction logic for ItemSelectionPage.xaml
    /// </summary>
    public partial class UploadItemsSelectionPage : Page, IWizardPage
    {
        public UploadItemToRepositoryWizard UploadItemToRepositoryWizard;
        public UploadItemsValidationPage itemValidate;
        bool isConvertPage = false;
        public UploadItemsSelectionPage(ObservableList<UploadItemSelection> items, bool isConvert)
        {
            InitializeComponent();
            isConvertPage = isConvert;
            SetSelectedItemsGridView();
            itemSelectionGrid.DataSourceList = items;
        }

        private void SetSelectedItemsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(UploadItemSelection.Selected), StyleType = GridColView.eGridColStyleType.CheckBox, WidthWeight = 10 },
                new GridColView() { Field = nameof(UploadItemSelection.ItemName), Header = "Item To Upload", WidthWeight = 20, ReadOnly = true },
                new GridColView()
                {
                    Field = nameof(UploadItemSelection.ReplaceType),
                    Header = "Replace Type",
                    WidthWeight = 20,
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = ucGrid.GetGridComboBoxTemplate(GingerCore.General.GetEnumValuesForCombo(typeof(UploadItemSelection.eActivityInstanceType)), nameof(UploadItemSelection.ReplaceType), false, false, nameof(UploadItemSelection.IsActivity), true),
                    ReadOnly = isConvertPage
                },
                new GridColView()
                {
                    Field = nameof(UploadItemSelection.TargetFolderDisplay),
                    Header = "Add to Folder",
                    WidthWeight = 20,
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = CreateSelectFolderTemplate(),
                    ReadOnly = false 
                },
                ]
            };

            GridColView GCWUploadType = new GridColView()
            {
                Field = nameof(UploadItemSelection.ItemUploadType),
                Header = "Upload Type",
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(UploadItemSelection.UploadTypeList), nameof(UploadItemSelection.ItemUploadType), false, false, nameof(UploadItemSelection.IsExistingItemParent), true),
                WidthWeight = 18
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
            defView.GridColsView.Add(new GridColView() { Field = nameof(UploadItemSelection.Comment), StyleType = GridColView.eGridColStyleType.Text, Header = "Comment", WidthWeight = 25, ReadOnly = true });
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

        private DataTemplate CreateSelectFolderTemplate()
        {
            var template = new DataTemplate();

            // Horizontal panel: [Selected path text] [ ... button ]
            var panel = new FrameworkElementFactory(typeof(StackPanel));
            panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            panel.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            panel.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Path text (bound to TargetFolderDisplay)
            var txt = new FrameworkElementFactory(typeof(TextBlock));
            txt.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            txt.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 6, 0));
            txt.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
            txt.SetValue(TextBlock.ToolTipProperty, new Binding(nameof(UploadItemSelection.TargetFolderDisplay)));
            txt.SetBinding(TextBlock.TextProperty, new Binding(nameof(UploadItemSelection.TargetFolderDisplay)) { Mode = BindingMode.OneWay });
            panel.AppendChild(txt);

            // Ellipsis button to open the folder selection window
            var btn = new FrameworkElementFactory(typeof(Button));
            btn.SetValue(Amdocs.Ginger.UserControls.ucButton.ButtonTypeProperty, Amdocs.Ginger.Core.eButtonType.ImageButton);
            btn.SetValue(Amdocs.Ginger.UserControls.ucButton.ButtonImageTypeProperty, Amdocs.Ginger.Common.Enums.eImageType.EllipsisH);
            btn.SetValue(FrameworkElement.ToolTipProperty, "Select Shared Repository Folder");
            btn.SetValue(FrameworkElement.WidthProperty, 24.0);
            btn.SetValue(FrameworkElement.HeightProperty, 22.0);
            btn.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            btn.AddHandler(Button.ClickEvent , new RoutedEventHandler(SelectFolder_Click));
            panel.AppendChild(btn);

            template.VisualTree = panel;
            return template;
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is UploadItemSelection row)
            {
                // Open folder picker; returns RepositoryFolderBase or null
                var owner = Window.GetWindow(this);
                var selectedFolder = SelectSharedRepositoryFolderPage.ShowWindow(owner, eWindowShowStyle.Dialog);

                if (selectedFolder != null)
                {
                    // Persist the chosen folder full path on the row
                    row.TargetFolderFullPath = selectedFolder.FolderFullPath;
                }
            }
        }
        private void SelectUnSelectAll(bool activeStatus)
        {
            if (UploadItemSelection.mSelectedItems.Count > 0)
            {
                foreach (UploadItemSelection usage in UploadItemSelection.mSelectedItems)
                {
                    usage.Selected = activeStatus;
                }
            }
        }
    }
}