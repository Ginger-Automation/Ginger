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
using System.Windows.Media;
using System.Windows.Threading;

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
            Bind();
        }


        private void CopyMouseDown(object sender, string txtToCopy)
        {
            CopyToClipBoard(txtToCopy);

            if (sender is Control control)
            {
                control.Foreground = new SolidColorBrush(Colors.Orange);
                ShowToolTip(control, "Copied to clipboard");
            }
        }

        public void CopyToClipBoard(string txtToCopy)
        {
            GingerCore.General.SetClipboardText(txtToCopy);
        }

        private void ShowToolTip(Control control, string message)
        {
            ToolTip toolTip = new ToolTip
            {
                Content = message,
                IsOpen = true,
                StaysOpen = false,
                PlacementTarget = control,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse
            };


            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
            timer.Tick += (_, _) =>
            {
                toolTip.IsOpen = false;
                control.Foreground = new SolidColorBrush(Colors.Black);
                timer.Stop();
            };
            timer.Start();

        }

        // Event handlers
        private void CopyMouseDownURL(object sender, MouseButtonEventArgs e)
        {
            CopyMouseDown(sender, SourceControlURLTextBox.Text);
        }

        private void CopyMouseDownBranch(object sender, MouseButtonEventArgs e)
        {
            CopyMouseDown(sender, xSourceControlBranchTextBox.Text);
        }
        private void CopyMouseDownUserID(object sender, MouseButtonEventArgs e)
        {
            CopyMouseDown(sender, SourceControlUserTextBox.Text);
        }
        private void CopyMouseDownPassword(object sender, MouseButtonEventArgs e)
        {
            CopyMouseDown(sender, SourceControlPassTextBox.Password);
        }
        private void CopyMouseDownAuthorName(object sender, MouseButtonEventArgs e)
        {
            CopyMouseDown(sender, SourceControlUserAuthorNameTextBox.Text);
        }
        private void CopyMouseDownAuthorMail(object sender, MouseButtonEventArgs e)
        {
            CopyMouseDown(sender, SourceControlAuthorEmailTextBox.Text);
        }
        private void CopyMouseDownTimeOut(object sender, MouseButtonEventArgs e)
        {
            CopyMouseDown(sender, xTextSourceControlConnectionTimeout.Text);
        }
        private void CopyMouseDownGitType(object sender, MouseButtonEventArgs e)
        {
            CopyMouseDown(sender, SourceControlClassTextBox.Text);
        }

        private void Bind()
        {

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlUserTextBox, TextBox.TextProperty, WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.Username));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlPassTextBox, TextBox.TextProperty, WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.Password));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSourceControlBranchTextBox, TextBox.TextProperty, WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.Branch));


            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xTextSourceControlConnectionTimeout, TextBox.TextProperty, WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.Timeout));

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
            SetSourceControlDetailsFromUserProfile();

        }

        private void SetSourceControlDetailsFromUserProfile()
        {
            try
            {
                var GingerSolutionSourceControl = WorkSpace.Instance.UserProfile.GetSolutionSourceControlInfo(WorkSpace.Instance.Solution.Guid);

                SourceControlClassTextBox.IsReadOnly = true;
                SourceControlClassTextBox.IsEnabled = false;

                SourceControlURLTextBox.IsReadOnly = true;
                SourceControlURLTextBox.IsEnabled = false;

                WorkSpace.Instance.UserProfile.UserProfileOperations.LoadPasswords(WorkSpace.Instance.UserProfile);

                if (GingerSolutionSourceControl != null)
                {

                    GingerSolutionSourceControl.SourceControlInfo.Url = SourceControlIntegration.GetRepositoryURL(WorkSpace.Instance.Solution.SourceControl);
                    SourceControlURLTextBox.Text = GingerSolutionSourceControl.SourceControlInfo.Url;
                    if (SourceControlURLTextBox.Text.Contains("git", StringComparison.OrdinalIgnoreCase))
                    {
                        SourceControlClassTextBox.Text = nameof(SourceControlBase.eSourceControlType.GIT);
                    }
                    else
                    {
                        SourceControlClassTextBox.Text = nameof(GingerSolutionSourceControl.SourceControlInfo.Type);
                    }
                    if (string.IsNullOrEmpty(GingerSolutionSourceControl.SourceControlInfo.Branch))
                    {
                        xSourceControlBranchTextBox.Text = SourceControlIntegration.GetCurrentBranchForSolution(WorkSpace.Instance.Solution.SourceControl);
                    }
                    else
                    {
                        xSourceControlBranchTextBox.Text = GingerSolutionSourceControl.SourceControlInfo.Branch;
                    }
                    SourceControlUserTextBox.Text = GingerSolutionSourceControl.SourceControlInfo.Username;
                    SourceControlPassTextBox.Password = GingerSolutionSourceControl.SourceControlInfo.Password;
                    SourceControlUserAuthorNameTextBox.Text = GingerSolutionSourceControl.SourceControlInfo.AuthorName;
                    SourceControlAuthorEmailTextBox.Text = GingerSolutionSourceControl.SourceControlInfo.AuthorEmail;

                    if (GingerSolutionSourceControl.SourceControlInfo.Timeout <= 0)
                    {
                        xTextSourceControlConnectionTimeout.Text = "80";
                    }
                    else
                    {
                        xTextSourceControlConnectionTimeout.Text = GingerSolutionSourceControl.SourceControlInfo.Timeout.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.ToString());
            }
        }

        private void SourceControlPassTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.Solution.SourceControl.Password = ((PasswordBox)sender).Password;
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

            if (TestSourceControlConnection(true))
            {
                if (SourceControlClassTextBox.Text != nameof(SourceControlBase.eSourceControlType.GIT))
                {
                    if (string.IsNullOrEmpty(xTextSourceControlConnectionTimeout.Text) || !Int32.TryParse(xTextSourceControlConnectionTimeout.Text, out int _))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Please provide valid value for connection timeout");
                        return;
                    }
                }
                Close();
                WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, Solution.eSolutionItemToSave.SourceControlSettings);
                WorkSpace.Instance.UserProfile.UserProfileOperations.SaveUserProfile();

            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Close()
        {
            if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.SourceControl != null)
            {

                WorkSpace.Instance.UserProfile.SetSourceControlPropertyOnUserProfile(WorkSpace.Instance.Solution.SourceControl, WorkSpace.Instance.Solution.Guid);

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
            try
            {
                WorkSpace.Instance.Solution.SourceControl.Timeout = Int32.Parse(xTextSourceControlConnectionTimeout.Text);
            }
            catch (Exception EX)
            {
                Reporter.ToLog(eLogLevel.ERROR, EX.ToString());
            }
        }
    }
}