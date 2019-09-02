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
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.UserControls;
using Ginger.MoveToGingerWPF.Run_Set_Pages;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

        public delegate void RunnerItemEventHandler(RunnerItemEventArgs EventArgs);
        private static event RunnerItemEventHandler RunnerItemEvent;
        public void OnRunnerItemEvent(RunnerItemEventArgs.eEventType eventType, RunnerItemPage runnerItemPage, eRunnerItemType runnerItemType, Object runnerItemObject)
        {
            RunnerItemEventHandler handler = RunnerItemEvent;
            if (handler != null)
            {
                handler(new RunnerItemEventArgs(eventType, runnerItemPage, runnerItemType, runnerItemObject));
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
                else if (ItemObject.GetType() == typeof(GingerCore.Activity))
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
                    LoadChildRunnerItems();
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

        public static void SetRunnerItemEvent(RunnerItemEventHandler runnerItemEvent)
        {
            if(RunnerItemEvent == null)
            {
                RunnerItemEvent -= runnerItemEvent;
                RunnerItemEvent += runnerItemEvent;
            }
        }

        public void ClearItemChilds()
        {
            if (mItemChilds != null)
            {
                for (int i = 0; i < mItemChilds.Count; i++)
                {
                    RunnerItemPage page = (RunnerItemPage)mItemChilds[i];
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
            mItemChilds = new ObservableList<RunnerItemPage>();
            
            if (ItemObject.GetType() == typeof(BusinessFlow))
            {               
                foreach (Activity ac in ((BusinessFlow)ItemObject).Activities)
                {
                    if (ac.GetType() == typeof(ErrorHandler)) continue;//do not show Error Handler for now

                    RunnerItemPage ri = new RunnerItemPage(ac);
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
            else if (ItemObject.GetType() == typeof(Activity))
            {                
                foreach (GingerCore.Actions.Act act in ((Activity)ItemObject).Acts)
                {
                    RunnerItemPage ri = new RunnerItemPage(act);
                    this.Context.Activity = (Activity)ItemObject;
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

        public RunnerItemPage(object Runnerobj = null, bool ViewMode= false)
        {
            InitializeComponent();

            //ItemsTotalCount++;
            //ItemsCount++;

            ItemObject = Runnerobj;
            if (ItemObject != null)
            {
                if (ItemObject.GetType() == typeof(GingerCore.BusinessFlow))
                {
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatus, StatusItem.StatusProperty, ItemObject, nameof(BusinessFlow.RunStatus), BindingMode.OneWay);
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatusIcon, ImageMakerControl.ImageTypeProperty, ItemObject, nameof(BusinessFlow.RunStatus),  bindingConvertor: new StatusIconConverter(), BindingMode.OneWay);
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xBusinessflowActive, ucButton.ButtonImageTypeProperty, ItemObject, nameof(BusinessFlow.Active), bindingConvertor: new ActiveIconConverter(), BindingMode.TwoWay);
                    ((BusinessFlow)ItemObject).PropertyChanged += RunnerItem_BusinessflowPropertyChanged;
                    xRunnerItemContinue.ToolTip = "Resume Run from this " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                    xViewRunnerItem.ToolTip = "View " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                }
                else if (ItemObject.GetType() == typeof(GingerCore.Activity))
                {
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatus, StatusItem.StatusProperty, ItemObject, nameof(Activity.Status), BindingMode.OneWay);
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatusIcon, ImageMakerControl.ImageTypeProperty, ItemObject, nameof(Activity.Status),  bindingConvertor: new StatusIconConverter(), BindingMode.OneWay);
                    ((Activity)ItemObject).PropertyChanged += RunnerItem_ActivityPropertyChanged;
                    xRunnerItemContinue.ToolTip = "Resume Run from this " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    xViewRunnerItem.ToolTip = "View " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    xRunnerItemMenu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatus, StatusItem.StatusProperty, ItemObject, nameof(Act.Status), BindingMode.OneWay);
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatusIcon, ImageMakerControl.ImageTypeProperty, ItemObject, nameof(Act.Status), bindingConvertor: new StatusIconConverter(), BindingMode.OneWay);
                    ((Act)ItemObject).PropertyChanged += RunnerItem_ActionPropertyChanged;
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

        }

        //static int ItemsCount = 0;
        //static int ItemsTotalCount = 0;
        //~RunnerItemPage()
        //{
        //    ItemsCount--;
        //}
        private void RunnerItem_ActionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Act.Status))
            {
                //Todo : need to see why it is blocking activity,businessflow selection.
                if (((Act)sender).Status == eRunStatus.Running)
                    OnRunnerItemEvent(RunnerItemEventArgs.eEventType.SetAsSelectedRequired, this, ItemtType, ItemObject);
            }
        }

        private void RunnerItem_ActivityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Activity.Status))
            {
                if (((Activity)sender).Status == eRunStatus.Running)
                    OnRunnerItemEvent(RunnerItemEventArgs.eEventType.SetAsSelectedRequired, this, ItemtType, ItemObject);
            }
        }

        private void RunnerItem_BusinessflowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BusinessFlow.RunStatus))
            {
                if (((BusinessFlow)sender).RunStatus == eRunStatus.Running)
                    OnRunnerItemEvent(RunnerItemEventArgs.eEventType.SetAsSelectedRequired, this, ItemtType, ItemObject);
            }
        }

        public event RoutedEventHandler Click;
        private void xconfig_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
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
                ExpandCollapseRunnerItem(true);
            else
                ExpandCollapseRunnerItem(false);
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
            if(((RunnerItemPage)sender).ItemObject is Act || ((RunnerItemPage)sender).ItemObject is Activity)
                OnRunnerItemEvent(RunnerItemEventArgs.eEventType.ViewRunnerItemRequired, this, ItemtType, ItemObject);
            else
                OnRunnerItemEvent(RunnerItemEventArgs.eEventType.ViewConfiguration, this, ItemtType, ItemObject);
        }

        private void xExportToAlm_Click(object sender, RoutedEventArgs e)
        {
            if (ItemObject.GetType() == typeof(GingerCore.BusinessFlow))
            {
                ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
                bfs.Add(((BusinessFlow)ItemObject));
               
                if (!ExportResultsToALMConfigPage.Instance.IsProcessing)
                {
                    ExportResultsToALMConfigPage.Instance.Init(bfs, new GingerCore.ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false));
                    ExportResultsToALMConfigPage.Instance.ShowAsWindow();
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
        public void ClearBindings()
        {
            if (ItemObject is BusinessFlow)
            {
                ((BusinessFlow)ItemObject).PropertyChanged -= RunnerItem_BusinessflowPropertyChanged;
            }
            else if (ItemObject is Activity)
            {
                ((Activity)ItemObject).PropertyChanged -= RunnerItem_ActivityPropertyChanged;
            }
            else
            {
                ((Act)ItemObject).PropertyChanged -= RunnerItem_ActionPropertyChanged;
            }
            this.xDetailView.Click -= xDetailView_Click;
            this.MouseDoubleClick -= UserControl_MouseDoubleClick;
            this.xRunnerItemContinue.Click -= xRunnerItemContinue_Click;
            this.xDuplicateBusinessflow.Click -= xDuplicateBusinessflow_Click;
            this.xautomateBusinessflow.Click -= xautomateBusinessflow_Click;
            this.xGenerateReport.Click -= xGenerateReport_Click;
            this.xExportToAlm.Click -= xExportToAlm_Click;
            this.xResetStatus_Buss_Flow_Actions.Click -= xResetStatus_Buss_Flow_Actions_Click;
            this.xViewRunnerItem.Click -= xViewRunnerItem_Click;
            this.xconfig.Click -= xconfig_Click;
            this.xremoveBusinessflow.Click -= xremoveBusinessflow_Click;
            this.xBusinessflowActive.Click -= xBusinessflowActive_Click;


            this.DuplicateClick -= xDuplicateBusinessflow_Click;
            this.ClickAutomate -= xautomateBusinessflow_Click;
            this.ClickGenerateReport -= xGenerateReport_Click;          
            this.ResetBusinessFlowStatus -= xResetStatus_Buss_Flow_Actions_Click;
            this.Click -= xconfig_Click;
            this.RemoveClick -= xremoveBusinessflow_Click;
            this.ClickActive -= xBusinessflowActive_Click;


            BindingOperations.ClearAllBindings(NormalBorder);
            BindingOperations.ClearAllBindings(xDetailView);

            BindingOperations.ClearAllBindings(xStatus);
            BindingOperations.ClearAllBindings(xStatusIcon);
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
    public class RunnerItemEventArgs
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

            public RunnerItemEventArgs(eEventType EventType, RunnerItemPage runnerItemPage, RunnerItemPage.eRunnerItemType runnerItemType,  object runnerItemObject)
            {
                this.EventType = EventType;
                this.RunnerItemPage = runnerItemPage;
                this.RunnerItemType = runnerItemType;
                this.RunnerItemObject = runnerItemObject;
            }
        }    
}