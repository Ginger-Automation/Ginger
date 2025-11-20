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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActLaunchWSJavaApplicationEditPage.xaml
    /// </summary>
    // Changing the classname will break the old actions. 
    public partial class ActLaunchJavaWSApplicationEditPage : Page
    {
        ActLaunchJavaWSApplication mAct;

        public ActLaunchJavaWSApplicationEditPage(ActLaunchJavaWSApplication act)
        {
            InitializeComponent();

            mAct = act;

            //initial content look
            LaunchJavaApplicationArgsPnl.Visibility = System.Windows.Visibility.Collapsed;
            JavaApplicationLaunchWaitForWinTitlePnl.Visibility = System.Windows.Visibility.Collapsed;
            LaunchWithAgentArgsPnl.Visibility = System.Windows.Visibility.Collapsed;

            DoBinding();

            SetInitialLookAfterBind();

            RemoveOldInputParams();
        }

        private void DoBinding()
        {
            JavaPathTextBox.Init(Context.GetAsContext(mAct.Context), mAct, ActLaunchJavaWSApplication.Fields.JavaWSEXEPath);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(LaunchJavaApplicationChkbox, CheckBox.IsCheckedProperty, mAct, ActLaunchJavaWSApplication.Fields.LaunchJavaApplication);
            JavaApplicationPathTextBox.Init(Context.GetAsContext(mAct.Context), mAct, ActLaunchJavaWSApplication.Fields.URL);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(JavaApplicationLaunchWaitForWinTitleChckBox, CheckBox.IsCheckedProperty, mAct, ActLaunchJavaWSApplication.Fields.WaitForWindowWhenDoingLaunch);
            WaitForWindowTitleTextBox.Init(Context.GetAsContext(mAct.Context), mAct, ActLaunchJavaWSApplication.Fields.WaitForWindowTitle);
            WaitForWindowTitleMaxTimeTextBox.Init(Context.GetAsContext(mAct.Context), mAct, ActLaunchJavaWSApplication.Fields.WaitForWindowTitleMaxTime);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(BlockingWindowChkbox, CheckBox.IsCheckedProperty, mAct, ActLaunchJavaWSApplication.Fields.BlockingJavaWindow);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(LaunchWithAgent, CheckBox.IsCheckedProperty, mAct, ActLaunchJavaWSApplication.Fields.LaunchWithAgent);
            AgentPathTextBox.Init(Context.GetAsContext(mAct.Context), mAct, ActLaunchJavaWSApplication.Fields.JavaAgentPath);
            JavaAgentPortTextBox.Init(Context.GetAsContext(mAct.Context), mAct, ActLaunchJavaWSApplication.Fields.Port);

            AttachAgentProcessSyncTime.Init(Context.GetAsContext(mAct.Context), mAct, ActLaunchJavaWSApplication.Fields.AttachAgentProcessSyncTime);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ShowAgent, CheckBox.IsCheckedProperty, mAct, ActLaunchJavaWSApplication.Fields.ShowAgent);

            rbGroupPortConfig.Init(typeof(ActLaunchJavaWSApplication.ePortConfigType), RadioButtonPanel, mAct.GetOrCreateInputParam(ActLaunchJavaWSApplication.Fields.PortConfigParam, ActLaunchJavaWSApplication.ePortConfigType.Manual.ToString()), new RoutedEventHandler(PortConfigRB_Click));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ApplicationProcessNameChkBox, CheckBox.IsCheckedProperty, mAct, ActLaunchJavaWSApplication.Fields.IsCustomApplicationProcessName);
            ApplicationProcessNameTextBox.Init(Context.GetAsContext(mAct.Context), mAct, ActLaunchJavaWSApplication.Fields.ApplicationProcessName);           

            UpdateAgentPortTextBoxEnabledStatus();
        }

        private void SetInitialLookAfterBind()
        {
            JavaPathHomeRdb.Content = "Use JAVA HOME Environment Variable (" + CommonLib.GetJavaHome() + ")";
            if (string.IsNullOrEmpty(mAct.JavaWSEXEPath))
            {
                JavaPathHomeRdb.IsChecked = true;
            }
            else
            {
                JavaPathOtherRdb.IsChecked = true;
            }

            if (string.IsNullOrEmpty(mAct.JavaAgentPath))
            {
                GingerAgentFromGingerFolderRdb.IsChecked = true;
            }
            else
            {
                GingerAgentFromOtherRdb.IsChecked = true;
            }

            if (mAct.IsCustomApplicationProcessName)
            {
                ApplicationProcessNameChkBox.IsChecked = true;
            }
            else
            {
                ApplicationProcessNameTextBox.Visibility = Visibility.Hidden;
                ApplicationProcessNameChkBox.IsChecked = false;
            }
        }

        private void RemoveOldInputParams()
        {
            if (mAct.InputValues.FirstOrDefault(x => x.Param == "Value" && x.Value == string.Empty) != null)
            {
                mAct.RemoveInputParam("Value");
            }

            if (mAct.InputValues.FirstOrDefault(x => x.Param == "Port") != null)
            {
                mAct.RemoveInputParam("Port");
            }

            if (mAct.InputValues.FirstOrDefault(x => x.Param == "URL") != null)
            {
                mAct.RemoveInputParam("URL");
            }
        }

        private void AddParam_Click(object sender, RoutedEventArgs e)
        {
            mAct.InputValues.Add(new ActInputValue() { Param = "ParamName", Value = "ParamValue" });
        }

        private void BrowseAgentPathButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = OpenFolderDialog("Select Agent Jars Folder", Environment.SpecialFolder.MyComputer, mAct.JavaAgentPath);
            if (string.IsNullOrEmpty(selectedFolder) == false)
            {
                mAct.JavaAgentPath = selectedFolder;
            }
        }

        private void JavaPathOtherRdb_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (JavaPathOtherRdb.IsChecked == true)
            {
                JavaPathTextBox.IsEnabled = true;
                BrowseJavaPath.IsEnabled = true;
            }
            else
            {
                if (JavaPathOtherRdb.IsVisible)
                {
                    mAct.JavaWSEXEPath = string.Empty;
                }

                JavaPathTextBox.IsEnabled = false;
                BrowseJavaPath.IsEnabled = false;
            }
        }

        private void GingerAgentFromOtherRdb_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (GingerAgentFromOtherRdb.IsChecked == true)
            {
                AgentPathTextBox.IsEnabled = true;
                BrowseAgentPath.IsEnabled = true;
            }
            else
            {
                if (GingerAgentFromOtherRdb.IsVisible)
                {
                    mAct.JavaAgentPath = string.Empty;
                }

                AgentPathTextBox.IsEnabled = false;
                BrowseAgentPath.IsEnabled = false;
            }
        }

        private void LaunchJavaApplicationChkbox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (LaunchJavaApplicationChkbox.IsChecked == true)
            {
                LaunchJavaApplicationChkbox.Content = "Launch Java Application:";
                LaunchJavaApplicationArgsPnl.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                LaunchJavaApplicationChkbox.Content = "Launch Java Application";
                LaunchJavaApplicationArgsPnl.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void LaunchWithAgent_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (LaunchWithAgent.IsChecked == true)
            {
                LaunchWithAgent.Content = "Attach Ginger Java Agent:";
                LaunchWithAgentArgsPnl.Visibility = System.Windows.Visibility.Visible;
                JavaApplicationLaunchWaitForWinTitleChckBox.IsChecked = true;
            }
            else
            {
                LaunchWithAgent.Content = "Attach Ginger Java Agent";
                LaunchWithAgentArgsPnl.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private string OpenFolderDialog(string desc, Environment.SpecialFolder rootFolder, string currentFolder = "")
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = desc,
                RootFolder = rootFolder
            };
            if (currentFolder != "")
            {
                dlg.SelectedPath = currentFolder;
            }

            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return dlg.SelectedPath;
            }
            return null;
        }

        private void BrowseJavaPath_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = OpenFolderDialog("Select Java Version Bin Folder", Environment.SpecialFolder.ProgramFilesX86, mAct.JavaWSEXEPath);
            if (string.IsNullOrEmpty(selectedFolder) == false)
            {
                mAct.JavaWSEXEPath = selectedFolder;
            }
        }

        private void BrowseJavaAppPath_Click(object sender, RoutedEventArgs e)
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()) is string fileName)
            {
                mAct.URL = fileName;
            }
        }

        private void JavaApplicationLaunchWaitForWinTitleChckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (JavaApplicationLaunchWaitForWinTitleChckBox.IsChecked == true)
            {
                JavaApplicationLaunchWaitForWinTitleChckBox.Content = "Wait for Java Application Window:";
                JavaApplicationLaunchWaitForWinTitlePnl.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                if (LaunchWithAgent.IsChecked == true)
                {

                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Wait for Java application window must be done if Attach Ginger Agent operation is selected.");
                    e.Handled = true;
                    JavaApplicationLaunchWaitForWinTitleChckBox.IsChecked = true;
                }
                else
                {
                    JavaApplicationLaunchWaitForWinTitleChckBox.Content = "Wait for Java Application Window";
                    JavaApplicationLaunchWaitForWinTitlePnl.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void PortConfigRB_Click(object sender, RoutedEventArgs e)
        {
            UpdateAgentPortTextBoxEnabledStatus();
        }

        private void UpdateAgentPortTextBoxEnabledStatus()
        {
            ActLaunchJavaWSApplication.ePortConfigType portConfigType = (ActLaunchJavaWSApplication.ePortConfigType)mAct.GetInputParamValue<ActLaunchJavaWSApplication.ePortConfigType>(ActLaunchJavaWSApplication.Fields.PortConfigParam);
            if (portConfigType == ActLaunchJavaWSApplication.ePortConfigType.AutoDetect)
            {
                JavaAgentPortTextBox.IsEnabled = false;
            }
            else
            {
                JavaAgentPortTextBox.IsEnabled = true;
            }

        }

        private void ApplicationProcessNameChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplicationProcessNameTextBox.Visibility = Visibility.Hidden;
            ApplicationProcessNameTextBox.ValueTextBox.Text = null;
        }

        private void ApplicationProcessNameChkBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationProcessNameTextBox.Visibility = Visibility.Visible;
        }
    }
}
