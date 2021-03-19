using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowsLibNew.AddActionMenu;
using Ginger.Reports;
using Ginger.UserControls;
using Ginger.UserControlsLib;
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
    public partial class UCElementDetails : UserControl
    {
        public Agent mAgent { get; set; }
        public Context Context;

        public bool ShowTestBtn { get; set; }

        public PlatformInfoBase Platform { get; set; }

        public bool ShowHelpColumn { get; set; }

        bool mSelectedElementChanged = false;
        private ElementInfo mSelectedElement;
        public ElementInfo SelectedElement 
        {
            get
            {
                return mSelectedElement;
            }
            set
            {
                if(value != mSelectedElement)
                {
                    mSelectedElement = value;
                    mCurrentControlTreeViewItem = WindowExplorerCommon.GetTreeViewItemForElementInfo(mSelectedElement);
                    RefreshPropertiesAndLocators();
                    RefreshElementAction();
                }
            }
        }

        public IWindowExplorer WindowExplorerDriver;
        ITreeViewItem mCurrentControlTreeViewItem;
        private ObservableList<ActInputValue> mActInputValues;

        private void RefreshElementAction()
        {
            try
            {
                mActInputValues = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetItemSpecificActionInputValues();

                if(xElementDetailsTabs.SelectedItem == xAddActionTab)
                {
                    UpdateElementActionTab();
                }
                else
                {
                    mSelectedElementChanged = true;
                }
            }
            catch(Exception exc)
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

        bool POMBasedAction = false;
        ElementInfo POMElement = null;

        //Act mAct = null;
        //ActUIElement UIElementAction = null;
        ///*ActionEditPage*/ ActUIElementEditPage actEditPage = null;
        //ActUIElementEditPage ActEditPage = null;

        public UCElementDetails()
        {
            InitializeComponent();

            //UIElementAction = new ActUIElement();

            // new ActUIElementEditPage(UIElementAction);
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

        public void InitLegacyLocatorsGridView()
        {
            // Grid View
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = "LocateBy", WidthWeight = 8, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = "LocateValue", WidthWeight = 20, ReadOnly = true });

            //if (mPlatform.PlatformType() != ePlatformType.Web && mPlatform.PlatformType() != ePlatformType.Java)
            //{
                xLocatorsGrid.ShowAdd = Visibility.Collapsed;
                xLocatorsGrid.ShowDelete = Visibility.Collapsed;
                xLocatorsGrid.ShowUpDown = Visibility.Collapsed;
                xLocatorsGrid.ShowCopyCutPast = Visibility.Collapsed;
                xLocatorsGrid.ShowClearAll = Visibility.Collapsed;
            //}

            xLocatorsGrid.SetAllColumnsDefaultView(view);
            xLocatorsGrid.InitViewItems();
        }

        internal void InitLocatorsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });

            List<ComboEnumItem> locateByList = GetPlatformLocatByList();

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 65 });
            defView.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xSelectedElementSectionGrid.Resources["xLocateValueVETemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 25 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = "Test", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xSelectedElementSectionGrid.Resources["xTestElementButtonTemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.StatusIcon), Header = "Status", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xSelectedElementSectionGrid.Resources["xTestStatusIconTemplate"] });
            xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xLocatorsGrid.InitViewItems();

            xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            xLocatorsGrid.AddToolbarTool(eImageType.Run, "Test All Elements Locators", new RoutedEventHandler(TestAllElementsLocators));
            xLocatorsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddLocatorButtonClicked));
            xLocatorsGrid.SetbtnDeleteHandler(new RoutedEventHandler(DeleteLocatorClicked));

            xLocatorsGrid.grdMain.PreparingCellForEdit += LocatorsGrid_PreparingCellForEdit;
            xLocatorsGrid.PasteItemEvent += PasteLocatorEvent;
        }

        private void TestAllElementsLocators(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedElement != null)
            {
                WindowExplorerDriver.TestElementLocators(mSelectedElement);
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
            if (mAgent != null && mAgent.Driver.IsDriverBusy)
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

        public void RefreshPropertiesAndLocators()
        {
            if(mSelectedElement != null)
            {
                if (mSelectedElement.Properties == null || mSelectedElement.Properties.Count == 0)
                    mSelectedElement.Properties = mSelectedElement.GetElementProperties();

                if (mSelectedElement.Properties == null || mSelectedElement.Properties.Count == 0)
                    mSelectedElement.Properties = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetElementProperties();

                if (mSelectedElement.Locators == null || mSelectedElement.Locators.Count == 0)
                    mSelectedElement.Locators = mSelectedElement.GetElementLocators();

                xPropertiesGrid.DataSourceList = mSelectedElement.Properties;
                xLocatorsGrid.DataSourceList = mSelectedElement.Locators;

                Dispatcher.Invoke(() =>
                {
                    xPropertiesTextBlock.Text = mSelectedElement.Properties == null ? "Properties" : string.Format("Properties ({0})", mSelectedElement.Properties.Count);
                    xLocatorsTextBlock.Text = mSelectedElement.Locators == null ? "Locators" : string.Format("Locators ({0})", mSelectedElement.Locators.Count);
                });
            }
        }

        private void xAddActionTab_GotFocus(object sender, RoutedEventArgs e)
        {
            if (mSelectedElementChanged)
            {
                UpdateElementActionTab();
            }
        }

        void SetPOMBasedChecks()
        {
            if(POMSelectionPending)
            {
                ShowPOMSelection();
            }

            try
            {
            if (xIntegratePOMChkBox.IsChecked == true && SelectedPOM != null)
            {
                //pomAllElementsPage = new PomAllElementsPage(xWindowSelection.SelectedPOM, PomAllElementsPage.eAllElementsPageContext.POMEditPage);
                ElementInfo matchingOriginalElement = (ElementInfo)WindowExplorerDriver.GetMatchingElement(SelectedElement, SelectedPOM.GetUnifiedElementsList());

                if (matchingOriginalElement == null)
                {
                    WindowExplorerDriver.LearnElementInfoDetails(SelectedElement);
                    matchingOriginalElement = (ElementInfo)WindowExplorerDriver.GetMatchingElement(SelectedElement, SelectedPOM.GetUnifiedElementsList());
                }

                if (SelectedPOM.MappedUIElements.Contains(matchingOriginalElement) || SelectedPOM.UnMappedUIElements.Contains(matchingOriginalElement))
                {
                    PomDeltaUtils pomDeltaUtils = new PomDeltaUtils(SelectedPOM, Context.Agent);
                    pomDeltaUtils.KeepOriginalLocatorsOrderAndActivation = true;

                    /// Not Required but 
                    pomDeltaUtils.DeltaViewElements.Clear();

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
                        if (Reporter.ToUser(eUserMsgKey.UpdateExistingPOMElement, matchingOriginalElement.ElementName) == eUserMsgSelection.Yes)
                        {
                            /// Replace existing element with new one
                            /// Element exists in Mapped Elements list
                            if (originalItemIndex > -1)
                            {
                                SelectedPOM.MappedUIElements.RemoveAt(originalItemIndex);
                                SelectedPOM.MappedUIElements.Insert(originalItemIndex, pomDeltaUtils.DeltaViewElements[0].ElementInfo);
                            }
                            /// Element exists in Un-Mapped Elements list
                            /// We'll remove Element from Unmapped list and add it as new into Mapped Elements list
                            else
                            {
                                SelectedPOM.MappedUIElements.Add(pomDeltaUtils.DeltaViewElements[0].ElementInfo);
                                SelectedPOM.UnMappedUIElements.Remove(matchingOriginalElement);
                            }

                            POMElement = pomDeltaUtils.DeltaViewElements[0].ElementInfo;
                        }
                        else
                        {
                            if (originalItemIndex == -1)
                            {
                                SelectedPOM.MappedUIElements.Add(pomDeltaUtils.DeltaViewElements[0].ElementInfo);
                                SelectedPOM.UnMappedUIElements.Remove(matchingOriginalElement);

                                POMElement = pomDeltaUtils.DeltaViewElements[0].ElementInfo;
                            }
                            else
                            {
                                POMElement = matchingOriginalElement;
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
                else
                {
                    if (Reporter.ToUser(eUserMsgKey.POMElementNotExist, SelectedElement.ElementName, SelectedPOM.Name) == eUserMsgSelection.Yes)
                    {
                        POMBasedAction = true;
                        SelectedPOM.MappedUIElements.Add(SelectedElement);

                        POMElement = SelectedElement;
                        POMElement.ParentGuid = SelectedPOM.Guid;
                    }
                    else
                    {
                        POMElement = null;
                        POMBasedAction = false;
                    }
                }
            }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in ShowCurrentControlInfo", ex);
                Reporter.ToUser(eUserMsgKey.ObjectLoad);
            }
        }

        void UpdateElementActionTab()
        {
            SetPOMBasedChecks();

            ControlActionsPage_New CAP = null;

            ObservableList<Act> list = new ObservableList<Act>();
            ObservableList<ActInputValue> actInputValuelist = new ObservableList<ActInputValue>();

            //var elmentPresentationinfo = mPlatform.GetElementPresentatnInfo(EI);//type A (ActionType to show) || type B

            //if(Type A)
            //        { }
            //else if (Type b)
            //        {
            //    list = ((IWindowExplorerTreeItem)iv).GetElementActions(); 
            //}

            if (Platform.PlatformType().Equals(ePlatformType.Web) || (Platform.PlatformType().Equals(ePlatformType.Java) && !mSelectedElement.ElementType.Contains("JEditor")))
            {
                //TODO: J.G: Remove check for element type editor and handle it in generic way in all places
                list = Platform.GetPlatformElementActions(mSelectedElement);
            }
            else
            {                                                               // this "else" is temporary. Currently only ePlatformType.Web is overided
                list = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetElementActions();   // case will be removed once all platforms will be overrided
            }                                                               //

            ////If no element actions returned then no need to get locator's. 
            //if (list == null || list.Count == 0)
            //{
            //    SetActionsTabDesign(false);
            //    return;
            //}
            //else
            //{
                Page DataPage = mCurrentControlTreeViewItem.EditPage(Context);
                actInputValuelist = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetItemSpecificActionInputValues();

                if (mSelectedElement.Locators.CurrentItem == null)
                {
                mSelectedElement.Locators.CurrentItem = mSelectedElement.Locators[0];
                }

                ElementLocator eiLocator = mSelectedElement.Locators.CurrentItem as ElementLocator;

                string elementVal = string.Empty;
                if (mSelectedElement.OptionalValuesObjectsList.Count > 0)
                {
                    elementVal = Convert.ToString(mSelectedElement.OptionalValuesObjectsList.Where(v => v.IsDefault).FirstOrDefault().Value);
                }

                ElementActionCongifuration actConfigurations;
                if (POMBasedAction)
                {
                    //ElementActionCongifuration actionConfigurations = new ElementActionCongifuration
                    //{
                    //    LocateBy = eLocateBy.POMElement,
                    //    LocateValue = elementInfo.ParentGuid.ToString() + "_" + elementInfo.Guid.ToString(),
                    //    ElementValue = elementVal,
                    //    AddPOMToAction = true,
                    //    POMGuid = elementInfo.ParentGuid.ToString(),
                    //    ElementGuid = elementInfo.Guid.ToString(),
                    //    LearnedElementInfo = elementInfo,
                    //};
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
                        Type = mSelectedElement.ElementTypeEnum,
                        ElementValue = elementVal,
                        ElementGuid = mSelectedElement.Guid.ToString(),
                        LearnedElementInfo = mSelectedElement
                    };
                }

                CAP = new ControlActionsPage_New(WindowExplorerDriver, mSelectedElement, list, DataPage, actInputValuelist, Context, actConfigurations);
            //}

            if (CAP == null)
            {
                xAddActionTab.Visibility = Visibility.Collapsed;
                xActUIPageFrame.Visibility = Visibility.Collapsed;
            }
            else
            {
                xActUIPageFrame.Content = CAP;
                xAddActionTab.Visibility = Visibility.Visible;
                xActUIPageFrame.Visibility = Visibility.Visible;
            }

            mSelectedElementChanged = false;
        }

        public LocateByPOMElementPage locateByPOMElementPage;
        public ApplicationPOMModel SelectedPOM;
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
        }

        void ShowPOMSelection()
        {
            if (locateByPOMElementPage == null)
            {
                ActUIElement act = new ActUIElement();

                locateByPOMElementPage = new LocateByPOMElementPage(Context, act, nameof(ActUIElement.ElementType), act, nameof(ActUIElement.ElementLocateValue), true);
                locateByPOMElementPage.SelectPOM_Click(this, null);

                locateByPOMElementPage.POMChangedPageEvent += LocateByPOMElementPage_POMChangedPageEvent; ;

                SelectedPOM = locateByPOMElementPage.SelectedPOM;
                xPOMSelectionFrame.Content = locateByPOMElementPage;
            }

            xPOMSelectionFrame.Visibility = Visibility.Visible;
            POMSelectionPending = false;
        }

        private void LocateByPOMElementPage_POMChangedPageEvent()
        {
            SelectedPOM = locateByPOMElementPage.SelectedPOM;
        }

        private void xIntegratePOMChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            xPOMSelectionFrame.Visibility = Visibility.Collapsed;
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
    }
}
