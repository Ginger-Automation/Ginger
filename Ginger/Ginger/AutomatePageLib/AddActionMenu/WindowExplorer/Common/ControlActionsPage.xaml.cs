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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowPages;
using Ginger.Reports;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Platforms;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.WindowExplorer
{
    /// <summary>
    /// Interaction logic for ControlActionsPage.xaml
    /// </summary>
    public partial class ControlActionsPage : Page
    {
        public Act mAction; // If we come here from EditAction page to update the locator
        public ObservableList<Act> mActions; // List of available actions to choose from
        public ObservableList<ElementLocator> mLocators;
        private IWindowExplorer mWindowExplorerDriver;        
        private ObservableList<ActInputValue> mActInputValues;
        ElementInfo mElementInfo = null;        
        Page mDataPage = null;
        double mLastDataGridRowHeight = 50;
        Context mContext;

        // when launching from Window explore we get also available actions to choose so user can add
        public ControlActionsPage(IWindowExplorer driver, ElementInfo ElementInfo, ObservableList<Act> Actions, Page DataPage, ObservableList<ActInputValue> actInputValues, Context context)
        {
            InitializeComponent();

            mElementInfo = ElementInfo;
            mWindowExplorerDriver = ElementInfo.WindowExplorer;         
            mActInputValues = actInputValues;
            mActions = Actions;
            mLocators = mWindowExplorerDriver.GetElementLocators(mElementInfo);
            mDataPage = DataPage;
            mContext = context;

            InitActionsGrid();
            InitLocatorsGrid();
            InitDataPage();
             
            SelectLocatorButton.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void InitDataPage()
        {
            if (mDataPage != null)
            {
                DataFrame.Visibility = System.Windows.Visibility.Visible;
                DataFrameRow.Height = new GridLength(mLastDataGridRowHeight, GridUnitType.Star);
                DataFrameSplitter.Visibility = System.Windows.Visibility.Visible;
                DataFrameScrollViewer.Visibility = System.Windows.Visibility.Visible;
                DataFrame.Content = mDataPage;
            }
            else
            {
                mLastDataGridRowHeight = DataFrameRow.Height.Value;
                DataFrame.Visibility = System.Windows.Visibility.Collapsed;
                DataFrameRow.Height = new GridLength(0);
                DataFrameSplitter.Visibility = System.Windows.Visibility.Collapsed;
                DataFrameScrollViewer.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        // when launching from Action Edit Page we show only Locator's to choose, and later on replace LovBy/LocValue
        public ControlActionsPage(Act Act, ElementInfo EI)
        {
            InitializeComponent();
            mAction = Act;
            if (mAction==null)
            {
                SelectLocatorButton.Visibility = System.Windows.Visibility.Collapsed;
            }
            mLocators = EI.GetElementLocators();

            //Hide actions grid and show current action info
            AvailableControlActionsGrid.Visibility = System.Windows.Visibility.Collapsed;
            CurrentActionInfoStackPanel.Visibility = System.Windows.Visibility.Visible;
            SelectedActionDescriptionTextBox.Text = mAction.Description;
            SelectedActionLocateByTextBox.Text = mAction.LocateBy.ToString();
            SelectedActionLocateValueTextBox.Text = mAction.LocateValue;
            AddActionButton.Visibility = System.Windows.Visibility.Collapsed;

            InitLocatorsGrid();
        }

        private void InitLocatorsGrid()
        {
            //TODO: need to add Help text or convert to icon...
            AvailableLocatorsGrid.AddButton("Test", TestSelectedLocator);
            AvailableLocatorsGrid.AddButton("Test All", TestAllLocators);

            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), WidthWeight = 10 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), WidthWeight = 30 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 20 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Count), WidthWeight = 10 });

            AvailableLocatorsGrid.SetAllColumnsDefaultView(defView);
            AvailableLocatorsGrid.InitViewItems();
            AvailableLocatorsGrid.DataSourceList = mLocators;
            AvailableLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
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
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = Act.Fields.Description, WidthWeight = 10 });
            AvailableControlActionsGrid.SetAllColumnsDefaultView(defView);
            AvailableControlActionsGrid.InitViewItems();
            AvailableControlActionsGrid.DataSourceList = mActions;
            AvailableControlActionsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }


        private void AddActionButton_Click(object sender, RoutedEventArgs e)
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
            act.Value = ValueTextBox.Text;
            //Set action unique input values
            if (mActInputValues != null)
            {
                foreach (ActInputValue iv in mActInputValues)
                {
                    if(iv.Value != null)
                    {
                        act.AddOrUpdateInputParamValue(iv.Param, iv.Value);
                    }
                    
                }
            }

            ElementLocator EL = (ElementLocator)mLocators.CurrentItem;

            if ((mActions.CurrentItem).GetType() == typeof(ActUIElement))
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
                else if(actUI.ElementAction.Equals(ActUIElement.eElementAction.TableCellAction))
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

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            TestStatusTextBlock.Text = "Executing";
            //TODO: do events?
            Act act;
            //We came from ActionEditPage 
            if (mAction != null)
            {
                act = (Act)((Act)(mAction)).CreateCopy();
            }
            else
            {
                act = (Act)((Act)(mActions.CurrentItem)).CreateCopy();
            }

            SetActionDetails(act);
            mContext.Runner.PrepActionValueExpression(act);
            ApplicationAgent ag =(ApplicationAgent)mContext.Runner.ApplicationAgents.Where(x => x.AppName == mContext.BusinessFlow.CurrentActivity.TargetApplication).FirstOrDefault();
            if (ag != null)
            {
                mContext.Runner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;
               ((Agent) ag.Agent).RunAction(act);
            }
            
            TestStatusTextBlock.Text = string.Empty;
            if (act.Status != null)
                TestStatusTextBlock.Text += act.Status + System.Environment.NewLine;
            if (string.IsNullOrEmpty(act.Error) == false)
                TestStatusTextBlock.Text += act.Error + System.Environment.NewLine;
            if (string.IsNullOrEmpty(act.ExInfo) == false)
                TestStatusTextBlock.Text += act.ExInfo + System.Environment.NewLine;

            string retval = GetAllRetVals(act);
            if (retval != null)
            {
                TestStatusTextBlock.Text += retval + System.Environment.NewLine;
            }
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
                 ValueTextBox.Text = ActValue;
             }
        }

        private void SelectLocatorButton_Click(object sender, RoutedEventArgs e)
        {
            //Copy the selected locator to the action Locator we edit
            ElementLocator EL = (ElementLocator)mLocators.CurrentItem;                        
            mAction.LocateBy = EL.LocateBy;
            mAction.LocateValue = EL.LocateValue;
        }
    }
}