using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Expressions;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowsLibNew.AddActionMenu;
using Ginger.BusinessFlowWindows;
using Ginger.Drivers;
using Ginger.Reports;
using Ginger.UserControls;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
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
using static Ginger.ExecuterService.Contracts.V1.ExecuterAgent.AgentApiRoutes;

namespace Ginger.WindowExplorer
{
    /// <summary>
    /// Interaction logic for ControlActionsPage_New.xaml
    /// </summary>
    public partial class ControlActionsPage_New : Page
    {
        public ControlActionsPage_New()
        {
            InitializeComponent();
        }

        public Act mAction; // If we come here from EditAction page to update the locator
        public ObservableList<Act> mActions; // List of available actions to choose from
        public ObservableList<ElementLocator> mLocators;
        private IWindowExplorer mWindowExplorerDriver;
        private ObservableList<ActInputValue> mActInputValues;
        PlatformInfoBase mPlatform { get; set; }
        ElementInfo mElementInfo = null;
        ITreeViewItem mCurrentControlTreeViewItem;
        Page mDataPage = null;
        double mLastDataGridRowHeight = 50;
        Context mContext;
        Page actEditPage;
        bool IsLegacyPlatform = false;

        // when launching from Window explore we get also available actions to choose so user can add
        public ControlActionsPage_New(IWindowExplorer driver, ElementInfo ElementInfo, Context context, ElementActionCongifuration actionConfigurations, ITreeViewItem CurrentControlTreeViewItem, PlatformInfoBase PlatformInfo)
        {
            InitializeComponent();

            mElementInfo = ElementInfo;
            mWindowExplorerDriver = driver;
            mLocators = mElementInfo.Locators;  // mWindowExplorerDriver.GetElementLocators(mElementInfo);
            mContext = context;
            mCurrentControlTreeViewItem = CurrentControlTreeViewItem;
            mPlatform = PlatformInfo;

            mAction = (mPlatform as IPlatformInfo).GetPlatformAction(mElementInfo, actionConfigurations);

            IsLegacyPlatform = mAction == null;

            mContext.Runner.PropertyChanged += Runner_PropertyChanged;
            SetPlatformBasedUIUpdates();

            //mAction.PropertyChanged -= Action_PropertyChanged;
            //mAction.PropertyChanged += Action_PropertyChanged;
        }

        private void InitOutputValuesGrid()
        {
            GridViewDef SimView = new GridViewDef(eGridView.All.ToString());
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            SimView.GridColsView = viewCols;

            //Simulation view
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Active, WidthWeight = 50, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Param, Header = "Parameter", WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Path, WidthWeight = 100 });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Actual, Header = "Return Value", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            xOutputValuesGrid.SetAllColumnsDefaultView(SimView);
            xOutputValuesGrid.InitViewItems();

            xOutputValuesGrid.AddSeparator();

            xOutputValuesGrid.ShowViewCombo = Visibility.Collapsed;
            xOutputValuesGrid.ShowEdit = Visibility.Collapsed;

            xOutputValuesGrid.AllowHorizentalScroll = true;
            xOutputValuesGrid.Grid.MaxHeight = 500;
        }

        private void Action_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Act.Status))
            {
                this.Dispatcher.Invoke(() =>
                {
                    xExecutionStatusIcon.Status = mAction.Status.Value;

                    if (mAction.Status.Value == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running)
                    {
                        xRunActBtn.IsEnabled = false;
                        xRunActBtn.Opacity = 0.4;
                    }
                    else
                    {
                        xRunActBtn.IsEnabled = true;
                        xRunActBtn.Opacity = 1;
                    }

                    string returnVals = GetAllRetVals(mAction);

                    if (!string.IsNullOrEmpty(mAction.Error))
                    {
                        xErrorTxtBlock.Visibility = Visibility.Visible;
                        xErrorTxtBlock.Text += mAction.Error;
                    }
                    else
                    {
                        xErrorTxtBlock.Visibility = Visibility.Collapsed;
                        xErrorTxtBlock.Text = "Error : ";
                    }

                    if (string.IsNullOrEmpty(mAction.ExInfo) == false)
                    {
                        xExecInfoTxtBlock.Visibility = Visibility.Visible;
                        xExecInfoTxtBlock.Text += mAction.ExInfo;
                    }
                    else
                    {
                        xExecInfoTxtBlock.Visibility = Visibility.Collapsed;
                        xExecInfoTxtBlock.Text = "Execution Info : ";
                    }

                    if (!string.IsNullOrEmpty(returnVals))
                    {
                        xExecInfoTxtBlock.Visibility = Visibility.Visible;
                        xExecInfoTxtBlock.Text += Environment.NewLine + "Return Values : " + returnVals;
                    }
                    else
                    {

                    }
                });
            }

            if (e.PropertyName == nameof(Act.ReturnValues))
            {
            }
        }

        void SetPlatformBasedUIUpdates()
        {
            if (IsLegacyPlatform)
            {
                if (mPlatform.PlatformType().Equals(ePlatformType.Web) || (mPlatform.PlatformType().Equals(ePlatformType.Java) && !mElementInfo.ElementType.Contains("JEditor")))
                {
                    //TODO: J.G: Remove check for element type editor and handle it in generic way in all places
                    mActions = mPlatform.GetPlatformElementActions(mElementInfo);
                }
                else
                {                                                               // this "else" is temporary. Currently only ePlatformType.Web is overided
                    mActions = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetElementActions();   // case will be removed once all platforms will be overrided
                }

                mDataPage = mCurrentControlTreeViewItem.EditPage(mContext);

                mActInputValues = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetItemSpecificActionInputValues();

                if (mActions.CurrentItem == null && mActions.Count > 0)
                {
                    mActions.CurrentItem = mActions[0];
                }

                mAction = (Act)mActions.CurrentItem;
                xActEditPageFrame.Visibility = Visibility.Collapsed;

                xOperationsScrollView.Visibility = Visibility.Visible;

                InitActionsGrid();
                InitLocatorsGrid();

                InitOutputValuesGrid();

                BindingHandler.ObjFieldBinding(xExecutionStatusIcon, UcItemExecutionStatus.StatusProperty, mAction, nameof(Act.Status));
                BindingHandler.ObjFieldBinding(xErrorTxtBlock, TextBlock.TextProperty, mAction, nameof(Act.Error));
                BindingHandler.ObjFieldBinding(xExecInfoTxtBlock, TextBlock.TextProperty, mAction, nameof(Act.ExInfo));
                BindingHandler.ObjFieldBinding(xOutputValuesGrid, DataGrid.ItemsSourceProperty, mAction, nameof(Act.ReturnValues));
                BindingHandler.ObjFieldBinding(xOutputValuesGrid, IsVisibleProperty, mAction, nameof(Act.ReturnValues), new OutPutValuesCountConverter());
                //BindingHandler.ObjFieldBinding(xActExecutionDetails, Expander.IsExpandedProperty, mAction, Convert.ToString(mAction.Status.Value == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed || mAction.Status.Value == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed), new CheckboxConfigConverter());
            }
            else
            {
                mAction.Context = mContext;
                SetActionDetails(mAction);
                actEditPage = new ActionEditPage(mAction, General.eRIPageViewMode.Explorer);

                xActEditPageFrame.Visibility = Visibility.Visible;
                xActEditPageFrame.Content = actEditPage;

                xOperationsScrollView.Visibility = Visibility.Collapsed;
            }

            InitDataPage();
        }

        private void Runner_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Run.GingerRunner.IsRunning))
            {
                if (mContext.Runner.IsRunning == false)
                {
                    WindowExplorerCommon.IsTestActionRunning = mContext.Runner.IsRunning;
                }
            }
        }

        private void InitDataPage()
        {
            if (mDataPage != null)
            {
                DataFrame.Visibility = System.Windows.Visibility.Visible;
                xDataFrameRow.Height = new GridLength(mLastDataGridRowHeight, GridUnitType.Star);
                xDataFrameSplitter.Visibility = System.Windows.Visibility.Visible;
                xDataFrameExpander.Visibility = System.Windows.Visibility.Visible;
                DataFrame.Content = mDataPage;
            }
            else
            {
                mLastDataGridRowHeight = xDataFrameRow.Height.Value;
                DataFrame.Visibility = System.Windows.Visibility.Collapsed;
                xDataFrameRow.Height = new GridLength(0);
                xDataFrameSplitter.Visibility = System.Windows.Visibility.Collapsed;
                xDataFrameExpander.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        // when launching from Action Edit Page we show only Locator's to choose, and later on replace LovBy/LocValue
        //public ControlActionsPage_New(Act Act, ElementInfo EI)
        //{
        //    InitializeComponent();
        //    mAction = Act;
        //    if (mAction == null)
        //    {
        //        //SelectLocatorButton.Visibility = System.Windows.Visibility.Collapsed;
        //    }
        //    mLocators = EI.GetElementLocators();

        //    //Hide actions grid and show current action info
        //    //AvailableControlActionsGrid.Visibility = System.Windows.Visibility.Collapsed;
        //    CurrentActionInfoStackPanel.Visibility = System.Windows.Visibility.Visible;
        //    SelectedActionDescriptionTextBox.Text = mAction.Description;
        //    SelectedActionLocateByTextBox.Text = mAction.LocateBy.ToString();
        //    SelectedActionLocateValueTextBox.Text = mAction.LocateValue;
        //    xAddActBtn.Visibility = System.Windows.Visibility.Collapsed;

        //    //InitLocatorsGrid();
        //}

        private void InitLocatorsGrid()
        {
            xDDLocateBy.ItemsSource = mLocators.Select(l => l.LocateBy).ToList();

            if (mLocators.CurrentItem == null)
                mLocators.CurrentItem = mElementInfo.Locators.CurrentItem;

            xDDLocateBy.SelectedItem = ((ElementLocator)mLocators.CurrentItem).LocateBy;

            //TODO: need to add Help text or convert to icon...
            //AvailableLocatorsGrid.AddButton("Test", TestSelectedLocator);
            //AvailableLocatorsGrid.AddButton("Test All", TestAllLocators);

            //GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            //defView.GridColsView = new ObservableList<GridColView>();
            //defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), WidthWeight = 10 });
            //defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), WidthWeight = 30 });
            //defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 20 });
            //defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Count), WidthWeight = 10 });

            //AvailableLocatorsGrid.SetAllColumnsDefaultView(defView);
            //AvailableLocatorsGrid.InitViewItems();
            //AvailableLocatorsGrid.DataSourceList = mLocators;
            //AvailableLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void TestAllLocators(object sender, RoutedEventArgs e)
        {
            foreach (ElementLocator EL in mLocators)
            {
                ObservableList<ElementInfo> list = mWindowExplorerDriver.GetElements(EL);
                EL.Count = list.Count;
            }
        }

        private void TestSelectedLocator(object sender, RoutedEventArgs e)
        {
            ElementLocator EL = (ElementLocator)mLocators.CurrentItem;
            ObservableList<ElementInfo> list = mWindowExplorerDriver.GetElements(EL);
            EL.Count = list.Count;
        }

        private void InitActionsGrid()
        {
            xDDActions.ItemsSource = mActions;
            xDDActions.SelectedItem = mActions.CurrentItem;
        }


        private void xAddActBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mActions.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectAction);
                return;
            }

            Act selectedAct = mActions.CurrentItem as Act;
            SetActionDetails(selectedAct);
            ActionsFactory.AddActionsHandler(selectedAct, mContext);
        }

        private Act SetActionDetails(Act act)
        {
            act.Active = true;
            act.AddNewReturnParams = true;
            act.Value = xValueTextBox.Text;
            //Set action unique input values
            if (mActInputValues != null)
            {
                foreach (ActInputValue iv in mActInputValues)
                {
                    if (iv.Value != null)
                    {
                        act.AddOrUpdateInputParamValue(iv.Param, iv.Value);
                    }

                }
            }

            ElementLocator EL = (ElementLocator)mLocators.CurrentItem;

            if (EL == null)
            {
                EL = (ElementLocator)mElementInfo.Locators.CurrentItem;
            }

            if (mAction.GetType() == typeof(ActUIElement))
            {
                //Set UIElement action locator
                ActUIElement actUI = (ActUIElement)act;

                if (EL != null && actUI.ElementLocateBy != eLocateBy.POMElement)
                {
                    actUI.ElementLocateBy = EL.LocateBy;
                    actUI.ElementLocateValue = EL.LocateValue;
                }

                //TODO: Remove below  if once one of the field from Value and Value to select is removed
                if (actUI.ElementAction == ActUIElement.eElementAction.Click
                    || actUI.ElementAction == ActUIElement.eElementAction.Select
                    || actUI.ElementAction == ActUIElement.eElementAction.GetControlProperty
                    || actUI.ElementAction == ActUIElement.eElementAction.AsyncSelect
                    || actUI.ElementAction == ActUIElement.eElementAction.SelectByIndex)
                {
                    actUI.AddOrUpdateInputParamValue(ActUIElement.Fields.ValueToSelect, act.Value);
                }
                else if (actUI.ElementAction.Equals(ActUIElement.eElementAction.TableCellAction))
                {
                    actUI.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlActionValue, act.Value);
                }

                actUI.AddOrUpdateInputParamValue(ActUIElement.Fields.ElementType, actUI.ElementType.ToString());

                act = actUI;
            }
            else if (mAction.GetType() == typeof(ActBrowserElement))
            {
                //Set UIElement action locator
                ActBrowserElement actBrowser = (ActBrowserElement)act;

                if (EL != null && actBrowser.LocateBy != eLocateBy.POMElement)
                {
                    actBrowser.LocateBy = EL.LocateBy;
                    actBrowser.LocateValue = EL.LocateValue;
                }

            }
            else
            {
                //Set action locator
                act.LocateBy = EL.LocateBy;
                act.LocateValue = EL.LocateValue;
            }
            return act;
        }

        private void xRunActBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mAction == null)
            {
                mAction = (Act)mActions.CurrentItem;
            }

            SetActionDetails(mAction);

            WindowExplorerCommon.IsTestActionRunning = true;

            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.RunCurrentAction, new Tuple<Activity, Act, bool>(null, mAction, true));

            //mContext.Runner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;
            //mContext.Runner.PrepActionValueExpression(mAction);

            //ApplicationAgent ag = (ApplicationAgent)mContext.Runner.ApplicationAgents.Where(x => x.AppName == mContext.BusinessFlow.CurrentActivity.TargetApplication).FirstOrDefault();
            //if (ag != null)
            //{
            //    mContext.Runner.RunAction(mAction);
            //    mContext.Agent.RunAction(act);
            //}

            //mContext.Agent.RunAction(mAction);
        }

        private string GetAllRetVals(Act act)
        {
            string retval = null;
            foreach (ActReturnValue ARV in act.ReturnValues)
            {
                if (!string.IsNullOrEmpty(retval))
                {
                    retval += ", ";
                }
                retval += ARV.Param + "=" + ARV.Actual;
            }

            return retval;
        }

        private void AvailableControlActionsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            if (mActions.CurrentItem == null) return;

            string ActValue = ((Act)mActions.CurrentItem).Value;
            if (!string.IsNullOrEmpty(ActValue))
            {
                xValueTextBox.Text = ActValue;
            }
        }

        private void SelectLocatorButton_Click(object sender, RoutedEventArgs e)
        {
            //Copy the selected locator to the action Locator we edit
            ElementLocator EL = (ElementLocator)mLocators.CurrentItem;
            mAction.LocateBy = EL.LocateBy;
            mAction.LocateValue = EL.LocateValue;
        }

        private void xDDActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xDDActions.SelectedItem == null)
                return;
            else
                mActions.CurrentItem = xDDActions.SelectedItem;

            string ActValue = ((Act)mActions.CurrentItem).Value;
            if (!string.IsNullOrEmpty(ActValue))
            {
                xValueTextBox.Text = ActValue;
            }
        }

        private void xDDLocateBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mLocators.CurrentItem = mLocators.Where(l => l.LocateBy == (eLocateBy)xDDLocateBy.SelectedItem).FirstOrDefault();
            mElementInfo.Locators.CurrentItem = mLocators.CurrentItem;

            xLocateValueTxtBlock.Text = (mElementInfo.Locators.CurrentItem as ElementLocator).LocateValue;
        }

        private void ControlsViewsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            xDataFrameRow.Height = new GridLength(200, GridUnitType.Star);
            xDataFrameRow.MaxHeight = Double.PositiveInfinity;
        }

        private void ControlsViewsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            xDataFrameRow.Height = new GridLength(35);
            xDataFrameRow.MaxHeight = 35;
        }
    }
}
