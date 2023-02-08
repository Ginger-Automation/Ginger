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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore.Drivers;
using GingerCore.GeneralLib;
using GingerCoreNET.Application_Models;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    /// <summary>
    /// Interaction logic for PomDeltaDeletedElementMappingWizardPage.xaml
    /// </summary>
    public partial class PomDeltaDeletedElementMappingWizardPage : Page,IWizardPage
    {
        PomDeltaWizard mPomWizard;
        List<NewAddedComboboxItem> NewAddedElementComboList = new List<NewAddedComboboxItem>();
       
        ObservableList<DeltaElementInfo> DeletedDeltaElementInfos = new ObservableList<DeltaElementInfo>();

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
                    DeletedDeltaElementInfos = new ObservableList<DeltaElementInfo>(mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Deleted)));
                    xDeletedElementsMappingGrid.DataSourceList = DeletedDeltaElementInfos;
                    break;
            }
        }

        private void SetDeletedElementsGridView()
        {
            xDeletedElementsMappingGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.ElementTypeImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });

            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.ElementName), Header = "Deleted Element Name", AllowSorting = true, ReadOnly = true, BindingMode = BindingMode.OneWay });

            GetNewAddedElementComboBoxItem();
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.MappedElementInfo), Header = "New Added Element", StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = NewAddedElementComboList, ComboboxDisplayMemberField = nameof(NewAddedComboboxItem.DisplayValue), ComboboxSelectedValueField = nameof(NewAddedComboboxItem.InternalValue), BindingMode = BindingMode.TwoWay });
            view.GridColsView.Add(new GridColView() { Field = " ", Header = " ",WidthWeight=15, BindingMode = BindingMode.OneWay, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["xMatchingElementTemplate"] });

            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.MappingElementStatus), Header = "Element Status", StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = GetElementStatusComoList() });

            xDeletedElementsMappingGrid.SetAllColumnsDefaultView(view);
            xDeletedElementsMappingGrid.InitViewItems();

            xDeletedElementsMappingGrid.ShowPaste = Visibility.Collapsed;
            xDeletedElementsMappingGrid.ShowAdd = Visibility.Collapsed;
            xDeletedElementsMappingGrid.ShowDelete = Visibility.Collapsed;
            xDeletedElementsMappingGrid.ShowCopy = Visibility.Collapsed;
            xDeletedElementsMappingGrid.btnMarkAll.Visibility = Visibility.Collapsed;
            xDeletedElementsMappingGrid.btnRefresh.Visibility = Visibility.Visible;
            xDeletedElementsMappingGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(ResetMappingElementInfo));

            xDeletedElementsMappingGrid.Grid.SelectionChanged += Grid_SelectionChanged;
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSelected = (DeltaElementInfo)xDeletedElementsMappingGrid.Grid.CurrentItem;

            RemoveSelectedElementFromCombobox(currentSelected);
        }

        private void RemoveSelectedElementFromCombobox(DeltaElementInfo currentSelected=null)
        {
            NewAddedElementComboList.Clear();
            GetNewAddedElementComboBoxItem();
            foreach (var item in DeletedDeltaElementInfos)
            {
                if (currentSelected != null && item.MappedElementInfo == currentSelected.MappedElementInfo)
                {
                    continue;
                }
                if (item.MappedElementInfo != null && item.MappedElementInfo.ToLower() != "none")
                {
                    var removeItem = NewAddedElementComboList.IndexOf(NewAddedElementComboList.Where(x => x.InternalValue == item.MappedElementInfo).FirstOrDefault());

                    if (removeItem != -1)
                    {
                        NewAddedElementComboList.RemoveAt(removeItem);
                    }

                }
            }

        }

        private List<ComboEnumItem> GetElementStatusComoList()
        {
            List<ComboEnumItem> elementStatus = new List<ComboEnumItem>();
            elementStatus.Add(new ComboEnumItem() { text = "Deleted Element", Value =DeltaElementInfo.eMappingStatus.DeletedElement });
            elementStatus.Add(new ComboEnumItem() { text = "Replace Existing Element", Value = DeltaElementInfo.eMappingStatus.ReplaceExistingElement });
            return elementStatus;
        }

        private void ResetMappingElementInfo(object sender, RoutedEventArgs e)
        {
            foreach (var item in mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Deleted)))
            {
                item.MappedElementInfo = null;
            }
            NewAddedElementComboList.Clear();

            Dispatcher.Invoke(() =>
            {
                SetDeletedElementsGridView();
            });
        }

        private void GetNewAddedElementComboBoxItem()
        {
            NewAddedElementComboList.Add(new NewAddedComboboxItem() { DisplayValue = "-None-", InternalValue = "None" });
            foreach (var item in mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Added)))
            {
                NewAddedElementComboList.Add(new NewAddedComboboxItem() { DisplayValue = item.ElementName, InternalValue = item.ElementInfo.Guid.ToString() });
            }
        }

        private void xManualMatchBtn_Click(object sender, RoutedEventArgs e)
        {
            //current selected deleted element
            var currentItem = (DeltaElementInfo)xDeletedElementsMappingGrid.CurrentItem;
            var deltaAddedList = mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Added));

            //filtering mapped added element
            var filteredAddedList = new ObservableList<DeltaElementInfo>(deltaAddedList.Where(x => NewAddedElementComboList.Any(y => y.InternalValue == x.ElementInfo.Guid.ToString())));

            mPomNewAddedElementSelectionPage = new PomNewAddedElementSelectionPage(filteredAddedList, mPomWizard.mPomDeltaUtils, Convert.ToString(currentItem.ElementTypeEnum),new GridColView() { Field = "Compare", Header = "Compare", BindingMode = BindingMode.OneWay, StyleType = GridColView.eGridColStyleType.Template, ReadOnly = false, CellTemplate = (DataTemplate)this.MainGrid.Resources["xCompareElementPropTemplate"] });

            var selectedElement = mPomNewAddedElementSelectionPage.ShowAsWindow("Added Elements");
            if (selectedElement != null)
            {
                currentItem.MappedElementInfo = selectedElement.ElementInfo.Guid.ToString();
                currentItem.MappingElementStatus = DeltaElementInfo.eMappingStatus.ReplaceExistingElement;
                RemoveSelectedElementFromCombobox(selectedElement);
            }
        }

        private void xCompareElementPropButton_Click(object sender, RoutedEventArgs e)
        {
            var currentItem = (DeltaElementInfo)xDeletedElementsMappingGrid.CurrentItem;
            var newAddedElement = mPomNewAddedElementSelectionPage.mPomDeltaViewPage.mSelectedElement;

            new PomDeltaMappingElementsComparePage(currentItem,newAddedElement).ShowAsWindow("Element Details Comparison");
        }
    }

    public class NewAddedComboboxItem
    {
        public string DisplayValue { get; set; }
        public string InternalValue { get; set; }
    }
}
