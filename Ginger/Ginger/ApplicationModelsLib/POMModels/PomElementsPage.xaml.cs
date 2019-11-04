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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.GeneralLib;
using GingerWPF.ApplicationModelsLib.APIModelWizard;
using GingerWPF.UserControlsLib.UCTreeView;
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
    public enum eElementsContext
    {
        Mapped,
        Unmapped,
    }

    public partial class PomElementsPage : Page
    {
        public eElementsContext mContext;
        ApplicationPOMModel mPOM;
        ObservableList<ElementInfo> mElements;
        const string parentFramePropertyName = "Parent IFrame";

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

        public PomElementsPage(ApplicationPOMModel pom, eElementsContext context)
        {
            InitializeComponent();
            mPOM = pom;
            mContext = context;
            if (mContext == eElementsContext.Mapped)
            {
                mElements = mPOM.MappedUIElements;
            }
            else if (mContext == eElementsContext.Unmapped)
            {
                mElements = mPOM.UnMappedUIElements;
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

        private void PasteElementEvent(PasteItemEventArgs EventArgs)
        {
            ElementInfo copiedElement = (ElementInfo)EventArgs.Item;
            copiedElement.IsAutoLearned = false;
            foreach (ElementLocator locator in copiedElement.Locators)
            {
                locator.IsAutoLearned = false;
            }
        }

        private void PasteLocatorEvent(PasteItemEventArgs EventArgs)
        {
            ElementLocator copiedLocator = (ElementLocator)EventArgs.Item;
            copiedLocator.IsAutoLearned = false;
        }

        private void Properties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (mSelectedElement != null)
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
            if(mSelectedElement != null)
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
            foreach (ElementInfo elemInfo in mElements)
            {
                elemInfo.ElementObject = null;
            }
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

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), Header = "Name", WidthWeight = 25, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Description), WidthWeight = 35, AllowSorting = true });

            List<ComboEnumItem> ElementTypeList = GetEnumValuesForCombo(typeof(eElementType));
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnum), Header = "Type", WidthWeight = 15, AllowSorting = true, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = ElementTypeList });

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.OptionalValuesObjectsListAsString), Header = "Possible Values", WidthWeight = 40, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 8, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["OpenEditOptionalValuesPage"] });

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = "", Header = "Highlight", WidthWeight = 10, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xHighlightButtonTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.StatusIcon), Header = "Status", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });

            GridViewDef mRegularView = new GridViewDef(eGridView.RegularView.ToString());
            mRegularView.GridColsView = new ObservableList<GridColView>();
            mRegularView.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.StatusIcon), Visible = false });

            xMainElementsGrid.AddCustomView(mRegularView);
            xMainElementsGrid.SetAllColumnsDefaultView(view);
            xMainElementsGrid.InitViewItems();
            xMainElementsGrid.ChangeGridView(eGridView.RegularView.ToString());

            if (mContext == eElementsContext.Mapped)
            {
                xMainElementsGrid.AddToolbarTool(eImageType.MapSigns, "Remove elements from mapped list", new RoutedEventHandler(RemoveElementsToMappedBtnClicked));
                xMainElementsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddMappedElementRow));
                xMainElementsGrid.ShowDelete = Visibility.Collapsed;

                xMainElementsGrid.AddToolbarTool(eImageType.DataSource, "Export Possible Values to DataSource", new RoutedEventHandler(ExportPossibleValuesToDataSource));
            }
            else
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

        /// <summary>
        /// This method is used to Export the Possible Values To DataSource
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportPossibleValuesToDataSource(object sender, RoutedEventArgs e)
        {
            try
            {
                Ginger.SolutionWindows.TreeViewItems.DataSourceFolderTreeItem dataSourcesRoot = new Ginger.SolutionWindows.TreeViewItems.DataSourceFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<DataSourceBase>(), DataSourceFolderTreeItem.eDataTableView.Customized);
                SingleItemTreeViewSelectionPage mDataSourceSelectionPage = new SingleItemTreeViewSelectionPage("DataSource - Customized Table", eImageType.DataSource, dataSourcesRoot, SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true);
                List<object> selectedRunSet = mDataSourceSelectionPage.ShowAsWindow();
                if (selectedRunSet != null && selectedRunSet.Count > 0)
                {
                    ImportOptionalValuesForParameters im = new ImportOptionalValuesForParameters();
                    DataSourceBase mDSDetails = (((DataSourceTable)selectedRunSet[0]).DSC);

                    string tableName = ((DataSourceTable)selectedRunSet[0]).FileName;
                    List<AppParameters> parameters = GetParameterList();
                    im.ExportSelectedParametersToDataSouce(parameters, mDSDetails, tableName);
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while exporting POM optional Values to Data Source", ex);
            }
        }

        /// <summary>
        /// This method is used to Get Parameter List
        /// </summary>
        /// <param name="im"></param>
        /// <returns></returns>
        private List<AppParameters> GetParameterList()
        {
            ImportOptionalValuesForParameters im = new ImportOptionalValuesForParameters();
            List<AppParameters> parameters = new List<AppParameters>();
            try
            {
                List<string> lstParName = new List<string>();
                foreach (var prms in mElements)
                {
                    if (ElementInfo.IsElementTypeSupportingOptionalValues(prms.ElementTypeEnum))
                    {
                        string parName = prms.ItemName.Replace("\r", "").Split('\n')[0];
                        int count = lstParName.Where(p => p == parName).Count();
                        lstParName.Add(parName);
                        if (count > 0)
                        {
                            parName = string.Format("{0}_{1}", parName, count);
                        }

                        AppParameters par = new AppParameters();
                        par.ItemName = parName;
                        par.OptionalValuesList = prms.OptionalValuesObjectsList;
                        par.OptionalValuesString = prms.OptionalValuesObjectsListAsString;
                        par.Description = prms.Description;
                        parameters.Add(par); 
                    }
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
            }
            return parameters;
        }

        private void OpenEditOptionalValuesPageButton_Click(object sender, RoutedEventArgs e)
        {
            IParentOptionalValuesObject parObj = (IParentOptionalValuesObject)(xMainElementsGrid.CurrentItem);
            ModelOptionalValuesPage MDPVP = new ModelOptionalValuesPage(parObj);
            MDPVP.ShowAsWindow();
        }

        private void XMainElementsGrid_SelectedItemChanged(object selectedItem)
        {
            HandelElementSelectionChange();
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandelElementSelectionChange();
        }

        // bool disabeledElementMsgShown;
        private void MainElementsGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if ((string)e.Column.Header == "Name" || (string)e.Column.Header == nameof(ElementInfo.Description))
            {
                return;
            }

            if (Convert.ToString(e.Column.Header) != "...")
            {
                ElementInfo ei = (ElementInfo)xMainElementsGrid.CurrentItem;
                if (ei.IsAutoLearned)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "You can not edit this field of an Element which was auto learned, please duplicate it and create customized Element.");
                    e.EditingElement.IsEnabled = false;
                } 
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Selected Element type do not support optional values.");
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
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            List<ComboEnumItem> locateByList = GingerCore.General.GetEnumValuesForCombo(typeof(eLocateBy));

            ComboEnumItem comboItem = locateByList.Where(x => ((eLocateBy)x.Value) == eLocateBy.POMElement).FirstOrDefault();
            if (comboItem != null)
                locateByList.Remove(comboItem);

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 65 });
            defView.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xLocateValueVETemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 25 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = "Test", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestElementButtonTemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.StatusIcon), Header = "Status", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });
            xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xLocatorsGrid.InitViewItems();

            xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            xLocatorsGrid.AddToolbarTool(eImageType.Run, "Test All Elements Locators", new RoutedEventHandler(TestAllElementsLocators));
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
            if (e.Column.Header.ToString() != nameof(ElementLocator.Active) && e.Column.Header.ToString() != nameof(ElementLocator.Help) && mSelectedLocator.IsAutoLearned)
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

            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Name), WidthWeight = 25 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Value), WidthWeight = 75 });

            xPropertiesGrid.SetAllColumnsDefaultView(view);
            xPropertiesGrid.InitViewItems();
            xPropertiesGrid.SetTitleLightStyle = true;
            xPropertiesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddPropertyHandler));
            xPropertiesGrid.grdMain.PreparingCellForEdit += PropertiesGrid_PreparingCellForEdit;
            xPropertiesGrid.grdMain.CellEditEnding += PropertiesGrid_CellEditEnding;
        }

        private void PropertiesGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if(mSelectedElement.IsAutoLearned == true || e.Column.Header.ToString() == nameof(ControlProperty.Name))
            {
                e.EditingElement.IsEnabled = false;
            }
        }

        private void PropertiesGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == nameof(ControlProperty.Value))
            {
                ControlProperty ctrlProp = e.EditingElement.DataContext as ControlProperty;
                mSelectedElement.Path = ctrlProp != null ? ctrlProp.Value : "";
            }
        }

        private void AddPropertyHandler(object sender, RoutedEventArgs e)
        {
            xPropertiesGrid.Grid.CommitEdit();

            ControlProperty elemProp = new ControlProperty() { Name = parentFramePropertyName };
            mSelectedElement.Properties.Add(elemProp);
            xPropertiesGrid.Grid.SelectedItem = elemProp;
            xPropertiesGrid.ScrollToViewCurrentItem();

            xPropertiesGrid.ShowAdd = Visibility.Collapsed;
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
                if(!mSelectedElement.IsAutoLearned && mSelectedElement.Properties.Where(c => c.Name == "Parent IFrame").FirstOrDefault() == null)
                {
                    xPropertiesGrid.ShowAdd = Visibility.Visible;
                }
                else
                {
                    xPropertiesGrid.ShowAdd = Visibility.Collapsed;
                }
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
            xMainElementsGrid.Grid.CancelEdit();
            xLocatorsGrid.Grid.CommitEdit();
            xLocatorsGrid.Grid.CancelEdit();
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

        private void XLocateValueVEButton_Click(object sender, RoutedEventArgs e)
        {
            ElementLocator selectedVarb = (ElementLocator)xLocatorsGrid.CurrentItem;
            if (selectedVarb.IsAutoLearned)
            {
                if (!disabeledLocatorsMsgShown)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "You can not edit Locator which was auto learned, please duplicate it and create customized Locator.");
                    disabeledLocatorsMsgShown = true;
                }
            }
            else
            {
                ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedVarb, nameof(ElementLocator.LocateValue), null);
                VEEW.ShowAsWindow();
            }
        }
    }
}
