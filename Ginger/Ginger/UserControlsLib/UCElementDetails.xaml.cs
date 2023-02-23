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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.Actions.UserControls;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowsLibNew.AddActionMenu;
using Ginger.Reports;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.UCListView;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.Application_Models;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger
{
    public class GridFilters
    {
    }

    /// <summary>
    /// Interaction logic for UCElementDetails.xaml
    /// </summary>
    public partial class UCElementDetails : UserControl, INotifyPropertyChanged
    {
        public Agent mAgent { get; set; }
        public Context Context;


        public bool ShowTestBtn { get; set; }

        public PlatformInfoBase Platform { get; set; }

        public bool ShowHelpColumn { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        bool SelectedElementChanged = false;
        private ElementInfo mSelectedElement;

        ScreenShotViewPage mScreenShotViewPage;
        public ElementInfo SelectedElement
        {
            get
            {
                return mSelectedElement;
            }
            set
            {
                mSelectedElement = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedElement)));
                }

                if (mSelectedElement != null)
                {
                    mCurrentControlTreeViewItem = WindowExplorerCommon.GetTreeViewItemForElementInfo(mSelectedElement);

                    if (mCurrentControlTreeViewItem != null && mCurrentControlTreeViewItem.NodeObject() is GingerCore.Actions.UIAutomation.UIAElementInfo)
                    {
                        (mCurrentControlTreeViewItem.NodeObject() as GingerCore.Actions.UIAutomation.UIAElementInfo).WindowExplorer = mSelectedElement.WindowExplorer;
                    }

                    SelectedElementChanged = true;
                    RefreshPropertiesAndLocators();

                    if (ShowActionTab)
                    {
                        RefreshElementAction();
                    }
                }
            }
        }

        public IWindowExplorer WindowExplorerDriver;
        ITreeViewItem mCurrentControlTreeViewItem;

        private void RefreshElementAction()
        {
            try
            {
                if (xElementDetailsTabs.SelectedItem == xAddActionTab)
                {
                    UpdateElementActionTab();
                }
            }
            catch (Exception exc)
            {
                Reporter.ToLog(eLogLevel.ERROR, exc.Message, exc);
            }
        }

        public bool ShowAutoLearnedColumn { get; set; }

        public bool ShowActionTab
        {
            get
            {
                return xAddActionTab.Visibility == Visibility.Visible;
            }
            set
            {
                if (value == true)
                {
                    xAddActionTab.Visibility = Visibility.Visible;
                }
                else
                {
                    xAddActionTab.Visibility = Visibility.Collapsed;
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

        ElementLocator mSelectedFriendlyLocator
        {
            get
            {
                if (xFriendlyLocatorsGrid.Grid.SelectedItem != null)
                {
                    return (ElementLocator)xFriendlyLocatorsGrid.Grid.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        bool POMBasedAction = false;
        bool POMElementsUpdated = false;
        ElementInfo POMElement = null;

        //Act mAct = null;
        //ActUIElement UIElementAction = null;
        ///*ActionEditPage*/ ActUIElementEditPage actEditPage = null;
        //ActUIElementEditPage ActEditPage = null;

        public UCElementDetails()
        {
            InitializeComponent();

            InitControlPropertiesGridView();

        }

        private void ElementDetailsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

                    if (xElementDetailsTabs.SelectedItem == xAddActionTab)
                    {
                        AddActionTab_Selected(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in POM Edit Page tabs style", ex);
            }
        }

        public enum eViewPage
        {
            POM, Explorer
        }

        private void InitControlPropertiesGridView()
        {
            // Grid View
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = "Name", WidthWeight = 8, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = "Value", WidthWeight = 20, ReadOnly = true });

            xPropertiesGrid.RowDoubleClick += XPropertiesGrid_RowDoubleClick;
            xPropertiesGrid.ToolTip = "Double click on a row to copy the selected property value";

            xPropertiesGrid.SetAllColumnsDefaultView(view);
            xPropertiesGrid.InitViewItems();
        }

        private void XPropertiesGrid_RowDoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty((sender as ControlProperty).Value))
            {
                Clipboard.SetText((sender as ControlProperty).Value);
            }
        }

        private void XLocatorsGrid_RowDoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetText((sender as ElementLocator).LocateValue);
        }

        bool LocatorsGridInitialized = false;

        public void InitLegacyLocatorsGridView()
        {
            if (!LocatorsGridInitialized)
            {
                // Grid View
                GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
                view.GridColsView = new ObservableList<GridColView>();

                view.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), WidthWeight = 8, ReadOnly = true });
                view.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), WidthWeight = 20, ReadOnly = true });

                //if (mPlatform.PlatformType() != ePlatformType.Web && mPlatform.PlatformType() != ePlatformType.Java)
                //{
                xLocatorsGrid.ShowAdd = Visibility.Collapsed;
                xLocatorsGrid.ShowDelete = Visibility.Collapsed;
                xLocatorsGrid.ShowUpDown = Visibility.Collapsed;
                xLocatorsGrid.ShowCopyCutPast = Visibility.Collapsed;
                xLocatorsGrid.ShowClearAll = Visibility.Collapsed;
                //}
                xLocatorsGrid.RowDoubleClick += XLocatorsGrid_RowDoubleClick;
                xLocatorsGrid.ToolTip = "Double click on a row to copy the selected Locator's Locate by value";

                xLocatorsGrid.SetAllColumnsDefaultView(view);
                xLocatorsGrid.InitViewItems();

                LocatorsGridInitialized = true;
            }
        }

        internal void InitLocatorsGridView()
        {
            if (!LocatorsGridInitialized)
            {
                GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
                defView.GridColsView = new ObservableList<GridColView>();
                defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });

                List<ComboEnumItem> locateByList = GetPlatformLocatByList();

                defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, });
                defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 65 });
                defView.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xSelectedElementSectionGrid.Resources["xLocateValueVETemplate"] });
                defView.GridColsView.Add(new GridColView() { Field = "", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xSelectedElementSectionGrid.Resources["xCopyLocatorButtonTemplate"] });
                defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.EnableFriendlyLocator), Header = "Friendly Locator", WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
                defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, ReadOnly = true });
                defView.GridColsView.Add(new GridColView() { Field = "Test", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xSelectedElementSectionGrid.Resources["xTestElementButtonTemplate"] });
                defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.StatusIcon), Header = "Status", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xSelectedElementSectionGrid.Resources["xTestStatusIconTemplate"] });
                xLocatorsGrid.SetAllColumnsDefaultView(defView);
                xLocatorsGrid.InitViewItems();

                xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
                xLocatorsGrid.AddToolbarTool(eImageType.Run, "Test All Elements Locators", new RoutedEventHandler(TestAllElementsLocators));
                xLocatorsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddLocatorButtonClicked));
                xLocatorsGrid.SetbtnDeleteHandler(new RoutedEventHandler(DeleteLocatorClicked));

                xLocatorsGrid.RowDoubleClick += XLocatorsGrid_RowDoubleClick;
                xLocatorsGrid.ToolTip = "Double click on a row to copy the selected Locator's Locate by value";

                xLocatorsGrid.grdMain.PreparingCellForEdit += LocatorsGrid_PreparingCellForEdit;
                xLocatorsGrid.PasteItemEvent += PasteLocatorEvent;

                LocatorsGridInitialized = true;
            }
        }

        private void TestAllElementsLocators(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (SelectedElement != null)
            {
                WindowExplorerDriver.TestElementLocators(SelectedElement);
            }
        }

        private void AddLocatorButtonClicked(object sender, RoutedEventArgs e)
        {
            xLocatorsGrid.Grid.CommitEdit();

            ElementLocator locator = new ElementLocator() { Active = true };
            SelectedElement.Locators.Add(locator);

            xLocatorsGrid.Grid.SelectedItem = locator;
            xLocatorsGrid.ScrollToViewCurrentItem();
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
                    SelectedElement.Locators.Remove(locator);
                }
            }
        }

        private void PasteLocatorEvent(PasteItemEventArgs EventArgs)
        {
            ElementLocator copiedLocator = (ElementLocator)EventArgs.Item;
            copiedLocator.IsAutoLearned = false;
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

        private void XFriendlyLocatorsGrid_RowDoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetText((sender as ElementLocator).LocateValue);
        }

        bool FriendlyLocatorsGridInitialized = false;

        public void InitLegacyFriendlyLocatorsGridView()
        {
            if (!FriendlyLocatorsGridInitialized)
            {
                // Grid View
                GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
                view.GridColsView = new ObservableList<GridColView>();

                view.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), WidthWeight = 8, ReadOnly = true });
                view.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), WidthWeight = 20, ReadOnly = true });

                //if (mPlatform.PlatformType() != ePlatformType.Web && mPlatform.PlatformType() != ePlatformType.Java)
                //{
                xFriendlyLocatorsGrid.ShowAdd = Visibility.Collapsed;
                xFriendlyLocatorsGrid.ShowDelete = Visibility.Collapsed;
                xFriendlyLocatorsGrid.ShowUpDown = Visibility.Collapsed;
                xFriendlyLocatorsGrid.ShowCopyCutPast = Visibility.Collapsed;
                xFriendlyLocatorsGrid.ShowClearAll = Visibility.Collapsed;
                //}
                xFriendlyLocatorsGrid.RowDoubleClick += XFriendlyLocatorsGrid_RowDoubleClick;
                xFriendlyLocatorsGrid.ToolTip = "Double click on a row to copy the selected Locator's Locate by value";

                xFriendlyLocatorsGrid.SetAllColumnsDefaultView(view);
                xFriendlyLocatorsGrid.InitViewItems();

                FriendlyLocatorsGridInitialized = true;
            }
        }

        private void TestAllElementsFriendlyLocators(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (SelectedElement != null)
            {
                WindowExplorerDriver.TestElementLocators(SelectedElement);
            }
        }

        private void AddFriendlyLocatorButtonClicked(object sender, RoutedEventArgs e)
        {
            xFriendlyLocatorsGrid.Grid.CommitEdit();

            ElementLocator locator = new ElementLocator() { Active = true };
            SelectedElement.Locators.Add(locator);

            xFriendlyLocatorsGrid.Grid.SelectedItem = locator;
            xFriendlyLocatorsGrid.ScrollToViewCurrentItem();
        }

        private void DeleteFriendlyLocatorClicked(object sender, RoutedEventArgs e)
        {
            bool msgShowen = false;
            List<ElementLocator> locatorsToDelete = xFriendlyLocatorsGrid.Grid.SelectedItems.Cast<ElementLocator>().ToList();
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
                    SelectedElement.Locators.Remove(locator);
                }
            }
        }

        private void PasteFriendlyLocatorEvent(PasteItemEventArgs EventArgs)
        {
            ElementLocator copiedLocator = (ElementLocator)EventArgs.Item;
            copiedLocator.IsAutoLearned = false;
        }

        bool disabeledFriendlyLocatorsMsgShown;
        private void FriendlyLocatorsGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header.ToString() != nameof(ElementLocator.Active) && e.Column.Header.ToString() != nameof(ElementLocator.Help) && mSelectedLocator.IsAutoLearned)
            {
                if (!disabeledFriendlyLocatorsMsgShown)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "You can not edit Locator which was auto learned, please duplicate it and create customized Locator.");
                    disabeledFriendlyLocatorsMsgShown = true;
                }
                e.EditingElement.IsEnabled = false;
            }
        }

        private bool ValidateDriverAvalability()
        {
            if (WindowExplorerDriver == null)
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

        private List<ComboEnumItem> GetPlatformLocatByList()
        {
            List<ComboEnumItem> locateByComboItemList = new List<ComboEnumItem>();

            if (Platform == null)
            {
                Platform = PlatformInfoBase.GetPlatformImpl(Context.Platform);  // ((Agent)ApplicationAgent.Agent).Platform);
            }

            List<eLocateBy> platformLocateByList = Platform.GetPlatformUIElementLocatorsList();

            foreach (var locateBy in platformLocateByList)
            {
                if (!locateBy.Equals(eLocateBy.POMElement))
                {
                    ComboEnumItem comboEnumItem = new ComboEnumItem();
                    comboEnumItem.text = GingerCore.General.GetEnumValueDescription(typeof(eLocateBy), locateBy);
                    comboEnumItem.Value = locateBy;
                    locateByComboItemList.Add(comboEnumItem);
                }
            }

            return locateByComboItemList;
        }


        private List<ComboEnumItem> GetPositionList()
        {
            List<ComboEnumItem> PositionComboItemList = new List<ComboEnumItem>();
            if (Platform == null)
            {
                Platform = PlatformInfoBase.GetPlatformImpl(Context.Platform);  // ((Agent)ApplicationAgent.Agent).Platform);
            }
            List<ePosition> positionList = Platform.GetElementPositionList();
            foreach (var positionBy in positionList)
            {
                ComboEnumItem comboEnumItem = new ComboEnumItem();
                comboEnumItem.text = GingerCore.General.GetEnumValueDescription(typeof(ePosition), positionBy);
                comboEnumItem.Value = positionBy;
                PositionComboItemList.Add(comboEnumItem);
            }
            return PositionComboItemList;
        }

        public void RefreshPropertiesAndLocators()
        {
            if (SelectedElement != null)
            {
                if (SelectedElement.Properties == null || SelectedElement.Properties.Count == 0)
                    SelectedElement.Properties = SelectedElement.GetElementProperties();

                if (SelectedElement.Properties == null || SelectedElement.Properties.Count == 0)
                    SelectedElement.Properties = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetElementProperties();

                if (SelectedElement.Locators == null || SelectedElement.Locators.Count == 0)
                    SelectedElement.Locators = SelectedElement.GetElementLocators();

                xPropertiesGrid.DataSourceList = GingerCore.General.ConvertListToObservableList(SelectedElement.Properties.Where(p => p.ShowOnUI).ToList());
                xPropertiesGrid.DataSourceList = SelectedElement.Properties;
                xLocatorsGrid.DataSourceList = SelectedElement.Locators;
                if (Context.Platform == ePlatformType.Web && (SelectedElement.FriendlyLocators == null || SelectedElement.FriendlyLocators.Count == 0))
                {
                    SelectedElement.FriendlyLocators = SelectedElement.GetElementFriendlyLocators();
                    xFriendlyLocatorsGrid.DataSourceList = SelectedElement.FriendlyLocators;
                    xFriendlyLocatorsGrid.ShowAdd = Visibility.Collapsed;
                    xFriendlyLocatorsGrid.ShowDelete = Visibility.Collapsed;
                    xFriendlyLocatorTab.Visibility = Visibility.Visible;
                }
                else
                {
                    xFriendlyLocatorTab.Visibility = Visibility.Collapsed;
                }

                Dispatcher.Invoke(() =>
                {
                    xPropertiesTextBlock.Text = SelectedElement.Properties == null ? "Properties" : string.Format("Properties ({0})", SelectedElement.Properties.Count);
                    xLocatorsTextBlock.Text = SelectedElement.Locators == null ? "Locators" : string.Format("Locators ({0})", SelectedElement.Locators.Count);
                    xFriendlyLocatorsTextBlock.Text = SelectedElement.FriendlyLocators == null ? "Friendly Locators" : String.Format("Friendly Locators({0})", SelectedElement.FriendlyLocators.Count);
                });
            }
        }

        private void AddActionTab_Selected(object sender, RoutedEventArgs e)
        {
            if (SelectedElement == null)
            {
                return;
            }

            if (WindowExplorerDriver == null)
            {
                WindowExplorerDriver = (IWindowExplorer)((AgentOperations)Context.Agent.AgentOperations).Driver;
            }

            if (WindowExplorerDriver.IsPOMSupported())
            {
                xIntegratePOMChkBox.Visibility = Visibility.Visible;

                /// If it's null, it means it's came into view or is selected for the first time 
                /// so, we'll check it and ask user to select a POM from a list of available POMs
                if (xIntegratePOMChkBox.IsChecked == null)
                {
                    xIntegratePOMChkBox.IsChecked = true;
                }
            }
            else
            {
                xIntegratePOMChkBox.Visibility = Visibility.Collapsed;
            }

            UpdateElementActionTab();
        }

        bool? POMCheckBoxToggled = null;

        void SetPOMBasedChecks()
        {
            if (POMSelectionPending)
            {
                ShowPOMSelection();
            }

            try
            {
                if (xIntegratePOMChkBox.IsChecked == true && SelectedPOM != null)
                {
                    if (POMElement != null && POMCheckBoxToggled == true)
                    {
                        POMBasedAction = true;
                        POMCheckBoxToggled = false;
                    }
                    else
                    {
                        //pomAllElementsPage = new PomAllElementsPage(xWindowSelection.SelectedPOM, PomAllElementsPage.eAllElementsPageContext.POMEditPage);
                        ElementInfo matchingOriginalElement = (ElementInfo)WindowExplorerDriver.GetMatchingElement(SelectedElement, SelectedPOM.GetUnifiedElementsList());

                        if (matchingOriginalElement == null)
                        {
                            WindowExplorerDriver.LearnElementInfoDetails(SelectedElement);
                            matchingOriginalElement = (ElementInfo)WindowExplorerDriver.GetMatchingElement(SelectedElement, SelectedPOM.GetUnifiedElementsList());
                        }

                        if (matchingOriginalElement != null &&
                                (SelectedPOM.MappedUIElements.Contains(matchingOriginalElement) || SelectedPOM.UnMappedUIElements.Contains(matchingOriginalElement)))
                        {
                            PomDeltaUtils pomDeltaUtils = new PomDeltaUtils(SelectedPOM, Context.Agent);
                            pomDeltaUtils.KeepOriginalLocatorsOrderAndActivation = true;

                            /// Not Required but 
                            pomDeltaUtils.DeltaViewElements.Clear();

                            CustomElementLocatorsCheck(matchingOriginalElement, SelectedElement);

                            /// To Do - POM Delta Run and if Updated Element is found then ask user if they would like to replace existing POM Element with New ?
                            pomDeltaUtils.SetMatchingElementDeltaDetails(matchingOriginalElement, SelectedElement);

                            int originalItemIndex = -1;
                            if ((ApplicationPOMModel.eElementGroup)matchingOriginalElement.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
                            {
                                originalItemIndex = SelectedPOM.MappedUIElements.IndexOf(matchingOriginalElement);
                            }

                            if (pomDeltaUtils.DeltaViewElements[0].DeltaStatus == eDeltaStatus.Changed)
                            {
                                //enter it to POM elements instead of existing one
                                if (xAutoUpdatePOMElementChkBox.IsChecked == true || Reporter.ToUser(eUserMsgKey.UpdateExistingPOMElement, matchingOriginalElement.ElementName) == eUserMsgSelection.Yes)
                                {
                                    /// Replace existing element with new one
                                    /// Element exists in Mapped Elements list
                                    if (originalItemIndex > -1)
                                    {
                                        SelectedPOM.MappedUIElements.RemoveAt(originalItemIndex);
                                        SelectedPOM.MappedUIElements.Insert(originalItemIndex, pomDeltaUtils.DeltaViewElements[0].ElementInfo);
                                        POMElementsUpdated = false;
                                    }
                                    /// Element exists in Un-Mapped Elements list
                                    /// We'll remove Element from Unmapped list and add it as new into Mapped Elements list
                                    else
                                    {
                                        SelectedPOM.MappedUIElements.Add(pomDeltaUtils.DeltaViewElements[0].ElementInfo);
                                        SelectedPOM.UnMappedUIElements.Remove(matchingOriginalElement);
                                        POMElementsUpdated = true;
                                    }

                                    POMElement = pomDeltaUtils.DeltaViewElements[0].ElementInfo;
                                    POMElement.ParentGuid = SelectedPOM.Guid;
                                }
                                else
                                {
                                    if (originalItemIndex == -1)
                                    {
                                        SelectedPOM.MappedUIElements.Add(pomDeltaUtils.DeltaViewElements[0].ElementInfo);
                                        SelectedPOM.UnMappedUIElements.Remove(matchingOriginalElement);

                                        POMElement = pomDeltaUtils.DeltaViewElements[0].ElementInfo;
                                        POMElementsUpdated = true;
                                    }
                                    else
                                    {
                                        POMElement = matchingOriginalElement;
                                        POMElementsUpdated = false;
                                    }

                                }
                            }
                            else
                            {
                                /// Element exist in UnMapped Elements List
                                if (originalItemIndex == -1)
                                {
                                    //if (Reporter.ToUser(eUserMsgKey.POMMoveElementFromUnmappedToMapped, matchingOriginalElement.ElementName, xWindowSelection.SelectedPOM.Name) == eUserMsgSelection.Yes)
                                    //{
                                    SelectedPOM.MappedUIElements.Add(matchingOriginalElement);
                                    SelectedPOM.UnMappedUIElements.Remove(matchingOriginalElement);
                                    //}
                                }

                                POMElement = matchingOriginalElement;
                            }
                            POMBasedAction = true;
                        }
                        /// Element doesn't exist on POM, perform New Element related checks
                        else
                        {
                            if (xAutoUpdatePOMElementChkBox.IsChecked == true || Reporter.ToUser(eUserMsgKey.POMElementNotExist, SelectedElement.ElementName, SelectedPOM.Name) == eUserMsgSelection.Yes)
                            {
                                POMBasedAction = true;
                                SelectedElement.IsAutoLearned = true;
                                SelectedPOM.MappedUIElements.Add(SelectedElement);

                                POMElement = SelectedElement;
                                POMElement.ParentGuid = SelectedPOM.Guid;
                                POMElementsUpdated = true;
                            }
                            else
                            {
                                POMElement = null;
                                POMBasedAction = false;
                                POMElementsUpdated = false;
                            }
                        }
                    }
                }
                else
                {
                    if (POMCheckBoxToggled == true)
                    {
                        POMCheckBoxToggled = false;
                    }
                    POMBasedAction = false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in ShowCurrentControlInfo", ex);
                Reporter.ToUser(eUserMsgKey.ObjectLoad);
            }
        }

        public void CustomElementLocatorsCheck(ElementInfo matchingOriginalElement, ElementInfo selectedElement)
        {
            if (matchingOriginalElement.Locators.Count != selectedElement.Locators.Count && matchingOriginalElement.Locators.Where(l => l.Help.Contains("Custom Locator")).Count() > 0)
            {
                foreach (ElementLocator customLocator in matchingOriginalElement.Locators.Where(l => l.Help.Contains("Custom Locator")))
                {
                    selectedElement.Locators.Add(customLocator);
                }
            }
        }

        void UpdateElementActionTab()
        {
            if (SelectedElement == null)
                return;

            if (LocatorChanged)
            {
                ElementLocator locator = xLocatorsGrid.CurrentItem as ElementLocator;
                if (locator != null && xActUIPageFrame.HasContent && (xActUIPageFrame.Content as ControlActionsPage_New).DefaultAction is ActUIElement)
                {
                    if (((xActUIPageFrame.Content as ControlActionsPage_New).DefaultAction as ActUIElement).ElementLocateBy != eLocateBy.POMElement)
                    {
                        ((xActUIPageFrame.Content as ControlActionsPage_New).DefaultAction as ActUIElement).ElementLocateBy = locator.LocateBy;
                        ((xActUIPageFrame.Content as ControlActionsPage_New).DefaultAction as ActUIElement).ElementLocateValue = locator.LocateValue;
                    }
                }

                LocatorChanged = false;
            }

            if (!SelectedElementChanged)
            {
                return;
            }

            SetPOMBasedChecks();

            ControlActionsPage_New CAP = null;
            if (xActUIPageFrame.HasContent)
            {
                xActUIPageFrame.Content = null;
            }

            if (SelectedElement.Locators.CurrentItem == null && SelectedElement.Locators.Count > 0)
            {
                SelectedElement.Locators.CurrentItem = SelectedElement.Locators[0];
            }

            ElementLocator eiLocator = SelectedElement.Locators.CurrentItem as ElementLocator;

            string elementVal = string.Empty;
            if (SelectedElement.OptionalValuesObjectsList.Count > 0)
            {
                elementVal = Convert.ToString(SelectedElement.OptionalValuesObjectsList.Where(v => v.IsDefault).FirstOrDefault().Value);
            }

            ElementActionCongifuration actConfigurations;
            if (POMBasedAction)
            {
                //POMElement
                actConfigurations = new ElementActionCongifuration
                {
                    LocateBy = eLocateBy.POMElement,
                    LocateValue = POMElement.ParentGuid.ToString() + "_" + POMElement.Guid.ToString(),
                    ElementValue = elementVal,
                    AddPOMToAction = true,
                    POMGuid = POMElement.ParentGuid.ToString(),
                    ElementGuid = POMElement.Guid.ToString(),
                    LearnedElementInfo = POMElement,
                    Type = POMElement.ElementTypeEnum
                };
            }
            else
            {
                //check if we have POM in context if yes set the Locate by and value to the specific POM if not so set it to the first active Locator in the list of Locators
                actConfigurations = new ElementActionCongifuration
                {
                    LocateBy = eiLocator.LocateBy,
                    LocateValue = eiLocator.LocateValue,
                    Type = SelectedElement.ElementTypeEnum,
                    ElementValue = elementVal,
                    ElementGuid = SelectedElement.Guid.ToString(),
                    LearnedElementInfo = SelectedElement
                };
            }
            CAP = new ControlActionsPage_New(WindowExplorerDriver, SelectedElement, Context, actConfigurations, mCurrentControlTreeViewItem);
            //}

            if (CAP == null)
            {
                xAddActionTab.Visibility = Visibility.Collapsed;
                xActUIPageFrame.Visibility = Visibility.Collapsed;
                xAddRunOperationPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (CAP.IsLegacyPlatform)
                {
                    xExecutionStatusIcon.Visibility = Visibility.Visible;
                    BindingHandler.ObjFieldBinding(xExecutionStatusIcon, UcItemExecutionStatus.StatusProperty, CAP.DefaultAction, nameof(Act.Status));
                }
                else
                {
                    xExecutionStatusIcon.Visibility = Visibility.Collapsed;
                }

                //xRunActBtn.Click += CAP.RunActionClicked;
                //xAddActBtn.Click += CAP.AddActionClicked;

                xActUIPageFrame.Content = CAP;
                xAddActionTab.Visibility = Visibility.Visible;
                xActUIPageFrame.Visibility = Visibility.Visible;
                xAddRunOperationPanel.Visibility = Visibility.Visible;
            }

            SelectedElementChanged = false;
        }

        public LocateByPOMElementPage locateByPOMElementPage;
        private ApplicationPOMModel mSelectedPOM;
        public ApplicationPOMModel SelectedPOM
        {
            get
            {
                return mSelectedPOM;
            }
            set
            {
                mSelectedPOM = value;
                if (mSelectedPOM == null)
                {
                    locateByPOMElementPage.xPomPathTextBox.Visibility = Visibility.Collapsed;

                    xIntegratePOMChkBox.IsChecked = false;
                    xIntegratePOMChkBox_Unchecked(null, null);
                }
                else
                {
                    locateByPOMElementPage.xPomPathTextBox.Visibility = Visibility.Visible;

                    xPOMSelectionFrame.Visibility = Visibility.Visible;
                    POMSelectionPending = false;
                    SelectedElementChanged = true;
                    UpdateElementActionTab();
                }
                HandlePOMOperationsPanelVisibility(true);
            }
        }

        bool POMSelectionPending = false;

        private void xIntegratePOMChkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (xElementDetailsTabs.SelectedItem != xAddActionTab)
            {
                POMSelectionPending = true;
            }
            else
            {
                ShowPOMSelection();
            }

            HandlePOMOperationsPanelVisibility(true);

            SelectedElementChanged = true;
            POMCheckBoxToggled = POMCheckBoxToggled == null ? false : true;
            RefreshElementAction();
        }

        void ShowPOMSelection()
        {
            if (locateByPOMElementPage == null)
            {
                ActUIElement act = new ActUIElement();

                locateByPOMElementPage = new LocateByPOMElementPage(Context, act, nameof(ActUIElement.ElementType), act, nameof(ActUIElement.ElementLocateValue), true);
                locateByPOMElementPage.SelectPOM_Click(this, null);

                locateByPOMElementPage.POMChangedPageEvent += LocateByPOMElementPage_POMChangedPageEvent;

                SelectedPOM = locateByPOMElementPage.SelectedPOM;
                xPOMSelectionFrame.Content = locateByPOMElementPage;

                POMSelectionPending = false;
            }

            xPOMSelectionFrame.Visibility = Visibility.Visible;
        }

        private void LocateByPOMElementPage_POMChangedPageEvent()
        {
            SelectedPOM = locateByPOMElementPage.SelectedPOM;
        }

        private void xIntegratePOMChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            xPOMSelectionFrame.Visibility = Visibility.Collapsed;
            HandlePOMOperationsPanelVisibility(false);

            if (sender != null)
            {
                SelectedElementChanged = true;
                POMCheckBoxToggled = true;
                RefreshElementAction();
            }
        }

        public void HandlePOMOperationsPanelVisibility(bool MakeVisible)
        {
            if (SelectedPOM != null && MakeVisible)
            {
                xPOMOperationsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                xPOMOperationsPanel.Visibility = Visibility.Collapsed;
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

        private void TestElementButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedLocator != null)
            {
                var testElement = new ElementInfo();
                testElement.Path = SelectedElement.Path;
                testElement.Locators = new ObservableList<ElementLocator>() { mSelectedLocator };

                //For Java Driver Widgets
                if (Platform.PlatformType() == ePlatformType.Java)  // WorkSpace.Instance.Solution.GetTargetApplicationPlatform(SelectedPOM.TargetApplicationKey).Equals(ePlatformType.Java))
                {
                    if (SelectedElement.GetType().Equals(typeof(GingerCore.Drivers.Common.HTMLElementInfo)))
                    {
                        var htmlElementInfo = new GingerCore.Drivers.Common.HTMLElementInfo() { Path = testElement.Path, Locators = testElement.Locators };
                        testElement = htmlElementInfo;
                        testElement.Properties = SelectedElement.Properties;
                    }
                }

                WindowExplorerDriver.TestElementLocators(testElement);
            }
        }

        private void xRunActBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xActUIPageFrame.Content != null && xActUIPageFrame.Content is ControlActionsPage_New)
            {
                (xActUIPageFrame.Content as ControlActionsPage_New).RunActionClicked(sender, e);
            }
        }

        private void xAddActBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xActUIPageFrame.Content != null && xActUIPageFrame.Content is ControlActionsPage_New)
            {
                (xActUIPageFrame.Content as ControlActionsPage_New).AddActionClicked(sender, e);

                if (POMElementsUpdated && (xAutoSavePOMChkBox.IsChecked == true
                    || Reporter.ToUser(eUserMsgKey.SavePOMChanges, SelectedPOM.Name) == eUserMsgSelection.Yes))
                {
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(SelectedPOM);
                }
            }
        }

        bool LocatorChanged = false;
        private void xLocatorsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            LocatorChanged = true;
        }

        private void xCopyLocatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedLocator != null)
            {
                Clipboard.SetText(mSelectedLocator.LocateValue);
            }
        }

        bool FriendlyLocatorChanged = false;
        private void xFriendlyLocatorsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            FriendlyLocatorChanged = true;
        }
    }
}
