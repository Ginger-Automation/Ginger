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
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.BusinessFlowLib;
using Amdocs.Ginger.Common.Repository.SolutionCategories;
using Ginger.Activities;
using Ginger.SolutionCategories;
using Ginger.UserControlsLib;
using GingerCore;
using GingerCore.Activities;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.BusinessFlowPages
{
    /// <summary>
    /// Interaction logic for ActivityConfigurationsPage.xaml
    /// </summary>
    public partial class ActivityDetailsPage : Page
    {
        Activity mActivity;
        Context mContext;
        General.eRIPageViewMode mPageViewMode;
        SolutionCategoriesPage mSolutionCategoriesPage = null;

        public ActivityDetailsPage(Activity activity, Context context, General.eRIPageViewMode pageViewMode)
        {
            InitializeComponent();

            mActivity = activity;
            mContext = context;
            mPageViewMode = pageViewMode;
            SetUI();
            BindControls();
        }

        public void UpdatePageViewMode(Ginger.General.eRIPageViewMode pageViewMode, Activity activity)
        {
            mPageViewMode = pageViewMode;
            mActivity = activity;
            SetUI();
        }
        private void SetUI()
        {
            if (mPageViewMode is General.eRIPageViewMode.View or General.eRIPageViewMode.ViewAndExecute)
            {
                xActivityNameTxtBox.IsEnabled = false;
                xActivityDescriptionTxt.IsEnabled = false;
                xTagsViewer.IsEnabled = false;
                xRunDescritpion.IsEnabled = false;
                xScreenTxt.IsEnabled = false;
                xExpectedTxt.IsEnabled = false;
                xMandatoryActivityCB.IsEnabled = false;
                xPublishcheckbox.IsEnabled = false;
                xTargetApplicationComboBox.IsEnabled = false;
                xRunOptionCombo.IsEnabled = false;
                xAutomationStatusCombo.IsEnabled = false;
                xHandlerTypeCombo.IsEnabled = false;
                xErrorHandlerMappingCmb.IsEnabled = false;
                xSpecificErrorHandlerBtn.IsEnabled = false;
                xSharedRepoInstanceUC.IsEnabled = false;
                xConsumerCB.IsEnabled = false;
                xCategoriesExpander.Visibility = Visibility.Visible;
                xCategoriesExpander.IsExpanded = true;
                xCategoriesExpander.IsEnabled = false;
            }
            else
            {
                xActivityNameTxtBox.IsEnabled = true;
                xActivityDescriptionTxt.IsEnabled = true;
                xTagsViewer.IsEnabled = true;
                xRunDescritpion.IsEnabled = true;
                xScreenTxt.IsEnabled = true;
                xExpectedTxt.IsEnabled = true;
                xMandatoryActivityCB.IsEnabled = true;
                xPublishcheckbox.IsEnabled = true;
                xTargetApplicationComboBox.IsEnabled = true;
                xRunOptionCombo.IsEnabled = true;
                xAutomationStatusCombo.IsEnabled = true;
                xHandlerTypeCombo.IsEnabled = true;
                xErrorHandlerMappingCmb.IsEnabled = true;
                xSpecificErrorHandlerBtn.IsEnabled = true;
                xSharedRepoInstanceUC.IsEnabled = true;
                xConsumerCB.IsEnabled = true;
                if (mActivity.Type == Amdocs.Ginger.Repository.eSharedItemType.Link || mActivity.ParentGuid != Guid.Empty)
                {
                    xCategoriesExpander.Visibility = Visibility.Visible;
                    xCategoriesExpander.IsEnabled = true;
                }
                else
                {
                    xCategoriesExpander.Visibility = Visibility.Collapsed;
                }
            }


            if (mPageViewMode == Ginger.General.eRIPageViewMode.SharedReposiotry)
            {
                xCategoriesExpander.Visibility = Visibility.Visible;
                xSharedRepoInstanceUC.Visibility = Visibility.Collapsed;
                xSharedRepoInstanceUCCol.Width = new GridLength(0);
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
            BindingOperations.ClearAllBindings(xConsumerCB);
            BindingOperations.ClearAllBindings(xAutomationStatusCombo);
            BindingOperations.ClearAllBindings(xMandatoryActivityCB);
            BindingOperations.ClearAllBindings(xPublishcheckbox);   
            BindingOperations.ClearAllBindings(xHandlerTypeCombo);
            BindingOperations.ClearAllBindings(xErrorHandlerMappingCmb);
            BindingOperations.ClearAllBindings(xHandlerPostExecutionCombo);
            BindingOperations.ClearAllBindings(xHandlerTriggerOnCombo);
            if (mSolutionCategoriesPage != null)
            {
                mSolutionCategoriesPage.CategoryValueChanged -= CategoriesPage_CategoryValueChanged;
            }
            xCategoriesFrame.ClearControlsBindings();
        }

        private void CategoriesPage_CategoryValueChanged(object sender, EventArgs e)
        {
            if (sender is ObservableList<SolutionCategoryDefinition> categories && mActivity != null)
            {
                mActivity.MergedCategoriesDefinitions = categories;
            }
        }

        private void BindControls()
        {
            //Details Tab Bindings
            xRunDescritpion.Init(mContext, mActivity, nameof(Activity.RunDescription));
            xRunOptionCombo.BindControl(mActivity, nameof(Activity.ActionRunOption));
            xSharedRepoInstanceUC.Init(mActivity, mContext.BusinessFlow);
            GingerCore.General.FillComboFromEnumObj(xErrorHandlerMappingCmb, mActivity.ErrorHandlerMappingType);
            xTagsViewer.Init(mActivity.Tags);
            xShowIDUC.Init(mActivity);
            BindingHandler.ObjFieldBinding(xActivityNameTxtBox, TextBox.TextProperty, mActivity, nameof(Activity.ActivityName));
            xActivityNameTxtBox.AddValidationRule(new ActivityNameValidationRule());
            BindingHandler.ObjFieldBinding(xActivityDescriptionTxt, TextBox.TextProperty, mActivity, nameof(Activity.Description));
            BindingHandler.ObjFieldBinding(xExpectedTxt, TextBox.TextProperty, mActivity, nameof(Activity.Expected));
            BindingHandler.ObjFieldBinding(xScreenTxt, TextBox.TextProperty, mActivity, nameof(Activity.Screen));
            xAutomationStatusCombo.BindControl(mActivity, nameof(Activity.AutomationStatus));
            BindingHandler.ObjFieldBinding(xMandatoryActivityCB, CheckBox.IsCheckedProperty, mActivity, nameof(Activity.Mandatory));
            BindingHandler.ObjFieldBinding(xPublishcheckbox, CheckBox.IsCheckedProperty, mActivity, nameof(Activity.Publish));
            if (mSolutionCategoriesPage == null)
            {
                mSolutionCategoriesPage = new SolutionCategoriesPage();
                xCategoriesFrame.ClearAndSetContent(mSolutionCategoriesPage);
                mSolutionCategoriesPage.CategoryValueChanged += CategoriesPage_CategoryValueChanged;
            }
            mSolutionCategoriesPage.Init(eSolutionCategoriesPageMode.ValuesSelection, mActivity.MergedCategoriesDefinitions);

            xTargetApplicationComboBox.ItemsSource = WorkSpace.Instance.Solution.GetSolutionTargetApplications();
            if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.ApplicationPlatforms != null)
            {
                WorkSpace.Instance.Solution.ApplicationPlatforms.CollectionChanged -= OnApplicationPlatformChanged;
                WorkSpace.Instance.Solution.ApplicationPlatforms.CollectionChanged += OnApplicationPlatformChanged;
            }

            xTargetApplicationlbl.Content = $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}:";
            xTargetApplicationComboBox.SelectedValuePath = nameof(TargetApplication.AppName);
            xTargetApplicationComboBox.DisplayMemberPath = nameof(TargetApplication.AppName);
            mActivity.DirtyTracking = Amdocs.Ginger.Common.Enums.eDirtyTracking.Paused;
            BindingHandler.ObjFieldBinding(xTargetApplicationComboBox, ComboBox.SelectedValueProperty, mActivity, nameof(Activity.TargetApplication));
            if (xTargetApplicationComboBox.SelectedValue == null && xTargetApplicationComboBox.ItemsSource.AsQueryable().Count() >= 1)
            {
                xTargetApplicationComboBox.SelectedValue = xTargetApplicationComboBox.ItemsSource.AsQueryable().FirstOrDefault();
            }
            mActivity.DirtyTracking = Amdocs.Ginger.Common.Enums.eDirtyTracking.Started;
            if (mActivity.GetType() == typeof(ErrorHandler))
            {
                xHandlerTypeStack.Visibility = Visibility.Visible;
                xHandlerPostExecutionActionStack.Visibility = Visibility.Visible;
                xHandlerPostExecutionCombo.BindControl(mActivity, nameof(ErrorHandler.ErrorHandlerPostExecutionAction));

                xHandlerTriggerOnStackPanel.Visibility = Visibility.Visible;
                xHandlerTriggerOnCombo.BindControl(mActivity, nameof(ErrorHandler.TriggerType));

                xHandlerMappingStack.Visibility = Visibility.Collapsed;
                xHandlerTypeCombo.BindControl(mActivity, nameof(ErrorHandler.HandlerType));
            }
            else if (mActivity.GetType() == typeof(CleanUpActivity))
            {
                xHandlerTypeStack.Visibility = Visibility.Collapsed;
                xHandlerMappingStack.Visibility = Visibility.Collapsed;
            }
            else
            {
                BindingHandler.ObjFieldBinding(xErrorHandlerMappingCmb, ComboBox.SelectedValueProperty, mActivity, nameof(Activity.ErrorHandlerMappingType));
                xHandlerMappingStack.Visibility = Visibility.Visible;
                xHandlerTypeStack.Visibility = Visibility.Collapsed;
                xHandlerTriggerOnStackPanel.Visibility = Visibility.Collapsed;
                xHandlerPostExecutionActionStack.Visibility = Visibility.Collapsed;
            }
            PropertyChangedEventManager.RemoveHandler(WorkSpace.Instance.UserProfile, UserProfile_PropertyChanged, string.Empty);
            PropertyChangedEventManager.AddHandler(WorkSpace.Instance.UserProfile, UserProfile_PropertyChanged, string.Empty);

            if (mContext.BusinessFlow != null)
            {
                CollectionChangedEventManager.RemoveHandler(source: mContext.BusinessFlow.TargetApplications, handler: AutoUpdate_ConsumerList);
                CollectionChangedEventManager.AddHandler(source: mContext.BusinessFlow.TargetApplications, handler: AutoUpdate_ConsumerList);
            }
            mActivity.DirtyTracking = Amdocs.Ginger.Common.Enums.eDirtyTracking.Paused;
            TargetAppSelectedComboBox();
            mActivity.DirtyTracking = Amdocs.Ginger.Common.Enums.eDirtyTracking.Started;
        }

        private void OnApplicationPlatformChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            xTargetApplicationComboBox.ItemsSource = WorkSpace.Instance.Solution.GetSolutionTargetApplications();

            if (xTargetApplicationComboBox.Items == null || mActivity == null || mActivity.TargetApplication == null)
            {
                return;
            }

            int pointer = 0;

            foreach (TargetApplication targetApplication in xTargetApplicationComboBox.Items)
            {

                if (targetApplication.AppName.Equals(mActivity.TargetApplication))
                {
                    xTargetApplicationComboBox.SelectedIndex = pointer;
                }

                pointer++;
            }

            TargetAppSelectedComboBox();
        }

        private void UserProfile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(UserProfile.ShowEnterpriseFeatures)))
            {
                TargetAppSelectedComboBox();
            }
        }
        public void UpdateTargetApplication()
        {
            Dispatcher.Invoke(() =>
            {
                int pointer = 0;
                foreach (TargetApplication targetApplication in xTargetApplicationComboBox.Items)
                {

                    var ApplicationPlatform = WorkSpace.Instance?.Solution?.ApplicationPlatforms?
                     .FirstOrDefault((appPlat) =>
                     {
                         if (appPlat.NameBeforeEdit != null && appPlat.NameBeforeEdit.Equals(targetApplication.AppName))
                         {
                             return true;
                         }
                         return false;
                     });
                    if (ApplicationPlatform != null)
                    {
                        targetApplication.AppName = ApplicationPlatform.AppName;

                        if (targetApplication.AppName.Equals(mActivity.TargetApplication) && xTargetApplicationComboBox.SelectedIndex != pointer)
                        {
                            xTargetApplicationComboBox.SelectedIndex = pointer;
                        }
                    }
                    pointer++;
                }

            });
        }
        private void AutoUpdate_ConsumerList(object? sender, NotifyCollectionChangedEventArgs e)
        {
            TargetAppSelectedComboBox();
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

        private void xTriggerOnSpecificErrorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mActivity.GetType() == typeof(ErrorHandler))
            {
                ConfigureErrorListPage configureErrorListPage = new ConfigureErrorListPage((ErrorHandler)mActivity);
                configureErrorListPage.ShowAsWindow();
            }
        }

        private void xHandlerTriggerOnCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xHandlerTriggerOnCombo.SelectedValue != null && xHandlerTriggerOnCombo.SelectedValue.ToString() == eTriggerType.SpecificError.ToString())
            {
                xTriggerOnSpecificErrorBtn.Visibility = Visibility.Visible;
            }
            else
            {
                xTriggerOnSpecificErrorBtn.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Returns the selected target application platform 
        /// </summary>
        /// <returns></returns>
        private ePlatformType GetCurrentActivityPlatform()
        {
            return WorkSpace.Instance.Solution.GetApplicationPlatformForTargetApp(mActivity.TargetApplication);
        }

        public void xTargetApplicationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TargetAppSelectedComboBox();
        }

        public void TargetAppSelectedComboBox()
        {
            Dispatcher.Invoke(() =>
            {
                if (xTargetApplicationComboBox.SelectedItem != null &&
                               WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures)
                {
                    PrepareAndLoadConsumerComboBox();
                }
                else
                {
                    xConsumerStack.Visibility = Visibility.Collapsed;
                    BindingOperations.ClearAllBindings(xConsumerCB);
                }
            });
        }
        private void PrepareAndLoadConsumerComboBox()
        {
            if (GetCurrentActivityPlatform() != ePlatformType.WebServices)
            {
                mActivity.ConsumerApplications.Clear();
                xConsumerStack.Visibility = Visibility.Collapsed;

            }
            else
            {
                xConsumerStack.Visibility = Visibility.Visible;

                //logic for Consumer ComboBox for Otoma
                ObservableList<TargetBase> targetApplications = WorkSpace.Instance.Solution.GetSolutionTargetApplications();
                ObservableList<Consumer> consumerList = [];
                if (mContext.BusinessFlow != null)
                {
                    // this logic is developed to support the backward compatibility where parent guids are empty
                    if (targetApplications.Any(f => f.ParentGuid.Equals(Guid.Empty)))
                    {
                        var solutionTAs = WorkSpace.Instance.Solution.GetSolutionTargetApplications();
                        foreach (var listofTA in targetApplications.Where(f => f.ParentGuid.Equals(Guid.Empty)))
                        {
                            var matchingApp = solutionTAs.FirstOrDefault(z => z.Name.Equals(listofTA.Name));
                            if (matchingApp != null)
                            {
                                listofTA.ParentGuid = matchingApp.Guid;
                            }
                        }
                    }
                }

                if (xTargetApplicationComboBox.SelectedItem != null)
                {
                    foreach (var targetApplication in targetApplications.OfType<TargetApplication>())
                    {
                        if (!targetApplication.AppName.Equals(xTargetApplicationComboBox.SelectedItem.ToString()))
                        {
                            Consumer consumer = new()
                            {
                                ConsumerGuid = targetApplication.TargetGuid != Guid.Empty ? targetApplication.TargetGuid :
                                targetApplication.Guid,
                                Name = targetApplication.ItemName
                            };
                            consumerList.Add(consumer);
                        }
                    }
                }

                xConsumerCB.ConsumerSource = consumerList;
                //Binding for the consumer ComboBox & EnterPrise flag check for consumer combobox
                BindingOperations.ClearAllBindings(xConsumerCB);
                BindingHandler.ObjFieldBinding(xConsumerCB, ConsumerComboBox.SelectedConsumerProperty, mActivity, nameof(mActivity.ConsumerApplications));
            }
        }
    }
}
