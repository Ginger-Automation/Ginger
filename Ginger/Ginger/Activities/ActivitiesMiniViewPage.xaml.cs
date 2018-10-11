#region License
/*
Copyright © 2014-2018 European Support Limited

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
        public ActivitiesMiniViewPage()
        {
            InitializeComponent();
            
            GingerCore.General.DoEvents();
            LoadActivitiesList();

            App.BusinessFlow.PropertyChanged += BizflowPropChanged;
            App.PropertyChanged += AppPropertyChanged;
        }

        private void LoadActivitiesList()
        {
            lstActivities.ItemsSource = App.BusinessFlow.Activities;
            lstActivities.DisplayMemberPath = "ActivityName";
            lstActivities.SelectedValue = nameof(RepositoryItemBase.Guid);
            try
            {
                lstActivities.SelectedItem = App.BusinessFlow.CurrentActivity;
            }
            catch (Exception) { }
        }

        private void AppPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           GingerCore.General.DoEvents();
            if (e.PropertyName == "BusinessFlow")
            {
                if (App.BusinessFlow != null)
                    LoadActivitiesList();
            }
        }

        private void BizflowPropChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            GingerCore.General.DoEvents();
            this.Dispatcher.Invoke(() => {
                if (e.PropertyName == "CurrentActivity")
                {
                    if (App.BusinessFlow != null)
                        lstActivities.SelectedItem = App.BusinessFlow.CurrentActivity;
                }
            });
            //TODO: use const field name, compile check
        }

        private void lstActivities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GingerCore.General.DoEvents();
            App.BusinessFlow.CurrentActivity = (Activity)lstActivities.SelectedItem;
        }
    }
}
