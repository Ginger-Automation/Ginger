#region License
/*
Copyright © 2014-2021 European Support Limited

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
        List<NewAddedComboboxItem> NewAddedElementList = new List<NewAddedComboboxItem>();

       
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
                    xDeletedElementsMappingGrid.DataSourceList = new ObservableList<DeltaElementInfo>(mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Deleted)));
                    break;
            }
        }

        private void SetDeletedElementsGridView()
        {
            xDeletedElementsMappingGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.ElementTypeImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.ElementName), Header = "Deleted Element Name", WidthWeight = 200, AllowSorting = true, ReadOnly = true, BindingMode = BindingMode.OneWay });

            NewAddedElementList = GetNewAddedElementComboBoxItem();

            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.MappedElementInfo), Header = "New Added Element", StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = NewAddedElementList, ComboboxDisplayMemberField = nameof(NewAddedComboboxItem.DisplayValue), ComboboxSelectedValueField = nameof(NewAddedComboboxItem.InternalValue) });

            xDeletedElementsMappingGrid.SetAllColumnsDefaultView(view);
            xDeletedElementsMappingGrid.InitViewItems();

            xDeletedElementsMappingGrid.ShowPaste = Visibility.Collapsed;
            xDeletedElementsMappingGrid.ShowAdd = Visibility.Collapsed;
            xDeletedElementsMappingGrid.ShowDelete = Visibility.Collapsed;
            xDeletedElementsMappingGrid.ShowCopy = Visibility.Collapsed;
            xDeletedElementsMappingGrid.btnMarkAll.Visibility = Visibility.Collapsed;
            xDeletedElementsMappingGrid.btnRefresh.Visibility = Visibility.Visible;
            xDeletedElementsMappingGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(ResetMappingElementInfo));

          xDeletedElementsMappingGrid.SelectedItemChanged += XDeletedElementsMappingGrid_SelectedItemChanged;
        }

       
        private void ResetMappingElementInfo(object sender, RoutedEventArgs e)
        {
            foreach (var item in mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Deleted)))
            {
                item.MappedElementInfo = null;
            }
            NewAddedElementList.Clear();
            NewAddedElementList = GetNewAddedElementComboBoxItem();
        }

        private List<NewAddedComboboxItem> GetNewAddedElementComboBoxItem()
        {
            var comboSource = new List<NewAddedComboboxItem>();
            comboSource.Add(new NewAddedComboboxItem() { DisplayValue = "-None-", InternalValue = "None" });
            foreach (var item in mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Added)))
            {
                comboSource.Add(new NewAddedComboboxItem() { DisplayValue = item.ElementName, InternalValue = item.ElementInfo.Guid.ToString() });
            }

            return comboSource;
        }

        private void XDeletedElementsMappingGrid_SelectedItemChanged(object selectedItem)
        {
            foreach (var item in mPomWizard.mPomDeltaUtils.DeltaViewElements.Where(x => x.DeltaStatus.Equals(eDeltaStatus.Deleted)))
            {
                if (item.MappedElementInfo != null && item.MappedElementInfo.ToLower() != "none")
                {
                    var removeItem = NewAddedElementList.IndexOf(NewAddedElementList.Where(x => x.InternalValue == item.MappedElementInfo).FirstOrDefault());
                    NewAddedElementList.RemoveAt(removeItem);
                }
            }
        }


    }

    public class NewAddedComboboxItem
    {
        public string DisplayValue { get; set; }
        public string InternalValue { get; set; }
    }
}
