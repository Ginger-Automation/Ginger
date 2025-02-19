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
using Amdocs.Ginger.Common.Repository.BusinessFlowLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.MoveToGingerWPF.Run_Set_Pages;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for RunnerItem.xaml
    /// </summary>
    public partial class RunnerItemPage : UserControl
    {
        public enum eRunnerItemType
        {
            BusinessFlow,
            Activity,
            Action
        }

        public event EventHandler<RunnerItemEventArgs> RunnerItemEvent;

        private readonly EventHandler<RunnerItemEventArgs>? _runnerItemEventHandler;

        public void OnRunnerItemEvent(RunnerItemEventArgs.eEventType eventType, RunnerItemPage runnerItemPage, eRunnerItemType runnerItemType, Object runnerItemObject)
        {
            EventHandler<RunnerItemEventArgs> handler = RunnerItemEvent;
            if (handler != null)
            {
                handler(sender: this, new RunnerItemEventArgs(eventType, runnerItemPage, runnerItemType, runnerItemObject));
            }
        }

        public Context Context { get; set; }

        ////TODO: why 2 events handler?
        //public delegate void SyncRunnerEventHandler(SyncRunnerItemEventArgs EventArgs);
        //public static event SyncRunnerEventHandler SyncRunnerItemEvent;

        public eRunnerItemType ItemtType
        {
            get
            {
                if (ItemObject.GetType() == typeof(GingerCore.BusinessFlow))
                {
                    return eRunnerItemType.BusinessFlow;
                }
                else if (ItemObject is Activity)
                {
                    return eRunnerItemType.Activity;
                }
                return eRunnerItemType.Action;
            }
        }

        public object ItemObject { get; set; }

        public string ItemName
        {
            get
            {
                return xItemName.Text.ToString();
            }
            set
            {
                xItemName.Text = value;
            }
        }

        public string ItemDescription
        {
            get
            {
                return xItemDescription.Text.ToString();
            }
            set
            {
                xItemDescription.Text = value;
            }
        }

        public Guid ItemGuid { get; set; }

        ObservableList<RunnerItemPage> mItemChilds = null;
        public ObservableList<RunnerItemPage> ChildItemPages
        {
            get
            {
                if (mItemChilds == null)
                {
                    LoadChildRunnerItems();
                }

                return mItemChilds;
            }
        }

        public string ItemTitleTooltip
        {
            get
            {
                return xItemName.ToolTip.ToString();
            }
            set
            {
                xItemName.ToolTip = value;
            }
        }


        public void ClearItemChilds()
        {
            if (mItemChilds != null)
            {
                for (int i = 0; i < mItemChilds.Count; i++)
                {
                    RunnerItemPage page = mItemChilds[i];
                    page.ClearBindings();
                    page = null;
                }
                //to make sure memory gets free
                mItemChilds.Clear();
                mItemChilds = null;
            }
        }

        public void LoadChildRunnerItems()
        {
            mItemChilds = [];

            if (ItemObject.GetType() == typeof(BusinessFlow))
            {
                foreach (Activity ac in ((BusinessFlow)ItemObject).Activities)
                {

                    RunnerItemPage ri = new RunnerItemPage(Runnerobj: ac, runnerItemEventHandler: _runnerItemEventHandler);
                    this.Context.BusinessFlow = (BusinessFlow)ItemObject;
                    ri.Context = this.Context;
                    ri.ItemName = ac.ActivityName;
                    if (string.IsNullOrEmpty(ac.Description))
                    {
                        ri.xItemSeparator.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ri.xItemSeparator.Visibility = Visibility.Visible;
                    }
                    ri.ItemDescription = ac.Description;
                    ri.ItemGuid = ac.Guid;
                    ri.xViewRunnerItem.Visibility = Visibility.Visible;
                    ri.xDetailView.ToolTip = "Expand / Collapse " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    mItemChilds.Add(ri);
                }
            }
            else if (ItemObject is Activity activity)
            {
                IEnumerable<GingerCore.Actions.Act> acts = ((Activity)ItemObject).Acts.OfType<GingerCore.Actions.Act>();
                if (acts != null)
                {

                    foreach (GingerCore.Actions.Act act in acts)
                    {
                        RunnerItemPage ri = new RunnerItemPage(Runnerobj: act, runnerItemEventHandler: _runnerItemEventHandler);
                        this.Context.Activity = activity;
                        ri.Context = this.Context;
                        act.Context = this.Context;
                        ri.xItemSeparator.Visibility = Visibility.Collapsed;
                        ri.ItemName = act.Description;
                        ri.ItemGuid = act.Guid;
                        ri.xViewRunnerItem.Visibility = Visibility.Visible;
                        ri.xDetailView.ToolTip = "Expand / Collapse Action";
                        mItemChilds.Add(ri);
                    }
                }
            }
        }

        public RunnerItemPage(object Runnerobj = null, bool ViewMode = false, EventHandler<RunnerItemEventArgs>? runnerItemEventHandler = null)
        {
            InitializeComponent();

            ItemObject = Runnerobj;
            _runnerItemEventHandler = runnerItemEventHandler;
            if (ItemObject != null)
            {
                if (ItemObject.GetType() == typeof(GingerCore.BusinessFlow))
                {
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatus, StatusItem.StatusProperty, ItemObject, nameof(BusinessFlow.RunStatus), BindingMode.OneWay);
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xBusinessflowActive, ucButton.ButtonImageTypeProperty, ItemObject, nameof(BusinessFlow.Active), bindingConvertor: new ActiveIconConverter(), BindingMode.TwoWay);
                    PropertyChangedEventManager.AddHandler(source: ((BusinessFlow)ItemObject), handler: RunnerItem_BusinessflowPropertyChanged, propertyName: allProperties);
                    xRunnerItemContinue.ToolTip = "Resume Run from this " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                    xViewRunnerItem.ToolTip = "View " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                }
                else if (ItemObject is Activity activity)
                {
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatus, StatusItem.StatusProperty, ItemObject, nameof(Activity.Status), BindingMode.OneWay);
                    PropertyChangedEventManager.AddHandler(source: activity, handler: RunnerItem_ActivityPropertyChanged, propertyName: allProperties);
                    xRunnerItemContinue.ToolTip = "Resume Run from this " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    xViewRunnerItem.ToolTip = "View " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    xRunnerItemMenu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatus, StatusItem.StatusProperty, ItemObject, nameof(Act.Status), BindingMode.OneWay);
                    PropertyChangedEventManager.AddHandler(source: ((Act)ItemObject), handler: RunnerItem_ActionPropertyChanged, propertyName: allProperties);
                    xRunnerItemContinue.ToolTip = "Resume Run from this Action";
                    xViewRunnerItem.ToolTip = "View Action";
                    xRunnerItemMenu.Visibility = Visibility.Collapsed;
                }
                if (ViewMode)
                {
                    pageGrid.IsEnabled = false;
                }
            }

            if (WorkSpace.Instance.RunningInExecutionMode)
            {
                xRunnerItemButtons.Visibility = Visibility.Collapsed;
                xRunnerItemMenu.Visibility = Visibility.Collapsed;
            }

            if (_runnerItemEventHandler != null)
            {
                WeakEventManager<RunnerItemPage, RunnerItemEventArgs>.RemoveHandler(source: this, eventName: nameof(RunnerItemEvent), handler: _runnerItemEventHandler);
                WeakEventManager<RunnerItemPage, RunnerItemEventArgs>.AddHandler(source: this, eventName: nameof(RunnerItemEvent), handler: _runnerItemEventHandler);
            }
        }

        private void RunnerItem_ActionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Act.Status))
            {
                //Todo : need to see why it is blocking activity,businessflow selection.
                if (((Act)sender).Status == eRunStatus.Running)
                {
                    OnRunnerItemEvent(RunnerItemEventArgs.eEventType.SetAsSelectedRequired, this, ItemtType, ItemObject);
                }
            }
        }

        private void RunnerItem_ActivityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Activity.Status))
            {
                if (((Activity)sender).Status == eRunStatus.Running)
                {
                    OnRunnerItemEvent(RunnerItemEventArgs.eEventType.SetAsSelectedRequired, this, ItemtType, ItemObject);
                }
            }
        }

        private void RunnerItem_BusinessflowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BusinessFlow.RunStatus))
            {
                if (((BusinessFlow)sender).RunStatus == eRunStatus.Running)
                {
                    OnRunnerItemEvent(RunnerItemEventArgs.eEventType.SetAsSelectedRequired, this, ItemtType, ItemObject);
                }
            }
        }

        public event RoutedEventHandler Click;
        private void xconfig_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
            if (e.Handled)
            {
                OnRunnerItemEvent(RunnerItemEventArgs.eEventType.ViewConfiguration, this, ItemtType, ItemObject);
            }
        }
        public event RoutedEventHandler ClickAutomate;
        private void xautomateBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (ClickAutomate != null)
            {
                ClickAutomate(this, e);
            }
        }
        public event RoutedEventHandler ClickGenerateReport;
        private void xGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (ClickGenerateReport != null)
            {
                ClickGenerateReport(this, e);
            }
        }
        public event RoutedEventHandler ClickRemove;
        private void xremove_Click(object sender, RoutedEventArgs e)
        {
            if (ClickRemove != null)
            {
                ClickRemove(this, e);
            }
        }


        private void xRunnerItemContinue_Click(object sender, RoutedEventArgs e)
        {
            OnRunnerItemEvent(RunnerItemEventArgs.eEventType.ContinueRunRequired, this, ItemtType, ItemObject);
        }

        public event RoutedEventHandler ClickActive;
        private void xBusinessflowActive_Click(object sender, RoutedEventArgs e)
        {
            if (ClickActive != null)
            {
                ClickActive(this, e);
            }
        }

        private void xViewRunnerItem_Click(object sender, RoutedEventArgs e)
        {
            OnRunnerItemEvent(RunnerItemEventArgs.eEventType.ViewRunnerItemRequired, this, ItemtType, ItemObject);
        }

        private void xDetailView_Click(object sender, RoutedEventArgs e)
        {
            if (pageGrid.RowDefinitions[1].Height.Value == 0)
            {
                ExpandCollapseRunnerItem(true);
            }
            else
            {
                ExpandCollapseRunnerItem(false);
            }
        }
        public void ExpandCollapseRunnerItem(bool isExpand)
        {
            if (isExpand)
            {
                pageGrid.RowDefinitions[1].Height = new GridLength(30);
                // xRunnerItemButtons.Visibility = Visibility.Visible;
                xDetailView.ButtonImageType = eImageType.Collapse;
            }
            else
            {
                pageGrid.RowDefinitions[1].Height = new GridLength(0);
                // xRunnerItemButtons.Visibility = Visibility.Collapsed;
                xDetailView.ButtonImageType = eImageType.Expand;
            }
        }
        public event RoutedEventHandler DuplicateClick;
        private void xDuplicateBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (DuplicateClick != null)
            {
                DuplicateClick(this, e);
            }
        }
        public event RoutedEventHandler RemoveClick;
        private void xremoveBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (RemoveClick != null)
            {
                RemoveClick(this, e);
            }
        }

        private void UserControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((RunnerItemPage)sender).ItemObject is Act or Activity)
            {
                OnRunnerItemEvent(RunnerItemEventArgs.eEventType.ViewRunnerItemRequired, this, ItemtType, ItemObject);
            }
            else
            {
                OnRunnerItemEvent(RunnerItemEventArgs.eEventType.ViewConfiguration, this, ItemtType, ItemObject);
            }
        }

        private void xExportToAlm_Click(object sender, RoutedEventArgs e)
        {
            if (ItemObject.GetType() == typeof(GingerCore.BusinessFlow))
            {
                ObservableList<BusinessFlow> bfs = [((BusinessFlow)ItemObject)];

                if (!ExportResultsToALMConfigPage.Instance.IsProcessing)
                {
                    if (ExportResultsToALMConfigPage.Instance.Init(bfs, new GingerCore.ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false)))
                    {
                        ExportResultsToALMConfigPage.Instance.ShowAsWindow();
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.ExportedExecDetailsToALMIsInProcess);
                }
            }
        }
        public event RoutedEventHandler ResetBusinessFlowStatus;
        private void xResetStatus_Buss_Flow_Actions_Click(object sender, RoutedEventArgs e)
        {
            if (ResetBusinessFlowStatus != null)
            {
                ResetBusinessFlowStatus(this, e);
            }
        }
        string allProperties = string.Empty;
        public void ClearBindings()
        {
            if (ItemObject is BusinessFlow)
            {
                PropertyChangedEventManager.RemoveHandler(source: ((BusinessFlow)ItemObject), handler: RunnerItem_BusinessflowPropertyChanged, propertyName: allProperties);
            }
            else if (ItemObject is Activity)
            {
                PropertyChangedEventManager.RemoveHandler(source: ((Activity)ItemObject), handler: RunnerItem_ActivityPropertyChanged, propertyName: allProperties);
            }
            else
            {
                PropertyChangedEventManager.RemoveHandler(source: ((Act)ItemObject), handler: RunnerItem_ActionPropertyChanged, propertyName: allProperties);
            }


            WeakEventManager<ucButton, RoutedEventArgs>.RemoveHandler(source: this.xDetailView, eventName: nameof(ucButton.Click), handler: xDetailView_Click);
            WeakEventManager<Control, MouseButtonEventArgs>.RemoveHandler(source: this, eventName: nameof(ucButton.MouseDoubleClick), handler: UserControl_MouseDoubleClick);
            WeakEventManager<ucButton, RoutedEventArgs>.RemoveHandler(source: this.xRunnerItemContinue, eventName: nameof(ucButton.Click), handler: xRunnerItemContinue_Click);
            WeakEventManager<MenuItem, RoutedEventArgs>.RemoveHandler(source: this.xDuplicateBusinessflow, eventName: nameof(MenuItem.Click), handler: xDuplicateBusinessflow_Click);
            WeakEventManager<MenuItem, RoutedEventArgs>.RemoveHandler(source: this.xautomateBusinessflow, eventName: nameof(MenuItem.Click), handler: xautomateBusinessflow_Click);
            WeakEventManager<MenuItem, RoutedEventArgs>.RemoveHandler(source: this.xGenerateReport, eventName: nameof(MenuItem.Click), handler: xGenerateReport_Click);
            WeakEventManager<MenuItem, RoutedEventArgs>.RemoveHandler(source: this.xExportToAlm, eventName: nameof(MenuItem.Click), handler: xExportToAlm_Click);
            WeakEventManager<MenuItem, RoutedEventArgs>.RemoveHandler(source: this.xResetStatus_Buss_Flow_Actions, eventName: nameof(MenuItem.Click), handler: xResetStatus_Buss_Flow_Actions_Click);
            WeakEventManager<ucButton, RoutedEventArgs>.RemoveHandler(source: this.xViewRunnerItem, eventName: nameof(ucButton.Click), handler: xViewRunnerItem_Click);
            WeakEventManager<ucButton, RoutedEventArgs>.RemoveHandler(source: this.xconfig, eventName: nameof(ucButton.Click), handler: xconfig_Click);
            WeakEventManager<ucButton, RoutedEventArgs>.RemoveHandler(source: this.xremoveBusinessflow, eventName: nameof(ucButton.Click), handler: xremoveBusinessflow_Click);
            WeakEventManager<ucButton, RoutedEventArgs>.RemoveHandler(source: this.xBusinessflowActive, eventName: nameof(ucButton.Click), handler: xBusinessflowActive_Click);
            WeakEventManager<ucButton, RoutedEventArgs>.RemoveHandler(source: this.xremoveBusinessflow, eventName: nameof(ucButton.Click), handler: xremoveBusinessflow_Click);
            WeakEventManager<RunnerItemPage, RoutedEventArgs>.RemoveHandler(source: this, eventName: nameof(RunnerItemPage.DuplicateClick), handler: xDuplicateBusinessflow_Click);
            WeakEventManager<RunnerItemPage, RoutedEventArgs>.RemoveHandler(source: this, eventName: nameof(RunnerItemPage.ClickAutomate), handler: xautomateBusinessflow_Click);
            WeakEventManager<RunnerItemPage, RoutedEventArgs>.RemoveHandler(source: this, eventName: nameof(RunnerItemPage.ClickGenerateReport), handler: xGenerateReport_Click);
            WeakEventManager<RunnerItemPage, RoutedEventArgs>.RemoveHandler(source: this, eventName: nameof(RunnerItemPage.ResetBusinessFlowStatus), handler: xResetStatus_Buss_Flow_Actions_Click);
            WeakEventManager<RunnerItemPage, RoutedEventArgs>.RemoveHandler(source: this, eventName: nameof(RunnerItemPage.Click), handler: xconfig_Click);
            WeakEventManager<RunnerItemPage, RoutedEventArgs>.RemoveHandler(source: this, eventName: nameof(RunnerItemPage.RemoveClick), handler: xremoveBusinessflow_Click);
            WeakEventManager<RunnerItemPage, RoutedEventArgs>.RemoveHandler(source: this, eventName: nameof(RunnerItemPage.ClickActive), handler: xBusinessflowActive_Click);



            BindingOperations.ClearAllBindings(NormalBorder);
            BindingOperations.ClearAllBindings(xDetailView);

            BindingOperations.ClearAllBindings(xStatus);
            //BindingOperations.ClearAllBindings(xStatusIcon);
            BindingOperations.ClearAllBindings(xItemName);
            BindingOperations.ClearAllBindings(xItemDescription);
            BindingOperations.ClearAllBindings(xItemSeparator);


            BindingOperations.ClearAllBindings(xRunnerItemContinue);
            BindingOperations.ClearAllBindings(xViewRunnerItem);
            BindingOperations.ClearAllBindings(xconfig);
            BindingOperations.ClearAllBindings(xremoveBusinessflow);
            BindingOperations.ClearAllBindings(xBusinessflowActive);
            BindingOperations.ClearAllBindings(xBorder);
            BindingOperations.ClearAllBindings(xItemSeparator);
            BindingOperations.ClearAllBindings(pageGrid);
            foreach (MenuItem extraOperation in xRunnerItemMenu.Items)
            {
                BindingOperations.ClearAllBindings(extraOperation);
            }
            //or
            BindingOperations.ClearAllBindings(xDuplicateBusinessflow);
            BindingOperations.ClearAllBindings(xautomateBusinessflow);
            BindingOperations.ClearAllBindings(xGenerateReport);
            BindingOperations.ClearAllBindings(xExportToAlm);
            BindingOperations.ClearAllBindings(xResetStatus_Buss_Flow_Actions);

            this.ClearControlsBindings();
        }

    }
    public class RunnerItemEventArgs : EventArgs
    {
        public enum eEventType
        {
            DoEventsRequired,
            ContinueRunRequired,
            SetAsSelectedRequired,
            ViewRunnerItemRequired,
            ViewConfiguration

        }

        public eEventType EventType;

        public RunnerItemPage RunnerItemPage;
        public RunnerItemPage.eRunnerItemType RunnerItemType;
        public Object RunnerItemObject;

        public RunnerItemEventArgs(eEventType EventType, RunnerItemPage runnerItemPage, RunnerItemPage.eRunnerItemType runnerItemType, object runnerItemObject)
        {
            this.EventType = EventType;
            this.RunnerItemPage = runnerItemPage;
            this.RunnerItemType = runnerItemType;
            this.RunnerItemObject = runnerItemObject;
        }
    }
}
