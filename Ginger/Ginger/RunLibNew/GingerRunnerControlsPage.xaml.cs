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

using Amdocs.Ginger.Common.Enums;
using Ginger;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCoreNET.RunLib;
using GingerWPF.AgentsLib;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.RunLib
{
    /// <summary>
    /// Interaction logic for GingerRunnerControlsPage.xaml
    /// </summary>
    public partial class GingerRunnerControlsPage : Page
    {
        GingerRunner mGingerRunner;
        System.Diagnostics.Stopwatch mStopwatch;
        public GingerRunnerControlsPage(GingerRunner GingerRunner)
        {
            InitializeComponent();
            mGingerRunner = GingerRunner;
            //FillAgentsCombo();
            //AppAgentLabel.BindControl(mGingerRunner, nameof(GingerRunner.ApplicationAgentsInfo));
            //mGingerRunner.GingerRunnerEvent += GingerRunner_GingerRunnerEvent;
            //SetMiniView(false);
        }

        private void GingerRunner_GingerRunnerEvent(GingerRunnerEventArgs EventArgs)
        {
            // Events will come from other threads so we use dispatcher
            StatusLabel.Dispatcher.Invoke(() =>
            {
                StatusLabel.Content = EventArgs.EventType.ToString();
            });
        }

        private void FillAgentsCombo()
        {
        }

        private void DisbleRunButtons()
        {            
            StopButton.IsEnabled = true;
            StopButton.Refresh();
            RunActionButton.IsEnabled = false;
            RunActivityButton.IsEnabled = false;
            RunButton.IsEnabled = false;

            StatusImageControl.ImageType = eImageType.Processing;

            if (mStopwatch == null) mStopwatch = new System.Diagnostics.Stopwatch();
            mStopwatch.Reset();
            mStopwatch.Start();
        }

        private void EnableRunButtons()
        {
            mStopwatch.Stop();
            StatusLabel.Content = "Elapsed: " + mStopwatch.ElapsedMilliseconds;

            StatusImageControl.ImageType = eImageType.Ready;                        

            StopButton.IsEnabled = false;
            RunActionButton.IsEnabled = true;
            RunActivityButton.IsEnabled = true;
            RunButton.IsEnabled = true;                        
        }

        // temp !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // Run Action
        private async void RunActionButton_Click(object sender, RoutedEventArgs e)
        {
            DisbleRunButtons();

            Act act = (Act)mGingerRunner.Executor.CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
            int result = await mGingerRunner.Executor.RunActionAsync(act);

            //TODO: temp we get 0 + take it to flag button or something !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            EnableRunButtons();
        }

        // Run Activity
        private async void RunActivityButton_Click(object sender, RoutedEventArgs e)
        {
            DisbleRunButtons();            
            int result = await mGingerRunner.Executor.RunActivityAsync((Activity)mGingerRunner.Executor.CurrentBusinessFlow.CurrentActivity);
            EnableRunButtons();
        }

        // Run Flow
        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            DisbleRunButtons();
            // var result = await mGingerRunner.RunFlowAsync();
            EnableRunButtons();
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            MapApplicationAgentPage p = new MapApplicationAgentPage(mGingerRunner.ApplicationAgents);
            p.ShowAsWindow(null);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            mGingerRunner.Executor.StopRun();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            mGingerRunner.Executor.CurrentBusinessFlow.Reset();
        }

        private void MiniButton_Click(object sender, RoutedEventArgs e)
        {
            SetMiniView(true);
            Grid g = MainGrid;            
            MainContent.Content = null;
            GingerRunnerControlsMiniWindow w2 = new GingerRunnerControlsMiniWindow(g, MainContent, SetMiniView);
            w2.Show();                               
        }

        public void SetMiniView(bool b)
        {
            ////TODO: make it nice small window
            //if (b)
            //{
            //    MiniButton.Visibility = Visibility.Collapsed;
            //    //TODO: show small icons only with tool tip
            //    StopButton.FontSize = 8;
            //    StopButton.Width = 30;
            //    ActivitiesComboBox.ItemsSource = mGingerRunner.CurrentBusinessFlow.Activities;
            //    ActivitiesComboBox.DisplayMemberPath = nameof(Activity.ActivityName);                
            //    ControlsBinding.ObjFieldBinding(ActivitiesComboBox, ComboBox.SelectedItemProperty, mGingerRunner.CurrentBusinessFlow, "CurrentActivity");
            //    MiniWindowStackPanel.Visibility = Visibility.Visible;
            //    Grid.SetRow(MiniWindowStackPanel,1);
            //    MaxiWindowStackPanel.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    MiniButton.Visibility = Visibility.Visible;
            //    StopButton.FontSize = 12;
            //    StopButton.Width = 40;
            //    MiniWindowStackPanel.Visibility = Visibility.Collapsed;
            //    MaxiWindowStackPanel.Visibility = Visibility.Visible;
            //}
        }
    }
}
