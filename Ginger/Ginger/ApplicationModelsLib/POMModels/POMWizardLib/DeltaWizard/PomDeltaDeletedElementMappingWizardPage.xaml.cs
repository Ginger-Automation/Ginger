#region License
/*
Copyright © 2014-2026 European Support Limited

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
using Ginger.UserControls;
using GingerCore.GeneralLib;
using GingerCoreNET.Application_Models;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    /// <summary>
    /// Interaction logic for PomDeltaDeletedElementMappingWizardPage.xaml
    /// </summary>
    public partial class PomDeltaDeletedElementMappingWizardPage : Page, IWizardPage
    {
        PomDeltaWizard mPomWizard;
        //List<NewAddedComboboxItem> NewAddedElementComboList = new List<NewAddedComboboxItem>();

        ObservableList<DeltaElementInfo> DeletedDeltaElementInfos = [];

        private PomNewAddedElementSelectionPage mPomNewAddedElementSelectionPage;

        public PomDeltaDeletedElementMappingWizardPage()
        {
            InitializeComponent();
        }
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mPomWizard = (PomDeltaWizard)WizardEventArgs.Wizard;
                    break;

                case EventType.Active:
                    SetDeletedElementsGridView();
                    DeletedDeltaElementInfos = new ObservableList<DeltaElementInfo>(mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Deleted) && x.IsSelected == true));
                    xDeletedElementsMappingGrid.DataSourceList = DeletedDeltaElementInfos;
                    break;
            }
        }

        private void SetDeletedElementsGridView()
        {
            xDeletedElementsMappingGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(DeltaElementInfo.ElementTypeImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 },
                new GridColView() { Field = nameof(DeltaElementInfo.ElementName), Header = "Deleted Existing Element", WidthWeight=100, AllowSorting = true, ReadOnly = true, BindingMode = BindingMode.OneWay },
                //GetNewAddedElementComboBoxItem();
                //view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.MappedElementInfo), Header = "Mapped New Element", StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = NewAddedElementComboList, ComboboxDisplayMemberField = nameof(NewAddedComboboxItem.DisplayValue), ComboboxSelectedValueField = nameof(NewAddedComboboxItem.InternalValue), BindingMode = BindingMode.TwoWay });
                new GridColView() { Field = nameof(DeltaElementInfo.MappedElementInfoName), Header = "Mapped New Element", WidthWeight = 100, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = " ", Header = " ", WidthWeight = 15, MaxWidth=50, BindingMode = BindingMode.OneWay, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["xMatchingElementTemplate"] },
                new GridColView() { Field = nameof(DeltaElementInfo.MappingElementStatus), WidthWeight = 50, Header = "Operation", StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = GetElementStatusComoList(), BindingMode = BindingMode.TwoWay },
            ]
            };

            xDeletedElementsMappingGrid.SetAllColumnsDefaultView(view);
            xDeletedElementsMappingGrid.InitViewItems();

            xDeletedElementsMappingGrid.ShowPaste = Visibility.Collapsed;
            xDeletedElementsMappingGrid.ShowAdd = Visibility.Collapsed;
            xDeletedElementsMappingGrid.ShowDelete = Visibility.Collapsed;
            xDeletedElementsMappingGrid.ShowCopy = Visibility.Collapsed;
            xDeletedElementsMappingGrid.btnMarkAll.Visibility = Visibility.Collapsed;
            xDeletedElementsMappingGrid.btnRefresh.Visibility = Visibility.Visible;
            xDeletedElementsMappingGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(ResetMappingElementInfo));

            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: xDeletedElementsMappingGrid.Grid, eventName: nameof(Selector.SelectionChanged), handler: Grid_SelectionChanged);
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSelected = (DeltaElementInfo)xDeletedElementsMappingGrid.Grid.CurrentItem;

            //RemoveSelectedElementFromCombobox(currentSelected);
        }

        //private void RemoveSelectedElementFromCombobox(DeltaElementInfo currentSelected = null)
        //{
        //    NewAddedElementComboList.Clear();
        //    GetNewAddedElementComboBoxItem();
        //    foreach (var item in DeletedDeltaElementInfos)
        //    {
        //        if (currentSelected != null && item.MappedElementInfo == currentSelected.MappedElementInfo)
        //        {
        //            continue;
        //        }
        //        if (item.MappedElementInfo != null && item.MappedElementInfo.ToLower() != "none")
        //        {
        //            var removeItem = NewAddedElementComboList.IndexOf(NewAddedElementComboList.FirstOrDefault(x => x.InternalValue == item.MappedElementInfo));

        //            if (removeItem != -1)
        //            {
        //                NewAddedElementComboList.RemoveAt(removeItem);
        //            }

        //        }
        //    }
        //}

        private List<ComboEnumItem> GetElementStatusComoList()
        {
            List<ComboEnumItem> elementStatus =
            [
                new ComboEnumItem() { text = "Delete Existing Element", Value = DeltaElementInfo.eMappingStatus.DeletedElement },
                new ComboEnumItem() { text = "Replace Existing Element with New", Value = DeltaElementInfo.eMappingStatus.ReplaceExistingElement },
                new ComboEnumItem() { text = "Merge Existing & New Elements", Value = DeltaElementInfo.eMappingStatus.MergeExistingElement },
            ];
            return elementStatus;
        }

        private void ResetMappingElementInfo(object sender, RoutedEventArgs e)
        {
            foreach (var item in mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Deleted) && x.IsSelected))
            {
                item.MappedElementInfo = null;
            }
            //NewAddedElementComboList.Clear();

            Dispatcher.Invoke(() =>
            {
                SetDeletedElementsGridView();
            });
        }

        //private void GetNewAddedElementComboBoxItem()
        //{
        //    NewAddedElementComboList.Add(new NewAddedComboboxItem() { DisplayValue = "-None-", InternalValue = "None" });
        //    foreach (var item in mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Added)))
        //    {
        //        NewAddedElementComboList.Add(new NewAddedComboboxItem() { DisplayValue = item.ElementName, InternalValue = item.ElementInfo.Guid.ToString() });
        //    }
        //}

        private void xManualMatchBtn_Click(object sender, RoutedEventArgs e)
        {
            //current selected deleted element
            var currentItem = (DeltaElementInfo)xDeletedElementsMappingGrid.CurrentItem;
            var deltaAddedList = new ObservableList<DeltaElementInfo>(mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Added) && x.IsSelected));

            //filtering mapped added element
            // var filteredAddedList = new ObservableList<DeltaElementInfo>(deltaAddedList.Where(x => NewAddedElementComboList.Any(y => y.InternalValue == x.ElementInfo.Guid.ToString())));

            mPomNewAddedElementSelectionPage = new PomNewAddedElementSelectionPage(deltaAddedList, mPomWizard.mPomDeltaUtils, Convert.ToString(currentItem.ElementTypeEnum), new GridColView() { Field = "Compare", Header = "Compare", BindingMode = BindingMode.OneWay, StyleType = GridColView.eGridColStyleType.Template, ReadOnly = false, CellTemplate = (DataTemplate)this.MainGrid.Resources["xCompareElementPropTemplate"] });

            var selectedElement = mPomNewAddedElementSelectionPage.ShowAsWindow("Added Elements");
            if (selectedElement != null)
            {
                currentItem.MappedElementInfoName = selectedElement.ElementInfo.ElementName;
                currentItem.MappedElementInfo = selectedElement.ElementInfo.Guid.ToString();
                currentItem.MappingElementStatus = DeltaElementInfo.eMappingStatus.ReplaceExistingElement;
                //RemoveSelectedElementFromCombobox(selectedElement);
            }
        }

        private void xCompareElementPropButton_Click(object sender, RoutedEventArgs e)
        {
            var currentItem = (DeltaElementInfo)xDeletedElementsMappingGrid.CurrentItem;
            var newAddedElement = mPomNewAddedElementSelectionPage.mPomDeltaViewPage.mSelectedElement;

            new PomDeltaMappingElementsComparePage(currentItem, newAddedElement).ShowAsWindow("Elements Details Comparison");
        }

    }

    //public class NewAddedComboboxItem
    //{
    //    public string DisplayValue { get; set; }
    //    public string InternalValue { get; set; }
    //}
}
