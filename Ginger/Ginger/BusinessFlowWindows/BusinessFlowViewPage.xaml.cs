#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Common.Repository;
using Ginger;
using Ginger.Agents;
using Ginger.ALM;
using Ginger.AnalyzerLib;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowWindows;
using Ginger.Repository;
using Ginger.SolutionWindows;
using Ginger.UserControlsLib;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for BusinessFlowViewPage.xaml
    /// </summary>
    public partial class BusinessFlowViewPage : GingerUIPage
    {
        BusinessFlow mBusinessFlow;
        Context mContext;
        Ginger.General.eRIPageViewMode mPageViewMode;
        private readonly bool _ignoreValidationRules;

        public ActivitiesListViewPage mActivitiesPage;
        public VariabelsListViewPage mVariabelsPage;
        BusinessFlowConfigurationsPage mConfigurationsPage;

        GenericWindow mGenericWin = null;

        public BusinessFlowViewPage(BusinessFlow businessFlow, Context context, Ginger.General.eRIPageViewMode pageViewMode, bool ignoreValidationRules = false)
        {
            InitializeComponent();
            ApplicationAgentsMapPage.BusinessFlowTargetApplicationChanged -= UpdateInfoSection;
            ApplicationAgentsMapPage.BusinessFlowTargetApplicationChanged += UpdateInfoSection;

            mBusinessFlow = businessFlow;
            CurrentItemToSave = mBusinessFlow;
            mContext = context;
            _ignoreValidationRules = ignoreValidationRules;
            if (mContext == null)
            {
                mContext = new Context();
            }
            mContext.BusinessFlow = mBusinessFlow;
            mPageViewMode = pageViewMode;

            SetUIView();
            BindControlsToBusinessFlow();
        }

        private void SetUIView()
        {
            if (mPageViewMode != Ginger.General.eRIPageViewMode.Standalone)
            {
                //xAutomateBtn.Visibility = Visibility.Collapsed;
                //xAutomateSplitter.Visibility = Visibility.Collapsed;
                xOperationsPnl.Visibility = Visibility.Collapsed;
            }
        }
        string allProperties = string.Empty;

        private void BindControlsToBusinessFlow()
        {
            //General Info Section Bindings
            BindingHandler.ObjFieldBinding(xNameTextBlock, TextBlock.TextProperty, mBusinessFlow, nameof(BusinessFlow.Name));
            BindingHandler.ObjFieldBinding(xNameTextBlock, TextBlock.ToolTipProperty, mBusinessFlow, nameof(BusinessFlow.Name));
            PropertyChangedEventManager.RemoveHandler(source: mBusinessFlow, handler: mBusinessFlow_PropertyChanged, propertyName: allProperties);
            PropertyChangedEventManager.AddHandler(source: mBusinessFlow, handler: mBusinessFlow_PropertyChanged, propertyName: allProperties);
            CollectionChangedEventManager.RemoveHandler(source: mBusinessFlow.Tags, handler: Tags_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: mBusinessFlow.Tags, handler: Tags_CollectionChanged);
            CollectionChangedEventManager.RemoveHandler(source: mBusinessFlow.TargetApplications, handler: TargetApplications_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: mBusinessFlow.TargetApplications, handler: TargetApplications_CollectionChanged);

            TargetApplicationsPage.OnActivityUpdate -= UpdateInfoSection;
            TargetApplicationsPage.OnActivityUpdate += UpdateInfoSection;
            UpdateInfoSection();
            //Activities Tab Bindings
            CollectionChangedEventManager.RemoveHandler(source: mBusinessFlow.Activities, handler: Activities_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: mBusinessFlow.Activities, handler: Activities_CollectionChanged);
            UpdateActivitiesTabHeader();
            if (mActivitiesPage != null && xActivitisTab.IsSelected)
            {
                mActivitiesPage.UpdateBusinessFlow(mBusinessFlow);
            }

            //Variables Tab Bindings      
            CollectionChangedEventManager.RemoveHandler(source: mBusinessFlow.Variables, handler: Variables_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: mBusinessFlow.Variables, handler: Variables_CollectionChanged);
            UpdateVariabelsTabHeader();
            if (mVariabelsPage != null && xVariablesTab.IsSelected)
            {
                mVariabelsPage.UpdateParent(mBusinessFlow);
            }

            //Configurations Tab Bindings
            if (mConfigurationsPage != null && xDetailsTab.IsSelected)
            {
                mConfigurationsPage.UpdateBusinessFlow(mBusinessFlow);
            }
        }

        private void TargetApplications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateInfoSection();
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateInfoSection();
        }

        TabItem mLastSelectedTab = null;
        private void XItemsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Ginger.General.eRIPageViewMode childPagesMode;
            if (mPageViewMode == Ginger.General.eRIPageViewMode.View)
            {
                childPagesMode = Ginger.General.eRIPageViewMode.View;
            }
            else if (mPageViewMode == Ginger.General.eRIPageViewMode.ViewAndExecute)
            {
                childPagesMode = Ginger.General.eRIPageViewMode.ViewAndExecute;
            }
            else
            {
                childPagesMode = Ginger.General.eRIPageViewMode.Child;
            }
            if (xItemsTabs.SelectedItem != mLastSelectedTab)
            {
                if (xActivitisTab.IsSelected == true)
                {
                    if (mActivitiesPage == null)
                    {
                        mActivitiesPage = new ActivitiesListViewPage(mBusinessFlow, mContext, childPagesMode);
                        mActivitiesPage.ListView.ListSelectionMode = SelectionMode.Extended;
                        mActivitiesPage.ListView.ListTitleVisibility = Visibility.Collapsed;
                        xActivitiesTabFrame.SetContent(mActivitiesPage);
                    }
                    else
                    {
                        mActivitiesPage.UpdateBusinessFlow(mBusinessFlow);
                    }
                }
                else if (xVariablesTab.IsSelected == true)
                {
                    if (mVariabelsPage == null)
                    {
                        mVariabelsPage = new VariabelsListViewPage(mBusinessFlow, mContext, childPagesMode);
                        if (mVariabelsPage.ListView != null)
                        {
                            mVariabelsPage.ListView.ListTitleVisibility = Visibility.Collapsed;
                        }
                        xVariabelsTabFrame.SetContent(mVariabelsPage);
                    }
                    else
                    {
                        mVariabelsPage.UpdateParent(mBusinessFlow);
                    }
                }
                else if (xDetailsTab.IsSelected == true)
                {
                    if (mConfigurationsPage == null)
                    {
                        mConfigurationsPage = new BusinessFlowConfigurationsPage(mBusinessFlow, mContext, childPagesMode, _ignoreValidationRules);
                        xDetailsTabFrame.SetContent(mConfigurationsPage);
                    }
                    else
                    {
                        mConfigurationsPage.UpdateBusinessFlow(mBusinessFlow);
                    }
                }

                mLastSelectedTab = (TabItem)xItemsTabs.SelectedItem;
            }
        }

        private void ClearBusinessFlowBindings()
        {
            PropertyChangedEventManager.RemoveHandler(source: mBusinessFlow, handler: mBusinessFlow_PropertyChanged, propertyName: allProperties);
            CollectionChangedEventManager.RemoveHandler(source: mBusinessFlow.Activities, handler: Activities_CollectionChanged);
            CollectionChangedEventManager.RemoveHandler(source: mBusinessFlow.Variables, handler: Variables_CollectionChanged);
        }

        public void UpdateBusinessFlow(BusinessFlow businessFlow)
        {
            if (mBusinessFlow != businessFlow)
            {
                ClearBusinessFlowBindings();
                mBusinessFlow = businessFlow;
                mContext.BusinessFlow = mBusinessFlow;
                if (mBusinessFlow != null)
                {
                    BindControlsToBusinessFlow();
                }
            }
        }

        private void mBusinessFlow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is (nameof(BusinessFlow.Description)) or (nameof(BusinessFlow.TargetApplications)))
            {
                UpdateInfoSection();
            }
        }

        private void UpdateInfoSection()
        {
            this.Dispatcher.Invoke(() =>
            {
                xDescriptionTextBlock.Text = string.Empty;
                TextBlockHelper xDescTextBlockHelper = new TextBlockHelper(xDescriptionTextBlock);
                //SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$PrimaryColor_Black")).ToString());

                if (!string.IsNullOrEmpty(mBusinessFlow.Description))
                {
                    if (mBusinessFlow.Description.Length > 100)
                    {
                        xDescTextBlockHelper.AddText("Description: " + mBusinessFlow.Description[..99] + "...");
                    }
                    else
                    {
                        xDescTextBlockHelper.AddText("Description: " + mBusinessFlow.Description);
                    }
                    xDescTextBlockHelper.AddText(" " + Ginger.General.GetTagsListAsString(mBusinessFlow.Tags));
                    xDescTextBlockHelper.AddLineBreak();
                }
                else
                {
                    xDescTextBlockHelper.AddText("Description: " + Ginger.General.GetTagsListAsString(mBusinessFlow.Tags));
                    xDescTextBlockHelper.AddLineBreak();
                }
                //if (!string.IsNullOrEmpty(mBusinessFlow.RunDescription))
                //{
                //    xDescTextBlockHelper.AddText("Run Description: " + mBusinessFlow.RunDescription);
                //    xDescTextBlockHelper.AddLineBreak();
                //}
                xDescTextBlockHelper.AddText("Target/s: ");
                foreach (TargetBase target in mBusinessFlow.TargetApplications)
                {
                    xDescTextBlockHelper.AddText(target.Name);
                    if (mBusinessFlow.TargetApplications.IndexOf(target) + 1 < mBusinessFlow.TargetApplications.Count)
                    {
                        xDescTextBlockHelper.AddText(", ");
                    }
                }
                xDescTextBlockHelper.AddLineBreak();
            });
        }

        private void Activities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateActivitiesTabHeader();
        }

        private void Variables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateVariabelsTabHeader();
        }

        private void UpdateVariabelsTabHeader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xVariabelsTabHeaderText.Text = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Variables), mBusinessFlow.Variables.Count);
            });
        }
        private void UpdateActivitiesTabHeader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xActivitiesTabHeaderText.Text = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Activities), mBusinessFlow.Activities.Count);
            });
        }

        private void xAutomate_Click(object sender, RoutedEventArgs e)
        {
            if (mGenericWin != null)
            {
                mGenericWin.Close();
            }
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.Automate, mBusinessFlow);
        }

        //FindAndReplacePage mfindAndReplacePageAutomate = null;
        //private void xSearchBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (mfindAndReplacePageAutomate == null)
        //    {
        //        mfindAndReplacePageAutomate = new FindAndReplacePage(FindAndReplacePage.eContext.AutomatePage, mBusinessFlow);
        //    }
        //    mfindAndReplacePageAutomate.ShowAsWindow();
        //}

        private void xAnalyzeBtn_Click(object sender, RoutedEventArgs e)
        {
            AnalyzerPage AP = new AnalyzerPage();
            AP.Init(mBusinessFlow);
            AP.ShowAsWindow();
        }

        private void xExportToAlmMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ALMIntegration.Instance.ExportBusinessFlowToALM(mBusinessFlow, true);
        }

        private void xExportToCSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Ginger.Export.GingerToCSV.BrowseForFilename();
            Ginger.Export.GingerToCSV.BusinessFlowToCSV(mBusinessFlow);
        }

        bool mSaveWasDone = false;
        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            string title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
            RoutedEventHandler CloseHandler = CloseWinClicked;
            string closeContent = "Undo & Close";
            ObservableList<Button> winButtons = [];
            switch (mPageViewMode)
            {
                //case Ginger.General.eRIPageViewMode.Automation:
                //    Button okBtn = new Button();
                //    okBtn.Content = "Ok";
                //    okBtn.Click += new RoutedEventHandler(okBtn_Click);
                //    Button undoBtn = new Button();
                //    undoBtn.Content = "Undo & Close";
                //    undoBtn.Click += new RoutedEventHandler(undoAndCloseBtn_Click);
                //    winButtons.Add(undoBtn);
                //    winButtons.Add(okBtn);
                //    break;

                case Ginger.General.eRIPageViewMode.Standalone:
                    mBusinessFlow.SaveBackup();
                    title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                    Button saveBtn = new Button
                    {
                        Content = "Save"
                    };
                    WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: saveBtn, eventName: nameof(ButtonBase.Click), handler: SaveBtn_Click);
                    Button undoBtnSr = new Button
                    {
                        Content = "Undo & Close"
                    };
                    WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: undoBtnSr, eventName: nameof(ButtonBase.Click), handler: UndoAndCloseBtn_Click);
                    winButtons.Add(undoBtnSr);
                    winButtons.Add(saveBtn);
                    break;

                case Ginger.General.eRIPageViewMode.View:
                case Ginger.General.eRIPageViewMode.ViewAndExecute:
                    title = "View " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                    Button okBtnView = new Button
                    {
                        Content = "Ok"
                    };
                    WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: okBtnView, eventName: nameof(ButtonBase.Click), handler: okBtn_Click);
                    winButtons.Add(okBtnView);
                    CloseHandler = new RoutedEventHandler(okBtn_Click);
                    closeContent = okBtnView.Content.ToString();
                    break;
            }

            GingerCore.General.LoadGenericWindow(ref mGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, closeContent, CloseHandler, startupLocationWithOffset: startupLocationWithOffset);
            return mSaveWasDone;
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.AskIfToUndoChanges) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                UndoChanges();
                mGenericWin.Close();
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mBusinessFlow, "change") == true)
            {
                mSaveWasDone = true;
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mBusinessFlow);
                mGenericWin.Close();
            }
        }

        private void UndoAndCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChanges();
            mGenericWin.Close();
        }

        private void UndoChanges()
        {
            Ginger.General.UndoChangesInRepositoryItem(mBusinessFlow, true);
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            //OKButtonClicked = true;
            mGenericWin.Close();
        }

        private void xAutomateBtn_Loaded(object sender, RoutedEventArgs e)
        {
            App.MainWindow.AddHelpLayoutToShow("BusinessFlowPage_AutomateBtnHelp", xAutomateBtn, "Click here to design your automation flow");
        }
    }
}
