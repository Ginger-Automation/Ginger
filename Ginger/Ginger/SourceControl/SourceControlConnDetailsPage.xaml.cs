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
            SourceControlClassTextBox.Text = SourceControlIntegration.GetSourceControlType( WorkSpace.Instance.Solution.SourceControl);
            SourceControlClassTextBox.IsReadOnly = true;
            SourceControlClassTextBox.IsEnabled = false;

            SourceControlURLTextBox.Text = SourceControlIntegration.GetRepositoryURL( WorkSpace.Instance.Solution.SourceControl);
            SourceControlURLTextBox.IsReadOnly = true;
            SourceControlURLTextBox.IsEnabled = false;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlUserTextBox, TextBox.TextProperty,  WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.SourceControlUser));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlPassTextBox, TextBox.TextProperty,  WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.SourceControlPass));
            if (SourceControlClassTextBox.Text == SourceControlBase.eSourceControlType.GIT.ToString())
            {
                xTimeoutPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xTextSourceControlConnectionTimeout, TextBox.TextProperty,  WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.SourceControlTimeout));
                xTimeoutPanel.Visibility = Visibility.Visible;
               
              
            }
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlUserAuthorNameTextBox, TextBox.TextProperty,  WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.SolutionSourceControlAuthorName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlAuthorEmailTextBox, TextBox.TextProperty,  WorkSpace.Instance.Solution.SourceControl, nameof(SourceControlBase.SolutionSourceControlAuthorEmail));

            if ( WorkSpace.Instance.Solution.SourceControl.IsSupportingLocks)
            {
                ShowIndicationkForLockedItems.Visibility = Visibility.Visible;
            }
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ShowIndicationkForLockedItems, CheckBox.IsCheckedProperty,  WorkSpace.Instance.Solution, nameof(Solution.ShowIndicationkForLockedItems));

            SourceControlPassTextBox.Password =  WorkSpace.Instance.Solution.SourceControl.SourceControlPass;

            SourceControlPassTextBox.PasswordChanged += SourceControlPassTextBox_PasswordChanged;

            if (String.IsNullOrEmpty( WorkSpace.Instance.Solution.SourceControl.SolutionSourceControlAuthorEmail))
            {
                 WorkSpace.Instance.Solution.SourceControl.SolutionSourceControlAuthorEmail =  WorkSpace.Instance.Solution.SourceControl.SourceControlUser;
            }
            if (String.IsNullOrEmpty( WorkSpace.Instance.Solution.SourceControl.SolutionSourceControlAuthorName))
            {
                 WorkSpace.Instance.Solution.SourceControl.SolutionSourceControlAuthorName =  WorkSpace.Instance.Solution.SourceControl.SourceControlUser;
            }
        }

        private void SourceControlPassTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
             WorkSpace.Instance.Solution.SourceControl.SourceControlPass = ((PasswordBox)sender).Password;
             WorkSpace.Instance.UserProfile.SaveUserProfile();//todo: check if needed
            SourceControlIntegration.Init( WorkSpace.Instance.Solution.SourceControl);
        }

        public bool TestSourceControlConnection()
        {
            bool result = SourceControlIntegration.TestConnection( WorkSpace.Instance.Solution.SourceControl, eSourceControlContext.ConnectionDetailsPage, false);
            Mouse.OverrideCursor = null;
            return result;
        }

        private void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            TestSourceControlConnection();
        }

        private void SaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
             WorkSpace.Instance.Solution.SaveSolution(true, Solution.eSolutionItemToSave.SourceControlSettings);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if ( WorkSpace.Instance.Solution != null &&  WorkSpace.Instance.Solution.SourceControl != null)
            {
                 WorkSpace.Instance.UserProfile.SolutionSourceControlUser =  WorkSpace.Instance.Solution.SourceControl.SourceControlUser;
                 WorkSpace.Instance.UserProfile.SolutionSourceControlPass =  WorkSpace.Instance.Solution.SourceControl.SourceControlPass;
                 WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName =  WorkSpace.Instance.Solution.SourceControl.SolutionSourceControlAuthorName;
                 WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail =  WorkSpace.Instance.Solution.SourceControl.SolutionSourceControlAuthorName;
                SourceControlIntegration.Disconnect( WorkSpace.Instance.Solution.SourceControl);

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
            SourceControlIntegration.Init( WorkSpace.Instance.Solution.SourceControl);
            
        }

        private void txtSourceControlConnectionTimeout_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceControlIntegration.Init( WorkSpace.Instance.Solution.SourceControl);
             WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout = Int32.Parse(xTextSourceControlConnectionTimeout.Text);
        }
    }
}