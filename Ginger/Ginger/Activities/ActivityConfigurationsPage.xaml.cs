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
using Ginger.Activities;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.BusinessFlowPages
{
    /// <summary>
    /// Interaction logic for ActivityConfigurationsPage.xaml
    /// </summary>
    public partial class ActivityConfigurationsPage : Page
    {
        Activity mActivity;
        Context mContext;
        General.eRIPageViewMode mPageViewMode;

        public ActivityConfigurationsPage(Activity activity, Context context, General.eRIPageViewMode pageViewMode)
        {
            InitializeComponent();

            mActivity = activity;
            mContext = context;
            mPageViewMode = pageViewMode;

            SetUI();
            BindControls();
        }

        private void SetUI()
        {
            if (mPageViewMode == General.eRIPageViewMode.View)
            {
                xActivityNameTxtBox.IsEnabled = false;
                xActivityDescriptionTxt.IsEnabled = false;
                xTagsViewer.IsEnabled = false;
                xRunDescritpion.IsEnabled = false;
                xScreenTxt.IsEnabled = false;
                xExpectedTxt.IsEnabled = false;
                xMandatoryActivityCB.IsEnabled = false;
                xTargetApplicationComboBox.IsEnabled = false;
                xRunOptionCombo.IsEnabled = false;
                xAutomationStatusCombo.IsEnabled = false;
                xHandlerTypeCombo.IsEnabled = false;
                xErrorHandlerMappingCmb.IsEnabled = false;
                xSpecificErrorHandlerBtn.IsEnabled = false;
            }
        }

        public void UpdateActivity(Activity activity)
        {
            if (mActivity != activity)
            {
                ClearBindings();
                mActivity = activity;
                if (mActivity != null)
                {
                    BindControls();
                }
            }
        }

        public void ClearBindings()
        {            
            BindingOperations.ClearAllBindings(xRunOptionCombo);
            BindingOperations.ClearAllBindings(xActivityNameTxtBox);
            BindingOperations.ClearAllBindings(xActivityDescriptionTxt);
            xTagsViewer.ClearBinding();
            BindingOperations.ClearAllBindings(xExpectedTxt);
            BindingOperations.ClearAllBindings(xScreenTxt);
            BindingOperations.ClearAllBindings(xTargetApplicationComboBox);
            BindingOperations.ClearAllBindings(xAutomationStatusCombo);
            BindingOperations.ClearAllBindings(xMandatoryActivityCB);
            BindingOperations.ClearAllBindings(xHandlerTypeCombo);
            BindingOperations.ClearAllBindings(xErrorHandlerMappingCmb);            
        }

        private void BindControls()
        {
            //Configurations Tab Bindings
            xRunDescritpion.Init(mContext, mActivity, nameof(Activity.RunDescription));
            xRunOptionCombo.BindControl(mActivity, nameof(Activity.ActionRunOption));
            GingerCore.General.FillComboFromEnumObj(xErrorHandlerMappingCmb, mActivity.ErrorHandlerMappingType);
            xTagsViewer.Init(mActivity.Tags);
            BindingHandler.ObjFieldBinding(xActivityNameTxtBox, TextBox.TextProperty, mActivity, nameof(Activity.ActivityName));
            BindingHandler.ObjFieldBinding(xActivityDescriptionTxt, TextBox.TextProperty, mActivity, nameof(Activity.Description));
            BindingHandler.ObjFieldBinding(xExpectedTxt, TextBox.TextProperty, mActivity, nameof(Activity.Expected));
            BindingHandler.ObjFieldBinding(xScreenTxt, TextBox.TextProperty, mActivity, nameof(Activity.Screen));
            xAutomationStatusCombo.BindControl(mActivity, nameof(Activity.AutomationStatus));
            BindingHandler.ObjFieldBinding(xMandatoryActivityCB, CheckBox.IsCheckedProperty, mActivity, nameof(Activity.Mandatory));
            if (mContext != null && mContext.BusinessFlow != null)
            {
                xTargetApplicationComboBox.ItemsSource = mContext.BusinessFlow.TargetApplications;
            }
            else
            {
                xTargetApplicationComboBox.ItemsSource = WorkSpace.Instance.Solution.GetSolutionTargetApplications();
            }
            xTargetApplicationComboBox.SelectedValuePath = nameof(TargetApplication.AppName);
            xTargetApplicationComboBox.DisplayMemberPath = nameof(TargetApplication.AppName);
            BindingHandler.ObjFieldBinding(xTargetApplicationComboBox, ComboBox.SelectedValueProperty, mActivity, nameof(Activity.TargetApplication));

            if (mActivity.GetType() == typeof(ErrorHandler))
            {
                xHandlerTypeStack.Visibility = Visibility.Visible;
                xHandlerMappingStack.Visibility = Visibility.Collapsed;
                xHandlerTypeCombo.BindControl(mActivity, nameof(ErrorHandler.HandlerType));
            }
            else
            {
                BindingHandler.ObjFieldBinding(xErrorHandlerMappingCmb, ComboBox.SelectedValueProperty, mActivity, nameof(Activity.ErrorHandlerMappingType));
                xHandlerMappingStack.Visibility = Visibility.Visible;
                xHandlerTypeStack.Visibility = Visibility.Collapsed;
            }
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
    }
}
