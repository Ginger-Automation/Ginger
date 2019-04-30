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
using Amdocs.Ginger.Common.Repository;
using Ginger;
using Ginger.BusinessFlowLib;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowWindows;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for BusinessFlowViewPage.xaml
    /// </summary>
    public partial class BusinessFlowViewPage : Page
    {
        BusinessFlow mBusinessFlow;
        Context mContext;
        Ginger.General.RepositoryItemPageViewMode mPageMode;

        ActivitiesListViewPage mActivitiesPage;
        VariabelsListViewPage mVariabelsPage;

        public BusinessFlowViewPage(BusinessFlow businessFlow, Context context, Ginger.General.RepositoryItemPageViewMode pageMode)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;
            mContext = context;
            if (mContext == null)
            {
                mContext = new Context();
            }
            mContext.BusinessFlow = mBusinessFlow;
            mPageMode = pageMode;

            SetUIControlsContent();
            BindControls();
        }

        private void SetUIControlsContent()
        {
            mActivitiesPage = new ActivitiesListViewPage(mBusinessFlow, mContext);
            mActivitiesPage.ListView.ListTitleVisibility = Visibility.Collapsed;
            xActivitiesTabFrame.Content = mActivitiesPage;

            mVariabelsPage = new VariabelsListViewPage(mBusinessFlow, mContext);
            mVariabelsPage.ListView.ListTitleVisibility = Visibility.Collapsed;
            xVariabelsTabFrame.Content = mVariabelsPage;
        }

        public void UpdateBusinessFlow(BusinessFlow businessFlow)
        {
            if (mBusinessFlow != businessFlow)
            {
                RemoveBindings();
                mBusinessFlow = businessFlow;
                mContext.BusinessFlow = mBusinessFlow;
                if (mBusinessFlow != null)
                {
                    BindControls();
                }
            }
        }

        private void RemoveBindings()
        {
            mBusinessFlow.Activities.CollectionChanged -= Activities_CollectionChanged;
            mBusinessFlow.Variables.CollectionChanged -= Variables_CollectionChanged;
        }

        private void BindControls()
        {
            //General Info Section Bindings
            BindingHandler.ObjFieldBinding(xNameTextBlock, TextBlock.TextProperty, mBusinessFlow, nameof(BusinessFlow.Name));
            mBusinessFlow.PropertyChanged += mBusinessFlow_PropertyChanged;
            UpdateDescription();

            //Actions Tab Bindings            
            mBusinessFlow.Activities.CollectionChanged += Activities_CollectionChanged;
            UpdateActivitiesTabHeader();
            mActivitiesPage.UpdateBusinessFlow(mBusinessFlow);

            //Variables Tab Bindings            
            mBusinessFlow.Variables.CollectionChanged += Variables_CollectionChanged;
            UpdateVariabelsTabHeader();
            mVariabelsPage.UpdateParent(mBusinessFlow);

            //Configurations Tab Bindings
            
        }

        private void mBusinessFlow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            xDescriptionTextBlock.Text = string.Empty;
            TextBlockHelper xDescTextBlockHelper = new TextBlockHelper(xDescriptionTextBlock);
            //SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$Color_DarkBlue")).ToString());

            if (!string.IsNullOrEmpty(mBusinessFlow.Description))
            {
                xDescTextBlockHelper.AddText("Description: " + mBusinessFlow.Description);
                xDescTextBlockHelper.AddText(" " + Ginger.General.GetTagsListAsString(mBusinessFlow.Tags));
                xDescTextBlockHelper.AddLineBreak();
            }
            if (!string.IsNullOrEmpty(mBusinessFlow.RunDescription))
            {
                xDescTextBlockHelper.AddText("Run Description: " + mBusinessFlow.RunDescription);
                xDescTextBlockHelper.AddLineBreak();
            }
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
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.Automate, mBusinessFlow);
        }
    }
}
