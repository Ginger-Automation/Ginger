#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowWindows;
using GingerCore;
using Ginger;
using GingerCore.Activities;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.Repository;
using Ginger.UserControls;

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

        public BusinessFlowConfigurationsPage(BusinessFlow businessFlow, Context context, Ginger.General.eRIPageViewMode pageViewMode)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;
            mContext = context;
            mPageViewMode = pageViewMode;

            mBusinessFlow.Activities.CollectionChanged += mBusinessFlowActivities_CollectionChanged;
            mBusinessFlow.TargetApplications.CollectionChanged += TargetApplications_CollectionChanged;
            TrackBusinessFlowAutomationPrecentage();
            BindControls();
        }

        private void TargetApplications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                xAppsGrid.DataSourceList = mBusinessFlow.TargetApplicationPlatforms;
            });
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPlatform.PlatformImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16, Style = FindResource("@DataGridColumn_Image") as Style });
            view.GridColsView.Add(new GridColView() { Field = "AppName", Header = "Application Name", WidthWeight = 50, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = "Platform", Header = "Platform", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay });

            xAppsGrid.SetAllColumnsDefaultView(view);
            xAppsGrid.InitViewItems();

            xAppsGrid.DataSourceList = mBusinessFlow.TargetApplicationPlatforms;
        }

       

        private void TrackBusinessFlowAutomationPrecentage()
        {
            foreach (Activity activity in mBusinessFlow.Activities)
            {
                activity.PropertyChanged -= mBusinessFlowActivity_PropertyChanged;
                activity.PropertyChanged += mBusinessFlowActivity_PropertyChanged;
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
                    ActivitiesGroup actGroup = mBusinessFlow.ActivitiesGroups.Where(x => x.Name == changedActivity.ActivitiesGroupID).FirstOrDefault();
                    if(actGroup != null)
                    {
                        actGroup.OnPropertyChanged(nameof(ActivitiesGroup.AutomationPrecentage));
                    }                    
                }
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
                        activity.PropertyChanged += mBusinessFlowActivity_PropertyChanged;
                    }
                }
            //}            
        }
        

        private void BindControls()
        {
            if (mPageViewMode == Ginger.General.eRIPageViewMode.View || mPageViewMode == Ginger.General.eRIPageViewMode.ViewAndExecute)
            {
                xNameTxtBox.IsEnabled = false;
                xDescriptionTxt.IsEnabled = false;
                xTagsViewer.IsEnabled = false;
                xRunDescritpion.IsEnabled = false;
                xStatusComboBox.IsEnabled = false;
                xCreatedByTextBox.IsEnabled = false;
                xAutoPrecentageTextBox.IsEnabled = false;
                xAppsGrid.IsEnabled = false;
                xAddTargetBtn.IsEnabled = false;
                xPublishcheckbox.IsEnabled = false;
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
                xAppsGrid.IsEnabled = true;
                xAddTargetBtn.IsEnabled = true;
                xPublishcheckbox.IsEnabled = true;
            }

            BindingHandler.ObjFieldBinding(xNameTxtBox, TextBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.Name));
            xNameTxtBox.AddValidationRule(new BusinessFlowNameValidationRule());
            xShowIDUC.Init(mBusinessFlow);
            BindingHandler.ObjFieldBinding(xDescriptionTxt, TextBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.Description));
            xTagsViewer.Init(mBusinessFlow.Tags);
            xRunDescritpion.Init(mContext, mBusinessFlow, nameof(BusinessFlow.RunDescription));
            GingerCore.General.FillComboFromEnumObj(xStatusComboBox, mBusinessFlow.Status);
            BindingHandler.ObjFieldBinding(xStatusComboBox, ComboBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.Status));
            BindingHandler.ObjFieldBinding(xCreatedByTextBox, TextBox.TextProperty, mBusinessFlow.RepositoryItemHeader, nameof(RepositoryItemHeader.CreatedBy));
            BindingHandler.ObjFieldBinding(xAutoPrecentageTextBox, TextBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.AutomationPrecentage), System.Windows.Data.BindingMode.OneWay);
            BindingHandler.ObjFieldBinding(xPublishcheckbox, CheckBox.IsCheckedProperty, mBusinessFlow, nameof(RepositoryItemBase.Publish));

            //// Per source we can show specific source page info
            //if (mBusinessFlow.Source == BusinessFlow.eSource.Gherkin)
            //{
            //    SourceGherkinPage SGP = new SourceGherkinPage(mBusinessFlow);
            //    SourceFrame.Content = SGP;
            //}

            SetGridView();
        }

        private void ClearBindings()
        {
            BindingOperations.ClearAllBindings(xNameTxtBox);
            BindingOperations.ClearAllBindings(xDescriptionTxt);
            xTagsViewer.ClearBinding();
            BindingOperations.ClearAllBindings(xStatusComboBox);
            BindingOperations.ClearAllBindings(xCreatedByTextBox);
            BindingOperations.ClearAllBindings(xAutoPrecentageTextBox);
            BindingOperations.ClearAllBindings(xAppsGrid);
            BindingOperations.ClearAllBindings(xPublishcheckbox);
        }

        public void UpdateBusinessFlow(BusinessFlow updateBusinessFlow)
        {
            if (mBusinessFlow != updateBusinessFlow)
            {
                ClearBindings();

                mBusinessFlow.Activities.CollectionChanged -= mBusinessFlowActivities_CollectionChanged;
                mBusinessFlow.TargetApplications.CollectionChanged -= TargetApplications_CollectionChanged;

                mBusinessFlow = updateBusinessFlow;
                mContext.BusinessFlow = mBusinessFlow;

                mBusinessFlow.Activities.CollectionChanged += mBusinessFlowActivities_CollectionChanged;
                mBusinessFlow.TargetApplications.CollectionChanged += TargetApplications_CollectionChanged;

                BindControls();
            }
        }

        private void AddPlatformButton_Click(object sender, RoutedEventArgs e)
        {
            EditBusinessFlowAppsPage EBFP = new EditBusinessFlowAppsPage(mBusinessFlow);
            EBFP.ShowAsWindow();

            //make sure all Activities mapped to Application after change
            foreach (Activity activity in mBusinessFlow.Activities)
                if (mBusinessFlow.TargetApplications.Where(x => x.Name == activity.TargetApplication).FirstOrDefault() == null)
                    activity.TargetApplication = mBusinessFlow.TargetApplications[0].Name;
        }

    }
}