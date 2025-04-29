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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.TextEditor;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Drivers.Common;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.Application_Models;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.ApplicationModelsLib.APIModelWizard;
using GingerWPF.UserControlsLib.UCTreeView;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        bool isEnableFriendlyLocator = false;

        bool mAddSelfHealingColumn = true;

        private Agent mAgent;
        IWindowExplorer mWinExplorer
        {
            get
            {
                if (mAgent != null && ((AgentOperations)mAgent.AgentOperations).Status == Agent.eStatus.Running)
                {
                    return ((AgentOperations)mAgent.AgentOperations).Driver as IWindowExplorer;
                }
                else
                {
                    if (mAgent != null)
                    {
                        mAgent.AgentOperations.Close();
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
                if (xElementDetails.xLocatorsGrid.Grid.SelectedItem != null)
                {
                    return (ElementLocator)xElementDetails.xLocatorsGrid.Grid.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        private readonly General.eRIPageViewMode _editMode;

        public PomElementsPage(ApplicationPOMModel pom, eElementsContext context, bool AddSelfHealingColumn, General.eRIPageViewMode editMode)
        {
            InitializeComponent();
            mPOM = pom;
            mContext = context;
            mAddSelfHealingColumn = AddSelfHealingColumn;
            _editMode = editMode;

            if (mContext == eElementsContext.Mapped)
            {
                mElements = mPOM.MappedUIElements;
            }
            else if (mContext == eElementsContext.Unmapped)
            {
                mElements = mPOM.UnMappedUIElements;
            }

            if (WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey) == ePlatformType.Web)
            {
                isEnableFriendlyLocator = true;
                xElementDetails.xFriendlyLocatorTab.Visibility = Visibility.Visible;
            }
            else
            {
                xElementDetails.xFriendlyLocatorTab.Visibility = Visibility.Collapsed;
            }

            SetElementsGridView();

            SetLocatorsGridView();
            if (isEnableFriendlyLocator)
            {
                SetFriendlyLocatorsGridView();
            }

            SetControlPropertiesGridView();

            //xElementDetails.InitGridView(this)

            xMainElementsGrid.DataSourceList = mElements;

            if (mElements.Count > 0)
            {
                xMainElementsGrid.Grid.SelectedItem = mElements[0];
            }
            else
            {
                DisableDetailsExpander();
            }
            SetEditMode();
        }

        private void SetEditMode()
        {
            if (_editMode is General.eRIPageViewMode.View or General.eRIPageViewMode.ViewAndExecute)
            {
                xMainElementsGrid.IsEnabled = false;
                xElementDetails.IsEnabled = false;
            }
            else
            {

                xMainElementsGrid.IsEnabled = true;
                xElementDetails.IsEnabled = true;
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
            {
                UpdatePropertiesHeader();
            }
        }
        private void UpdatePropertiesHeader()
        {
            Dispatcher.Invoke(() =>
            {
                xElementDetails.xPropertiesTextBlock.Text = string.Format("Properties ({0})", mSelectedElement.Properties.Count);
            });
        }

        private void Locators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (mSelectedElement != null)
            {
                UpdateLocatorsHeader();
            }
        }

        private void FriendlyLocators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (mSelectedElement != null)
            {
                UpdateFriendlyLocatorsHeader();
            }
        }
        private void UpdateLocatorsHeader()
        {
            Dispatcher.Invoke(() =>
            {
                xElementDetails.xLocatorsTextBlock.Text = string.Format("Locators ({0})", mSelectedElement.Locators.Count);
            });
        }

        private void UpdateFriendlyLocatorsHeader()
        {
            Dispatcher.Invoke(() =>
            {
                xElementDetails.xFriendlyLocatorsTextBlock.Text = string.Format("Friendly Locators ({0})", mSelectedElement.FriendlyLocators.Count);
            });
        }

        private void AddElementsToMappedBtnClicked(object sender, RoutedEventArgs e)
        {
            List<ElementInfo> ItemsToAddList = xMainElementsGrid.Grid.SelectedItems.Cast<ElementInfo>().ToList();
            if (ItemsToAddList != null && ItemsToAddList.Count > 0)
            {
                //remove
                Dispatcher.InvokeAsync(() =>
                {
                    for (int indx = 0; indx < ItemsToAddList.Count; indx++)
                    {
                        mPOM.UnMappedUIElements.Remove(ItemsToAddList[indx]);
                    }
                });
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
                Dispatcher.InvokeAsync(() =>
                {
                    for (int indx = 0; indx < ItemsToRemoveList.Count; indx++)
                    {
                        mPOM.MappedUIElements.Remove(ItemsToRemoveList[indx]);
                    }
                });
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
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ElementInfo.ElementTypeImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 },
                new GridColView() { Field = nameof(ElementInfo.ElementName), Header = "Name", WidthWeight = 25, AllowSorting = true },
            ]
            };

            List<ComboEnumItem> ElementTypeList = GetEnumValuesForCombo(typeof(eElementType));
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnum), Header = "Type", WidthWeight = 10, AllowSorting = true, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = ElementTypeList, HorizontalAlignment = System.Windows.HorizontalAlignment.Center });

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.OptionalValuesObjectsListAsString), Header = "Possible Values", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["OpenEditOptionalValuesPage"] });

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, ReadOnly = true, HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            view.GridColsView.Add(new GridColView() { Field = "", Header = "Highlight", WidthWeight = 8, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xHighlightButtonTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.StatusIcon), Header = "Status", WidthWeight = 8, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });

            if (mAddSelfHealingColumn)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.GetSelfHealingInfo), StyleType = GridColView.eGridColStyleType.Text, Header = "Self Healing Info.", WidthWeight = 22, ReadOnly = true, PropertyConverter = (new ColumnPropertyConverter(new SelfHealingInfoStatusConverter(), TextBlock.ForegroundProperty)) });
                view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.LastUpdatedTime), Header = "Last Updated", WidthWeight = 10 });
            }

            GridViewDef mRegularView = new GridViewDef(eGridView.RegularView.ToString())
            {
                GridColsView = [new GridColView() { Field = nameof(ElementInfo.StatusIcon), Visible = false }]
            };

            xMainElementsGrid.AddCustomView(mRegularView);
            xMainElementsGrid.SetAllColumnsDefaultView(view);
            xMainElementsGrid.InitViewItems();
            xMainElementsGrid.ChangeGridView(eGridView.RegularView.ToString());

            if (mContext == eElementsContext.Mapped)
            {
                xMainElementsGrid.AddToolbarTool(eImageType.MapSigns, "Remove elements from mapped list", new RoutedEventHandler(RemoveElementsToMappedBtnClicked));
                xMainElementsGrid.btnAdd.AddHandler(System.Windows.Controls.Button.ClickEvent, new RoutedEventHandler(AddMappedElementRow));
                xMainElementsGrid.ShowDelete = Visibility.Collapsed;

                xMainElementsGrid.AddToolbarTool(eImageType.DataSource, "Export Possible Values to DataSource", new RoutedEventHandler(ExportPossibleValuesToDataSource));
                xMainElementsGrid.AddToolbarTool(eImageType.Merge, "Merge Selected Elements", new RoutedEventHandler(MergeSelectedElements));
            }
            else
            {
                xMainElementsGrid.AddToolbarTool(eImageType.MapSigns, "Add elements to mapped list", new RoutedEventHandler(AddElementsToMappedBtnClicked));
                xMainElementsGrid.btnAdd.AddHandler(System.Windows.Controls.Button.ClickEvent, new RoutedEventHandler(AddUnMappedElementRow));
                xMainElementsGrid.SetbtnDeleteHandler(DeleteUnMappedElementRow);
            }


            WeakEventManager<DataGrid, DataGridPreparingCellForEditEventArgs>.AddHandler(source: xMainElementsGrid.grdMain, eventName: nameof(DataGrid.PreparingCellForEdit), handler: MainElementsGrid_PreparingCellForEdit);
            xMainElementsGrid.PasteItemEvent += PasteElementEvent;
            xMainElementsGrid.SelectedItemChanged += XMainElementsGrid_SelectedItemChanged;

            WeakEventManager<DataGrid, SelectionChangedEventArgs>.AddHandler(source: xMainElementsGrid.grdMain, eventName: nameof(DataGrid.SelectionChanged), handler: Grid_SelectionChanged);

            xMainElementsGrid.AddToolbarTool(eImageType.Category, toolTip: "Set missing Categories for selected Elements", new RoutedEventHandler(SetMissingCategoriesForSelectedElements));
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
        /// Merge 2 selected elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MergeSelectedElements(object sender, RoutedEventArgs e)
        {
            try
            {
                if (xMainElementsGrid.Grid.SelectedItems.Count == 2)
                {
                    if (Reporter.ToUser(eUserMsgKey.StaticQuestionsMessage, string.Format("Are you sure you want to perform merge of selected elements?" + Environment.NewLine + Environment.NewLine + "Note: '{0}' element will be deleted post merge.", ((ElementInfo)xMainElementsGrid.Grid.SelectedItems[1]).ItemName)) == eUserMsgSelection.Yes)
                    {
                        xMainElementsGrid.Grid.CommitEdit();

                        ElementInfo firstElement = (ElementInfo)xMainElementsGrid.Grid.SelectedItems[0];
                        ElementInfo secondElement = (ElementInfo)xMainElementsGrid.Grid.SelectedItems[1];

                        PomDeltaUtils.MergeElements(firstElement, secondElement);

                        mPOM.MappedUIElements.Remove(secondElement);

                        xMainElementsGrid.Grid.SelectedItem = firstElement;
                        xMainElementsGrid.ScrollToViewCurrentItem();
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please select 2 elements to merge");
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while merging POM elements", ex);
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
            List<AppParameters> parameters = [];
            try
            {
                List<string> lstParName = [];
                foreach (var prms in mElements)
                {
                    if (ElementInfo.IsElementTypeSupportingOptionalValues(prms.ElementTypeEnum))
                    {
                        string parName = prms.ItemName.Replace("\r", "").Split('\n')[0];
                        int count = lstParName.Count(p => p == parName);
                        lstParName.Add(parName);
                        if (count > 0)
                        {
                            parName = string.Format("{0}_{1}", parName, count);
                        }

                        AppParameters par = new AppParameters
                        {
                            ItemName = parName,
                            OptionalValuesList = prms.OptionalValuesObjectsList,
                            OptionalValuesString = prms.OptionalValuesObjectsListAsString,
                            Description = prms.Description
                        };
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
            if ((string)e.Column.Header is "Name" or (nameof(ElementInfo.Description)))
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
            var elementsToDelete = xMainElementsGrid.Grid.SelectedItems.Cast<ElementInfo>();
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

            var EI = IntializeElement();

            mPOM.MappedUIElements.Add(EI);

            xMainElementsGrid.Grid.SelectedItem = EI;
            xMainElementsGrid.ScrollToViewCurrentItem();
        }

        private ElementInfo IntializeElement()
        {
            if (WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey) == ePlatformType.Java)
            {
                if (Reporter.ToUser(eUserMsgKey.WarnAddSwingOrWidgetElement, eUserMsgOption.YesNo, eUserMsgSelection.No) == eUserMsgSelection.Yes)
                {
                    var htmlElementInfo = new HTMLElementInfo();
                    htmlElementInfo.Properties.Add(new ControlProperty() { Name = ElementProperty.ParentBrowserPath });
                    return htmlElementInfo;
                }
            }

            return new ElementInfo();
        }

        private void AddUnMappedElementRow(object sender, RoutedEventArgs e)
        {
            xMainElementsGrid.Grid.CommitEdit();

            var EI = IntializeElement();
            mPOM.UnMappedUIElements.Add(EI);

            xMainElementsGrid.Grid.SelectedItem = EI;
            xMainElementsGrid.ScrollToViewCurrentItem();
        }

        private void SetLocatorsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox },
            ]
            };

            List<ComboEnumItem> locateByList = GetPlatformLocatByList();

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 65 });
            defView.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xLocateValueVETemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = "", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xCopyLocatorButtonTemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.EnableFriendlyLocator), Visible = isEnableFriendlyLocator, Header = "Friendly Locator", WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Category), Header = nameof(ElementLocator.Category), WidthWeight = 10, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = GetPossibleCategories(), BindingMode = BindingMode.TwoWay });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = "Test", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestElementButtonTemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.StatusIcon), Header = "Status", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });
            xElementDetails.xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xElementDetails.xLocatorsGrid.InitViewItems();

            xElementDetails.xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            xElementDetails.xLocatorsGrid.AddToolbarTool(eImageType.Run, "Test All Elements Locators", new RoutedEventHandler(TestAllElementsLocators));
            xElementDetails.xLocatorsGrid.btnAdd.AddHandler(System.Windows.Controls.Button.ClickEvent, new RoutedEventHandler(AddLocatorButtonClicked));
            xElementDetails.xLocatorsGrid.SetbtnDeleteHandler(new RoutedEventHandler(DeleteLocatorClicked));
            WeakEventManager<DataGrid, DataGridPreparingCellForEditEventArgs>.AddHandler(source: xElementDetails.xLocatorsGrid.grdMain, eventName: nameof(DataGrid.PreparingCellForEdit), handler: LocatorsGrid_PreparingCellForEdit);

            xElementDetails.xLocatorsGrid.PasteItemEvent += PasteLocatorEvent;
        }

        private List<ComboEnumItem> GetPossibleCategories()
        {
            ePlatformType mAppPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey);
            List<ePomElementCategory> categoriesList = PlatformInfoBase.GetPlatformImpl(mAppPlatform)?.GetPlatformPOMElementCategories();
            if (categoriesList == null)
            {
                return [];
            }
            List<ComboEnumItem> elementStatus = [];
            foreach (ePomElementCategory category in categoriesList)
            {
                elementStatus.Add(new ComboEnumItem() { text = category.ToString(), Value = category });
            }
            return elementStatus;
        }

        private List<string> GetPossibleCategoriesAsString()
        {
            ePlatformType mAppPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey);
            List<ePomElementCategory> categoriesList = PlatformInfoBase.GetPlatformImpl(mAppPlatform)?.GetPlatformPOMElementCategories();
            if (categoriesList == null)
            {
                return [];
            }

            List<string> categories = [];
            foreach (ePomElementCategory category in categoriesList)
            {
                categories.Add(category.ToString());
            }
            return categories;
        }

        private List<ComboEnumItem> GetPositionList()
        {
            List<ComboEnumItem> PositionComboItemList = [];
            var targetPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey);
            if (!targetPlatform.Equals(ePlatformType.NA))
            {
                PlatformInfoBase platformInfoBase = PlatformInfoBase.GetPlatformImpl(targetPlatform);
                List<ePosition> positionList = platformInfoBase.GetElementPositionList();
                foreach (var positionBy in positionList)
                {
                    ComboEnumItem comboEnumItem = new ComboEnumItem
                    {
                        text = GingerCore.General.GetEnumValueDescription(typeof(ePosition), positionBy),
                        Value = positionBy
                    };
                    PositionComboItemList.Add(comboEnumItem);
                }
            }
            return PositionComboItemList;
        }
        private List<ComboEnumItem> GetPlatformLocatByList()
        {
            List<ComboEnumItem> locateByComboItemList = [];

            var targetPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey);

            if (!targetPlatform.Equals(ePlatformType.NA))
            {
                PlatformInfoBase platformInfoBase = PlatformInfoBase.GetPlatformImpl(targetPlatform);
                List<eLocateBy> platformLocateByList = platformInfoBase.GetPlatformUIElementLocatorsList();

                foreach (var locateBy in platformLocateByList)
                {
                    if (!locateBy.Equals(eLocateBy.POMElement))
                    {
                        ComboEnumItem comboEnumItem = new ComboEnumItem
                        {
                            text = GingerCore.General.GetEnumValueDescription(typeof(eLocateBy), locateBy),
                            Value = locateBy
                        };
                        locateByComboItemList.Add(comboEnumItem);
                    }
                }
            }

            return locateByComboItemList;
        }

        private void DeleteLocatorClicked(object sender, RoutedEventArgs e)
        {
            bool msgShowen = false;
            List<ElementLocator> locatorsToDelete = xElementDetails.xLocatorsGrid.Grid.SelectedItems.Cast<ElementLocator>().ToList();
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
            xElementDetails.xLocatorsGrid.Grid.CommitEdit();

            ElementLocator locator = new ElementLocator() { Active = true };
            mSelectedElement.Locators.Add(locator);

            xElementDetails.xLocatorsGrid.Grid.SelectedItem = locator;
            xElementDetails.xLocatorsGrid.ScrollToViewCurrentItem();
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

        private void DeleteFriendlyLocatorClicked(object sender, RoutedEventArgs e)
        {
            bool msgShowen = false;
            List<ElementLocator> friendlyLocatorsToDelete = xElementDetails.xFriendlyLocatorsGrid.Grid.SelectedItems.Cast<ElementLocator>().ToList();
            foreach (ElementLocator friendlyLocator in friendlyLocatorsToDelete)
            {
                if (friendlyLocator.IsAutoLearned)
                {
                    if (!msgShowen)
                    {
                        Reporter.ToUser(eUserMsgKey.POMCannotDeleteAutoLearnedElement);
                        msgShowen = true;
                    }
                }
                else
                {
                    mSelectedElement.FriendlyLocators.Remove(friendlyLocator);
                }
            }
        }

        private void AddFriendlyLocatorButtonClicked(object sender, RoutedEventArgs e)
        {
            xElementDetails.xFriendlyLocatorsGrid.Grid.CommitEdit();

            ElementLocator friendlyLocator = new ElementLocator() { Active = true };
            mSelectedElement.FriendlyLocators.Add(friendlyLocator);

            xElementDetails.xFriendlyLocatorsGrid.Grid.SelectedItem = friendlyLocator;
            xElementDetails.xFriendlyLocatorsGrid.ScrollToViewCurrentItem();
        }

        bool disabeledFriendlyLocatorsGrid_PreparingCellForEditLocatorsMsgShown;
        private void FriendlyLocatorsGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header.ToString() != nameof(ElementLocator.Active) && e.Column.Header.ToString() != nameof(ElementLocator.Help) && mSelectedLocator.IsAutoLearned)
            {
                if (!disabeledFriendlyLocatorsGrid_PreparingCellForEditLocatorsMsgShown)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "You can not edit Locator which was auto learned, please duplicate it and create customized Locator.");
                    disabeledFriendlyLocatorsGrid_PreparingCellForEditLocatorsMsgShown = true;
                }
                e.EditingElement.IsEnabled = false;
            }
        }

        private void SetControlPropertiesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ControlProperty.Name), WidthWeight = 25 },
                new GridColView() { Field = nameof(ControlProperty.Value), WidthWeight = 75 },
                new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xPropertyValueVETemplate"] },
            ]
            };

            xElementDetails.xPropertiesGrid.SetAllColumnsDefaultView(view);
            xElementDetails.xPropertiesGrid.InitViewItems();
            xElementDetails.xPropertiesGrid.SetTitleLightStyle = true;
            xElementDetails.xPropertiesGrid.btnAdd.AddHandler(System.Windows.Controls.Button.ClickEvent, new RoutedEventHandler(AddPropertyHandler));
            WeakEventManager<DataGrid, DataGridPreparingCellForEditEventArgs>.AddHandler(source: xElementDetails.xPropertiesGrid.grdMain, eventName: nameof(DataGrid.PreparingCellForEdit), handler: PropertiesGrid_PreparingCellForEdit);
            WeakEventManager<DataGrid, DataGridCellEditEndingEventArgs>.AddHandler(source: xElementDetails.xPropertiesGrid.grdMain, eventName: nameof(DataGrid.CellEditEnding), handler: PropertiesGrid_CellEditEnding);


        }

        private void PropertiesGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (mSelectedElement.IsAutoLearned == true || e.Column.Header.ToString() == nameof(ControlProperty.Name))
            {
                e.EditingElement.IsEnabled = false;
            }
        }

        private void PropertiesGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == nameof(ControlProperty.Value))
            {
                mSelectedElement.Path = e.EditingElement.DataContext is ControlProperty ctrlProp ? ctrlProp.Value : "";
            }
        }

        private void AddPropertyHandler(object sender, RoutedEventArgs e)
        {
            xElementDetails.xPropertiesGrid.Grid.CommitEdit();

            //for java Swing ParentIframe is not required
            if (WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey).Equals(ePlatformType.Java) && !mSelectedElement.GetType().Equals(typeof(HTMLElementInfo)))
            {
                return;
            }

            ControlProperty elemProp = new ControlProperty() { Name = ElementProperty.ParentIFrame };
            mSelectedElement.Properties.Add(elemProp);
            xElementDetails.xPropertiesGrid.Grid.SelectedItem = elemProp;
            xElementDetails.xPropertiesGrid.ScrollToViewCurrentItem();

            xElementDetails.xPropertiesGrid.ShowAdd = Visibility.Collapsed;
        }
        bool collapsed = false;

        private void HandelElementSelectionChange()
        {
            if (mSelectedElement != null)
            {
                if (IsFirstSelection)
                {
                    IsFirstSelection = false;
                }
                mPOM.IsCollapseDetailsExapander = false;

                if (mSelectedElement.ElementName != null)
                {
                    string title;
                    if (mSelectedElement.ElementName.Length > 100)
                    {
                        title = string.Format("'{0}...' Details", mSelectedElement.ElementName[..25]);
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
                CollectionChangedEventManager.RemoveHandler(source: mSelectedElement.Locators, handler: Locators_CollectionChanged);
                CollectionChangedEventManager.AddHandler(source: mSelectedElement.Locators, handler: Locators_CollectionChanged);


                xElementDetails.xLocatorsGrid.DataSourceList = mSelectedElement.Locators;
                UpdateLocatorsHeader();
                if (isEnableFriendlyLocator)
                {
                    CollectionChangedEventManager.RemoveHandler(source: mSelectedElement.FriendlyLocators, handler: FriendlyLocators_CollectionChanged);
                    CollectionChangedEventManager.AddHandler(source: mSelectedElement.FriendlyLocators, handler: FriendlyLocators_CollectionChanged);


                    for (int j = 0; j < mSelectedElement.FriendlyLocators.Count; j++)
                    {
                        ElementLocator felementLocator = mSelectedElement.FriendlyLocators[j];
                        ElementInfo newelementinfo = mElements.FirstOrDefault(x => x.Guid.ToString() == felementLocator.LocateValue);
                        if (newelementinfo != null)
                        {
                            felementLocator.LocateValue = newelementinfo.Guid.ToString();
                            felementLocator.ReferanceElement = newelementinfo.ElementName + "[" + newelementinfo.ElementType + "]";
                        }
                        else
                        {
                            mSelectedElement.FriendlyLocators.Remove(felementLocator);
                        }
                    }
                    xElementDetails.xFriendlyLocatorsGrid.DataSourceList = mSelectedElement.FriendlyLocators;
                    xElementDetails.xFriendlyLocatorsGrid.ShowAdd = Visibility.Collapsed;
                    xElementDetails.xFriendlyLocatorsGrid.ShowDelete = Visibility.Collapsed;
                    UpdateFriendlyLocatorsHeader();
                }
                else
                {
                    xElementDetails.xFriendlyLocatorTab.Visibility = Visibility.Collapsed;
                }
                string allProperties = string.Empty;
                mSelectedElement.Properties.CollectionChanged -= Properties_CollectionChanged;
                mSelectedElement.Properties.CollectionChanged += Properties_CollectionChanged;


                xElementDetails.xPropertiesGrid.DataSourceList = GingerCore.General.ConvertListToObservableList(mSelectedElement.Properties.Where(p => p.ShowOnUI).ToList());
                if (!mSelectedElement.IsAutoLearned && mSelectedElement.Properties.FirstOrDefault(c => c.Name == "Parent IFrame") == null)
                {
                    xElementDetails.xPropertiesGrid.ShowAdd = Visibility.Visible;
                }
                else
                {
                    xElementDetails.xPropertiesGrid.ShowAdd = Visibility.Collapsed;
                }
                UpdatePropertiesHeader();

                //update screenshot
                BitmapSource source = null;
                if (mSelectedElement.ScreenShotImage != null)
                {
                    source = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(mSelectedElement.ScreenShotImage.ToString()));
                }
                xElementDetails.xElementScreenShotFrame.ClearAndSetContent(new ScreenShotViewPage(mSelectedElement?.ElementName, source, false));
            }
            else
            {
                DisableDetailsExpander();
            }
        }

        private void DisableDetailsExpander()
        {
            this.Dispatcher.Invoke(() =>
            {
                xDetailsExpanderLabel.Content = "Element Details";
            });
        }

        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedElement != null)
            {

                try
                {
                    mWinExplorer.HighLightElement(mSelectedElement, true, mPOM?.MappedUIElements);
                }
                catch (Exception ex)
                {
                    if (ex is NoSuchElementException)
                    {
                        Reporter.ToUser(eUserMsgKey.ElementNotFound);
                    }
                    Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);

                }
            }
        }

        private void DetailsGrid_Expanded(object sender, RoutedEventArgs e)
        {
            Row2.Height = new GridLength(200);
        }

        private void DetailsGrid_Collapsed(object sender, RoutedEventArgs e)
        {
            Row2.Height = new GridLength(35);
        }

        private bool IsTestBtnClicked = false;
        private void TestElementButtonClicked(object sender, RoutedEventArgs e)
        {

            if (IsTestBtnClicked)
            {
                Reporter.ToUser(eUserMsgKey.LocatorTestInProgress);
                return;
            }

            if (!ValidateDriverAvalability())
            {
                return;
            }

            IsTestBtnClicked = true;


            ElementInfo CurrentEI = (ElementInfo)MainElementsGrid.CurrentItem;

            if (mSelectedLocator != null)
            {
                var testElement = new ElementInfo
                {
                    Path = CurrentEI.Path,
                    Locators = [mSelectedLocator],
                    FriendlyLocators = CurrentEI.FriendlyLocators
                };
                //For Java Driver Widgets

                if (WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey).Equals(ePlatformType.Java))
                {
                    if (mSelectedElement.GetType().Equals(typeof(GingerCore.Drivers.Common.HTMLElementInfo)))
                    {
                        var htmlElementInfo = new GingerCore.Drivers.Common.HTMLElementInfo() { Path = testElement.Path, Locators = testElement.Locators };
                        testElement = htmlElementInfo;
                        testElement.Properties = CurrentEI.Properties;
                    }
                }
                else if (WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey).Equals(ePlatformType.Web))
                {
                    var htmlElementInfo = new HTMLElementInfo
                    {
                        Path = testElement.Path,
                        Locators = testElement.Locators,
                        Properties = CurrentEI.Properties,
                        FriendlyLocators = testElement.FriendlyLocators
                    };
                    testElement = htmlElementInfo;
                }

                Task.Run(() =>
                {
                    try
                    {
                        mWinExplorer.TestElementLocators(testElement, false, mPOM);
                    }
                    finally
                    {
                        IsTestBtnClicked = false;
                    }
                });
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
                mWinExplorer.TestElementLocators(mSelectedElement, mPOM: mPOM);
            }
        }

        private void SetMissingCategoriesForSelectedElements(object sender, RoutedEventArgs e)
        {
            if (xMainElementsGrid.Grid.SelectedItems.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please select elements to set missing categories.");
                return;
            }

            bool isCategoryUpdated = false;
            string selectedCategory = "";
            if (InputBoxWindow.OpenDialog("Set Missing Categories", "Select Category to set:", ref selectedCategory, GetPossibleCategoriesAsString()))
            {
                if (!string.IsNullOrEmpty(selectedCategory))
                {
                    try
                    {
                        Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, "setting all missing categories for selected elements...");
                        foreach (ElementInfo element in xMainElementsGrid.Grid.SelectedItems)
                        {
                            if (element.Properties.All(y => y.Category == null) && element.Locators.All(y => y.Category == null))
                            {
                                SetMissingCategoriesForElement(element, (ePomElementCategory)Enum.Parse(typeof(ePomElementCategory), selectedCategory));
                                isCategoryUpdated = true;
                            }

                        }
                        if (isCategoryUpdated)
                        {
                            Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Missing categories updated successfully.");
                        }
                        else
                        {
                            Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No missing categoriess found.");
                        }

                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error in setting missing categories for selected elements", ex);
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Error in setting missing categories for all/some elements");
                    }
                    finally
                    {
                        Reporter.HideStatusMessage();
                    }
                }
            }
        }

        private void SetMissingCategoriesForElement(ElementInfo element, ePomElementCategory category)
        {
            foreach (ElementLocator locator in element.Locators)
            {
                if (locator.Category == null)
                {
                    locator.Category = category;
                }
            }
            foreach (ControlProperty property in element.Properties)
            {
                if (property.Category == null)
                {
                    property.Category = category;
                }
            }
        }

        private void xCopyLocatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (mSelectedLocator != null)
            {
                GingerCore.General.SetClipboardText(mSelectedLocator.LocateValue);
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
            if (mAgent != null && ((AgentOperations)mAgent.AgentOperations).Driver.IsDriverBusy)
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
            xElementDetails.xLocatorsGrid.Grid.CommitEdit();
            xElementDetails.xLocatorsGrid.Grid.CancelEdit();
        }


        private void xElementDetailsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set the selected tab text style
            try
            {
                if (xElementDetails.xElementDetailsTabs.SelectedItem != null)
                {
                    foreach (TabItem tab in xElementDetails.xElementDetailsTabs.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)
                        {
                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (xElementDetails.xElementDetailsTabs.SelectedItem == tab)
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                }
                                else
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$PrimaryColor_Black");
                                } ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
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
            ElementLocator selectedVarb = (ElementLocator)xElementDetails.xLocatorsGrid.CurrentItem;
            if (selectedVarb.IsAutoLearned)
            {
                if (!string.IsNullOrEmpty(selectedVarb.ToString()))
                {

                    try
                    {
                        string tempFilePath = GingerCoreNET.GeneralLib.General.CreateTempTextFile(selectedVarb.ToString());
                        try
                        {
                            DocumentEditorPage docPage = new DocumentEditorPage(tempFilePath, enableEdit: false, UCTextEditorTitle: "Note : You can not edit Locator which was auto learned");
                            docPage.ShowAsWindow("Locator Value");
                        }
                        finally
                        {
                            GingerCoreNET.GeneralLib.General.DeleteTempTextFile(tempFilePath);

                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error displaying locator value", ex);
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to display locator value: " + ex.Message);
                    }
                }

            }
            else
            {
                ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedVarb, nameof(ElementLocator.LocateValue), null);
                VEEW.ShowAsWindow();
            }
        }

        private void xPropertyValueVEButton_Click(object sender, RoutedEventArgs e)
        {
            ControlProperty selectedVerb = (ControlProperty)xElementDetails.xPropertiesGrid.CurrentItem;
            ElementInfo elementInfo = (ElementInfo)xMainElementsGrid.CurrentItem;
            if (elementInfo.IsAutoLearned)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "You can not edit Property which was auto learned.");
                disabeledLocatorsMsgShown = true;
            }
            else
            {
                ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedVerb, nameof(ControlProperty.Value), null);
                VEEW.ShowAsWindow();
            }
        }

        private void SetFriendlyLocatorsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox },
            ]
            };
            List<ComboEnumItem> positionList = GetPositionList();
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Position), Header = "Position", WidthWeight = 25, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = positionList });

            List<ComboEnumItem> locateByList = GetPlatformLocatByList();
            ComboEnumItem comboEnumItem = new ComboEnumItem
            {
                text = GingerCore.General.GetEnumValueDescription(typeof(eLocateBy), eLocateBy.POMElement),
                Value = eLocateBy.POMElement
            };
            locateByList.Add(comboEnumItem);

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.ReferanceElement), Header = "Locate Value", WidthWeight = 50 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, ReadOnly = true });
            xElementDetails.xFriendlyLocatorsGrid.SetAllColumnsDefaultView(defView);
            xElementDetails.xFriendlyLocatorsGrid.InitViewItems();

            xElementDetails.xFriendlyLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));

            WeakEventManager<DataGrid, DataGridPreparingCellForEditEventArgs>.AddHandler(source: xElementDetails.xFriendlyLocatorsGrid.grdMain, eventName: nameof(DataGrid.PreparingCellForEdit), handler: FriendlyLocatorsGrid_PreparingCellForEdit);
            xElementDetails.xFriendlyLocatorsGrid.PasteItemEvent += PasteLocatorEvent;


        }

    }

    internal class SelfHealingInfoStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value.ToString();

            var enumItems = GingerCore.General.GetEnumValuesForCombo(typeof(SelfHealingInfoEnum));

            foreach (var enumItem in enumItems)
            {
                if (enumItem.Value.Equals(SelfHealingInfoEnum.ElementDeleted) && status.Equals(enumItem.text))
                {
                    return System.Windows.Media.Brushes.Red;
                }
            }

            return System.Drawing.Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
