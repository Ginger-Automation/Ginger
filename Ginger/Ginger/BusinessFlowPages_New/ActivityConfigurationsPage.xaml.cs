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

        public ActivityConfigurationsPage(Activity activity, Context context)
        {
            InitializeComponent();

            mActivity = activity;
            mContext = context;

            xAutomationStatusCombo.ItemsSource = GingerCore.General.GetEnumValues(typeof(eActivityAutomationStatus));
            xHandlerTypeCombo.ItemsSource = GingerCore.General.GetEnumValues(typeof(eHandlerType));
            xRunOptionCombo.ItemsSource = GingerCore.General.GetEnumValues(typeof(eActionRunOption));

            BindControls();
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
            BindingOperations.ClearBinding(xRunOptionCombo, ComboBox.TextProperty);
            BindingOperations.ClearBinding(xActivityNameTxtBox, TextBox.TextProperty);
            BindingOperations.ClearBinding(xActivityDescriptionTxt, TextBox.TextProperty);
            BindingOperations.ClearBinding(xExpectedTxt, TextBox.TextProperty);
            BindingOperations.ClearBinding(xScreenTxt, TextBox.TextProperty);
            BindingOperations.ClearBinding(xTargetApplicationComboBox, CheckBox.IsCheckedProperty);
            BindingOperations.ClearBinding(xAutomationStatusCombo, ComboBox.TextProperty);
            BindingOperations.ClearBinding(xMandatoryActivityCB, CheckBox.IsCheckedProperty);
            BindingOperations.ClearBinding(xHandlerTypeCombo, ComboBox.TextProperty);
            BindingOperations.ClearBinding(xErrorHandlerMappingCmb, ComboBox.SelectedValueProperty);
        }

        private void BindControls()
        {
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
            xTargetApplicationComboBox.SelectedValuePath = nameof(TargetApplication.AppName);
            xTargetApplicationComboBox.DisplayMemberPath = nameof(TargetApplication.AppName);
            BindingHandler.ObjFieldBinding(xTargetApplicationComboBox, ComboBox.SelectedValueProperty, mActivity, nameof(Activity.TargetApplication));

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
