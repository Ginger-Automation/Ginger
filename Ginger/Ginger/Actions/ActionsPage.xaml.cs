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
using GingerWPF.DragDropLib;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Ginger.BusinessFlowFolder;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;
using Ginger.Repository;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActionsPage.xaml
    /// </summary>
    public partial class ActionsPage : Page
    {
        Activity mCurrentActivity;
        
        public General.RepositoryItemPageViewMode EditMode { get; set; }

        public ActionsPage(Activity activity=null, General.RepositoryItemPageViewMode editMode = General.RepositoryItemPageViewMode.Automation)
        {
            InitializeComponent();            
            EditMode = editMode;
            if (activity != null)
            {
                //static Activity               
                mCurrentActivity = activity;
                grdActions.Title = "Actions";
                grdActions.DataSourceList = mCurrentActivity.Acts;
                grdActions.ShowRefresh = System.Windows.Visibility.Collapsed;
            }
            else
            {
                EditMode = General.RepositoryItemPageViewMode.Automation;               
                //App.BusinessFlow daynamic Activity
                UpdateActionGrid();

                // Hook to Business flow properties changes
                App.BusinessFlow.PropertyChanged += BusinessFlowPropertyChanged;
                //Hook when App Property changes
                App.PropertyChanged += AppPropertyChanged;
                App.ActionsGrid = grdActions;

                grdActions.AddToolbarTool("@Split_16x16.png", "Split to " + GingerDicser.GetTermResValue(eTermResKey.Activities), new RoutedEventHandler(Split));
                grdActions.AddToolbarTool(eImageType.Reset, "Reset Run Details", new RoutedEventHandler(ResetAction));
                grdActions.AddFloatingImageButton("@ContinueFlow_16x16.png", "Continue Run Action", FloatingContinueRunActionButton_Click, 4);
                grdActions.AddFloatingImageButton("@RunAction_20x20.png", "Run Action", FloatingRunActionButton_Click, 4);
            }            
            SetActionsGridView();
                                   
            SetGridRowStyle();
            //Todo : need to see how to use local Editmode property
            if (editMode == General.RepositoryItemPageViewMode.View)
            {
                SetViewMode();
            }
        }

        public void FloatingRunActionButton_Click(object sender, RoutedEventArgs e)
        {
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.RunCurrentAction, null);
            
        }
        public void FloatingContinueRunActionButton_Click(object sender, RoutedEventArgs e)
        {
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.ContinueActionRun, null);            
        }
        

        private void ResetAction(object sender, RoutedEventArgs e)
        {
            Act currentAction = (Act)grdActions.CurrentItem;
            currentAction.Reset();
        }

        public void SetViewMode()
        {           
                grdActions.ShowToolsBar = Visibility.Collapsed;                
                grdActions.ToolsTray.Visibility = Visibility.Collapsed;
                grdActions.RowDoubleClick -= grdActions_grdMain_MouseDoubleClick;
                grdActions.DisableGridColoumns();
        }
        private void SetGridRowStyle()
        {
            // Define some style per row binded to data value
            
            // If Action is not Active - show it gray
            Style st = grdActions.grdMain.RowStyle;              
            DataTrigger DT = new DataTrigger();
            PropertyPath PT = new PropertyPath(Act.Fields.Active);
            DT.Value = false;
            DT.Binding = new Binding() { Path = PT };            
            DT.Setters.Add(new Setter(DataGridRow.FontWeightProperty, FontWeights.Light));
            DT.Setters.Add(new Setter(DataGridRow.ForegroundProperty, Brushes.Gray));                        
            st.Triggers.Add(DT);

            // if BreakPoint true- Show the row header (where line number) - in Red
            Style st2 = grdActions.grdMain.RowHeaderStyle;
            DataTrigger DT2 = new DataTrigger();
            PropertyPath PT2 = new PropertyPath(Act.Fields.BreakPoint);
            DT2.Binding = new Binding() { Path = PT2 };
            DT2.Value = true;
            DT2.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Red));            
            st2.Triggers.Add(DT2);
        }

        private void MarkUnMarkAllActions(bool ActiveStatus)
        {
            if (mCurrentActivity.Acts.Count <= 0) return;
            if (mCurrentActivity.Acts.Count > 0)
            {
                foreach (Act a in mCurrentActivity.Acts)
                    a.Active = ActiveStatus;
            }
        }
        
        // Drag Drop handlers
        private void grdActions_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(Act)))            
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
            }            
        }

        private void grdActions_ItemDropped(object sender, EventArgs e)
        {
            Act a = (Act)((DragInfo)sender).Data;
            Act instance = (Act)a.CreateInstance(true);
            mCurrentActivity.Acts.Add(instance);
            
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
        }

        private void LoadActionsVariablesDependenciesPage(object sender, RoutedEventArgs e)
        {
            Ginger.Variables.VariablesDependenciesPage actionsDepPage = new Ginger.Variables.VariablesDependenciesPage(mCurrentActivity);
            actionsDepPage.ShowAsWindow();
        }

        private void ResetActivity(object sender, RoutedEventArgs e)
        {
            foreach (Act a in mCurrentActivity.Acts)
            {
                a.Error = null;
                a.Elapsed = null;
                a.ExInfo = null;
                a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;               
            }
        }

        private void TakeUnTakeScreenShots(object sender, RoutedEventArgs e)
        {            
            bool takeValue = false;
            if (mCurrentActivity.Acts.Count > 0)
            {
                takeValue = !mCurrentActivity.Acts[0].TakeScreenShot;//decide if to take or not
                foreach (Act a in mCurrentActivity.Acts)
                    a.TakeScreenShot = takeValue;
            }
        }

        private void Split(object sender, RoutedEventArgs e)
        {
            Act CurrentAction = (Act)grdActions.CurrentItem;
            Activity activity = new Activity() { Active=true};
            activity.TargetApplication = mCurrentActivity.TargetApplication;
            activity.ActivityName = CurrentAction.Description;
            App.BusinessFlow.AddActivity(activity);

            // Find the action index to split on
            int i = 0;
            for (i = 0; i < mCurrentActivity.Acts.Count; i++)
            {                
                if (mCurrentActivity.Acts[i] == CurrentAction)
                {
                    break;
                }                
            }

            // Move the actions to the new activity
            for (int j = i; j < mCurrentActivity.Acts.Count; j++)
            {
                Act a1 = mCurrentActivity.Acts[j];
                activity.Acts.Add(a1);                
            }

            // remove the actions to from current activity - need to happen in 2 steps so the array count will not change while looping backwords
            for (int j = mCurrentActivity.Acts.Count - 1; j >= i; j--)
            {
                Act a1 = mCurrentActivity.Acts[j];
               mCurrentActivity.Acts.Remove(a1);
            }            
        }

        private void AddAction(object sender, RoutedEventArgs e)
        {
            if (mCurrentActivity == null)
            {
                Reporter.ToUser(eUserMsgKeys.SelectItemToAdd);
            }
            else
            {
                AddActionPage addAction = new AddActionPage();
                addAction.ShowAsWindow(mCurrentActivity.Acts);
            }
        }
        
        private void ActsPropChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {  
            if (e.PropertyName == "CurrentItem")
            {
                App.AutomateTabGingerRunner.HighlightActElement((Act)grdActions.CurrentItem);
            }            
        }

        private void Activity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Acts" && mCurrentActivity==App.BusinessFlow.CurrentActivity)
            {
                UpdateActionGrid();           
            }
        }

        private void AppPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           GingerCore.General.DoEvents();
            if (e.PropertyName == "BusinessFlow")
            {
                if (App.BusinessFlow != null)
                {
                    if (App.BusinessFlow.CurrentActivity == null)
                        App.BusinessFlow.CurrentActivity = App.BusinessFlow.Activities.FirstOrDefault();
                    if (App.BusinessFlow.CurrentActivity != null)
                    {
                        UpdateActionGrid();
                    }
                    //rehook
                    App.BusinessFlow.PropertyChanged += BusinessFlowPropertyChanged;
                }
            }
        }

        private void AddToRepository(object sender, RoutedEventArgs e)
        {
            Repository.SharedRepositoryOperations.AddItemsToRepository(grdActions.Grid.SelectedItems.Cast<RepositoryItemBase>().ToList());
        } 

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            UpdateActionGrid();
        }

        private void EditAction(object sender, RoutedEventArgs e)
        {
            if (grdActions.CurrentItem != null)
            {
                Act a=(Act)grdActions.CurrentItem;
                ActionEditPage actedit = new ActionEditPage(a,EditMode);
                actedit.ap = this;
                actedit.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.AskToSelectItem);
            }
        }
        
        private void SetActionsGridView()
        {
            //# Default View
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();          
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.Image, Header = " ", StyleType = GridColView.eGridColStyleType.Image, WidthWeight = 2.5, MaxWidth = 20 });            
            view.GridColsView.Add(new GridColView() { Field = nameof(RepositoryItemBase.SharedRepoInstanceImage), Header = "S.R.", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 2.5, MaxWidth = 20 });
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.Active, WidthWeight = 2.5, MaxWidth=50, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.BreakPoint, Header="B. Point", WidthWeight = 2.5, MaxWidth = 55, StyleType = GridColView.eGridColStyleType.CheckBox });            
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.Description, WidthWeight = 20 });
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.ActionDescription, Header = "Type", WidthWeight = 7, BindingMode = BindingMode.OneWay });
            //view.GridColsView.Add(new GridColView() { Field = Act.Fields.Details, Header = "Details", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.getDataColActionDetailsTemplate("Details"), ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(Act.WaitVE), WidthWeight = 3, Header = "Wait", MaxWidth = 50 });
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.EnableRetryMechanism, WidthWeight = 2.5, Header = "Retry", MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.TakeScreenShot, WidthWeight = 6, MaxWidth = 100, Header = "Take S. Shot", StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.SupportSimulation, Header = "S. Simulation", WidthWeight = 6, MaxWidth = 100, StyleType = GridColView.eGridColStyleType.CheckBox });
            //view.GridColsView.Add(new GridColView() { Field = Act.Fields.FlowControls, Header = "Flow Control", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetDataColGridTemplate("ActFlowControls"), ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(Act.FlowControlsInfo), Header = "Flow Controls", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay });
            //view.GridColsView.Add(new GridColView() { Field = Act.Fields.ReturnValues, Header = "Output Values", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetDataColGridTemplate("ActReturnValues"), ReadOnly = true, BindingMode = BindingMode.OneWay });  
            view.GridColsView.Add(new GridColView() { Field = nameof(Act.ReturnValuesInfo), Header = "Output Values", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay });  
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.Status, Header="Run Status", WidthWeight = 5, BindingMode = BindingMode.OneWay, PropertyConverter = (new ColumnPropertyConverter(new StatusConverter(), TextBlock.ForegroundProperty)) });           
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.ElapsedSecs, Header="Elapsed", WidthWeight = 5, BindingMode = BindingMode.OneWay, HorizontalAlignment= System.Windows.HorizontalAlignment.Right});
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.Error, WidthWeight = 10, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.ExInfo, Header="Extra Info", WidthWeight = 10, BindingMode = BindingMode.OneWay });            
            grdActions.SetAllColumnsDefaultView(view);

            //# Custome Views
            GridViewDef desView = new GridViewDef(eAutomatePageViewStyles.Design.ToString());
            desView.GridColsView = new ObservableList<GridColView>();
            desView.GridColsView.Add(new GridColView() { Field = Act.Fields.Status, Visible = false });
            desView.GridColsView.Add(new GridColView() { Field = Act.Fields.ElapsedSecs, Visible = false });
            desView.GridColsView.Add(new GridColView() { Field = Act.Fields.Error, Visible = false });
            desView.GridColsView.Add(new GridColView() { Field = Act.Fields.ExInfo, Visible = false });
            grdActions.AddCustomView(desView);

            GridViewDef execView = new GridViewDef(eAutomatePageViewStyles.Execution.ToString());
            execView.GridColsView = new ObservableList<GridColView>();
            //execView.GridColsView.Add(new GridColView() { Field = Act.Fields.Details, Visible = false });
            execView.GridColsView.Add(new GridColView() { Field = nameof(Act.FlowControlsInfo), Visible = false });
            execView.GridColsView.Add(new GridColView() { Field = nameof(Act.ReturnValuesInfo), Visible = false });
            execView.GridColsView.Add(new GridColView() { Field = Act.Fields.TakeScreenShot, Visible = false });
            grdActions.AddCustomView(execView);
            
            grdActions.InitViewItems();

            //Tool Bar
            grdActions.btnMarkAll.Visibility = Visibility.Visible;
            grdActions.ShowCopyCutPast = System.Windows.Visibility.Visible;
            grdActions.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            grdActions.ShowTagsFilter = Visibility.Visible;
            grdActions.btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditAction));
            grdActions.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddAction));
            grdActions.AddToolbarTool("@Image_16x16.png", "Take/Un-Take Screen Shots", new RoutedEventHandler(TakeUnTakeScreenShots));
            grdActions.AddToolbarTool("@RoadSign_16x16.png", "Set Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies", new RoutedEventHandler(LoadActionsVariablesDependenciesPage));
            grdActions.AddToolbarTool("@UploadStar_16x16.png", "Add to Shared Repository", new RoutedEventHandler(AddToRepository));

            //Events
            grdActions.grdMain.CanUserSortColumns = false;
            grdActions.RowDoubleClick += grdActions_grdMain_MouseDoubleClick;
            grdActions.ItemDropped += grdActions_ItemDropped;
            grdActions.PreviewDragItem += grdActions_PreviewDragItem;
            grdActions.MarkUnMarkAllActive += MarkUnMarkAllActions;
        }
        
        private void BusinessFlowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           GingerCore.General.DoEvents();
            //TODO: use const string
            this.Dispatcher.Invoke(() => {
                if (e.PropertyName == "CurrentActivity")
                {
                    UpdateActionGrid();
                }            
            });
        }

        public void UpdateActionGrid()
        {
            if (App.BusinessFlow !=null &&  App.BusinessFlow.CurrentActivity != null)
            {
                if (mCurrentActivity != App.BusinessFlow.CurrentActivity)
                {
                    mCurrentActivity = App.BusinessFlow.CurrentActivity;
                    mCurrentActivity.PropertyChanged += Activity_PropertyChanged;
                    mCurrentActivity.Acts.PropertyChanged += ActsPropChanged;                    
                }
                grdActions.Title = "'" + mCurrentActivity.ActivityName + "' - Actions";
                ObservableList<Act> SharedActions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
                SharedRepositoryOperations.MarkSharedRepositoryItems((IEnumerable<object>)mCurrentActivity.Acts, (IEnumerable<object>)SharedActions);
                grdActions.DataSourceList = mCurrentActivity.Acts;
            }
            else
            {
                mCurrentActivity = null;
                grdActions.DataSourceList = new ObservableList<Act>();
                grdActions.Title = "Actions";                
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public class StatusConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                    System.Globalization.CultureInfo culture)
            {
                string status = value.ToString();
                if (status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running.ToString())) return Brushes.Purple;

                if (status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed.ToString())) return App.Current.TryFindResource("$PassedStatusColor") as SolidColorBrush;
                if (status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString())) return App.Current.TryFindResource("$FailedStatusColor") as SolidColorBrush;
                if (status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending.ToString())) return App.Current.TryFindResource("$PendingStatusColor") as SolidColorBrush;
                if (status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked.ToString())) return App.Current.TryFindResource("$BlockedStatusColor") as SolidColorBrush;
                if (status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped.ToString())) return App.Current.TryFindResource("$StoppedStatusColor") as SolidColorBrush;
                if (status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped.ToString())) return App.Current.TryFindResource("$SkippedStatusColor") as SolidColorBrush;

                return Brushes.Gray;
            }

            public object ConvertBack(object value, Type targetType,
                object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private void grdActions_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            Act a = (Act)grdActions.CurrentItem;
            a.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();
            ActionEditPage actedit = new ActionEditPage(a, EditMode);
            actedit.ap = this;
            actedit.ShowAsWindow();
        }
    }
}
