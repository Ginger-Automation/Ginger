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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static GingerCore.General;

namespace Ginger.ApplicationModelsLib.POMModels
{
    public partial class PomElementsPage : Page
    {
        public PomAllElementsPage.eElementsContext mContext;
        ApplicationPOMModel mPOM;
        ObservableList<ElementInfo> mElements;

        ComboEnumItem mCurrentGroupByFilter;
        ComboEnumItem mCurrentDeltaStatusFilter;

        bool IsFirstSelection = true;


        private Agent mAgent;
        IWindowExplorer mWinExplorer
        {
            get
            {
                if (mAgent != null && mAgent.Status == Agent.eStatus.Running)
                {                    
                    return mAgent.Driver as IWindowExplorer;
                }
                else
                {
                    if (mAgent != null)
                    {
                        mAgent.Close();
                    }
                    return null;
                }
            }
        }

        public ucGrid MainElementsGrid
        {
            get
            {
                return xMainElementsGrid;
            }
        }

        ElementInfo mSelectedElement
        {
            get
            {
                if (xMainElementsGrid.Grid.SelectedItem != null)
                {
                    return (ElementInfo)xMainElementsGrid.Grid.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }
        ElementLocator mSelectedLocator
        {
            get
            {
                if (xLocatorsGrid.Grid.SelectedItem != null)
                {
                    return (ElementLocator)xLocatorsGrid.Grid.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }



        public PomElementsPage(ApplicationPOMModel pom, PomAllElementsPage.eElementsContext context,ObservableList<ElementInfo> CopiedUnienedList = null)
        {
            InitializeComponent();
            mPOM = pom;
            mContext = context;
            if (mContext == PomAllElementsPage.eElementsContext.Mapped)
            {
                mElements = mPOM.MappedUIElements;
            }
            else if (mContext == PomAllElementsPage.eElementsContext.Unmapped)
            {
                mElements = mPOM.UnMappedUIElements;
            }
            else if (mContext == PomAllElementsPage.eElementsContext.AllDeltaElements)
            {
                mElements = CopiedUnienedList;

                foreach (ElementInfo EI in mElements)
                {
                    EI.DeltaStatus = ElementInfo.eDeltaStatus.Unchanged;
                }
            }

            SetElementsGridView();
            SetLocatorsGridView();
            SetControlPropertiesGridView();
            xMainElementsGrid.DataSourceList = mElements;

            if (mElements.Count > 0)
            {
                xMainElementsGrid.Grid.SelectedItem = mElements[0];
            }
            else
            {
                DisableDetailsExpander();
            }

            
        }

        public void DoEndOfRelearnElementsSorting()
        {
            
            ObservableList<ElementInfo> CurrentElementsList = new ObservableList<ElementInfo>(mElements);
            List<ElementInfo> DeletedMappedElements = CurrentElementsList.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Deleted && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<ElementInfo> ModifiedMappedElements = CurrentElementsList.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Modified && x.ElementGroup.ToString() == ApplicationPOMModel.eElementGroup.Mapped.ToString()).ToList();
            List<ElementInfo> NewMappedElements = CurrentElementsList.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.New && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();

            List<ElementInfo> DeletedUnMappedElements = CurrentElementsList.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Deleted && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();
            List<ElementInfo> ModifiedUnMappedElements = CurrentElementsList.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Modified && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();
            List<ElementInfo> NewUnMappedElements = CurrentElementsList.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.New && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();

            List<ElementInfo> UnchangedMapped = CurrentElementsList.Where(x => (x.DeltaStatus == ElementInfo.eDeltaStatus.Unchanged )  && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<ElementInfo> UnchangedUnmapped = CurrentElementsList.Where(x => (x.DeltaStatus == ElementInfo.eDeltaStatus.Unchanged ) && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();

            List<List<ElementInfo>> ElementsLists = new List<List<ElementInfo>>() { DeletedMappedElements, ModifiedMappedElements, NewMappedElements, DeletedUnMappedElements, ModifiedUnMappedElements, NewUnMappedElements, UnchangedMapped, UnchangedUnmapped };
            mElements.Clear();


            foreach (List<ElementInfo> elementsList in ElementsLists)
            {
                foreach (ElementInfo EI in elementsList)
                {
                    mElements.Add(EI);
                }
            }

        }

        private void PasteElementEvent(PasteItemEventArgs EventArgs)
        {
            ((ElementInfo)EventArgs.RepositoryItemBaseObject).IsAutoLearned = false;
        }

        private void PasteLocatorEvent(PasteItemEventArgs EventArgs)
        {
            ((ElementLocator)EventArgs.RepositoryItemBaseObject).IsAutoLearned = false;
        }

        private void Properties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdatePropertiesHeader();
        }
        private void UpdatePropertiesHeader()
        {
            Dispatcher.Invoke(() =>
            {
                xPropertiesTextBlock.Text = string.Format("Properties ({0})", mSelectedElement.Properties.Count);
            });
        }

        private void Locators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateLocatorsHeader();
        }
        private void UpdateLocatorsHeader()
        {
            Dispatcher.Invoke(() =>
            {
                xLocatorsTextBlock.Text = string.Format("Locators ({0})", mSelectedElement.Locators.Count);
            });
        }

        private void AddElementsToMappedBtnClicked(object sender, RoutedEventArgs e)
        {
            List<ElementInfo> ItemsToAddList = xMainElementsGrid.Grid.SelectedItems.Cast<ElementInfo>().ToList();
            if (ItemsToAddList != null && ItemsToAddList.Count > 0)
            {
                //remove
                for (int indx = 0; indx < ItemsToAddList.Count; indx++)
                {
                    mPOM.UnMappedUIElements.Remove(ItemsToAddList[indx]);
                }
                //add
                foreach (ElementInfo EI in ItemsToAddList)
                {
                    mPOM.MappedUIElements.Add(EI);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void RemoveElementsToMappedBtnClicked(object sender, RoutedEventArgs e)
        {
            List<ElementInfo> ItemsToRemoveList = xMainElementsGrid.Grid.SelectedItems.Cast<ElementInfo>().ToList();
            if (ItemsToRemoveList != null && ItemsToRemoveList.Count > 0)
            {
                //remove
                for (int indx = 0; indx < ItemsToRemoveList.Count; indx++)
                {
                    mPOM.MappedUIElements.Remove(ItemsToRemoveList[indx]);
                }
                //add
                foreach (ElementInfo EI in ItemsToRemoveList)
                {
                    mPOM.UnMappedUIElements.Add(EI);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        internal void SetAgent(Agent agent)
        {
            mAgent = agent;
        }

        public enum eGridView
        {
            RegularView,
        }
        private void SetElementsGridView()
        {
            xMainElementsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            if (mContext == PomAllElementsPage.eElementsContext.AllDeltaElements)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.IsSelected), Header = "Update", WidthWeight = 50, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["FieldUpdate"] });
                ObservableList<object> elementGroupList = new ObservableList<object>();
                elementGroupList.Add(ApplicationPOMModel.eElementGroup.Mapped);
                elementGroupList.Add(ApplicationPOMModel.eElementGroup.Unmapped);

                List<GingerCore.General.ComboEnumItem> GroupTypeList = GingerCore.General.GetEnumValuesForCombo(typeof(ApplicationPOMModel.eElementGroup));

                view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementGroup), Header = "Elements Group", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(GroupTypeList, nameof(ElementInfo.ElementGroup), true,false,string.Empty,false, GroupComboboxSelectionChanged), WidthWeight = 200 });

            }

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), Header = "Name", WidthWeight = 200, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Description), WidthWeight = 250, AllowSorting = true });

            List<GingerCore.General.ComboEnumItem> ElementTypeList = GingerCore.General.GetEnumValuesForCombo(typeof(eElementType));
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnum), Header = "Type", WidthWeight = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = ElementTypeList });

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.IsAutoLearned), Header = "Auto Learned", WidthWeight = 250, MaxWidth = 100, AllowSorting = true, ReadOnly = true });


            if (mContext == PomAllElementsPage.eElementsContext.AllDeltaElements)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.DeltaStatusIcon), Header = "Comparison Status", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xDeltaStatusIconTemplate"] });
                List<GingerCore.General.ComboEnumItem> deltaExtraDetailsList = GingerCore.General.GetEnumValuesForCombo(typeof(ElementInfo.eDeltaExtraDetails));
                view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.DeltaExtraDetails), WidthWeight = 200, Header = "Comparison Details", StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = deltaExtraDetailsList });

                xMainElementsGrid.ShowCopy = Visibility.Collapsed;
                xMainElementsGrid.ShowPaste = Visibility.Collapsed;
                xMainElementsGrid.ShowAdd = Visibility.Collapsed;
                xMainElementsGrid.ShowDelete = Visibility.Collapsed;
                xMainElementsGrid.btnMarkAll.Visibility = Visibility.Visible;
                xMainElementsGrid.MarkUnMarkAllActive += MarkUnMarkAllActions;

                xMainElementsGrid.AddComboBoxToolbarTool("Filter By Group:",typeof(ApplicationPOMModel.eElementGroup), GroupFilter_SelectionChanged,"All");
                xMainElementsGrid.AddComboBoxToolbarTool("Filter By Comperison Status:", typeof(ElementInfo.eDeltaStatus), ComperisonStatusFilter_SelectionChanged, "All");
            }

            view.GridColsView.Add(new GridColView() { Field = "", Header = "Highlight", WidthWeight = 80, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xHighlightButtonTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.StatusIcon), Header = "Identification Status", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });


            GridViewDef mRegularView = new GridViewDef(eGridView.RegularView.ToString());
            mRegularView.GridColsView = new ObservableList<GridColView>();
            mRegularView.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.StatusIcon), Visible = false });

            xMainElementsGrid.AddCustomView(mRegularView);
            xMainElementsGrid.SetAllColumnsDefaultView(view);
            xMainElementsGrid.InitViewItems();
            if (mContext == PomAllElementsPage.eElementsContext.AllDeltaElements)
            {
                xMainElementsGrid.ChangeGridView(GridViewDef.DefaultViewName);
            }
            else
            {
                xMainElementsGrid.ChangeGridView(eGridView.RegularView.ToString());
            }


          
            if (mContext == PomAllElementsPage.eElementsContext.Mapped)
            {
                xMainElementsGrid.AddToolbarTool(eImageType.MapSigns, "Remove elements from mapped list", new RoutedEventHandler(RemoveElementsToMappedBtnClicked));
                xMainElementsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddMappedElementRow));
                xMainElementsGrid.ShowDelete = Visibility.Collapsed;
            }
            else if (mContext == PomAllElementsPage.eElementsContext.Unmapped)
            {
                xMainElementsGrid.AddToolbarTool(eImageType.MapSigns, "Add elements to mapped list", new RoutedEventHandler(AddElementsToMappedBtnClicked));
                xMainElementsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddUnMappedElementRow));
                xMainElementsGrid.SetbtnDeleteHandler(DeleteUnMappedElementRow);
            }

            xMainElementsGrid.grdMain.PreparingCellForEdit += MainElementsGrid_PreparingCellForEdit;
            xMainElementsGrid.PasteItemEvent += PasteElementEvent;


            xMainElementsGrid.SelectedItemChanged += XMainElementsGrid_SelectedItemChanged;
            xMainElementsGrid.Grid.SelectionChanged += Grid_SelectionChanged;
        }

        private void GroupComboboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ElementInfo EI = (ElementInfo)xMainElementsGrid.CurrentItem;
            if (EI != null)
            {
                EI.DeltaStatus = ElementInfo.eDeltaStatus.Modified;
                EI.IsSelected = true;
            }
        }

        private void ComperisonStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mCurrentDeltaStatusFilter = (ComboEnumItem)e.AddedItems[0];
            FilterListByFiltersCreteria();
        }

        private void GroupFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mCurrentGroupByFilter = (ComboEnumItem)e.AddedItems[0];
            FilterListByFiltersCreteria();
        }

        private void FilterListByFiltersCreteria()
        {
            if ((mCurrentGroupByFilter == null || mCurrentGroupByFilter.text == "All") && (mCurrentDeltaStatusFilter == null || mCurrentDeltaStatusFilter.text == "All"))
            {
                xMainElementsGrid.DataSourceList = mElements;
            }
            else if (mCurrentGroupByFilter == null || mCurrentGroupByFilter.text == "All")
            {
                xMainElementsGrid.DataSourceList = GingerCore.General.ConvertListToObservableList(mElements.Where(x => x.DeltaStatus == (ElementInfo.eDeltaStatus)mCurrentDeltaStatusFilter.Value).ToList());
            }
            else if (mCurrentDeltaStatusFilter == null || mCurrentDeltaStatusFilter.text == "All")
            {
                xMainElementsGrid.DataSourceList = GingerCore.General.ConvertListToObservableList(mElements.Where(x => x.ElementGroup.ToString() == mCurrentGroupByFilter.Value.ToString()).ToList());
            }
            else
            {
                xMainElementsGrid.DataSourceList = GingerCore.General.ConvertListToObservableList(mElements.Where(x => x.ElementGroup.ToString() == mCurrentGroupByFilter.Value.ToString() && x.DeltaStatus == (ElementInfo.eDeltaStatus)mCurrentDeltaStatusFilter.Value).ToList());
            }
        }

        private void MarkUnMarkAllActions(bool Status)
        {
            if (xMainElementsGrid.DataSourceList.Count <= 0)
            {
                return;
            }

            if (xMainElementsGrid.DataSourceList.Count > 0)
            {
                ObservableList<ElementInfo> lstMarkUnMarkAPI = (ObservableList<ElementInfo>)xMainElementsGrid.DataSourceList;
                foreach (ElementInfo EI in lstMarkUnMarkAPI)
                {
                    if (EI.DeltaStatus != ElementInfo.eDeltaStatus.Unchanged)
                    {
                        EI.IsSelected = Status;
                    }
                }
                xMainElementsGrid.DataSourceList = lstMarkUnMarkAPI;
            }
        }

  

        private void XMainElementsGrid_SelectedItemChanged(object selectedItem)
        {
            HandelElementSelectionChange();
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandelElementSelectionChange();
        }

        bool disabeledElementMsgShown;
        private void MainElementsGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header == "Name" || e.Column.Header == nameof(ElementInfo.Description))
            {
                return;
            }

            ElementInfo ei = (ElementInfo)xMainElementsGrid.CurrentItem;
            if (ei.IsAutoLearned)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "You can not edit this field of an Element which was auto learned, please duplicate it and create customized Element.");
                e.EditingElement.IsEnabled = false;
            }
        }

        private void DeleteUnMappedElementRow(object sender, RoutedEventArgs e)
        {
            bool msgShowen = false;
            List<ElementInfo> elementsToDelete = xMainElementsGrid.Grid.SelectedItems.Cast<ElementInfo>().ToList();
            foreach (ElementInfo element in elementsToDelete)
            {
                if (element.IsAutoLearned)
                {
                    if (!msgShowen)
                    {
                        Reporter.ToUser(eUserMsgKey.POMCannotDeleteAutoLearnedElement);
                        msgShowen = true;
                    }
                }
                else
                {
                    mPOM.UnMappedUIElements.Remove(element);
                }
            }
        }

        private void AddMappedElementRow(object sender, RoutedEventArgs e)
        {
            xMainElementsGrid.Grid.CommitEdit();

            ElementInfo EI = new ElementInfo();
            mPOM.MappedUIElements.Add(EI);
            
            xMainElementsGrid.Grid.SelectedItem = EI;
            xMainElementsGrid.ScrollToViewCurrentItem();
        }

        private void AddUnMappedElementRow(object sender, RoutedEventArgs e)
        {
            xMainElementsGrid.Grid.CommitEdit();

            ElementInfo EI = new ElementInfo();
            mPOM.UnMappedUIElements.Add(EI);

            xMainElementsGrid.Grid.SelectedItem = EI;
            xMainElementsGrid.ScrollToViewCurrentItem();
        }

        private void SetLocatorsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 50, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            List<GingerCore.General.ComboEnumItem> locateByList = GingerCore.General.GetEnumValuesForCombo(typeof(eLocateBy));


            GingerCore.General.ComboEnumItem comboItem = locateByList.Where(x => ((eLocateBy)x.Value) == eLocateBy.POMElement).FirstOrDefault();
            if (comboItem != null)
                locateByList.Remove(comboItem);

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 200 });

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 50, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 100,  ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = "Test", WidthWeight = 50, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestElementButtonTemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.StatusIcon), Header = "Identification Status", WidthWeight = 100, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });



            if (mContext == PomAllElementsPage.eElementsContext.AllDeltaElements)
            {
                defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.DeltaStatusIcon), Header = "Comparison Status", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xDeltaStatusIconTemplate"] });
                defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.DeltaExtraDetails), Header = "Comparison Details", WidthWeight = 250, AllowSorting = true, ReadOnly = true });

                xLocatorsGrid.ShowCopy = Visibility.Collapsed;
                xLocatorsGrid.ShowPaste = Visibility.Collapsed;
                xLocatorsGrid.ShowAdd = Visibility.Collapsed;
                xLocatorsGrid.ShowDelete = Visibility.Collapsed;
                xLocatorsGrid.ShowUpDown = Visibility.Collapsed;
            }

            xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xLocatorsGrid.InitViewItems();

            xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            xLocatorsGrid.AddToolbarTool(eImageType.Play, "Test All Elements Locators", new RoutedEventHandler(TestAllElementsLocators));
            xLocatorsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddLocatorButtonClicked));
            xLocatorsGrid.SetbtnDeleteHandler(new RoutedEventHandler(DeleteLocatorClicked));

            xLocatorsGrid.grdMain.PreparingCellForEdit += LocatorsGrid_PreparingCellForEdit;
            xLocatorsGrid.PasteItemEvent += PasteLocatorEvent;
        }

        private void DeleteLocatorClicked(object sender, RoutedEventArgs e)
        {
            bool msgShowen = false;
            List<ElementLocator> locatorsToDelete = xLocatorsGrid.Grid.SelectedItems.Cast<ElementLocator>().ToList();
            foreach (ElementLocator locator in locatorsToDelete)
            {
                if (locator.IsAutoLearned)
                {
                    if (!msgShowen)
                    {
                        Reporter.ToUser(eUserMsgKey.POMCannotDeleteAutoLearnedElement);
                        msgShowen = true;
                    }
                }
                else
                {
                    mSelectedElement.Locators.Remove(locator);
                }
            }
        }

        private void AddLocatorButtonClicked(object sender, RoutedEventArgs e)
        {
            xLocatorsGrid.Grid.CommitEdit();

            ElementLocator locator = new ElementLocator() { Active = true };
            mSelectedElement.Locators.Add(locator);

            xLocatorsGrid.Grid.SelectedItem = locator;
            xLocatorsGrid.ScrollToViewCurrentItem();
        }

        bool disabeledLocatorsMsgShown;
        private void LocatorsGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header.ToString() != nameof(ElementLocator.Active) && mSelectedLocator.IsAutoLearned)
            {
                if (!disabeledLocatorsMsgShown)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "You can not edit Locator which was auto learned, please duplicate it and create customized Locator.");
                    disabeledLocatorsMsgShown = true;
                }
                e.EditingElement.IsEnabled = false;
            }
        }

        private void SetControlPropertiesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Name), WidthWeight = 150, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Value), WidthWeight = 250, ReadOnly = true });


            if (mContext == PomAllElementsPage.eElementsContext.AllDeltaElements)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(POMElementProperty.DeltaStatusIcon), Header = "Comparison Status", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xDeltaStatusIconTemplate"] });
                view.GridColsView.Add(new GridColView() { Field = nameof(POMElementProperty.DeltaExtraDetails), Header = "Comparison Details", WidthWeight = 250, AllowSorting = true, ReadOnly = true });
            }

            xPropertiesGrid.SetAllColumnsDefaultView(view);
            xPropertiesGrid.InitViewItems();
            xPropertiesGrid.SetTitleLightStyle = true;
            xPropertiesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddPropertyHandler));
        }

        private void AddPropertyHandler(object sender, RoutedEventArgs e)
        {
            mSelectedElement.Properties.Add(new ControlProperty());
        }

        private void HandelElementSelectionChange()
        {                      
            if (mSelectedElement != null)
            {
                xDetailsExpander.IsEnabled = true;
                if (IsFirstSelection)
                {
                    xDetailsExpander.IsExpanded = true;
                    IsFirstSelection = false;
                }

                if (mSelectedElement.ElementName != null)
                {
                    string title;
                    if (mSelectedElement.ElementName.Length > 100)
                    {
                        title = string.Format("'{0}...' Details", mSelectedElement.ElementName.Substring(0, 25));
                    }
                    else
                    {
                        title = string.Format("'{0}' Details", mSelectedElement.ElementName);
                    }
                    xDetailsExpanderLabel.Content = title;
                }
                else
                {
                    xDetailsExpanderLabel.Content = "Element Details";
                }

                mSelectedElement.Locators.CollectionChanged -= Locators_CollectionChanged;
                mSelectedElement.Locators.CollectionChanged += Locators_CollectionChanged;
                xLocatorsGrid.DataSourceList = mSelectedElement.Locators;
                UpdateLocatorsHeader();

                mSelectedElement.Properties.CollectionChanged -= Properties_CollectionChanged;
                mSelectedElement.Properties.CollectionChanged += Properties_CollectionChanged;
                xPropertiesGrid.DataSourceList = mSelectedElement.Properties;
                UpdatePropertiesHeader();

            }
            else
            {
                DisableDetailsExpander();
            }
        }

        private void DisableDetailsExpander()
        {
            xDetailsExpanderLabel.Content = "Element Details";
            xDetailsExpander.IsEnabled = false;
            xDetailsExpander.IsExpanded = false;
        }

        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedElement != null)
            {
                mWinExplorer.HighLightElement(mSelectedElement, true);               
            }
        }

        private void DetailsGrid_Expanded(object sender, RoutedEventArgs e)
        {
            Row2.Height = new GridLength(30, GridUnitType.Star);
        }

        private void DetailsGrid_Collapsed(object sender, RoutedEventArgs e)
        {
            Row2.Height = new GridLength(35);
        }

        private void TestElementButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            ElementInfo CurrentEI = (ElementInfo)MainElementsGrid.CurrentItem;

            if (mSelectedLocator != null)
            {
                mWinExplorer.TestElementLocators(new ElementInfo() { Path = CurrentEI.Path, Locators = new ObservableList<ElementLocator>() { mSelectedLocator } });
            }
        }

        private void TestAllElementsLocators(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedElement != null)
            {
                mWinExplorer.TestElementLocators(mSelectedElement);
            }
        }

        private bool ValidateDriverAvalability()
        {
            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKey.POMAgentIsNotRunning);
                return false;
            }

            if (IsDriverBusy())
            {
                Reporter.ToUser(eUserMsgKey.POMDriverIsBusy);
                return false;
            }

            return true;
        }

        private bool IsDriverBusy()
        {
            if (mAgent != null && mAgent.Driver.IsDriverBusy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void FinishEditInGrids()
        {
            xMainElementsGrid.Grid.CommitEdit();
            xLocatorsGrid.Grid.CommitEdit();
        }


        private void xElementDetailsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set the selected tab text style
            try
            {
                if (xElementDetailsTabs.SelectedItem != null)
                {
                    foreach (TabItem tab in xElementDetailsTabs.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (xElementDetailsTabs.SelectedItem == tab)
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$Color_DarkBlue");

                                ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in POM Edit Page tabs style", ex);
            }
        }
    }
}
