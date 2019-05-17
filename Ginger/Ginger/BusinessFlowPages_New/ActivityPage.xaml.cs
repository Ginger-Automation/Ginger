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
using Ginger;
using Ginger.Activities;
using Ginger.BusinessFlowPages;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using GingerCore.Platforms;
using System.Windows;
using System.Windows.Controls;
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
        Ginger.General.RepositoryItemPageViewMode mPageMode;

        ActionsListViewPage mActionsPage;
        VariabelsListViewPage mVariabelsPage;

        // We keep a static page so even if we move between activities the Run controls and info stay the same
        public ActivityPage(Activity activity, Context context, Ginger.General.RepositoryItemPageViewMode pageMode)
        {
            InitializeComponent();

            mActivity = activity;
            mContext = context;
            mPageMode = pageMode;

            SetUIControlsContent();
            BindControls();
        }

        private void SetUIControlsContent()
        {
            xAutomationStatusCombo.ItemsSource = GingerCore.General.GetEnumValues(typeof(eActivityAutomationStatus));
            xHandlerTypeCombo.ItemsSource = GingerCore.General.GetEnumValues(typeof(eHandlerType));
            xRunOptionCombo.ItemsSource = GingerCore.General.GetEnumValues(typeof(eActionRunOption));

            mActionsPage = new ActionsListViewPage(mActivity, mContext);
            mActionsPage.ListView.ListTitleVisibility = Visibility.Collapsed;
            xActionsTabFrame.Content = mActionsPage;

            mVariabelsPage = new VariabelsListViewPage(mActivity, mContext);
            mVariabelsPage.ListView.ListTitleVisibility = Visibility.Collapsed;
            xVariabelsTabFrame.Content = mVariabelsPage;
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
            if (mActivity != null)
            {
                mActivity.Acts.CollectionChanged -= Acts_CollectionChanged;
                mActivity.Variables.CollectionChanged -= Variables_CollectionChanged;             
            }
        }

        private void BindControls()
        {
            //General Info Section Bindings
            BindingHandler.ObjFieldBinding(xNameTextBlock, TextBlock.TextProperty, mActivity, nameof(Activity.ActivityName));
            mActivity.PropertyChanged += mActivity_PropertyChanged;
            UpdateDescription();

            //Actions Tab Bindings            
            mActivity.Acts.CollectionChanged += Acts_CollectionChanged;
            UpdateActionsTabHeader();
            mActionsPage.UpdateActivity(mActivity);

            //Variables Tab Bindings            
            mActivity.Variables.CollectionChanged += Variables_CollectionChanged;
            UpdateVariabelsTabHeader();
            mVariabelsPage.UpdateParent(mActivity);

            //Configurations Tab Bindings
            xRunDescritpion.Init(mContext, mActivity, nameof(Activity.RunDescription));                        
            BindingHandler.ObjFieldBinding(xRunOptionCombo, ComboBox.TextProperty, mActivity, nameof(Activity.ActionRunOption));
            GingerCore.General.FillComboFromEnumObj(xErrorHandlerMappingCmb, mActivity.ErrorHandlerMappingType);
            xTagsViewer.Init(mActivity.Tags);
            BindingHandler.ObjFieldBinding(xActivityNameTxtBox, TextBox.TextProperty, mActivity, nameof(Activity.ActivityName));
            BindingHandler.ObjFieldBinding(xActivityDescriptionTxt, TextBox.TextProperty, mActivity, nameof(Activity.Description));
            BindingHandler.ObjFieldBinding(xExpectedTxt, TextBox.TextProperty, mActivity, nameof(Activity.Expected));
            BindingHandler.ObjFieldBinding(xScreenTxt, TextBox.TextProperty, mActivity, nameof(Activity.Screen));           
            BindingHandler.ObjFieldBinding(xAutomationStatusCombo, ComboBox.TextProperty, mActivity, nameof(Activity.AutomationStatus));
            BindingHandler.ObjFieldBinding(xMandatoryActivityCB, CheckBox.IsCheckedProperty, mActivity, nameof(Activity.Mandatory));


            if (mContext != null && mContext.BusinessFlow != null)
            {
                xTargetApplicationComboBox.ItemsSource = mContext.BusinessFlow.TargetApplications;
            }
            else
            {
                xTargetApplicationComboBox.ItemsSource = WorkSpace.Instance.Solution.GetSolutionTargetApplications();
            }

            BindingHandler.ObjFieldBinding(xTargetApplicationComboBox, ComboBox.SelectedValueProperty, mActivity, nameof(Activity.TargetApplication));
            xTargetApplicationComboBox.SelectedValuePath = nameof(TargetApplication.AppName);
            xTargetApplicationComboBox.DisplayMemberPath = nameof(TargetApplication.AppName);

            if (mActivity.GetType() == typeof(ErrorHandler))
            {
                xHandlerTypeStack.Visibility = Visibility.Visible;
                xHandlerMappingStack.Visibility = Visibility.Collapsed;                
                BindingHandler.ObjFieldBinding(xHandlerTypeCombo, ComboBox.TextProperty, mActivity, nameof(ErrorHandler.HandlerType));
            }
            else
            {
                BindingHandler.ObjFieldBinding(xErrorHandlerMappingCmb, ComboBox.SelectedValueProperty, mActivity, nameof(Activity.ErrorHandlerMappingType));
                xHandlerMappingStack.Visibility = Visibility.Visible;                
                xHandlerTypeStack.Visibility = Visibility.Collapsed;
            }
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

        }

        private void xRunBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void xContinueRunBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void xErrorHandlerMappingCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xErrorHandlerMappingCmb.SelectedValue != null && xErrorHandlerMappingCmb.SelectedValue.ToString() == eHandlerMappingType.SpecificErrorHandlers.ToString())
            {
                xSpecificErrorHandlerBtn.Visibility = Visibility.Visible;
            }
            else
            {
                xSpecificErrorHandlerBtn.Visibility = Visibility.Collapsed;
            }

        }

        private void xSpecificErrorHandlerBtn_Click(object sender, RoutedEventArgs e)
        {            
            ErrorHandlerMappingPage errorHandlerMappingPage = new ErrorHandlerMappingPage(mActivity, mContext.BusinessFlow);
            errorHandlerMappingPage.ShowAsWindow();            
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
    }
}
