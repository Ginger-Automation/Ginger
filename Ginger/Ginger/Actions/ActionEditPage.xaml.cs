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
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Expressions;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowWindows;
using Ginger.Help;
using Ginger.Repository;
using Ginger.Run;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.TextEditor;
using Ginger.UserControlsLib.UCListView;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Java;
using GingerCore.Actions.Windows;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using static GingerCore.Actions.Act;

namespace Ginger.Actions
{
    enum eGridView
    {
        All, NonSimulation,
        RegularView
    }

    enum eColumnVisibility
    {
        CustomDynamicView
    }
    public partial class ActionEditPage : GingerUIPage
    {
        //static int ActionEditNum = 0;
        //static int LiveActionEditCounter = 0;
        //~ActionEditPage()
        //{
        //    LiveActionEditCounter--;
        //}

        private Act mAction;
        static public string sMultiLocatorVals = "";
        GenericWindow _pageGenericWin = null!;

        bool IsPageClosing = false;

        private static readonly List<ComboEnumItem> OperatorList = GingerCore.General.GetEnumValuesForCombo(typeof(eOperator));

        ObservableList<DataSourceBase> mDSList = [];
        ObservableList<DataSourceTable> mDSTableList = [];
        List<string> mDSNames = [];
        private DataSourceTable mDSTable;
        private string mDataSourceName;
        List<String> mColNames = null!;
        ObservableList<String> mStoreToVarsList = [];

        private BusinessFlow mActParentBusinessFlow = null!;
        private Activity mActParentActivity = null!;

        Button mSimulateRunBtn = new Button();
        Button mRunActionBtn = new Button();
        Button mStopRunBtn = new Button();
        string? columnPreferences;

        private bool saveWasDone = false;
        ActionFlowControlPage mAFCP;
        Context mContext;

        private bool datasourceGridToolbarItemsAdded = false;
        private bool outputValuesGridViewSet = false;
        private bool outputValuesGridToolbarItemsAdded = false;
        private CheckBox? addParameterAutomaticallyCheckbox;
        private CheckBox? supportSimulationCheckbox;
        private MultiSelectComboBox? columnMultiSelectComboBox;
        GridViewDef customDynamicView;
        int columnCount = 0;

        public int SelectedTabIndx
        {
            get
            {
                return xActionTabs.SelectedIndex;
            }
            set
            {
                xActionTabs.SelectedIndex = value;
            }
        }

        ObservableList<UCArtifact> mArtifactsItems = null;
        public ObservableList<UCArtifact> ArtifactsItems
        {
            get
            {
                if (mArtifactsItems == null)
                {
                    mArtifactsItems = [];
                }
                return mArtifactsItems;
            }
            set
            {
                mArtifactsItems = value;
            }
        }


        public General.eRIPageViewMode EditMode { get; set; }

        public ActionEditPage(Act act, General.eRIPageViewMode editMode = General.eRIPageViewMode.Automation, BusinessFlow? actParentBusinessFlow = null, Activity? actParentActivity = null)
        {
            InitializeComponent();
            Init(act, editMode, actParentBusinessFlow, actParentActivity);
        }

        public void Init(Act act, General.eRIPageViewMode editMode = General.eRIPageViewMode.Automation, BusinessFlow? actParentBusinessFlow = null, Activity? actParentActivity = null)
        {
            Clear();


            //ActionEditNum++;
            //LiveActionEditCounter++;

            EditMode = editMode;

            mAction = act;
            mAction.PauseDirtyTracking();
            if (editMode is not General.eRIPageViewMode.View and not General.eRIPageViewMode.ViewAndExecute and not General.eRIPageViewMode.Explorer)
            {
                mAction.SaveBackup();
            }

            string allProperties = string.Empty;
            PropertyChangedEventManager.RemoveHandler(source: mAction, handler: ActionPropertyChanged, propertyName: allProperties);
            PropertyChangedEventManager.AddHandler(source: mAction, handler: ActionPropertyChanged, propertyName: allProperties);

            CollectionChangedEventManager.RemoveHandler(source: mAction.InputValues, handler: InputValues_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: mAction.InputValues, handler: InputValues_CollectionChanged);

            CollectionChangedEventManager.RemoveHandler(source: mAction.FlowControls, handler: FlowControls_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: mAction.FlowControls, handler: FlowControls_CollectionChanged);

            CollectionChangedEventManager.RemoveHandler(source: mAction.ReturnValues, handler: ReturnValues_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: mAction.ReturnValues, handler: ReturnValues_CollectionChanged);

            CollectionChangedEventManager.RemoveHandler(source: mAction.ScreenShots, handler: ScreenShots_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: mAction.ScreenShots, handler: ScreenShots_CollectionChanged);
            CollectionChangedEventManager.RemoveHandler(source: mAction.Artifacts, handler: Artifacts__CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: mAction.Artifacts, handler: Artifacts__CollectionChanged);

            mContext = Context.GetAsContext(mAction.Context);
            if (mContext != null && mContext.Runner != null)
            {
                mContext.Runner.PrepActionValueExpression(mAction, actParentBusinessFlow);
            }

            if (actParentBusinessFlow != null)
            {
                mActParentBusinessFlow = actParentBusinessFlow;
            }
            else if (mAction.Context != null && mContext.BusinessFlow != null)
            {
                mActParentBusinessFlow = mContext.BusinessFlow;
            }

            if (actParentActivity != null)
            {
                mActParentActivity = actParentActivity;
            }
            else if (mAction.Context != null && mContext.Activity != null)
            {
                mActParentActivity = mContext.Activity;
            }

            GingerHelpProvider.SetHelpString(this, act.ActionDescription);

            InitView();
            mAction.ResumeDirtyTracking();
        }

        public void Clear()
        {
            if (mAction != null)
            {
                string allProperties = string.Empty;
                PropertyChangedEventManager.RemoveHandler(source: mAction, handler: ActionPropertyChanged, propertyName: allProperties);
                CollectionChangedEventManager.RemoveHandler(source: mAction.InputValues, handler: InputValues_CollectionChanged);
                CollectionChangedEventManager.RemoveHandler(source: mAction.FlowControls, handler: FlowControls_CollectionChanged);
                CollectionChangedEventManager.RemoveHandler(source: mAction.ReturnValues, handler: ReturnValues_CollectionChanged);
                CollectionChangedEventManager.RemoveHandler(source: mAction.ScreenShots, handler: ScreenShots_CollectionChanged);
                CollectionChangedEventManager.RemoveHandler(source: mAction.Artifacts, handler: Artifacts__CollectionChanged);
            }

            xDetailsTab.Tag = false;
            xOperationSettingsTab.Tag = false;
            xFlowControlTab.Tag = false;
            xOutputValuesTab.Tag = false;
            xExecutionReportTab.Tag = false;
            xHelpTab.Tag = false;

            xOutputValuesGrid.btnRefresh.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(RefreshOutputValuesGridElements));
            xOutputValuesGrid.btnAdd.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(AddReturnValue));

            if (addParameterAutomaticallyCheckbox != null)
            {
                BindingOperations.ClearBinding(addParameterAutomaticallyCheckbox, CheckBox.IsCheckedProperty);
            }
            if (supportSimulationCheckbox != null)
            {
                BindingOperations.ClearBinding(supportSimulationCheckbox, CheckBox.IsCheckedProperty);
            }

            xActionsDetailsPnl.IsEnabled = true;
            xOperationSettingsPnl.IsEnabled = true;
            xWaitTimeoutPnl.IsEnabled = true;
            xRetryMechanismPnl.IsEnabled = true;
            xAddOutputToDataSourcePnl.IsEnabled = true;
            xExecutionReportConfigPnl.IsEnabled = true;

            xDataSourceConfigGrid.ToolsTray.Visibility = Visibility.Visible;
            xDataSourceConfigGrid.EnableGridColumns();
            xOutputValuesGrid.ToolsTray.Visibility = Visibility.Visible;
            xOutputValuesGrid.EnableGridColumns();


            WeakEventManager<Selector, SelectionChangedEventArgs>.RemoveHandler(source: xdsOutputParamMapType, eventName: nameof(Selector.SelectionChanged), handler: OutDSParamType_SelectionChanged);
            WeakEventManager<Selector, SelectionChangedEventArgs>.RemoveHandler(source: xDataSourceNameCombo, eventName: nameof(Selector.SelectionChanged), handler: cmbDataSourceName_SelectionChanged);
            WeakEventManager<Selector, SelectionChangedEventArgs>.RemoveHandler(source: xDataSourceTableNameCombo, eventName: nameof(Selector.SelectionChanged), handler: cmbDataSourceTableName_SelectionChanged);
            WeakEventManager<ToggleButton, RoutedEventArgs>.RemoveHandler(source: xAddOutToDSCheckbox, eventName: nameof(ToggleButton.Checked), handler: AddOutDS_Checked);
            WeakEventManager<ToggleButton, RoutedEventArgs>.RemoveHandler(source: xAddOutToDSCheckbox, eventName: nameof(ToggleButton.Unchecked), handler: AddOutDS_Unchecked);





            mAction = null!;
            mContext = null!;

            _pageGenericWin = null!;

            IsPageClosing = false;

            mDSList = [];
            mDSTableList = [];
            mDSNames = [];
            mColNames = null!;
            xOutputValuesGrid.DataSourceList = new ObservableList<ActReturnValue>();
            mStoreToVarsList.Clear();

            mActParentBusinessFlow = null!;
            mActParentActivity = null!;
            mSimulateRunBtn = new Button();
            mRunActionBtn = new Button();
            mStopRunBtn = new Button();
            saveWasDone = false;

            ClearPageBindings();
        }

        private void InitView()
        {
            UpdateTabsHeaders();

            //allowing return values automatically in Edit Action window
            if (mAction.AddNewReturnParams == null && mAction.ReturnValues.Count == 0)
            {
                mAction.AddNewReturnParams = true;
            }

            if (EditMode is General.eRIPageViewMode.Automation or General.eRIPageViewMode.View or General.eRIPageViewMode.ViewAndExecute or General.eRIPageViewMode.Explorer)
            {
                BindingHandler.ObjFieldBinding(xExecutionStatusTabImage, UcItemExecutionStatus.StatusProperty, mAction, nameof(Act.Status));
            }
            else
            {
                xExecutionStatusTabImage.Visibility = Visibility.Collapsed;
            }

            if (EditMode is General.eRIPageViewMode.View or General.eRIPageViewMode.ViewAndExecute)
            {
                SetViewMode();
            }
            else if (EditMode == General.eRIPageViewMode.Explorer)
            {
                SetExplorerMode();
            }

            object previousSelectedItem = xActionTabs.SelectedItem;
            object newSelectedItem;
            if ((EditMode == General.eRIPageViewMode.Automation || EditMode == General.eRIPageViewMode.View || EditMode == General.eRIPageViewMode.ViewAndExecute) &&
                       (mAction.Status != null && mAction.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending))
            {
                newSelectedItem = xExecutionReportTab;
            }
            else
            {
                newSelectedItem = xOperationSettingsTab;
            }
            xActionTabs.SelectedItem = newSelectedItem;
            if (previousSelectedItem == newSelectedItem)
            {
                SetSelectedTabFrameContent();
            }
        }

        private void SetExplorerMode()
        {
            xHelpTab.Visibility = Visibility.Collapsed;
            xHelpButton.Visibility = Visibility.Collapsed;
            xFlowControlTab.Visibility = Visibility.Collapsed;

            xOutputValuesTabHeaderTextBlock.FontSize = 10;
            xDetailsTabTextBlock.FontSize = 10;
            xExecutionReportTabTextBlock.FontSize = 10;
            xOperationsTabTextBlock.FontSize = 10;
        }

        private void InitDetailsTabView()
        {
            xDetailsTab.Tag = true;//marking that binding was done

            BindingHandler.ObjFieldBinding(xTypeLbl, Label.ContentProperty, mAction, nameof(Act.ActionType), BindingMode: BindingMode.OneWay);
            xDescriptionTextBox.BindControl(mAction, nameof(Act.Description));
            xShowIDUC.Init(mAction);
            xRunDescritpionUC.Init(mContext, mAction, nameof(Act.RunDescription));
            xTagsViewer.Init(mAction.Tags);

            if (EditMode == General.eRIPageViewMode.Automation)
            {
                xSharedRepoInstanceUC.Init(mAction, null);
            }
            else
            {
                xSharedRepoInstanceUC.Visibility = Visibility.Collapsed;
                xSharedRepoInstanceUCCol.Width = new GridLength(0);
            }
        }

        private void InitOperationSettingsTabView()
        {
            xOperationSettingsTab.Tag = true;//marking that bindings were done

            if (mAction.ObjectLocatorConfigsNeeded)
            {
                List<eLocateBy> locatorsTypeList = mAction.AvailableLocateBy().Where(e => e != eLocateBy.POMElement).ToList();
                xLocateByCombo.BindControl(mAction, nameof(Act.LocateBy), locatorsTypeList);
                xLocateValueVE.BindControl(mContext, mAction, nameof(Act.LocateValue));
                BindingHandler.ObjFieldBinding(xLocateValueVE, TextBox.ToolTipProperty, mAction, nameof(Act.LocateValue));
                xActionLocatorPnl.Visibility = Visibility.Visible;
            }
            else
            {
                xActionLocatorPnl.Visibility = Visibility.Collapsed;
            }

            SwitchingInputValueBoxAndGrid(mAction);
            SetActInputValuesGrid();

            LoadOperationSettingsEditPage(mAction);
        }

        private void InitFlowControlTabView()
        {
            xFlowControlTab.Tag = true;//marking that bindings were done

            //Wait/Timeout
            xWaitVeUC.BindControl(mContext, mAction, nameof(Act.WaitVE));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xTimeoutTextBox, TextBox.TextProperty, mAction, nameof(Act.Timeout));

            //Retry
            if (mActParentActivity != null && mActParentActivity.GetType() == typeof(ErrorHandler))
            {
                xRetryExpander.Visibility = Visibility.Collapsed;
            }
            else
            {
                BindingHandler.ObjFieldBinding(xEnableRetryMechanismCheckBox, CheckBox.IsCheckedProperty, mAction, nameof(Act.EnableRetryMechanism));
                BindingHandler.ObjFieldBinding(xRetryMechanismIntervalTextBox, TextBox.TextProperty, mAction, nameof(Act.RetryMechanismInterval));
                BindingHandler.ObjFieldBinding(xRetryMechanismMaxRetriesTextBox, TextBox.TextProperty, mAction, nameof(Act.MaxNumberOfRetries));
                SetRetryMechConfigsPnlView();
            }

            //Flow Controls Conditions
            if (EditMode == General.eRIPageViewMode.View)
            {
                mAFCP = new ActionFlowControlPage(mAction, mActParentBusinessFlow, mActParentActivity, General.eRIPageViewMode.View);
            }
            else if (EditMode == General.eRIPageViewMode.ViewAndExecute)
            {
                mAFCP = new ActionFlowControlPage(mAction, mActParentBusinessFlow, mActParentActivity, General.eRIPageViewMode.ViewAndExecute);
            }
            else if (EditMode == General.eRIPageViewMode.SharedReposiotry)
            {
                mAFCP = new ActionFlowControlPage(mAction, mActParentBusinessFlow, mActParentActivity, General.eRIPageViewMode.SharedReposiotry);
            }
            else
            {
                mAFCP = new ActionFlowControlPage(mAction, mActParentBusinessFlow, mActParentActivity);
            }
            xFlowControlConditionsFrame.SetContent(mAFCP);

            if (mAction.FlowControls.Count > 0)
            {
                xFlowControlConditionsExpander.IsExpanded = true;
            }
        }

        private void InitOutputValuesTabView()
        {
            xOutputValuesTab.Tag = true;//marking that bindings were done
            LoadArtifacts();
            if (!datasourceGridToolbarItemsAdded)
            {
                datasourceGridToolbarItemsAdded = true;
                xDataSourceConfigGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All", new RoutedEventHandler(CheckUnCheckGridRow));
            }

            if (mAction.ConfigOutputDS == true && mAction.DSOutputConfigParams.Count > 0)
            {
                xDataSourceExpander.IsExpanded = true;
                mAction.OutDataSourceName = mAction.DSOutputConfigParams[0].DSName;
                mAction.OutDataSourceTableName = mAction.DSOutputConfigParams[0].DSTable;
                if (mAction.DSOutputConfigParams[0].OutParamMap == null)
                {
                    mAction.OutDSParamMapType = Act.eOutputDSParamMapType.ParamToRow.ToString();
                }
                else if (mAction.DSOutputConfigParams.Any(x => x.OutParamMap == Act.eOutputDSParamMapType.ParamToCol.ToString()))
                {
                    mAction.OutDSParamMapType = Act.eOutputDSParamMapType.ParamToCol.ToString();
                }
                else
                {
                    mAction.OutDSParamMapType = mAction.DSOutputConfigParams[0].OutParamMap;
                }
            }
            else
            {
                xDataSourceExpander.IsExpanded = false;
            }

            BindingHandler.ObjFieldBinding(xAddOutToDSCheckbox, CheckBox.IsCheckedProperty, mAction, nameof(Act.ConfigOutputDS));

            PopulateDataSourceNames();
            PopulateParamMapTypes();

            if (mAction.ConfigOutputDS)
            {
                SetSelectedDataSource(mAction.OutDataSourceName);
            }

            BindingHandler.ObjFieldBinding(xDataSourceNameCombo, ComboBox.TextProperty, mAction, nameof(Act.OutDataSourceName));
            BindingHandler.ObjFieldBinding(xDataSourceTableNameCombo, ComboBox.TextProperty, mAction, nameof(Act.OutDataSourceTableName));
            BindingHandler.ObjFieldBinding(xdsOutputParamMapType, ComboBox.TextProperty, mAction, nameof(Act.OutDSParamMapType));
            BindingHandler.ObjFieldBinding(xRawResponseValuesBtn, Button.VisibilityProperty, mAction, nameof(Act.RawResponseValues), bindingConvertor: new StringVisibilityConverter(), BindingMode: BindingMode.OneWay);
            BindingHandler.ObjFieldBinding(xdsOutputParamAutoCheck, CheckBox.IsCheckedProperty, mAction, nameof(Act.ConfigOutDSParamAutoCheck));

            if (mAction.ConfigOutputDS)
            {
                updateDSOutGrid();
            }

            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: xdsOutputParamMapType, eventName: nameof(Selector.SelectionChanged), handler: OutDSParamType_SelectionChanged);
            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: xDataSourceNameCombo, eventName: nameof(Selector.SelectionChanged), handler: cmbDataSourceName_SelectionChanged);
            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: xDataSourceTableNameCombo, eventName: nameof(Selector.SelectionChanged), handler: cmbDataSourceTableName_SelectionChanged);
            WeakEventManager<ToggleButton, RoutedEventArgs>.AddHandler(source: xAddOutToDSCheckbox, eventName: nameof(ToggleButton.Checked), handler: AddOutDS_Checked);
            WeakEventManager<ToggleButton, RoutedEventArgs>.AddHandler(source: xAddOutToDSCheckbox, eventName: nameof(ToggleButton.Unchecked), handler: AddOutDS_Unchecked);






            if (mAction.ConfigOutDSParamAutoCheck)
            {
                xDataSourceConfigGrid.Visibility = Visibility.Collapsed;
            }

            mDSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            if (mDSList.Count == 0)
            {
                xAddOutToDSCheckbox.IsEnabled = false;
            }
            WeakEventManager<UIElement, RoutedEventArgs>.AddHandler(source: xDataSourceConfigGrid, eventName: nameof(UIElement.LostFocus), handler: DataSourceConfigGrid_LostFocus);
            //Output Values


            SetActReturnValuesGrid();

            if (mAction.ActReturnValues.Count > 0 || mAction.Artifacts.Count > 0)
            {
                xOutputValuesExpander.IsExpanded = true;
            }

            if (EditMode is General.eRIPageViewMode.View or General.eRIPageViewMode.ViewAndExecute)
            {
                xOutputValuesGrid.DisableGridColoumns();
            }
        }


        private void InitExecutionReportTabView()
        {
            xExecutionReportTab.Tag = true;//marking that bindings were done           

            //configs section
            xStatusConvertorCombo.BindControl(mAction, nameof(Act.StatusConverter));
            BindingHandler.ObjFieldBinding(xFailIgnoreCheckBox, CheckBox.IsCheckedProperty, mAction, nameof(Act.FailIgnored));

            BindingHandler.ObjFieldBinding(xEnableActionLogConfigCheckBox, CheckBox.IsCheckedProperty, mAction, nameof(Act.EnableActionLogConfig));
            InitActionLog();
            //execution details section
            if (EditMode is General.eRIPageViewMode.Automation or General.eRIPageViewMode.View or
                General.eRIPageViewMode.ViewAndExecute or General.eRIPageViewMode.Explorer or General.eRIPageViewMode.SharedReposiotry)
            {
                BindingHandler.ObjFieldBinding(xExecutionStatusImage, UcItemExecutionStatus.StatusProperty, mAction, nameof(Act.Status));
                BindingHandler.ObjFieldBinding(xExecutionStatusLabel, UcItemExecutionStatus.StatusProperty, mAction, nameof(Act.Status));

                BindingHandler.ObjFieldBinding(xExecutionTimeLbl, Label.ContentProperty, mAction, nameof(Act.ElapsedSecs), BindingMode.OneWay);

                BindingHandler.ObjFieldBinding(xExecutionExtraInfoText, TextBox.TextProperty, mAction, nameof(Act.ExInfo), BindingMode.OneWay);
                BindingHandler.ObjFieldBinding(xExecutionExtraInfoPnl, StackPanel.VisibilityProperty, mAction, nameof(Act.ExInfo), bindingConvertor: new StringVisibilityConverter(), BindingMode: BindingMode.OneWay);

                BindingHandler.ObjFieldBinding(xExecutionErrorDetailsText, TextBox.TextProperty, mAction, nameof(Act.Error), BindingMode.OneWay);
                BindingHandler.ObjFieldBinding(xExecutionErrorDetailsPnl, StackPanel.VisibilityProperty, mAction, nameof(Act.Error), bindingConvertor: new StringVisibilityConverter(), BindingMode: BindingMode.OneWay);

                if (mActParentActivity != null && mActParentActivity.GetType() == typeof(ErrorHandler))
                {
                    xScreenshotsConfigsPnl.Visibility = Visibility.Collapsed;
                    xScreenShotsPnl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    BindingHandler.ObjFieldBinding(xTakeScreenShotCheckBox, CheckBox.IsCheckedProperty, mAction, nameof(Act.TakeScreenShot));

                    BindingHandler.ObjFieldBinding(xAutoScreenShotOnFailureCheckBox, CheckBox.IsCheckedProperty, mAction, nameof(Act.AutoScreenShotOnFailure));

                    xWindowsToCaptureCombo.BindControl(mAction, nameof(Act.WindowsToCapture));
                    //remove full page for other platforms excepts web
                    if (mAction.Platform != GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Web)
                    {
                        RemoveCaptureTypeFromComboItems(Act.eWindowsToCapture.FullPage);
                        RemoveCaptureTypeFromComboItems(Act.eWindowsToCapture.FullPageWithUrlAndTimestamp);
                    }
                    SetScreenshotsPnlView();
                    UpdateScreenShots();
                }
            }
            else
            {
                xExecutionDetailsExpander.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadArtifacts()
        {
            ArtifactsItems = [];
            foreach (ArtifactDetails a in mAction.Artifacts)
            {
                UCArtifact artifact = new UCArtifact
                {
                    ArtifactPath = a.ArtifactReportStoragePath,
                    ArtifactName = a.ArtifactOriginalName
                };
                artifact.IntiArtifact();
                ArtifactsItems.Add(artifact);
            }
            xFilesListView.ItemsSource = ArtifactsItems;

            if (ArtifactsItems.Count > 0)
            {
                xFilesListView.Visibility = Visibility.Visible;
                xlbl_msg.Visibility = Visibility.Collapsed;
            }
            else
            {
                xFilesListView.Visibility = Visibility.Collapsed;
                xlbl_msg.Visibility = Visibility.Visible;
            }
            xFilesTabTextBlock.Text = string.Concat("Output Files (", ArtifactsItems.Count, ")");
        }
        private void RemoveCaptureTypeFromComboItems(Act.eWindowsToCapture captureType)
        {
            var comboEnumItem = xWindowsToCaptureCombo.Items.Cast<ComboEnumItem>().FirstOrDefault(x => x.Value.ToString() == captureType.ToString());
            xWindowsToCaptureCombo.Items.Remove(comboEnumItem);
        }

        private void InitHelpTabView()
        {
            xHelpTab.Tag = true;//marking that bindings were done

            ActDescriptionPage desPage = new ActDescriptionPage(mAction);
            xActionHelpDetailsFram.SetContent(desPage);
        }

        private void ScreenShots_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                UpdateScreenShots();
            });
        }
        private void Artifacts__CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                LoadArtifacts();
            });
        }

        public void StopEdit()
        {
            if (mAFCP != null)
            {
                mAFCP.FlowControlGrid.Grid.CommitEdit();
                mAFCP.FlowControlGrid.Grid.CancelEdit();
            }
            if (xInputValuesGrid != null)
            {
                xInputValuesGrid.Grid.CommitEdit();
                xInputValuesGrid.Grid.CancelEdit();
            }
            if (xDataSourceConfigGrid != null)
            {
                xDataSourceConfigGrid.Grid.CommitEdit();
                xDataSourceConfigGrid.Grid.CancelEdit();
            }
            if (xOutputValuesGrid != null)
            {
                xOutputValuesGrid.Grid.CommitEdit();
                xOutputValuesGrid.Grid.CancelEdit();
            }
        }

        private void ReturnValues_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateOutputValuesTabHeader();
            mAction.OnPropertyChanged(nameof(Act.ReturnValuesCount));
            this.Dispatcher.Invoke(() =>
            {
                if (mAction.ActReturnValues.Count > 0 || mAction.Artifacts.Count > 0)
                {
                    xOutputValuesExpander.IsExpanded = true;
                }
            });
        }

        private void FlowControls_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateFlowControlsTabHeader();
            mAction.OnPropertyChanged(nameof(Act.FlowControlsInfo));
        }

        private void ClearUnusedParameter(object sender, RoutedEventArgs e)
        {
            mAction.ClearUnUsedReturnParams();
        }

        private void RefreshOutputColumns(object sender, RoutedEventArgs e)
        {
            ColumnMultiSelectComboBox_ItemCheckBoxClick(null, null);
        }

        private void InputValues_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SwitchingInputValueBoxAndGrid(mAction);
        }

        //FIXME - remove after moving to ActUIElement !!!!
        //TODO: Ugly code. Find a better way to do it
        // Split the flags in to two one for value field needed and other for grid needed
        // and grid needed flag will be set only for those actions where it is needed.
        // This way we will remove all below ugly else if conditions
        private void SwitchingInputValueBoxAndGrid(Act a)
        {
            if (IsPageClosing)
            {
                return; // no need to update the UI since we are closing, when done in Undo changes/Cancel 
            }
            // we do restore and don't want to raise events which will cause exception  (a.Value = ""  - is the messier)

            if (mAction.ValueConfigsNeeded == false)
            {
                this.Dispatcher.Invoke(() =>
                {
                    xInputValuesEditControlsPnl.Visibility = System.Windows.Visibility.Collapsed;
                });
                return;
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    xInputValuesEditControlsPnl.Visibility = Visibility.Visible;
                });
            }

            //TODO: Remove all if else and handle it dynamically based on if Input value grid is needed or not
            int minimumInputValuesToHideGrid = 1;
            if (mAction.ObjectLocatorConfigsNeeded)
            {
                //For actions with locator's config needed, Locate by , locate value is also added to input value                
                minimumInputValuesToHideGrid = 3;
            }

            if (a.GetType() == typeof(ActCLIOrchestration))
            {
                //for CLI Orchestration need to show by default
                minimumInputValuesToHideGrid = -1;
            }
            if (a.GetType() == typeof(ActPublishArtifacts))
            {
                minimumInputValuesToHideGrid = -1;
                xInputValuesGrid.Title = "File Path of Artifacts to be Published.";
            }
            else
            {
                xInputValuesGrid.Title = "Input Value(s)";
            }

            if (a.GetType() != typeof(ActDBValidation) && a.GetType() != typeof(ActTableElement) &&
                a.GetType() != typeof(ActLaunchJavaWSApplication) && a.GetType() != typeof(ActJavaEXE) &&
                a.GetType() != typeof(ActGenElement) && a.GetType() != typeof(ActScript) && a.GetType() != typeof(ActConsoleCommand) &&
                a.GetType() != typeof(ActSetVariableValue) && a.GetType() != typeof(ActCreatePDFChart) && a.GetType() != typeof(ActCompareImgs) &&
                a.GetType() != typeof(ActGenerateFileFromTemplate) && a.GetType() != typeof(ActPBControl) && a.GetType() != typeof(ActWindowsControl) &&
                a.GetType() != typeof(ActMenuItem) && a.GetType() != typeof(ActJavaElement))
            {
                if (a.InputValues.Count > minimumInputValuesToHideGrid)
                {
                    xInputValuesGrid.Visibility = Visibility.Visible;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
                else if (a.InputValues.Count == minimumInputValuesToHideGrid)
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Visible;
                    ActInputValue inputValue = a.InputValues.FirstOrDefault(x => x.Param == "Value");
                    if (inputValue != null)
                    {
                        xValueVE.Init(mContext, inputValue, nameof(ActInputValue.Value));
                        xValueVE.ValueTextBox.Text = inputValue.Value;
                        xValueLbl.Content = inputValue.Param;
                    }
                }
                else
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Visible;
                    a.Value = "";
                    ActInputValue inputValue = a.InputValues.FirstOrDefault(x => x.Param == "Value");
                    xValueVE.Init(Context.GetAsContext(a.Context), inputValue, nameof(ActInputValue.Value));
                }
            }
            else if (a.GetType() == typeof(ActGenElement) || a.GetType() == typeof(ActTableElement))
            {

                ActInputValue inputValue = a.InputValues.FirstOrDefault(x => x.Param == "Value");

                if (inputValue == null)
                {
                    a.AddOrUpdateInputParamValue("Value", "");
                    inputValue = a.InputValues.FirstOrDefault(x => x.Param == "Value");
                }
                xInputValuesGrid.Visibility = Visibility.Collapsed;
                xValueBoxPnl.Visibility = Visibility.Visible;
                xValueVE.Init(Context.GetAsContext(a.Context), inputValue, nameof(ActInputValue.Value));

            }
            else if (a.GetType() == typeof(ActLaunchJavaWSApplication) || a.GetType() == typeof(ActJavaEXE))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count <= 1)
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
                else if (a.InputValues.Count >= 2)
                {
                    xInputValuesGrid.Visibility = Visibility.Visible;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
            }
            else if (a.GetType() == typeof(ActDBValidation))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count == 1)
                {
                    xValueVE.Init(mContext, mAction.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                    xValueVE.ValueTextBox.Text = mAction.InputValues.FirstOrDefault().Value;
                    xValueLbl.Content = a.InputValues.FirstOrDefault().Param;
                }
                else
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                    xValueVE.Init(mContext, a.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                }
            }
            else if (a.GetType() == typeof(ActScript))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count > 1)
                {
                    xInputValuesGrid.Visibility = Visibility.Visible;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
                else if (a.InputValues.Count == 1)
                {
                    xValueVE.Init(mContext, mAction.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Visible;
                    xValueVE.ValueTextBox.Text = a.InputValues.FirstOrDefault().Value;
                    xValueLbl.Content = a.InputValues.FirstOrDefault().Param;
                }
                else
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
            }
            else if (a.GetType() == typeof(ActConsoleCommand))//TODO: Fix Action implementation to not base on the Action edit page Input values controls- to have it own controls
            {
                if (a.InputValues.Count == 1)
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Visible;
                    xValueVE.Init(mContext, mAction.InputValues.FirstOrDefault(), nameof(ActInputValue.Value));
                    xValueVE.ValueTextBox.Text = a.InputValues.FirstOrDefault().Value;
                    xValueLbl.Content = a.InputValues.FirstOrDefault().Param;
                }
                else if (a.InputValues.Count > 1)
                {
                    xInputValuesGrid.Visibility = Visibility.Visible;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    xInputValuesGrid.Visibility = Visibility.Collapsed;
                    xValueBoxPnl.Visibility = Visibility.Collapsed;
                }
            }
            else if (a.GetType() == typeof(ActCompareImgs))
            {
                xInputValuesGrid.Visibility = Visibility.Collapsed;
                xValueBoxPnl.Visibility = Visibility.Collapsed;
            }
            else if (a.GetType() == typeof(ActSetVariableValue) || a.GetType() == typeof(ActCreatePDFChart)
                        || a.GetType() == typeof(ActGenerateFileFromTemplate)
                        || a.GetType() == typeof(ActPBControl) || a.GetType() == typeof(ActWindowsControl)
                        || a.GetType() == typeof(ActMenuItem) || a.GetType() == typeof(ActJavaElement))
            {
                xInputValuesGrid.Visibility = Visibility.Collapsed;
                xValueBoxPnl.Visibility = Visibility.Visible;
                if (!a.InputValues.Any(x => x.Param == "Value"))
                {
                    a.AddOrUpdateInputParamValue("Value", "");
                }
                ActInputValue inputValue = a.InputValues.FirstOrDefault(x => x.Param == "Value");
                xValueVE.Init(mContext, inputValue, nameof(ActInputValue.Value));
                if (inputValue != null)
                {
                    xValueVE.ValueTextBox.Text = inputValue.Value;
                    xValueLbl.Content = inputValue.Param;
                }
            }
        }

        private void AddReturnValue(object sender, RoutedEventArgs e)
        {
            mAction.ReturnValues.Add(new ActReturnValue() { Active = true, Operator = eOperator.Equals });
        }

        private void RefreshOutputValuesGridElements(object sender, RoutedEventArgs e)
        {
            //refresh Variables StoreTo options
            GenerateStoreToVarsList();
        }

        private void AddInputValue(object sender, RoutedEventArgs e)
        {
            mAction.InputValues.Add(new ActInputValue()
            {
                Param = GetUniqueParamName()
            });
        }

        private string GetUniqueParamName()
        {
            if (mAction is ActPublishArtifacts)
            {
                return "Artifact " + (mAction.InputValues.Select(iv => int.TryParse(iv.Param.AsSpan("Artifact ".Length), out var n) ? n : -1).DefaultIfEmpty(-1).Max() + 1);
            }

            return "p" + (mAction.InputValues.Select(iv => int.TryParse(iv.Param?.TrimStart('p'), out var n) ? n : -1).DefaultIfEmpty(-1).Max() + 1);
        }

        private void SetActDataSourceConfigGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = [];
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = ActOutDataSourceConfig.Fields.Active, WidthWeight = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            viewCols.Add(new GridColView() { Field = ActOutDataSourceConfig.Fields.OutputType, Header = "Output Type", WidthWeight = 150, ReadOnly = true });
            viewCols.Add(new GridColView() { Field = ActOutDataSourceConfig.Fields.TableColumn, Header = "Table Column", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(ActOutDataSourceConfig.Fields.PossibleValues, ActOutDataSourceConfig.Fields.TableColumn) });
            if (xDataSourceConfigGrid.SelectedViewName is not null and not "")
            {
                xDataSourceConfigGrid.updateAndSelectCustomView(view);
            }
            else
            {
                xDataSourceConfigGrid.SetAllColumnsDefaultView(view);
            }

            xDataSourceConfigGrid.InitViewItems();
            xDataSourceConfigGrid.SetTitleLightStyle = true;
        }

        private void CheckUnCheckGridRow(object sender, RoutedEventArgs e)
        {
            if (mAction.DSOutputConfigParams.Count > 0)
            {
                bool valueToSet = !mAction.DSOutputConfigParams[0].Active;
                foreach (var elem in mAction.DSOutputConfigParams)
                {
                    elem.Active = valueToSet;
                }
            }
        }

        private void SetActReturnValuesGrid()
        {
            GenerateStoreToVarsList();

            if (!outputValuesGridViewSet)
            {
                outputValuesGridViewSet = true;

                GridViewDef SimView = new GridViewDef(eGridView.All.ToString());
                ObservableList<GridColView> viewCols = [];
                SimView.GridColsView = viewCols;

                //Simulation view
                viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Active, WidthWeight = 60, MaxWidth = 60, StyleType = GridColView.eGridColStyleType.CheckBox });
                viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Param, Header = "Parameter", WidthWeight = 180 });
                viewCols.Add(new GridColView() { Field = "..", Header = " ...", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["ParamValueExpressionButton"] });
                viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Description, Header = "Description", WidthWeight = 150 });
                viewCols.Add(new GridColView() { Field = "...", Header = "  ...", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["DescriptionValueExpressionButton"] });
                viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Path, WidthWeight = 180 });
                viewCols.Add(new GridColView() { Field = "....", WidthWeight = 30, MaxWidth = 30, Header = "  ...", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["PathValueExpressionButton"] });
                viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.SimulatedActual, Header = "Simulated Value", WidthWeight = 180 });
                viewCols.Add(new GridColView() { Field = ".....", Header = "  ...", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["SimulatedlValueExpressionButton"] });
                viewCols.Add(new GridColView() { Field = "<<", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["AddActualToSimulButton"] });
                viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Actual, Header = "Actual Value", WidthWeight = 180, BindingMode = BindingMode.OneWay });
                viewCols.Add(new GridColView() { Field = ".......", Header = "...", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["ShowActualValueButton"] });
                viewCols.Add(new GridColView() { Field = ">>", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["AddActualToExpectButton"] });
                viewCols.Add(new GridColView() { Field = nameof(ActReturnValue.Operator), Header = "Operator", WidthWeight = 130, BindingMode = BindingMode.TwoWay, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = OperatorList });
                // viewCols.Add(new GridColView() { Field = ">>", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["AddActualToExpectButton"] });
                viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Expected, Header = "Expected Value", WidthWeight = 180 });
                viewCols.Add(new GridColView() { Field = "......", Header = "  ...", WidthWeight = 30, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["ValueExpressionButton"] });
                viewCols.Add(new GridColView() { Field = "Clear Expected Value", Header = "X", WidthWeight = 50, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["ClearExpectedValueBtnTemplate"] });
                viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.ExpectedCalculated, Header = "Calculated Expected", WidthWeight = 220, BindingMode = BindingMode.OneWay });
                viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Status, WidthWeight = 100, MaxWidth = 100, BindingMode = BindingMode.OneWay, PropertyConverter = (new ColumnPropertyConverter(new ActReturnValueStatusConverter(), TextBlock.ForegroundProperty)) });
                viewCols.Add(new GridColView()
                {
                    Field = ActReturnValue.Fields.StoreToValue,
                    Header = "Store To",
                    WidthWeight = 350,
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = UCDataMapping.GetTemplate(new UCDataMapping.TemplateOptions(
                        dataTypeProperty: ActReturnValue.Fields.StoreTo,
                        dataValueProperty: ActReturnValue.Fields.StoreToValue)
                    {
                        _VariabelsSourceList = mStoreToVarsList
                    })
                });

                //Default mode view
                xOutputValuesGrid.SetAllColumnsDefaultView(SimView);
                //Custom Dynamic View
                customDynamicView = new GridViewDef(eColumnVisibility.CustomDynamicView.ToString());

                xOutputValuesGrid.AddCustomView(customDynamicView);

                xOutputValuesGrid.InitViewItems();
            }
            xOutputValuesGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshOutputValuesGridElements));
            xOutputValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddReturnValue));

            if (!outputValuesGridToolbarItemsAdded)
            {
                outputValuesGridToolbarItemsAdded = true;
                xOutputValuesGrid.AddSeparator();
                xOutputValuesGrid.AddToolbarTool(eImageType.VerticalBars, "Choose optional columns", new RoutedEventHandler(MultiSelectComboBox_Visbility), imageSize: 14);
                columnMultiSelectComboBox = xOutputValuesGrid.AddMultiSelectComboBox();
                xOutputValuesGrid.AddToolbarTool(eImageType.Reset, "Clear Unused Parameters", new RoutedEventHandler(ClearUnusedParameter), imageSize: 18);
                addParameterAutomaticallyCheckbox = xOutputValuesGrid.AddCheckBox("Auto Add Parameters", null);
                supportSimulationCheckbox = xOutputValuesGrid.AddCheckBox("Support Simulation", new RoutedEventHandler(RefreshOutputColumns));


                //Added the check box list in multi-selected combo box
                columnMultiSelectComboBox.ItemsSource = new Dictionary<string, object>
                        {
                            { "Description", ActReturnValue.Fields.Description },
                            { "Path", ActReturnValue.Fields.Path },
                            { "Actual Value", ActReturnValue.Fields.Actual },
                            { "Expected Value", ActReturnValue.Fields.Expected },
                            { "Store To", ActReturnValue.Fields.StoreTo }
                        };
                columnMultiSelectComboBox.Visibility = Visibility.Collapsed;
                columnMultiSelectComboBox.Margin = new Thickness(0, 0, 15, 0);
                columnMultiSelectComboBox.Width = 80;
                columnMultiSelectComboBox.ItemCheckBoxClick += ColumnMultiSelectComboBox_ItemCheckBoxClick;


                columnPreferences = WorkSpace.Instance.UserProfile.ActionOutputValueUserPreferences;


                foreach (Node node in columnMultiSelectComboBox._nodeList)
                {
                    try
                    {
                        switch (node.Title)
                        {
                            case "Description":
                                node.IsSelected = columnPreferences.Contains("Description", StringComparison.OrdinalIgnoreCase);
                                break;
                            case "Path":
                                node.IsSelected = columnPreferences.Contains("Path", StringComparison.OrdinalIgnoreCase);
                                break;
                            case "Actual Value":
                                node.IsSelected = columnPreferences.Contains("ActualValue", StringComparison.OrdinalIgnoreCase);
                                break;
                            case "Expected Value":
                                node.IsSelected = columnPreferences.Contains("ExpectedValue", StringComparison.OrdinalIgnoreCase);
                                break;
                            case "Store To":
                                node.IsSelected = columnPreferences.Contains("StoreTo", StringComparison.OrdinalIgnoreCase);
                                break;
                            case "All":
                                break;
                            default:
                                Reporter.ToLog(eLogLevel.ERROR, "Invalid format in column preferences");
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Invalid format in column preferences", ex);
                    }
                }

                CheckBox descriptionCheckBox = new CheckBox
                {
                    Content = "Description",
                    IsChecked = columnPreferences.Contains("Description", StringComparison.OrdinalIgnoreCase)
                };


                columnMultiSelectComboBox.CheckBox_Click(descriptionCheckBox, null);
            }
            BindingHandler.ObjFieldBinding(addParameterAutomaticallyCheckbox!, CheckBox.IsCheckedProperty, mAction, nameof(Act.AddNewReturnParams));
            BindingHandler.ObjFieldBinding(supportSimulationCheckbox!, CheckBox.IsCheckedProperty, mAction, nameof(Act.SupportSimulation));

            xOutputValuesGrid.ShowViewCombo = Visibility.Collapsed;
            xOutputValuesGrid.ShowEdit = Visibility.Collapsed;

            xOutputValuesGrid.AllowHorizentalScroll = true;
            xOutputValuesGrid.Grid.MaxHeight = 500;

            xOutputValuesGrid.DataSourceList = mAction.ReturnValues;
        }

        private void MultiSelectComboBox_Visbility(object sender, RoutedEventArgs e)
        {
            if (columnMultiSelectComboBox.Visibility == Visibility.Collapsed)
            {
                columnMultiSelectComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                columnMultiSelectComboBox.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the click event of the check-boxes within the ColumnMultiSelectComboBox.
        /// Updates the GridColsView of the customDynamicView based on the selected columns.
        /// If the "All" check-box is clicked, it selects or deselects all columns accordingly.
        /// Iterates through the node list to add the selected columns to the GridColsView.
        /// Updates the column count and sets the text of the ColumnMultiSelectComboBox to reflect the number of selected columns.
        /// If the action supports simulation, it adds simulated actual columns to the GridColsView.
        /// Finally, it updates and selects the custom view in the xOutputValuesGrid.
        /// </summary>
        private void ColumnMultiSelectComboBox_ItemCheckBoxClick(object? sender, EventArgs e)
        {
            customDynamicView.GridColsView = [];

            if (sender is System.Windows.Controls.CheckBox checkBox)
            {
                if (checkBox.Content.ToString() == "All")
                {


                    if (checkBox.IsChecked == true)
                    {

                        for (int i = 1; i < 6; i++)
                        {
                            columnMultiSelectComboBox._nodeList[i].IsSelected = true;
                        }

                    }
                    else
                    {
                        for (int i = 1; i < 6; i++)
                        {
                            columnMultiSelectComboBox._nodeList[i].IsSelected = false;
                        }
                    }
                }
            }
            columnPreferences = "";
            columnCount = 0;
            foreach (Node node in columnMultiSelectComboBox._nodeList)
            {
                switch (node.Title)
                {
                    case "Description":
                        customDynamicView.GridColsView.Add(new GridColView() { Field = ActReturnValue.Fields.Description, Visible = node.IsSelected, WidthWeight = 180 });
                        customDynamicView.GridColsView.Add(new GridColView() { Field = "...", Header = "  ...", Visible = node.IsSelected });
                        columnCount = node.IsSelected ? columnCount + 1 : columnCount;
                        columnPreferences = node.IsSelected ? "Description," : "";
                        break;

                    case "Path":
                        customDynamicView.GridColsView.Add(new GridColView() { Field = ActReturnValue.Fields.Path, Visible = node.IsSelected, WidthWeight = 180 });
                        customDynamicView.GridColsView.Add(new GridColView() { Field = "....", Header = "  ...", Visible = node.IsSelected });
                        columnCount = node.IsSelected ? columnCount + 1 : columnCount;
                        columnPreferences += node.IsSelected ? "Path," : "";
                        break;

                    case "Actual Value":
                        customDynamicView.GridColsView.Add(new GridColView() { Field = ActReturnValue.Fields.Actual, Visible = node.IsSelected, WidthWeight = 180 });
                        customDynamicView.GridColsView.Add(new GridColView() { Field = ".......", Header = " ...", Visible = node.IsSelected });
                        customDynamicView.GridColsView.Add(new GridColView() { Field = ">>", Visible = node.IsSelected });
                        columnCount = node.IsSelected ? columnCount + 1 : columnCount;
                        columnPreferences += node.IsSelected ? "ActualValue," : "";
                        break;

                    case "Expected Value":
                        customDynamicView.GridColsView.Add(new GridColView() { Field = ActReturnValue.Fields.Expected, Visible = node.IsSelected, WidthWeight = 180 });
                        customDynamicView.GridColsView.Add(new GridColView() { Field = "......", Header = "  ...", Visible = node.IsSelected });
                        customDynamicView.GridColsView.Add(new GridColView() { Field = "Clear Expected Value", Header = "X", Visible = node.IsSelected });
                        columnCount = node.IsSelected ? columnCount + 1 : columnCount;
                        columnPreferences += node.IsSelected ? "ExpectedValue," : "";
                        break;

                    case "Store To":
                        customDynamicView.GridColsView.Add(new GridColView()
                        {
                            Field = ActReturnValue.Fields.StoreToValue,
                            Visible = node.IsSelected,
                            WidthWeight = 350,
                            Header = "Store To"
                        });
                        columnCount = node.IsSelected ? columnCount + 1 : columnCount;
                        columnPreferences += node.IsSelected ? "StoreTo" : "";
                        break;
                    case "All":
                        break;
                    default:
                        Reporter.ToLog(eLogLevel.ERROR, "Invalid format in column preferences");
                        break;
                }
            }



            WorkSpace.Instance.UserProfile.ActionOutputValueUserPreferences = columnPreferences;

            bool isVisible = mAction.SupportSimulation;
            customDynamicView.GridColsView.Add(new GridColView() { Field = ActReturnValue.Fields.SimulatedActual, Header = "Simulated Value", Visible = isVisible, WidthWeight = 200 });
            customDynamicView.GridColsView.Add(new GridColView() { Field = ".....", Header = "  ...", Visible = isVisible });
            customDynamicView.GridColsView.Add(new GridColView() { Field = "<<", Visible = isVisible });

            xOutputValuesGrid.updateAndSelectCustomView(customDynamicView);
            columnMultiSelectComboBox.Text = "Columns (" + columnCount + ")";

        }



        private void GenerateStoreToVarsList()
        {
            List<string> tempList = [];
            if (mActParentBusinessFlow != null)
            {
                tempList = mActParentBusinessFlow.GetAllVariables(mActParentActivity).Where(a => a.SupportSetValue == true).Select(a => a.Name).ToList();
            }
            else
            {
                tempList = WorkSpace.Instance.Solution.Variables.Where(a => a.SupportSetValue == true).Select(a => a.Name).ToList();
                if (mActParentActivity != null)
                {
                    foreach (GingerCore.Variables.VariableBase var in mActParentActivity.Variables)
                    {
                        tempList.Add(var.Name);
                    }
                }
            }
            tempList.Sort();
            if (tempList.Count > 0)
            {
                tempList.Insert(0, string.Empty);//to be used for clearing selection
            }

            //mStoreToVarsList.LoadDataFromList(tempList, true);

            //Add new
            foreach (string var in tempList)
            {
                if (mStoreToVarsList.Contains(var) == false)
                {
                    mStoreToVarsList.Add(var);
                }
            }

            //remove old
            for (int indx = 0; indx < mStoreToVarsList.Count; indx++)
            {
                if (tempList.Contains(mStoreToVarsList[indx]) == false)
                {
                    mStoreToVarsList.RemoveAt(indx);
                    indx--;
                }
            }

            if (mStoreToVarsList.Count > 0)
            {
                mStoreToVarsList.Move(mStoreToVarsList.IndexOf(string.Empty), 0);//making sure the empty option is first
            }
        }

        private void SetActInputValuesGrid()
        {
            //Show/hide if needed
            xInputValuesGrid.SetTitleLightStyle = true;
            xInputValuesGrid.btnAdd.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(AddInputValue));
            xInputValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddInputValue));

            xInputValuesGrid.ClearTools();
            xInputValuesGrid.ShowDelete = System.Windows.Visibility.Visible;
            if (mAction.GetType() == typeof(ActCLIOrchestration) || mAction.GetType() == typeof(ActPublishArtifacts))
            {
                xInputValuesGrid.ShowAdd = System.Windows.Visibility.Visible;
                xInputValuesGrid.ShowClearAll = System.Windows.Visibility.Visible;
            }

            GridViewDef view;
            if (mAction.GetType() == typeof(ActPublishArtifacts))
            {
                view = GetGridViewForFilePathsInputValues();
            }
            else
            { 
                view = GetGridViewForParamValueInputValues(); 
            }
            //view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Value ForDriver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            xInputValuesGrid.SetAllColumnsDefaultView(view);
            xInputValuesGrid.InitViewItems();

            xInputValuesGrid.DataSourceList = mAction.InputValues;
        }

        private GridViewDef GetGridViewForParamValueInputValues()
        {
            return new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView = [
                    new GridColView() { Field = nameof(ActInputValue.Param), WidthWeight = 10 },
                    new GridColView() { Field = nameof(ActInputValue.Value), WidthWeight = 55 },
                    new GridColView() { Field = "...", WidthWeight = 5, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["InputValueExpressionButton"] }
                    ]
            };
        }

        private GridViewDef GetGridViewForFilePathsInputValues()
        {
            return new GridViewDef("OnlyFilePaths")
            {
                GridColsView = [
                    new GridColView() { Field = nameof(ActInputValue.Param), Visible = false},
                    new GridColView() { Field = nameof(ActInputValue.Value), WidthWeight = 55 },
                    new GridColView() { Field = "...", WidthWeight = 5, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["InputValueExpressionButton"]},
                    new GridColView() { Field = "Browse", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["GridInputValuesBrowseBtnTemplate"] }
                    ]
            };
        }

        private void LoadOperationSettingsEditPage(Act a)
        {
            //Each Action need to implement ActionEditPage which return the name of the page for edit
            //TODO: check all action are working and showing the correct Edit Page
            if (a.ActionEditPage != null)
            {
                Page actEditPage = ActionsFactory.GetActionEditPage(a, mContext);

                if (actEditPage != null)
                {
                    // Load the page
                    xActionPrivateConfigsFrame.SetContent(actEditPage);
                    if (actEditPage is ActPublishArtifactsEditPage)
                    {
                        xActionPrivateConfigsFrame.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        xActionPrivateConfigsFrame.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
            else
            {
                xActionPrivateConfigsFrame.Visibility = System.Windows.Visibility.Collapsed;
            }
        }



        //private void NextActionButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

        //    if (ap == null)
        //    {
        //        Reporter.ToUser(eUserMsgKey.CurrentActionNotSaved);
        //    }
        //    else if (ap.grdActions.grdMain.Items.CurrentPosition < ap.grdActions.grdMain.Items.Count - 1)
        //    {
        //        ap.grdActions.grdMain.Items.MoveCurrentToNext();
        //        Act tempact = (Act)ap.grdActions.grdMain.Items.CurrentItem;
        //        if (tempact != null)
        //        {
        //            mAction = tempact;

        //            ActionEditPage actedit = new ActionEditPage(mAction);
        //            actedit.ap = ap;
        //            actedit.ShowAsWindow();
        //            _pageGenericWin.Close();
        //        }
        //    }
        //    else
        //    {
        //        Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Action to move to.");
        //    }

        //    Mouse.OverrideCursor = null;
        //}

        //private void PrevActionButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        //    if (ap == null)
        //    {
        //        Reporter.ToUser(eUserMsgKey.CurrentActionNotSaved);
        //    }
        //    else if (ap.grdActions.grdMain.Items.CurrentPosition >= 1)
        //    {
        //        ap.grdActions.grdMain.Items.MoveCurrentToPrevious();
        //        Act tempact = (Act)ap.grdActions.grdMain.Items.CurrentItem;
        //        if (tempact != null)
        //        {
        //            mAction = tempact;
        //            ActionEditPage actedit = new ActionEditPage(mAction);
        //            actedit.ap = ap;
        //            actedit.ShowAsWindow();
        //            _pageGenericWin.Close();
        //        }
        //    }
        //    else
        //    {
        //        Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Action to move to.");
        //    }

        //    Mouse.OverrideCursor = null;
        //}

        //private async void RunActionInSimulationButton_Click(object sender, RoutedEventArgs e)
        //{
        //    bool originalSimulationFlagValue = mContext.Runner.RunInSimulationMode;
        //    mContext.Runner.RunInSimulationMode = true;

        //    int res = await RunAction().ConfigureAwait(false);

        //    mContext.Runner.RunInSimulationMode = originalSimulationFlagValue;
        //}

        //private async void RunActionButton_Click(object sender, RoutedEventArgs e)
        //{
        //    bool originalSimulationFlagValue = mContext.Runner.RunInSimulationMode;
        //    mContext.Runner.RunInSimulationMode = false;

        //    int res = await RunAction().ConfigureAwait(false);

        //    mContext.Runner.RunInSimulationMode = originalSimulationFlagValue;
        //}

        //private void StopRunBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.StopRun, null);
        //}

        //private void ShowHideRunStopButtons()
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        if (mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running)
        //        {
        //            mRunActionBtn.Visibility = Visibility.Collapsed;
        //            mStopRunBtn.Visibility = Visibility.Visible;
        //        }
        //        else
        //        {
        //            mRunActionBtn.Visibility = Visibility.Visible;
        //            mStopRunBtn.Visibility = Visibility.Collapsed;
        //        }
        //    });
        //}

        //private async Task<int> RunAction()
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        mAction.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
        //        if (mAction.GetType() == typeof(ActLowLevelClicks))
        //            App.MainWindow.WindowState = WindowState.Minimized;
        //        mAction.IsSingleAction = true;

        //        App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.SetupRunnerForExecution, null);

        //        //No need for agent for some actions like DB and read for excel. For other need agent   
        //        if (!(typeof(ActWithoutDriver).IsAssignableFrom(mAction.GetType())))
        //        {
        //            mContext.Runner.SetCurrentActivityAgent();
        //        }

        //        mContext.Runner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;
        //    });

        //    var result = await mContext.Runner.RunActionAsync(mAction, false, true).ConfigureAwait(false);

        //    this.Dispatcher.Invoke(() =>
        //    {
        //        mAction.IsSingleAction = false;
        //        //UpdateTabsHeaders();
        //        //UpdateScreenShots();
        //        Mouse.OverrideCursor = null;
        //    });
        //    return result;
        //}

        private void GridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.Expected, mContext);
            VEEW.ShowAsWindow();
        }
        private void InputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)xInputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), mContext);
            VEEW.ShowAsWindow();
        }
        private void GridAddActualToExpectButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ARV.Expected = ARV.Actual;
        }

        private void GridShowActualValueButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            if (!string.IsNullOrEmpty(ARV.Actual))
            {
                string tempFilePath = GingerCoreNET.GeneralLib.General.CreateTempTextFile(ARV.Actual);

                DocumentEditorPage docPage = new DocumentEditorPage(tempFilePath, enableEdit: false, UCTextEditorTitle: string.Empty);
                docPage.ShowAsWindow("Actual Value");

                GingerCoreNET.GeneralLib.General.DeleteTempTextFile(tempFilePath);
            }
        }

        private void GridAddActualToSimulButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ARV.SimulatedActual = ARV.Actual;
        }

        private void SimulatedOutputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.SimulatedActual, mContext);
            VEEW.ShowAsWindow();
        }

        private string RemoveActionWord(string str)
        {
            return str.Replace("Action", "").Trim();
        }


        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free, bool startupLocationWithOffset = false)
        {
            //changed the style to be free - since many other windows get stuck and doesn't show
            // Need to find a solution if 2 windows show as Dialog...
            string title = "Edit " + RemoveActionWord(mAction.ActionDescription) + " Action";

            RoutedEventHandler closeHandler = CloseWinClicked;
            string closeContent = "Undo & Close";

            ObservableList<Button> winButtons = [];
            Button okBtn = new Button
            {
                Content = "Ok"
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: okBtn, eventName: nameof(ButtonBase.Click), handler: okBtn_Click);


            Button undoBtn = new Button
            {
                Content = "Undo & Close"
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: undoBtn, eventName: nameof(ButtonBase.Click), handler: undoBtn_Click);



            Button saveBtn = new Button
            {
                Content = "Save"
            };
            switch (EditMode)
            {
                case General.eRIPageViewMode.Automation:
                    winButtons.Add(okBtn);
                    //winButtons.Add(undoBtn);

                    //mRunActionBtn.Content = "Run";
                    //mRunActionBtn.Click += new RoutedEventHandler(RunActionButton_Click);
                    //mRunActionBtn.Margin = new Thickness(0, 0, 60, 0);
                    //winButtons.Add(mRunActionBtn);
                    //mSimulateRunBtn.Content = "Simulate Run";
                    //mSimulateRunBtn.Click += new RoutedEventHandler(RunActionInSimulationButton_Click);
                    //ShowHideRunSimulation();
                    //winButtons.Add(mSimulateRunBtn);

                    //mStopRunBtn.Content = "Stop";
                    //mStopRunBtn.Click += new RoutedEventHandler(StopRunBtn_Click);
                    //mStopRunBtn.Margin = new Thickness(0, 0, 60, 0);
                    //winButtons.Add(mStopRunBtn);
                    //mStopRunBtn.Visibility = Visibility.Collapsed;
                    break;

                case General.eRIPageViewMode.SharedReposiotry:
                    title = "Edit Shared Repository " + RemoveActionWord(mAction.ActionDescription) + " Action";
                    WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: saveBtn, eventName: nameof(ButtonBase.Click), handler: SharedRepoSaveBtn_Click);



                    winButtons.Add(saveBtn);
                    winButtons.Add(undoBtn);
                    break;

                case General.eRIPageViewMode.Child:
                    title = "Edit " + RemoveActionWord(mAction.ActionDescription) + " Action";
                    winButtons.Add(okBtn);
                    winButtons.Add(undoBtn);
                    break;

                case General.eRIPageViewMode.ChildWithSave:
                    title = "Edit " + RemoveActionWord(mAction.ActionDescription) + " Action";
                    WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: saveBtn, eventName: nameof(ButtonBase.Click), handler: ParentSaveButton_Click);



                    winButtons.Add(saveBtn);
                    winButtons.Add(undoBtn);
                    break;

                case General.eRIPageViewMode.View:
                case General.eRIPageViewMode.ViewAndExecute:
                    title = "View " + RemoveActionWord(mAction.ActionDescription) + " Action";
                    winButtons.Add(okBtn);
                    closeHandler = new RoutedEventHandler(okBtn_Click);
                    closeContent = okBtn.Content.ToString();
                    break;
            }

            this.Height = 800;
            this.Width = 1000;
            //ShowHideRunStopButtons();
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, closeContent, closeHandler, startupLocationWithOffset: startupLocationWithOffset);
            SwitchingInputValueBoxAndGrid(mAction);
            return saveWasDone;
        }

        private void SetViewMode()
        {
            xActionsDetailsPnl.IsEnabled = false;

            xOperationSettingsPnl.IsEnabled = false;

            xWaitTimeoutPnl.IsEnabled = false;
            xRetryMechanismPnl.IsEnabled = false;

            xAddOutputToDataSourcePnl.IsEnabled = false;
            xDataSourceConfigGrid.ToolsTray.Visibility = Visibility.Collapsed;
            xDataSourceConfigGrid.DisableGridColoumns();
            xOutputValuesGrid.ToolsTray.Visibility = Visibility.Collapsed;
            xOutputValuesGrid.DisableGridColoumns();

            xExecutionReportConfigPnl.IsEnabled = false;
            //xActionRunDetailsPnl.IsEnabled = false;
        }

        private void UndoChangesAndClose()
        {
            IsPageClosing = true;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                mAction.RestoreFromBackup(true);
                _pageGenericWin.Close();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.AskIfToUndoChanges) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                UndoChangesAndClose();
            }
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            // cause prop change so grid will refresh the data
            //mAction.OnPropertyChanged(Act.Fields.Details);
            //mAction.OnPropertyChanged(Act.Fields.FlowControls);
            IsPageClosing = true;
            _pageGenericWin.Close();
        }

        private void SharedRepoSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckIfUserWantToSave();
        }

        private void ParentSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if ((mActParentBusinessFlow != null && Reporter.ToUser(eUserMsgKey.SaveItemParentWarning, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), mActParentBusinessFlow.Name) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                || (mActParentActivity != null && Reporter.ToUser(eUserMsgKey.SaveItemParentWarning, GingerDicser.GetTermResValue(eTermResKey.Activity), mActParentActivity.ActivityName) == Amdocs.Ginger.Common.eUserMsgSelection.Yes))
            {
                if (mActParentBusinessFlow != null)
                {
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mActParentBusinessFlow);
                }
                else
                {
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mActParentActivity);
                }

                saveWasDone = true;
            }

            IsPageClosing = true;
            _pageGenericWin.Close();
        }
        private void CheckIfUserWantToSave()
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mAction, "change") == true)
            {
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mAction);
                saveWasDone = true;
                IsPageClosing = true;
                _pageGenericWin.Close();
            }
        }

        private void cboLocateBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xLocateByCombo.SelectedItem != null && xLocateByCombo.SelectedItem.ToString() == "ByMulitpleProperties")
            {
                sMultiLocatorVals = xLocateValueVE.ValueTextBox.Text;
                xLocateValueVE.Background = System.Windows.Media.Brushes.LightGray;
                xEditLocatorBtn.Width = 30;
                if (mAction.LocateBy != eLocateBy.ByMulitpleProperties)
                {
                    EditLocatorsWindow.sMultiLocatorVals = xLocateValueVE.ValueTextBox.Text;
                    EditLocatorsWindow ELW = new EditLocatorsWindow(mActParentBusinessFlow);
                    ELW.ShowDialog();
                    xLocateValueVE.ValueTextBox.Text = EditLocatorsWindow.sMultiLocatorVals;
                }
            }
            else
            {
                xLocateValueVE.Background = System.Windows.Media.Brushes.White;
                xEditLocatorBtn.Width = 0;
            }
        }

        private void btnEditLocator_Click(object sender, RoutedEventArgs e)
        {
            EditLocatorsWindow.sMultiLocatorVals = xLocateValueVE.ValueTextBox.Text;
            EditLocatorsWindow ELW = new EditLocatorsWindow(mActParentBusinessFlow);
            ELW.ShowDialog();
            xLocateValueVE.ValueTextBox.Text = EditLocatorsWindow.sMultiLocatorVals;
        }

        private void GridParamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.Param, mContext);
            VEEW.ShowAsWindow();
        }
        private void GridDescriptionVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.Description, mContext);
            VEEW.ShowAsWindow();
        }
        private void GridPathVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ARV, ActReturnValue.Fields.Path, mContext);
            VEEW.ShowAsWindow();
        }

        private void actionInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            ActDescriptionPage desPage = new ActDescriptionPage(mAction);
            desPage.ShowAsWindow();
        }

        private void xActionTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetSelectedTabFrameContent();
        }

        private void SetSelectedTabFrameContent()
        {
            if (mAction == null)
            {
                return;
            }

            if (xActionTabs.SelectedItem == xDetailsTab && bool.Parse(xDetailsTab.Tag.ToString()) != true)
            {
                InitDetailsTabView();
            }
            else if (xActionTabs.SelectedItem == xOperationSettingsTab && bool.Parse(xOperationSettingsTab.Tag.ToString()) != true)
            {
                InitOperationSettingsTabView();
            }
            else if (xActionTabs.SelectedItem == xFlowControlTab && bool.Parse(xFlowControlTab.Tag.ToString()) != true)
            {
                InitFlowControlTabView();
            }
            else if (xActionTabs.SelectedItem == xOutputValuesTab && bool.Parse(xOutputValuesTab.Tag.ToString()) != true)
            {
                InitOutputValuesTabView();
            }
            else if (xActionTabs.SelectedItem == xExecutionReportTab && bool.Parse(xExecutionReportTab.Tag.ToString()) != true)
            {
                InitExecutionReportTabView();
            }
            else if (xActionTabs.SelectedItem == xHelpTab && bool.Parse(xHelpTab.Tag.ToString()) != true)
            {
                InitHelpTabView();
            }
        }

        private void UpdateScreenShots()
        {
            xScreenShotsViewPnl.Children.Clear();

            if (mAction != null && mAction.ScreenShots.Count > 0)
            {
                xScreenShotsPnl.Visibility = Visibility.Visible;

                for (int i = 0; i < mAction.ScreenShots.Count; i++)
                {
                    //TODO: clean me when Screen-shots changed to class instead of list of strings
                    // just in case we don't have name, TOOD: fix all places where we add screen shots to include name
                    string Name = "";
                    if (mAction.ScreenShotsNames.Count > i)
                    {
                        Name = mAction.ScreenShotsNames[i];
                    }
                    ScreenShotViewPage screenShotPage = new ScreenShotViewPage(Name, mAction.ScreenShots[i], 0.5);
                    Frame fram = new Frame
                    {
                        NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden
                    };
                    fram.NavigationService.RemoveBackEntry();
                    fram.Margin = new Thickness(20);
                    DockPanel.SetDock(fram, Dock.Top);
                    fram.HorizontalAlignment = HorizontalAlignment.Center;
                    fram.VerticalAlignment = VerticalAlignment.Center;
                    fram.SetContent(screenShotPage);
                    xScreenShotsViewPnl.Children.Add(fram);
                }
            }
            else
            {
                xScreenShotsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void HighLightElementButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: fix me - Currently working with first agent
            ApplicationAgent aa = (ApplicationAgent)((GingerExecutionEngine)mContext.Runner).GingerRunner.ApplicationAgents[0];
            if (aa != null)
            {
                DriverBase driver = ((AgentOperations)aa.Agent.AgentOperations).Driver;
                mContext.Runner.PrepActionValueExpression(mAction);
                if (driver != null)
                {
                    driver.HighlightActElement(mAction);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.MissingImplementation2);
            }
        }

        private void ControlSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationAgent aa = (ApplicationAgent)((GingerExecutionEngine)mContext.Runner).GingerRunner.ApplicationAgents.FirstOrDefault(x => x.AppName == mActParentActivity.TargetApplication);
            if (aa != null)
            {
                if (((AgentOperations)aa.Agent.AgentOperations).Driver == null)
                {
                    aa.Agent.DSList = mDSList;
                    aa.Agent.AgentOperations.StartDriver();
                }
                DriverBase driver = ((AgentOperations)aa.Agent.AgentOperations).Driver;
                //Instead of check make it disabled ?
                if (driver is IWindowExplorer)
                {
                    WindowExplorerPage WEP = new WindowExplorerPage(aa, mContext, mAction);
                    WEP.ShowAsWindow();
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Control selection is not available yet for driver - " + driver.GetType().ToString());
                }
            }
        }

        private void txtTimeout_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (xTimeoutTextBox.Text == String.Empty || xTimeoutTextBox.Text == null)
            {
                xTimeoutTextBox.Text = null;
                xTimeoutTextBox.CaretIndex = 1;
            }
        }

        void UpdateTabsHeaders()
        {
            UpdateFlowControlsTabHeader();
            UpdateOutputValuesTabHeader();
        }

        //Output Tab
        void UpdateOutputValuesTabHeader()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (mAction.ReturnValues.Any())
                {
                    xOutputValuesTabTextBlock.Text = string.Format("Validations / Assignments ({0})", mAction.ReturnValues.Count);
                    xOutputValuesTabHeaderTextBlock.Text = string.Format("Output Values ({0})", mAction.ReturnValues.Count);
                }
                else
                {
                    xOutputValuesTabTextBlock.Text = "Validations / Assignments";
                    xOutputValuesTabHeaderTextBlock.Text = "Output Values";
                }
            });
        }

        public class ActReturnValueStatusConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                    System.Globalization.CultureInfo culture)
            {
                string? status = value == null ? "" : value.ToString();
#pragma warning disable CS8603 // Possible null reference return.
                return status switch
                {
                    nameof(eRunStatus.Passed) => Application.Current.FindResource("$PassedStatusColor") as System.Windows.Media.Brush,
                    nameof(eRunStatus.Failed) => Application.Current.FindResource("$FailedStatusColor") as System.Windows.Media.Brush,
                    nameof(eRunStatus.FailIgnored) => Application.Current.FindResource("$IgnoredStatusColor") as System.Windows.Media.Brush,
                    nameof(eRunStatus.Pending) => Application.Current.FindResource("$PendingStatusColor") as System.Windows.Media.Brush,
                    nameof(eRunStatus.Running) => Application.Current.FindResource("$RunningStatusColor") as System.Windows.Media.Brush,
                    nameof(eRunStatus.Stopped) => Application.Current.FindResource("$StoppedStatusColor") as System.Windows.Media.Brush,
                    nameof(eRunStatus.Blocked) => Application.Current.FindResource("$BlockedStatusColor") as System.Windows.Media.Brush,
                    nameof(eRunStatus.Skipped) => Application.Current.FindResource("$SkippedStatusColor") as System.Windows.Media.Brush,
                    _ => Application.Current.FindResource("$PendingStatusColor") as System.Windows.Media.Brush,
                };
#pragma warning restore CS8603 // Possible null reference return.

            }

            public object ConvertBack(object value, Type targetType,
                object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private void UpdateFlowControlsTabHeader()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (mAction.FlowControls.Any())
                {
                    xFlowControlTabHeaderTextBlock.Text = string.Format("Flow Control ({0})", mAction.FlowControls.Count);
                }
                else
                {
                    xFlowControlTabHeaderTextBlock.Text = "Flow Control";
                }
            });
        }

        private void GridDSVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActReturnValue ARV = (ActReturnValue)xOutputValuesGrid.CurrentItem;
            ActDataSourcePage ADSP = new ActDataSourcePage(ARV, ActReturnValue.Fields.StoreToDataSource);
            ADSP.ShowAsWindow();
        }

        private void xDataSourceExpander_Expanded(object sender, RoutedEventArgs e)
        {
            SetDataSourceConfigTabView();
        }

        private void AddOutDS_Checked(object sender, RoutedEventArgs e)
        {
            PopulateDataSourceNames();

            // Added Check to get already saved ActOutDataSourceConfig params
            if (mAction.DSOutputConfigParams.Count > 0)
            {
                if (mAction.OutDSParamMapType == null)
                {
                    mAction.OutDSParamMapType = mAction.DSOutputConfigParams[0].OutParamMap;
                }
                if (mAction.OutDataSourceName == null || mAction.OutDataSourceTableName == null)
                {
                    mAction.OutDataSourceName = mAction.DSOutputConfigParams[0].DSName;
                    mAction.OutDataSourceTableName = mAction.DSOutputConfigParams[0].DSTable;
                }
            }
            if (mAction.OutDataSourceName != null && mAction.OutDataSourceTableName != null && mAction.OutDataSourceName != "" && mAction.OutDataSourceTableName != "")
            {
                xDataSourceNameCombo.SelectedValue = mAction.OutDataSourceName;
                xDataSourceTableNameCombo.SelectedValue = mAction.OutDataSourceTableName;
            }
            else
            {
                xDataSourceNameCombo.SelectedIndex = 0;
                mDataSourceName = mDSNames[0];
            }

            PopulateParamMapTypes();
            if (mAction.OutDSParamMapType is null or "")
            {
                xdsOutputParamMapType.SelectedValue = Act.eOutputDSParamMapType.ParamToRow;
            }
            else
            {
                Enum.TryParse(mAction.OutDSParamMapType, out Act.eOutputDSParamMapType selecedEnumVal);
                xdsOutputParamMapType.SelectedValue = selecedEnumVal;
            }


            SetDataSourceConfigTabView();

            UpdateOutputValuesTabHeader();
        }

        private void PopulateDataSourceNames()
        {
            mDSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            if (!mDSList.Any())
            {
                return;
            }

            mDSNames.Clear();
            foreach (DataSourceBase ds in mDSList)
            {
                mDSNames.Add(ds.Name);
            }

            GingerCore.General.FillComboFromList(xDataSourceNameCombo, mDSNames);
        }

        private void PopulateParamMapTypes()
        {
            xdsOutputParamMapType.ItemsSource = Enum.GetValues(typeof(Act.eOutputDSParamMapType)).Cast<Act.eOutputDSParamMapType>();
        }

        private void AddOutDS_Unchecked(object sender, RoutedEventArgs e)
        {
            mAction.DSOutputConfigParams.Clear();
            SetDataSourceConfigTabView();
            UpdateOutputValuesTabHeader();
        }
        private void SetDataSourceConfigTabView()
        {
            if (mAction.ConfigOutputDS)
            {
                xAddOutputToDataSourceConfigPnl.Visibility = Visibility.Visible;
            }
            else
            {
                xAddOutputToDataSourceConfigPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void updateDSOutGrid()
        {
            if (xDataSourceTableNameCombo == null || xDataSourceTableNameCombo.Items.Count == 0 || xDataSourceTableNameCombo.SelectedValue == null)
            {
                return;
            }

            List<ActOutDataSourceConfig> DSConfigParam = mAction.DSOutputConfigParams.Where(x => x.DSName == mDataSourceName && x.DSTable == mDSTable.Name && x.OutParamMap == mAction.OutDSParamMapType).ToList();
            SetDataSourceConfigTabView();

            if (mDSTable.DSTableType == DataSourceTable.eDSTableType.Customized)
            {
                xdsOutputParamMapType.IsEnabled = true;
                mColNames.Remove("GINGER_ID");
                if (mColNames.Contains("GINGER_LAST_UPDATED_BY"))
                {
                    mColNames.Remove("GINGER_LAST_UPDATED_BY");
                }

                if (mColNames.Contains("GINGER_LAST_UPDATE_DATETIME"))
                {
                    mColNames.Remove("GINGER_LAST_UPDATE_DATETIME");
                }

                if (mColNames.Contains("GINGER_USED"))
                {
                    mColNames.Remove("GINGER_USED");
                }

                if (mAction.OutDSParamMapType == Act.eOutputDSParamMapType.ParamToRow.ToString())
                {
                    mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter.ToString(), "Parameter", "", mColNames);
                    mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Path.ToString(), "Path", "", mColNames);
                    mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Actual.ToString(), "Actual", "", mColNames);
                }
                else
                {
                    foreach (ActOutDataSourceConfig oDSParam in DSConfigParam)
                    {
                        mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, oDSParam.OutputType, oDSParam.TableColumn, mAction.ConfigOutDSParamAutoCheck.ToString(), mColNames, mAction.OutDSParamMapType);
                    }
                }
            }
            else
            {
                xdsOutputParamMapType.IsEnabled = false;
                xdsOutputParamMapType.SelectedValue = Act.eOutputDSParamMapType.ParamToRow;
                mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter.ToString(), "GINGER_KEY_NAME");
                mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter_Path.ToString(), "GINGER_KEY_NAME");
                mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Actual.ToString(), "GINGER_KEY_VALUE");

            }

            DSConfigParam = mAction.DSOutputConfigParams.Where(x => x.DSName == mDataSourceName && x.DSTable == mDSTable.Name && x.OutParamMap == mAction.OutDSParamMapType).ToList();
            List<ActOutDataSourceConfig> aOutDSConfigParam = [.. DSConfigParam];


            xDataSourceConfigGrid.Visibility = Visibility.Visible;

            SetActDataSourceConfigGrid();
            //mAction.DSOutputConfigParams = aOutDSConfigParam;
            if (aOutDSConfigParam.Count > 0)
            {
                mAction.DSOutputConfigParams.Clear();
                aOutDSConfigParam.ForEach(param => mAction.DSOutputConfigParams.Add(param));
            }
            xDataSourceConfigGrid.DataSourceList = mAction.DSOutputConfigParams;
        }

        private void cmbDataSourceTableName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xDataSourceTableNameCombo == null || xDataSourceTableNameCombo.Items.Count == 0 || xDataSourceTableNameCombo.SelectedValue == null)
            {
                return;
            }

            foreach (DataSourceTable dst in mDSTableList)
            {
                if (dst.Name == xDataSourceTableNameCombo.SelectedValue.ToString())
                {
                    mDSTable = dst;

                    mColNames = mDSTable.DSC.GetColumnList(mDSTable.Name);
                    if (mAction.OutDSParamMapType == null)
                    {
                        mAction.OutDSParamMapType = Act.eOutputDSParamMapType.ParamToRow.ToString();
                    }

                    xdsOutputParamMapType.SelectedValue = mAction.OutDSParamMapType;
                    updateDSOutGrid();
                    SetDSGridVisibility();
                    break;
                }
            }
        }

        private void DataSourceConfigGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            if (mDSTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
            {
                if (e.OriginalSource.GetType() == typeof(CheckBox))
                {
                    DataGridCell cell = (DataGridCell)((CheckBox)e.OriginalSource).Parent;
                    ActOutDataSourceConfig currRow = (ActOutDataSourceConfig)cell.DataContext;
                    if (currRow.OutputType == "Actual")
                    {
                        mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Actual.ToString(), "GINGER_KEY_VALUE", "true");
                    }
                    else if (currRow.OutputType == "Parameter")
                    {
                        mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter_Path.ToString(), "GINGER_KEY_NAME", (!currRow.Active).ToString());
                    }
                    else
                    {
                        mAction.AddOrUpdateOutDataSourceParam(mDataSourceName, mDSTable.Name, ActOutDataSourceConfig.eOutputType.Parameter.ToString(), "GINGER_KEY_NAME", (!currRow.Active).ToString());
                    }

                    xDataSourceConfigGrid.DataSourceList = mAction.DSOutputConfigParams;
                }
            }
        }

        private void cmbDataSourceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xDataSourceNameCombo.Items.IsEmpty || xDataSourceNameCombo.SelectedValue == null)
            {
                return;
            }

            string dataSourceName = xDataSourceNameCombo.SelectedValue.ToString()!;

            SetSelectedDataSource(dataSourceName);
            SetSelectedDataSourceTableByIndex(index: 0);
        }

        private void SetSelectedDataSource(string dataSourceName)
        {
            DataSourceBase? dataSource = mDSList.FirstOrDefault(ds => string.Equals(ds.Name, dataSourceName));
            if (dataSource == null)
            {
                return;
            }

            mDataSourceName = dataSourceName;
            dataSource.FileFullPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(dataSource.FilePath);

            PopulateDataSourceTableNames(dataSource.GetTablesList());
        }

        private void PopulateDataSourceTableNames(IEnumerable<DataSourceTable> tables)
        {
            mDSTableList.Clear();
            List<string> dsTableNames = [];
            foreach (DataSourceTable table in tables)
            {
                mDSTableList.Add(table);
                dsTableNames.Add(table.Name);
                //if (string.Equals(xDataSourceTableNameCombo.SelectedValue?.ToString(), table.Name))
                if (string.Equals(mAction.OutDataSourceTableName, table.Name))
                {
                    mDSTable = table;
                    mColNames = table.DSC.GetColumnList(table.Name);
                }
            }

            if (mDSTableList.Count == 0)
            {
                return;
            }

            GingerCore.General.FillComboFromList(xDataSourceTableNameCombo, dsTableNames);
        }

        private void SetSelectedDataSourceTableByIndex(int index)
        {
            mDSTable = mDSTableList[index];
            xDataSourceTableNameCombo.SelectedIndex = index;
        }

        //private void ShowHideRunSimulation()
        //{
        //    if (mAction.SupportSimulation)
        //        mSimulateRunBtn.Visibility = Visibility.Visible;
        //    else
        //        mSimulateRunBtn.Visibility = Visibility.Collapsed;
        //}

        private void ActionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Act.Status))
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running)
                    {
                        CollectionChangedEventManager.RemoveHandler(source: mAction.ReturnValues, handler: ReturnValues_CollectionChanged);
                        xOutputValuesGrid.DataSourceList = null;
                    }
                    else
                    {
                        CollectionChangedEventManager.RemoveHandler(source: mAction.ReturnValues, handler: ReturnValues_CollectionChanged);
                        CollectionChangedEventManager.AddHandler(source: mAction.ReturnValues, handler: ReturnValues_CollectionChanged);
                        xOutputValuesGrid.DataSourceList = mAction.ReturnValues;
                        ReturnValues_CollectionChanged(null, null);
                    }
                });
            }

        }



        private void InitActionLog()
        {
            if (mAction.EnableActionLogConfig)
            {
                ShowActionLogConfig();
            }
        }

        private void ShowActionLogConfig()
        {
            if (mAction.ActionLogConfig == null)
            {
                mAction.ActionLogConfig = new ActionLogConfig();
            }
            xActionLogConfigFrame.SetContent(new ActionLogConfigPage(mAction.ActionLogConfig));
        }

        private void EnableActionLogConfigCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mAction.EnableActionLogConfig = true;
            if (mAction.ActionLogConfig == null)
            {
                mAction.ActionLogConfig = new ActionLogConfig();
            }
            ResetActionLog();
        }

        private void EnableActionLogConfigCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            if (mAction != null)
            {
                mAction.EnableActionLogConfig = false;
            }
            ResetActionLog();
        }

        private void ResetActionLog()
        {
            if (mAction != null && mAction.EnableActionLogConfig)
            {
                ShowActionLogConfig();
            }
            else
            {
                xActionLogConfigFrame.SetContent(null);
            }
        }

        private void GridClearExpectedValueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xOutputValuesGrid.Grid.SelectedItem != null)
            {
                ((ActReturnValue)xOutputValuesGrid.Grid.SelectedItem).Expected = null;
            }
        }

        public void ClearPageBindings()
        {
            StopEdit();
            BindingOperations.ClearAllBindings(xDescriptionTextBox);
            BindingOperations.ClearAllBindings(xLocateByCombo);
            BindingOperations.ClearAllBindings(xWindowsToCaptureCombo);
            BindingOperations.ClearAllBindings(xLocateValueVE);
            BindingOperations.ClearAllBindings(xExecutionTimeLbl);
            BindingOperations.ClearAllBindings(xExecutionErrorDetailsText);
            BindingOperations.ClearAllBindings(xExecutionExtraInfoText);
            BindingOperations.ClearAllBindings(xLocateValueVE);
            BindingOperations.ClearAllBindings(xTakeScreenShotCheckBox);
            BindingOperations.ClearAllBindings(xFailIgnoreCheckBox);
            BindingOperations.ClearAllBindings(xStatusConvertorCombo);
            BindingOperations.ClearAllBindings(xWaitVeUC);
            BindingOperations.ClearAllBindings(xTimeoutTextBox);
            BindingOperations.ClearAllBindings(xEnableRetryMechanismCheckBox);
            BindingOperations.ClearAllBindings(xRetryMechanismIntervalTextBox);
            BindingOperations.ClearAllBindings(xRetryMechanismMaxRetriesTextBox);
            BindingOperations.ClearAllBindings(xAddOutToDSCheckbox);
            BindingOperations.ClearAllBindings(xDataSourceNameCombo);
            BindingOperations.ClearAllBindings(xDataSourceTableNameCombo);
            BindingOperations.ClearAllBindings(xdsOutputParamMapType);
            BindingOperations.ClearAllBindings(xEnableActionLogConfigCheckBox);
            BindingOperations.ClearAllBindings(xLocateValueVE);
            xValueVE.ClearControlsBindings();
            BindingOperations.ClearAllBindings(xValueVE);

            BindingOperations.ClearAllBindings(xRawResponseValuesBtn);
            xTagsViewer.ClearBinding();
            //this.ClearControlsBindings();
            if (mAction != null)
            {
                string allProperties = string.Empty;
                PropertyChangedEventManager.RemoveHandler(source: mAction, ActionPropertyChanged, propertyName: allProperties);
                CollectionChangedEventManager.RemoveHandler(source: mAction.InputValues, handler: InputValues_CollectionChanged);
                CollectionChangedEventManager.RemoveHandler(source: mAction.FlowControls, handler: FlowControls_CollectionChanged);
                CollectionChangedEventManager.RemoveHandler(source: mAction.ReturnValues, handler: ReturnValues_CollectionChanged);
                CollectionChangedEventManager.RemoveHandler(source: mAction.ScreenShots, handler: ScreenShots_CollectionChanged);
                CollectionChangedEventManager.RemoveHandler(source: mAction.Artifacts, handler: Artifacts__CollectionChanged);
                mAction = null;
            }
            xFlowControlConditionsFrame.NavigationService.RemoveBackEntry();
            xActionPrivateConfigsFrame.NavigationService.RemoveBackEntry();
            xActionHelpDetailsFram.NavigationService.RemoveBackEntry();
            xActionLogConfigFrame.NavigationService.RemoveBackEntry();
        }

        private void XEnableRetryMechanismCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            SetRetryMechConfigsPnlView();
        }

        private void SetRetryMechConfigsPnlView()
        {
            if (xEnableRetryMechanismCheckBox.IsChecked == true)
            {
                xRetryMechConfigsPnl.Visibility = Visibility.Visible;
                xRetryExpander.IsExpanded = true;
                if (mAction?.MaxNumberOfRetries != null)
                {
                    xRetryMechanismMaxRetriesTextBox.Text = mAction.MaxNumberOfRetries.ToString();
                }
            }
            else
            {
                xRetryMechConfigsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void XTakeScreenShotCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            SetScreenshotsPnlView();
        }

        private void SetScreenshotsPnlView()
        {
            if (xTakeScreenShotCheckBox.IsChecked == true)
            {
                xScreenshotsCaptureTypeConfigsPnl.Visibility = Visibility.Visible;
            }
            else
            {
                xScreenshotsCaptureTypeConfigsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void XHelpButton_Click(object sender, RoutedEventArgs e)
        {
            xActionTabs.SelectedItem = xHelpTab;
        }

        private void xdsOutputParamAutoCheck_Click(object sender, RoutedEventArgs e)
        {
            SetDSGridVisibility();
        }

        private void SetDSGridVisibility()
        {
            if (xdsOutputParamAutoCheck.IsChecked == true)
            {
                xDataSourceConfigGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                xDataSourceConfigGrid.Visibility = Visibility.Visible;
            }
        }

        private void OutDSParamType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xdsOutputParamMapType.SelectedValue is not null)
            {
                mAction.OutDSParamMapType = ((eOutputDSParamMapType)xdsOutputParamMapType.SelectedValue).ToString();
            }
            updateDSOutGrid();
            SetDSGridVisibility();
        }

        private void xRawResponseValuesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mAction.RawResponseValues != string.Empty)
            {
                string tempFilePath = GingerCoreNET.GeneralLib.General.CreateTempTextFile(mAction.RawResponseValues);
                if (System.IO.File.Exists(tempFilePath))
                {
                    DocumentEditorPage docPage = new DocumentEditorPage(tempFilePath, enableEdit: false, UCTextEditorTitle: string.Empty)
                    {
                        Width = 800,
                        Height = 800
                    };
                    docPage.ShowAsWindow("Raw Output Values");
                    System.IO.File.Delete(tempFilePath);
                    return;
                }
            }
            Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to load raw response view, see log for details.");
        }

        protected override void IsVisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (EditMode == General.eRIPageViewMode.SharedReposiotry && mAction != null && !String.IsNullOrEmpty(mAction.ContainingFolder))
            {
                CurrentItemToSave = mAction;
                base.IsVisibleChangedHandler(sender, e);
            }
        }

        private void xTimeoutTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            bool isEmptyString = string.IsNullOrEmpty(e.Text);
            if (isEmptyString)
            {
                //allow
                e.Handled = false;
                return;
            }

            string currentText = xTimeoutTextBox.Text != null ? xTimeoutTextBox.Text : string.Empty;
            bool isValidInteger = int.TryParse(currentText + e.Text, out _);
            if (isValidInteger)
            {
                //allow
                e.Handled = false;
                return;
            }
            else
            {
                //don't allow
                e.Handled = true;
            }

        }

        private void GridInputValuesBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue item = (ActInputValue)xInputValuesGrid.CurrentItem;

            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = "*.*",
                Filter = "All files (All Files)|*.*"
            }) is string fileName)
            {
                item.Value = fileName;
                xInputValuesGrid.DataSourceList.CurrentItem = item;
            }
        }
    }
}
