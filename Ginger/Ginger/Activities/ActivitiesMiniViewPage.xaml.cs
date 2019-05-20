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

using Amdocs.Ginger.Repository;
using GingerCore;
using System;
using System.Windows.Controls;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for ActivitiesBusinessFlowTree.xaml
    /// </summary>
    //TODO:  Rename to ActivitiesMiniViewPage
    public partial class ActivitiesMiniViewPage : Page
    {
        BusinessFlow mBusinessFlow;

        public ActivitiesMiniViewPage(BusinessFlow businessFlow)
        {
            InitializeComponent();

            UpdateBusinessFlow(businessFlow);
        }

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            mBusinessFlow = bf;
            if (mBusinessFlow != null)
            {
                mBusinessFlow.PropertyChanged += BizflowPropChanged;
                GingerCore.General.DoEvents();
                LoadActivitiesList();
            }
        }

        private void LoadActivitiesList()
        {
            lstActivities.ItemsSource = mBusinessFlow.Activities;
            lstActivities.DisplayMemberPath = nameof(Activity.ActivityName);
            lstActivities.SelectedValue = nameof(RepositoryItemBase.Guid);
            try
            {
                lstActivities.SelectedItem = mBusinessFlow.CurrentActivity;
            }
            catch (Exception) { }
        }

        private void BizflowPropChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            GingerCore.General.DoEvents();
            this.Dispatcher.Invoke(() => {
                if (e.PropertyName == nameof(BusinessFlow.CurrentActivity))
                {
                    if (mBusinessFlow != null)
                        lstActivities.SelectedItem = mBusinessFlow.CurrentActivity;
                }
            });
        }

        private void lstActivities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GingerCore.General.DoEvents();
            mBusinessFlow.CurrentActivity = (Activity)lstActivities.SelectedItem;
        }
    }
}
