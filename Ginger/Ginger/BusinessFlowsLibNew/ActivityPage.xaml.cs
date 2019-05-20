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
using Ginger;
using Ginger.ActionLib;
using Ginger.BusinessFlowLib;
using Ginger.Run;
using GingerCore;
using GingerWPF.GeneralLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for ActivityPage.xaml
    /// </summary>
    public partial class ActivityPage : Page
    {
        Activity mActivity;
        Context mContext;

        // We keep a static page so even if we move between activities the Run controls and info stay the same
        public ActivityPage(Activity Activity, Context context)
        {
            InitializeComponent();

            mActivity = Activity;
            mContext = context;

            ActivityNameLabel.Content = mActivity.ActivityName; // TODO: use binding !!!!!!!!!!!!!!!!!!!!!!!
           
            mContext.Runner.CurrentBusinessFlow.Activities.PropertyChanged += CurrentBusinessFlow_PropertyChanged;

            ShowActionsList();

            ActionsCountLabel.BindControl(mActivity.Acts, nameof(IObservableList.Count));
        }

        private void ShowActionsList()
        {
            // TODO: cache the page
            MainFrame.Dispatcher.Invoke(() =>
            {
                //Since we might get event from Ginger runner which is running on another thread we need dispatcher
                MainFrame.Content = new ActivityActionsPage((Activity)mContext.Runner.CurrentBusinessFlow.CurrentActivity);
            });
        }

        private void CurrentBusinessFlow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IObservableList.CurrentItem))
            {
                ShowActionsList();
            }
        }

        private void ListViewButton_Click(object sender, RoutedEventArgs e)
        {
            ShowActionsList();
        }

        private void FlowViewButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: cache the page
            // !!!!!!!!!!!!!!!!!!!!!! use only activity diagram flow

            // MainFrame.Content = new BusinessFlowDiagramPage(WorkSpace.Instance.GingerRunner.CurrentBusinessFlow); // TODO: show only the current activity

            MainFrame.Content = new BusinessFlowDiagramPage(mContext.Runner.CurrentBusinessFlow); // TODO: show only the current activity
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            List<ActionSelectorItem> actions = new List<ActionSelectorItem>();
            actions.Add(new ActionSelectorItem() { Name = "Record", Action = RecordAction });
            actions.Add(new ActionSelectorItem() { Name = "New Empty Action", Action = AddEmptyAction });
            actions.Add(new ActionSelectorItem() { Name = "Add Action from shared repository", Action = AddActionFromSharedRepository });

            ActionSelectorWindow w = new ActionSelectorWindow("Add Action, please select method", actions);
            w.Show();
        }

        private void AddActionFromSharedRepository()
        {
            throw new NotImplementedException();
        }

        private void AddEmptyAction()
        {
            NewAddActionPage p = new NewAddActionPage();
            p.ShowAsWindow(mContext.Runner.CurrentBusinessFlow.CurrentActivity.Acts);
        }

        private void RecordAction()
        {
            throw new NotImplementedException();
        }

        private void VariablesButton_Click(object sender, RoutedEventArgs e)
        {
            ActivityVariablesPage p = new ActivityVariablesPage(mActivity.Variables);
            MainFrame.Content = p;
        }

        private void ActionsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowActionsList();
        }
    }
}
