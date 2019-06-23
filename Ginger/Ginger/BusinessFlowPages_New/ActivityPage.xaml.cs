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
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Activities;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowWindows;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using GingerCore.Platforms;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for ActivityPage.xaml
    /// </summary>
    public partial class ActivityPage : Page
    {
        Activity mActivity;
        Context mContext;
        Ginger.General.eRIPageViewMode mPageViewMode;

        ActionsListViewPage mActionsPage;
        VariabelsListViewPage mVariabelsPage;
        ActivityConfigurationsPage mConfigurationsPage;

        // We keep a static page so even if we move between activities the Run controls and info stay the same
        public ActivityPage(Activity activity, Context context, Ginger.General.eRIPageViewMode pageViewMode)
        {
            InitializeComponent();

            mActivity = activity;
            mContext = context;
            mPageViewMode = pageViewMode;

            SetUIControlsContent();
            BindControls();
        }

        private void SetUIControlsContent()
        {
            mActionsPage = new ActionsListViewPage(mActivity, mContext, mPageViewMode);
            mActionsPage.ListView.ListTitleVisibility = Visibility.Collapsed;
            xActionsTabFrame.Content = mActionsPage;

            mVariabelsPage = new VariabelsListViewPage(mActivity, mContext, mPageViewMode);
            mVariabelsPage.ListView.ListTitleVisibility = Visibility.Collapsed;
            xVariabelsTabFrame.Content = mVariabelsPage;

            mConfigurationsPage = new ActivityConfigurationsPage(mActivity, mContext);
            xConfigurationsFrame.Content = mConfigurationsPage;
        }

        public void UpdateActivity(Activity activity)
        {
            if (mActivity != activity)
            {
                RemoveBindings();
                mActivity = activity;
                if (mActivity != null)
                {
                    BindControls();
                }
            }
        }

        private void RemoveBindings()
        {
            mActivity.Acts.CollectionChanged -= Acts_CollectionChanged;
            mActivity.Variables.CollectionChanged -= Variables_CollectionChanged;

            BindingOperations.ClearBinding(xNameTextBlock, TextBlock.TextProperty);
                  
        }

        private void BindControls()
        {
            //General Info Section Bindings
            BindingHandler.ObjFieldBinding(xNameTextBlock, TextBlock.TextProperty, mActivity, nameof(Activity.ActivityName));
            mActivity.PropertyChanged += mActivity_PropertyChanged;
            UpdateDescription();
            xSharedRepoInstanceUC.Init(mActivity, mContext.BusinessFlow);

            //Actions Tab Bindings            
            mActivity.Acts.CollectionChanged += Acts_CollectionChanged;
            UpdateActionsTabHeader();
            mActionsPage.UpdateActivity(mActivity);

            //Variables Tab Bindings            
            mActivity.Variables.CollectionChanged += Variables_CollectionChanged;
            UpdateVariabelsTabHeader();
            mVariabelsPage.UpdateParent(mActivity);

            //Configurations Tab Bindings
            mConfigurationsPage.UpdateActivity(mActivity);
        }

        private void mActivity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            this.Dispatcher.Invoke(() =>
            {
            xDescriptionTextBlock.Text = string.Empty;
            TextBlockHelper xDescTextBlockHelper = new TextBlockHelper(xDescriptionTextBlock);
            SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$Color_DarkBlue")).ToString());
                if (mActivity != null)
                {
                    //Application info
                    if (!string.IsNullOrEmpty(mActivity.Description))
                    {
                        xDescTextBlockHelper.AddText("Description: " + mActivity.Description);
                        xDescTextBlockHelper.AddText(" " + Ginger.General.GetTagsListAsString(mActivity.Tags));
                        xDescTextBlockHelper.AddLineBreak();
                    }
                    if (!string.IsNullOrEmpty(mActivity.RunDescription))
                    {
                        xDescTextBlockHelper.AddText("Run Description: " + mActivity.RunDescription);
                        xDescTextBlockHelper.AddLineBreak();
                    }
                    if (!string.IsNullOrEmpty(mActivity.ActivitiesGroupID))
                    {
                        xDescTextBlockHelper.AddText("Group: " + mActivity.ActivitiesGroupID);
                        xDescTextBlockHelper.AddLineBreak();
                    }
                    xDescTextBlockHelper.AddText("Target: " + mActivity.TargetApplication);
                }
            });
        
        }

        private void xSaveBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void xUploadToShareRepoMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            List<RepositoryItemBase> list = new List<RepositoryItemBase>();
            list.Add(mActivity);
            (new Ginger.Repository.SharedRepositoryOperations()).AddItemsToRepository(mContext, list);
        }

        private void xRunBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mContext.BusinessFlow.CurrentActivity = mActivity;
            mContext.Runner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = Ginger.Reports.ExecutionLoggerConfiguration.AutomationTabContext.ActivityRun;
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.RunCurrentActivity, null);
        }

        private void xContinueRunBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mContext.BusinessFlow.CurrentActivity = mActivity;
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.ContinueActivityRun, null);
        }

        private void Acts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateActionsTabHeader();
        }

        private void Variables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateVariabelsTabHeader();
        }

        private void UpdateVariabelsTabHeader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xVariabelsTabHeaderText.Text = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Variables), mActivity.Variables.Count);
            });
        }
        private void UpdateActionsTabHeader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xActionsTabHeaderText.Text = string.Format("Actions ({0})", mActivity.Acts.Count);
            });
        }

        private void xRunActionBtn_Click(object sender, RoutedEventArgs e)
        {
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.RunCurrentAction, null);
        }

        private void xResetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            mActivity.Reset();
        }
    }
}
