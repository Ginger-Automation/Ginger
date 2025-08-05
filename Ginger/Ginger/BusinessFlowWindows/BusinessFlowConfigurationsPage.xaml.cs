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
using Amdocs.Ginger.Common.Repository.SolutionCategories;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.BusinessFlowWindows;
using Ginger.Run;
using Ginger.SolutionCategories;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for BusinessFlowInfoPage.xaml
    /// </summary>
    public partial class BusinessFlowConfigurationsPage : Page
    {
        BusinessFlow mBusinessFlow;
        Context mContext;
        Ginger.General.eRIPageViewMode mPageViewMode;
        private readonly bool _ignoreValidationRules;
        SolutionCategoriesPage mSolutionCategoriesPage = null;

        public BusinessFlowConfigurationsPage(BusinessFlow businessFlow, Context context, Ginger.General.eRIPageViewMode pageViewMode, bool ignoreValidationRules = false)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;
            mContext = context;
            mPageViewMode = pageViewMode;
            _ignoreValidationRules = ignoreValidationRules;

            CollectionChangedEventManager.AddHandler(source: mBusinessFlow.Activities, handler: mBusinessFlowActivities_CollectionChanged);
            TrackBusinessFlowAutomationPrecentage();
            BindControls();
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ApplicationPlatform.PlatformImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16, Style = FindResource("@DataGridColumn_Image") as Style },
                new GridColView() { Field = "AppName", Header = "Application Name", WidthWeight = 50, ReadOnly = true, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = "Platform", Header = "Platform", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay },
            ]
            };

        }

        string allProperties = string.Empty;

        private void TrackBusinessFlowAutomationPrecentage()
        {
            foreach (Activity activity in mBusinessFlow.Activities)
            {
                PropertyChangedEventManager.RemoveHandler(source: activity, handler: mBusinessFlowActivity_PropertyChanged, propertyName: allProperties);
                PropertyChangedEventManager.AddHandler(source: activity, handler: mBusinessFlowActivity_PropertyChanged, propertyName: allProperties);
            }
        }

        private void mBusinessFlowActivity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Activity.AutomationStatus))
            {
                mBusinessFlow.OnPropertyChanged(nameof(BusinessFlow.AutomationPrecentage));
                Activity changedActivity = (Activity)sender;
                if (string.IsNullOrEmpty(changedActivity.ActivitiesGroupID) == false)
                {
                    ActivitiesGroup actGroup = mBusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == changedActivity.ActivitiesGroupID);
                    if (actGroup != null)
                    {
                        actGroup.OnPropertyChanged(nameof(ActivitiesGroup.AutomationPrecentage));
                    }
                }
            }
            else if (e.PropertyName is (nameof(Activity.Active)) or (nameof(Activity.Mandatory)))
            {
                mBusinessFlow.OnPropertyChanged(nameof(BusinessFlow.Activities));
            }
        }

        private void mBusinessFlowActivities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            mBusinessFlow.OnPropertyChanged(nameof(BusinessFlow.AutomationPrecentage));

            //Perf imrprovements
            //if (WorkSpace.Instance.BetaFeatures.BFPageActivitiesHookOnlyNewActivities)
            //{
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object o in e.NewItems)
                {
                    Activity activity = (Activity)o;
                    PropertyChangedEventManager.AddHandler(source: activity, handler: mBusinessFlowActivity_PropertyChanged, propertyName: allProperties);
                }
            }
            //}            
        }


        private void BindControls()
        {
            if (mPageViewMode is Ginger.General.eRIPageViewMode.View or Ginger.General.eRIPageViewMode.ViewAndExecute)
            {
                xNameTxtBox.IsEnabled = false;
                xDescriptionTxt.IsEnabled = false;
                xTagsViewer.IsEnabled = false;
                xRunDescritpion.IsEnabled = false;
                xStatusComboBox.IsEnabled = false;
                xCreatedByTextBox.IsEnabled = false;
                xAutoPrecentageTextBox.IsEnabled = false;
                xPublishcheckbox.IsEnabled = false;
                xExternalId.IsEnabled = false;
            }
            else
            {
                xNameTxtBox.IsEnabled = true;
                xDescriptionTxt.IsEnabled = true;
                xTagsViewer.IsEnabled = true;
                xRunDescritpion.IsEnabled = true;
                xStatusComboBox.IsEnabled = true;
                xCreatedByTextBox.IsEnabled = true;
                xAutoPrecentageTextBox.IsEnabled = true;
                xPublishcheckbox.IsEnabled = true;
                xExternalId.IsEnabled = true;
            }
            BindingHandler.ObjFieldBinding(xNameTxtBox, TextBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.Name));
            if (!_ignoreValidationRules)
            {
                xNameTxtBox.AddValidationRule(new BusinessFlowNameValidationRule());
            }
            xShowIDUC.Init(mBusinessFlow);
            BindingHandler.ObjFieldBinding(xDescriptionTxt, TextBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.Description));
            xTagsViewer.Init(mBusinessFlow.Tags);
            xRunDescritpion.Init(mContext, mBusinessFlow, nameof(BusinessFlow.RunDescription));

            if (WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures)
            {
                xExternalId.Init(mContext, mBusinessFlow, nameof(BusinessFlow.ExternalID));
            }
            else
            {
                xPnlExternalId.Visibility = Visibility.Collapsed;
            }
            GingerCore.General.FillComboFromEnumObj(xStatusComboBox, mBusinessFlow.Status);
            BindingHandler.ObjFieldBinding(xStatusComboBox, ComboBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.Status));
            BindingHandler.ObjFieldBinding(xCreatedByTextBox, TextBox.TextProperty, mBusinessFlow.RepositoryItemHeader, nameof(RepositoryItemHeader.CreatedBy));
            BindingHandler.ObjFieldBinding(xAutoPrecentageTextBox, TextBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.AutomationPrecentage), System.Windows.Data.BindingMode.OneWay);
            BindingHandler.ObjFieldBinding(xPublishcheckbox, CheckBox.IsCheckedProperty, mBusinessFlow, nameof(RepositoryItemBase.Publish));
            if (mSolutionCategoriesPage == null)
            {
                mSolutionCategoriesPage = new SolutionCategoriesPage();
                xCategoriesFrame.ClearAndSetContent(mSolutionCategoriesPage);
                mSolutionCategoriesPage.CategoryValueChanged += CategoriesPage_CategoryValueChanged;
            }
            mSolutionCategoriesPage.Init(eSolutionCategoriesPageMode.ValuesSelection, mBusinessFlow.MergedCategoriesDefinitions);
            //// Per source we can show specific source page info
            //if (mBusinessFlow.Source == BusinessFlow.eSource.Gherkin)
            //{
            //    SourceGherkinPage SGP = new SourceGherkinPage(mBusinessFlow);
            //    SourceFrame.Content = SGP;
            //}

            SetGridView();
        }

        private void CategoriesPage_CategoryValueChanged(object sender, EventArgs e)
        {
            if (sender is ObservableList<SolutionCategoryDefinition> categories && mBusinessFlow != null)
            {
                mBusinessFlow.MergedCategoriesDefinitions = categories;
            }
        }

        private void ClearBindings()
        {
            BindingOperations.ClearAllBindings(xNameTxtBox);
            BindingOperations.ClearAllBindings(xDescriptionTxt);
            xTagsViewer.ClearBinding();
            BindingOperations.ClearAllBindings(xStatusComboBox);
            BindingOperations.ClearAllBindings(xCreatedByTextBox);
            BindingOperations.ClearAllBindings(xAutoPrecentageTextBox);
            BindingOperations.ClearAllBindings(xPublishcheckbox);
            if (mSolutionCategoriesPage != null)
            {
                mSolutionCategoriesPage.CategoryValueChanged -= CategoriesPage_CategoryValueChanged;
            }
            xCategoriesFrame.ClearControlsBindings();
        }

        public void UpdateBusinessFlow(BusinessFlow updateBusinessFlow)
        {
            if (mBusinessFlow != updateBusinessFlow)
            {
                ClearBindings();

                CollectionChangedEventManager.RemoveHandler(source: mBusinessFlow.Activities, handler: mBusinessFlowActivities_CollectionChanged);

                mBusinessFlow = updateBusinessFlow;
                mContext.BusinessFlow = mBusinessFlow;

                CollectionChangedEventManager.AddHandler(source: mBusinessFlow.Activities, handler: mBusinessFlowActivities_CollectionChanged);
                BindControls();
            }
        }

        private void AddPlatformButton_Click(object sender, RoutedEventArgs e)
        {
            EditBusinessFlowAppsPage EBFP = new EditBusinessFlowAppsPage(mBusinessFlow);
            EBFP.ShowAsWindow();

            //make sure all Activities mapped to Application after change
            foreach (Activity activity in mBusinessFlow.Activities)
            {
                if (mBusinessFlow.TargetApplications.FirstOrDefault(x => x.Name == activity.TargetApplication) == null)
                {
                    activity.TargetApplication = mBusinessFlow.TargetApplications[0].Name;
                }
            }
        }

    }
}