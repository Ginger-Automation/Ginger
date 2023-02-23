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

using GingerCore.GeneralLib;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Run
{
    public partial class RunsetRunnersConfigPage : Page
    {
        GenericWindow mGenWin = null;
        RunSetConfig mRunSetConfig = null;

        public RunsetRunnersConfigPage(RunSetConfig runSetConfig)
        {
            InitializeComponent();

            mRunSetConfig = runSetConfig;

            SetControls();
        }            

        private void SetControls()
        {
            BindingHandler.ObjFieldBinding(xRunAnalyzerChkbox, CheckBox.IsCheckedProperty, mRunSetConfig, nameof(RunSetConfig.RunWithAnalyzer));
            BindingHandler.ObjFieldBinding(xStopRunnersExecutionOnFailureChkbox, CheckBox.IsCheckedProperty, mRunSetConfig, nameof(RunSetConfig.StopRunnersOnFailure));

            if (mRunSetConfig.RunModeParallel)
            {
                xParallelOptionRdBtn.IsChecked = true;
            }
            else
            {
                xSequentiallyOptionRdBtn.IsChecked = true;
            }              
        }

        private void xParallelOptionRdBtn_Checked(object sender, RoutedEventArgs e)
        {
            mRunSetConfig.RunModeParallel = true;
            mRunSetConfig.StopRunnersOnFailure = false;
            xStopRunnersExecutionOnFailureChkbox.IsEnabled = false;
        }

        private void xSequentiallyOptionRdBtn_Checked(object sender, RoutedEventArgs e)
        {
            mRunSetConfig.RunModeParallel = false;
            xStopRunnersExecutionOnFailureChkbox.IsEnabled = true;
        }

        public void ShowAsWindow()
        {
            GingerCore.General.LoadGenericWindow(ref mGenWin, App.MainWindow, eWindowShowStyle.Dialog, this.Title, this);
        }
    }
}
