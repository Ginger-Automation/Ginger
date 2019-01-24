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

using Amdocs.Ginger.Common;
using Ginger.Environments;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using amdocs.ginger.GingerCoreNET;

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
            SourceControlClassTextBox.Text = SourceControlIntegration.GetSourceControlType( WorkSpace.UserProfile.Solution.SourceControl);
            SourceControlClassTextBox.IsReadOnly = true;
            SourceControlClassTextBox.IsEnabled = false;

            SourceControlURLTextBox.Text = SourceControlIntegration.GetRepositoryURL( WorkSpace.UserProfile.Solution.SourceControl);
            SourceControlURLTextBox.IsReadOnly = true;
            SourceControlURLTextBox.IsEnabled = false;

            App.ObjFieldBinding(SourceControlUserTextBox, TextBox.TextProperty,  WorkSpace.UserProfile.Solution.SourceControl, nameof(SourceControlBase.SourceControlUser));
            App.ObjFieldBinding(SourceControlPassTextBox, TextBox.TextProperty,  WorkSpace.UserProfile.Solution.SourceControl, nameof(SourceControlBase.SourceControlPass));
            if (SourceControlClassTextBox.Text == SourceControlBase.eSourceControlType.GIT.ToString())
            {
                xTimeoutPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                App.ObjFieldBinding(xTextSourceControlConnectionTimeout, TextBox.TextProperty,  WorkSpace.UserProfile.Solution.SourceControl, nameof(SourceControlBase.SourceControlTimeout));
                xTimeoutPanel.Visibility = Visibility.Visible;
               
              
            }
            App.ObjFieldBinding(SourceControlUserAuthorNameTextBox, TextBox.TextProperty,  WorkSpace.UserProfile.Solution.SourceControl, nameof(SourceControlBase.SolutionSourceControlAuthorName));
            App.ObjFieldBinding(SourceControlAuthorEmailTextBox, TextBox.TextProperty,  WorkSpace.UserProfile.Solution.SourceControl, nameof(SourceControlBase.SolutionSourceControlAuthorEmail));

            if ( WorkSpace.UserProfile.Solution.SourceControl.IsSupportingLocks)
            {
                ShowIndicationkForLockedItems.Visibility = Visibility.Visible;
            }
            App.ObjFieldBinding(ShowIndicationkForLockedItems, CheckBox.IsCheckedProperty,  WorkSpace.UserProfile.Solution, nameof(Solution.ShowIndicationkForLockedItems));

            SourceControlPassTextBox.Password =  WorkSpace.UserProfile.Solution.SourceControl.SourceControlPass;

            SourceControlPassTextBox.PasswordChanged += SourceControlPassTextBox_PasswordChanged;

            if (String.IsNullOrEmpty( WorkSpace.UserProfile.Solution.SourceControl.SolutionSourceControlAuthorEmail))
            {
                 WorkSpace.UserProfile.Solution.SourceControl.SolutionSourceControlAuthorEmail =  WorkSpace.UserProfile.Solution.SourceControl.SourceControlUser;
            }
            if (String.IsNullOrEmpty( WorkSpace.UserProfile.Solution.SourceControl.SolutionSourceControlAuthorName))
            {
                 WorkSpace.UserProfile.Solution.SourceControl.SolutionSourceControlAuthorName =  WorkSpace.UserProfile.Solution.SourceControl.SourceControlUser;
            }
        }

        private void SourceControlPassTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
             WorkSpace.UserProfile.Solution.SourceControl.SourceControlPass = ((PasswordBox)sender).Password;
             WorkSpace.UserProfile.SaveUserProfile();//todo: check if needed
            SourceControlIntegration.Init( WorkSpace.UserProfile.Solution.SourceControl);
        }

        public bool TestSourceControlConnection()
        {
            bool result = SourceControlIntegration.TestConnection( WorkSpace.UserProfile.Solution.SourceControl, eSourceControlContext.ConnectionDetailsPage, false);
            Mouse.OverrideCursor = null;
            return result;
        }

        private void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            TestSourceControlConnection();
        }

        private void SaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
             WorkSpace.UserProfile.Solution.SaveSolution(true, Solution.eSolutionItemToSave.SourceControlSettings);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if ( WorkSpace.UserProfile.Solution != null &&  WorkSpace.UserProfile.Solution.SourceControl != null)
            {
                 WorkSpace.UserProfile.SolutionSourceControlUser =  WorkSpace.UserProfile.Solution.SourceControl.SourceControlUser;
                 WorkSpace.UserProfile.SolutionSourceControlPass =  WorkSpace.UserProfile.Solution.SourceControl.SourceControlPass;
                 WorkSpace.UserProfile.SolutionSourceControlAuthorName =  WorkSpace.UserProfile.Solution.SourceControl.SolutionSourceControlAuthorName;
                 WorkSpace.UserProfile.SolutionSourceControlAuthorEmail =  WorkSpace.UserProfile.Solution.SourceControl.SolutionSourceControlAuthorName;
                SourceControlIntegration.Disconnect( WorkSpace.UserProfile.Solution.SourceControl);

            }
            genWin.Close();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button testConnBtn = new Button();
            testConnBtn.Content = "Test Connection";
            testConnBtn.Click += new RoutedEventHandler(TestConnection_Click);

            Button SaveBtn = new Button();
            SaveBtn.Content = "Save Configuration";
            SaveBtn.Click += new RoutedEventHandler(SaveConfiguration_Click);

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { testConnBtn, SaveBtn }, true, "Close", new RoutedEventHandler(Close_Click));
        }

        private void SourceControlUserDetails_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceControlIntegration.Init( WorkSpace.UserProfile.Solution.SourceControl);
            
        }

        private void txtSourceControlConnectionTimeout_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceControlIntegration.Init( WorkSpace.UserProfile.Solution.SourceControl);
             WorkSpace.UserProfile.SolutionSourceControlTimeout = Int32.Parse(xTextSourceControlConnectionTimeout.Text);
        }
    }
}