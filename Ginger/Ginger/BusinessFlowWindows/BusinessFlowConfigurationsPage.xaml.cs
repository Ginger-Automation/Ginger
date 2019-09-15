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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowWindows;
using GingerCore;
using GingerCore.Activities;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
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

        public BusinessFlowConfigurationsPage(BusinessFlow businessFlow, Context context, Ginger.General.eRIPageViewMode pageViewMode)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;
            mContext = context;
            mPageViewMode = pageViewMode;

            mBusinessFlow.Activities.CollectionChanged += mBusinessFlowActivities_CollectionChanged;
            TrackBusinessFlowAutomationPrecentage();
            BindControls();
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
            if (mPageViewMode == Ginger.General.eRIPageViewMode.View)
            {
                xNameTxtBox.IsEnabled = false;
                xDescriptionTxt.IsEnabled = false;
                xTagsViewer.IsEnabled = false;
                xRunDescritpion.IsEnabled = false;
                xStatusComboBox.IsEnabled = false;
                xCreatedByTextBox.IsEnabled = false;
                xAutoPrecentageTextBox.IsEnabled = false;
                xTargetsListBox.IsEnabled = false;
                xAddTargetBtn.IsEnabled = false;
            }

            BindingHandler.ObjFieldBinding(xNameTxtBox, TextBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.Name));
            BindingHandler.ObjFieldBinding(xDescriptionTxt, TextBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.Description));
            xTagsViewer.Init(mBusinessFlow.Tags);
            xRunDescritpion.Init(mContext, mBusinessFlow, nameof(BusinessFlow.RunDescription));
            General.FillComboFromEnumObj(xStatusComboBox, mBusinessFlow.Status);
            BindingHandler.ObjFieldBinding(xStatusComboBox, ComboBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.Status));
            BindingHandler.ObjFieldBinding(xCreatedByTextBox, TextBox.TextProperty, mBusinessFlow.RepositoryItemHeader, nameof(RepositoryItemHeader.CreatedBy));
            BindingHandler.ObjFieldBinding(xAutoPrecentageTextBox, TextBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.AutomationPrecentage), System.Windows.Data.BindingMode.OneWay);

            //// Per source we can show specific source page info
            //if (mBusinessFlow.Source == BusinessFlow.eSource.Gherkin)
            //{
            //    SourceGherkinPage SGP = new SourceGherkinPage(mBusinessFlow);
            //    SourceFrame.Content = SGP;
            //}

            xTargetsListBox.ItemsSource = mBusinessFlow.TargetApplications;
            xTargetsListBox.DisplayMemberPath = nameof(TargetApplication.AppName);
        }

        private void ClearBindings()
        {
            BindingOperations.ClearAllBindings(xNameTxtBox);
            BindingOperations.ClearAllBindings(xDescriptionTxt);
            xTagsViewer.ClearBinding();
            BindingOperations.ClearAllBindings(xStatusComboBox);
            BindingOperations.ClearAllBindings(xCreatedByTextBox);
            BindingOperations.ClearAllBindings(xAutoPrecentageTextBox);
            BindingOperations.ClearAllBindings(xTargetsListBox);
        }

        public void UpdateBusinessFlow(BusinessFlow updateBusinessFlow)
        {
            if (mBusinessFlow != updateBusinessFlow)
            {
                ClearBindings();
                mBusinessFlow = updateBusinessFlow;
                mContext.BusinessFlow = mBusinessFlow;
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