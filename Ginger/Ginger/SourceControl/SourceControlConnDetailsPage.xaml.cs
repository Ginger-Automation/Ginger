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
using Ginger.SolutionGeneral;
using GingerCoreNET.SourceControl;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.SourceControl
{
    /// <summary>
    /// Interaction logic for SvnConnectionDetailsPage.xaml
    /// </summary>
    public partial class SourceControlConnDetailsPage : Page
    {
        GenericWindow genWin = null;

        public enum eSourceControlContext
        {
            ConnectionDetailsPage,
            DownloadProjectPage
        }

        public SourceControlConnDetailsPage()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            Bind();
        }

        private void Bind()
        {
            SourceControlClassTextBox.Text = SourceControlIntegration.GetSourceControlType(WorkSpace.Instance.Solution.SourceControl);
            SourceControlClassTextBox.IsReadOnly = true;
            SourceControlClassTextBox.IsEnabled = false;

            string repositoryURL = SourceControlIntegration.GetRepositoryURL(WorkSpace.Instance.Solution.SourceControl);
            SourceControlURLTextBox.Text = repositoryURL;
            SourceControlURLTextBox.IsReadOnly = true;
            SourceControlURLTextBox.IsEnabled = false;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlUserTextBox, TextBox.TextProperty, WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.Username));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlPassTextBox, TextBox.TextProperty, WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.Password));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSourceControlBranchTextBox, TextBox.TextProperty, WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.BranchName));

            if (SourceControlClassTextBox.Text == SourceControlBase.eSourceControlType.GIT.ToString())
            {
                xTimeoutRow.Height = new GridLength(0);
            }
            else
            {
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xTextSourceControlConnectionTimeout, TextBox.TextProperty, WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.Timeout));
                xBranchRow.Height = new GridLength(0);
            }
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlUserAuthorNameTextBox, TextBox.TextProperty, WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.AuthorName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlAuthorEmailTextBox, TextBox.TextProperty, WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.AuthorEmail));

            if (WorkSpace.Instance.Solution.SourceControl.IsSupportingLocks)
            {
                ShowIndicationkForLockedItems.Visibility = Visibility.Visible;
            }
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ShowIndicationkForLockedItems, CheckBox.IsCheckedProperty, WorkSpace.Instance.Solution, nameof(Solution.ShowIndicationkForLockedItems));

            SourceControlPassTextBox.Password = WorkSpace.Instance.Solution.SourceControl.Password;

            SourceControlPassTextBox.PasswordChanged += SourceControlPassTextBox_PasswordChanged;

            if (String.IsNullOrEmpty(WorkSpace.Instance.Solution.SourceControl.AuthorEmail))
            {
                WorkSpace.Instance.Solution.SourceControl.AuthorEmail = WorkSpace.Instance.Solution.SourceControl.Username;
            }
            if (String.IsNullOrEmpty(WorkSpace.Instance.Solution.SourceControl.AuthorName))
            {
                WorkSpace.Instance.Solution.SourceControl.AuthorName = WorkSpace.Instance.Solution.SourceControl.Username;
            }
        }

        private void SourceControlPassTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.Solution.SourceControl.Password = ((PasswordBox)sender).Password;
            WorkSpace.Instance.UserProfile.UserProfileOperations.SaveUserProfile();//todo: check if needed
            SourceControlIntegration.Init(WorkSpace.Instance.Solution.SourceControl);
        }

        public bool TestSourceControlConnection(bool ignorePopup = false)
        {
            bool result = SourceControlUI.TestConnection(WorkSpace.Instance.Solution.SourceControl, ignorePopup);
            Mouse.OverrideCursor = null;
            return result;
        }

        private void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            TestSourceControlConnection();
        }

        private void SaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            var GingerSolutionSourceControl = WorkSpace.Instance.UserProfile.SourceControlInfo(WorkSpace.Instance.Solution.Guid);

            if (TestSourceControlConnection(true))
            {
                if (SourceControlClassTextBox.Text != SourceControlBase.eSourceControlType.GIT.ToString())
                {
                    if (string.IsNullOrEmpty(xTextSourceControlConnectionTimeout.Text) || !Int32.TryParse(xTextSourceControlConnectionTimeout.Text, out int _))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Please provide valid value for connection timeout");
                        return;
                    }
                }
                GingerSolutionSourceControl.SourceControlInfo.Password = WorkSpace.Instance.Solution.SourceControl.Password;
                GingerSolutionSourceControl.SourceControlInfo.Username = WorkSpace.Instance.Solution.SourceControl.Username;
                WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, Solution.eSolutionItemToSave.SourceControlSettings);
                WorkSpace.Instance.UserProfile.UserProfileOperations.SaveUserProfile();
                Close();
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Close()
        {
            var GingerSolutionSourceControl = WorkSpace.Instance.UserProfile.SourceControlInfo(WorkSpace.Instance.Solution.Guid);
            if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.SourceControl != null)
            {
                GingerSolutionSourceControl.SourceControlInfo.Username = WorkSpace.Instance.Solution.SourceControl.Username;
                GingerSolutionSourceControl.SourceControlInfo.Password = WorkSpace.Instance.Solution.SourceControl.Password;
                GingerSolutionSourceControl.SourceControlInfo.AuthorName = WorkSpace.Instance.Solution.SourceControl.AuthorName;
                GingerSolutionSourceControl.SourceControlInfo.AuthorEmail = WorkSpace.Instance.Solution.SourceControl.AuthorName;
                SourceControlIntegration.Disconnect(WorkSpace.Instance.Solution.SourceControl);
            }
            genWin.Close();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button testConnBtn = new Button
            {
                Content = "Test Connection"
            };
            testConnBtn.Click += new RoutedEventHandler(TestConnection_Click);

            Button SaveBtn = new Button
            {
                Content = "Save Configuration"
            };
            SaveBtn.Click += new RoutedEventHandler(SaveConfiguration_Click);

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, [testConnBtn, SaveBtn], true, "Close", Close_Click);
        }

        private void SourceControlUserDetails_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceControlIntegration.Init(WorkSpace.Instance.Solution.SourceControl);

        }

        private void txtSourceControlConnectionTimeout_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceControlIntegration.Init(WorkSpace.Instance.Solution.SourceControl);
            if (string.IsNullOrEmpty(xTextSourceControlConnectionTimeout.Text) || Int32.TryParse(xTextSourceControlConnectionTimeout.Text, out int _))
            {
                return;
            }
            var GingerSolutionSourceControl = WorkSpace.Instance.UserProfile.SourceControlInfo(WorkSpace.Instance.Solution.Guid);

            GingerSolutionSourceControl.SourceControlInfo.Timeout = Int32.Parse(xTextSourceControlConnectionTimeout.Text);
        }
    }
}