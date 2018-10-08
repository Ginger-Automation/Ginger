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
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ginger.Actions;
using Ginger.UserControls;
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using GingerCore.Platforms;
using Ginger.Reports;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.UIElement;
using GingerCore;

namespace Ginger.WindowExplorer
{
    /// <summary>
    /// Interaction logic for ControlActionsPage.xaml
    /// </summary>
    public partial class ControlActionsPage : Page
    {
        public Act mAction; // If we come here from EditAction page to update the locator
        public ObservableList<Act> mActions; // List of availble actions to choos from
        public ObservableList<ElementLocator> mLocators;
        private IWindowExplorer mWindowExplorerDriver;
        ElementInfo mElementInfo = null;
        Page mDataPage = null;
        double mLastDataGridRowHeight = 50;

        // when launching from Window explore we get also availble actions to choose so user can add
        public ControlActionsPage(IWindowExplorer driver, ElementInfo ElementInfo, ObservableList<Act> Actions, Page DataPage)
        {
            InitializeComponent();

            mElementInfo = ElementInfo;
            mWindowExplorerDriver = ElementInfo.WindowExplorer;
            mActions = Actions;
            mLocators = mWindowExplorerDriver.GetElementLocators(mElementInfo);
            mDataPage = DataPage;

            InitActionsGrid();
            InitLocatorsGrid();
            InitDataPage();
            AddToPageList();            
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

        // when launching from Action Edit Page we show only Locators to choose, and later on replcae LovBy/LocValue
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
            AddToPageList();
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

        private void AddToPageList()
        {
            ControlActionsPage tempPage = App.PageList.FirstOrDefault(Page => Page is ControlActionsPage) as ControlActionsPage;
            if (tempPage == null) App.PageList.Add(this);
            else
            {
                App.PageList.Remove(tempPage);
                App.PageList.Add(this);
            }
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (mActions.CurrentItem == null)
            {                
                Reporter.ToUser(eUserMsgKeys.AskToSelectAction);
                return;
            }

            Act act = (Act)((Act)(mActions.CurrentItem)).CreateCopy();          
            act.Active = true;
            act.AddNewReturnParams = true;
            ElementLocator EL = (ElementLocator)mLocators.CurrentItem;
            if ((mActions.CurrentItem).GetType() == typeof(ActUIElement))
            {
                ActUIElement aaa = (ActUIElement)mActions.CurrentItem;
                ActUIElement actUI = (ActUIElement)act;
                actUI.ElementLocateBy = EL.LocateBy;
                actUI.ElementLocateValue = EL.LocateValue;
                actUI.Value = ValueTextBox.Text;
                actUI.GetOrCreateInputParam(ActUIElement.Fields.ControlActionValue, ValueTextBox.Text);
                actUI.GetOrCreateInputParam(ActUIElement.Fields.ElementType, aaa.GetInputParamValue(ActUIElement.Fields.ElementType));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.ControlAction, aaa.GetInputParamValue(ActUIElement.Fields.ControlAction));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.ElementAction, aaa.GetInputParamValue(ActUIElement.Fields.ElementAction));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.WhereColSelector, aaa.GetInputParamValue(ActUIElement.Fields.WhereColSelector));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnTitle, aaa.GetInputParamValue(ActUIElement.Fields.WhereColumnTitle));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnValue, aaa.GetInputParamValue(ActUIElement.Fields.WhereColumnValue));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.WhereOperator, aaa.GetInputParamValue(ActUIElement.Fields.WhereOperator));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.WhereProperty, aaa.GetInputParamValue(ActUIElement.Fields.WhereProperty));
                act = actUI;
            }
            else
            {                
                act.LocateBy = EL.LocateBy;
                act.LocateValue = EL.LocateValue;
                act.Value = ValueTextBox.Text;
            }
            App.BusinessFlow.AddAct(act);

            int selectedActIndex = -1;
            ObservableList<Act> actsList = App.BusinessFlow.CurrentActivity.Acts;
            if (actsList.CurrentItem != null)
            {
                selectedActIndex = actsList.IndexOf((Act)actsList.CurrentItem);
            }
            if (selectedActIndex >= 0)
            {
                actsList.Move(actsList.Count - 1, selectedActIndex + 1);
            }
            ActionEditPage AEP = new ActionEditPage(act);
            AEP.ShowAsWindow();
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
            
            // Copy from the selected Locator
            ElementLocator EL = (ElementLocator)mLocators.CurrentItem;
            act.AddNewReturnParams = true;
            act.Active = true;
            if ((mActions.CurrentItem).GetType() == typeof(ActUIElement))
            {
                ActUIElement aaa = (ActUIElement)mActions.CurrentItem;
                ActUIElement actUI = (ActUIElement)act;
                actUI.ElementLocateBy = EL.LocateBy;
                actUI.ElementLocateValue = EL.LocateValue;
                actUI.Value = ValueTextBox.Text;
                actUI.GetOrCreateInputParam(ActUIElement.Fields.ControlActionValue, ValueTextBox.Text);
                actUI.GetOrCreateInputParam(ActUIElement.Fields.ElementType, aaa.GetInputParamValue(ActUIElement.Fields.ElementType));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.ControlAction, aaa.GetInputParamValue(ActUIElement.Fields.ControlAction));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.ElementAction, aaa.GetInputParamValue(ActUIElement.Fields.ElementAction));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.WhereColSelector, aaa.GetInputParamValue(ActUIElement.Fields.WhereColSelector));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnTitle, aaa.GetInputParamValue(ActUIElement.Fields.WhereColumnTitle));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnValue, aaa.GetInputParamValue(ActUIElement.Fields.WhereColumnValue));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.WhereOperator, aaa.GetInputParamValue(ActUIElement.Fields.WhereOperator));
                actUI.GetOrCreateInputParam(ActUIElement.Fields.WhereProperty, aaa.GetInputParamValue(ActUIElement.Fields.WhereProperty));
                act = actUI;
            }
            else
            {
                act.LocateBy = EL.LocateBy;
                act.LocateValueCalculated = EL.LocateValue;
                act.Value = ValueTextBox.Text;
            }

            App.AutomateTabGingerRunner.PrepActionVE(act);
            ApplicationAgent ag = App.AutomateTabGingerRunner.ApplicationAgents.Where(x => x.AppName == App.BusinessFlow.CurrentActivity.TargetApplication).FirstOrDefault();
            if (ag != null)
            {
                App.AutomateTabGingerRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;
                ag.Agent.RunAction(act);
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