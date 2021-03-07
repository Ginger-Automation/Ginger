using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowsLibNew.AddActionMenu;
using Ginger.Reports;
using Ginger.UserControls;
using Ginger.WindowExplorer;
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

        //private ApplicationAgent mApplicationAgent { get; set; }
        //public ApplicationAgent AppAgent { get; set; }
        //{
        //    get
        //    {
        //        return mApplicationAgent;
        //    }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            mApplicationAgent = value;
        //            mAgent = mApplicationAgent.Agent;
        //        }
        //    }
        //}

        public bool ShowTestBtn { get; set; }

        PlatformInfoBase mPlatform { get; set; }

        public bool ShowHelpColumn { get; set; }

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
                    RefreshPropertiesAndLocators();
                    RefreshElementAction();
                }
            }
        }

        ITreeViewItem mCurrentControlTreeViewItem;
        private ObservableList<ActInputValue> mActInputValues;

        private void RefreshElementAction()
        {
            mCurrentControlTreeViewItem = WindowExplorerCommon.GetTreeViewItemForElementInfo(SelectedElement);
            try
            {
                mActInputValues = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetItemSpecificActionInputValues();
            }
            catch(Exception exc)
            {
                Reporter.ToLog(eLogLevel.ERROR, exc.Message, exc);
            }
            //UIElementAction.ElementType = SelectedElement.ElementTypeEnum;
            //UIElementAction.ElementLocateBy = eLocateBy.ByXPath;

            //UIElementAction.LocateBy = (SelectedElement.Locators.CurrentItem as ElementLocator).LocateBy;
            //UIElementAction.LocateValue = (SelectedElement.Locators.CurrentItem as ElementLocator).LocateValue;

            //UIElementAction.ElementLocateValue = SelectedElement.XPath;
            //UIElementAction.Platform = Context.Agent.Platform;  // AppAgent.Agent.Platform;

            //actEditPage = new ActUIElementEditPage(UIElementAction);

            //actEditPage.xLocateByLbl.Visibility = Visibility.Collapsed;
            //actEditPage.ElementLocateByComboBox.Visibility = Visibility.Collapsed;
            //actEditPage.LocateValueLable.Visibility = Visibility.Collapsed;
            //actEditPage.LocateValueEditFrame.Visibility = Visibility.Collapsed;

            ////actEditPage.xLocateByCombo.Items.Clear();
            ////actEditPage.xLocateByCombo.ItemsSource = SelectedElement.Locators.Select(l => l.LocateBy);
            ////actEditPage.xLocateValueVE.ValueTextBox.IsEnabled = false;

            //xActUIPageFrame.Content = actEditPage;

            xExecutionStatusIcon.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
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

        //private void xRunActBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    xExecutionStatusIcon.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;

        //    Act act;
        //    //We came from ActionEditPage 
        //    if (UIElementAction != null)
        //    {
        //        act = (Act)UIElementAction.CreateCopy();     // (Act)((Act)(mAction)).CreateCopy();

        //        SetActionDetails(act);

        //        Context.Runner.PrepActionValueExpression(act);
        //        ApplicationAgent ag = (ApplicationAgent)Context.Runner.ApplicationAgents.Where(x => x.AppName == Context.BusinessFlow.CurrentActivity.TargetApplication).FirstOrDefault();
        //        if (ag != null)
        //        {
        //            Context.Runner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;
        //            ((Agent)ag.Agent).RunAction(act);
        //        }

        //        if (act.Status != null)
        //            xExecutionStatusIcon.Status = act.Status.Value;
        //        else
        //            xExecutionStatusIcon.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
        //    }

        //}

        private Act SetActionDetails(Act act)
        {
            act.Active = true;
            act.AddNewReturnParams = true;
            //act.Value = UIElementAction.Value;  // ActEditPage.text ValueTextBox.Text;
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

            ElementLocator EL = (ElementLocator)SelectedElement.Locators.CurrentItem;

            if (act.GetType() == typeof(ActUIElement))
            {
                //Set UIElement action locator
                ActUIElement actUI = (ActUIElement)act;
                actUI.ElementLocateBy = EL.LocateBy;
                actUI.ElementLocateValue = EL.LocateValue;
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

                act = actUI;
            }
            else
            {
                //Set action locator
                act.LocateBy = EL.LocateBy;
                act.LocateValue = EL.LocateValue;
            }
            return act;
        }

        private void xAddActBtn_Click(object sender, RoutedEventArgs e)
        {
            //SetActionDetails(UIElementAction);
            //ActionsFactory.AddActionsHandler(UIElementAction, Context);
        }

        internal void InitGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 65 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, ReadOnly = true });

            List<ComboEnumItem> locateByList = GetPlatformLocatByList();

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, });

            xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xLocatorsGrid.InitViewItems();
        }

        private List<ComboEnumItem> GetPlatformLocatByList()
        {
            List<ComboEnumItem> locateByComboItemList = new List<ComboEnumItem>();

            List<eLocateBy> platformLocateByList = mPlatform.GetPlatformUIElementLocatorsList();

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
    }
}
