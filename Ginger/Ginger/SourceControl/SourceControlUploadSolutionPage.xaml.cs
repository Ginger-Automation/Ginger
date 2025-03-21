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
using Amdocs.Ginger.UserControls;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.SourceControl
{
    /// <summary>
    /// Interaction logic for SourceControlUploadSolutionPage.xaml
    /// </summary>
    public partial class SourceControlUploadSolutionPage : Page
    {
        GenericWindow genWin = null;

        ImageMakerControl loaderElement;

        public static SourceControlBase mSourceControl;

        public SourceControlUploadSolutionPage()
        {
            InitializeComponent();

            SourceControlInit();

            Init();

        }

        private void SourceControlInit()
        {
            mSourceControl = new GITSourceControl();
            WorkSpace.Instance.UserProfile.GetSourceControlPropertyFromUserProfile(mSourceControl, WorkSpace.Instance.Solution.Guid);
            FetchBranchRadioBtn.IsChecked = true;
        }



        private void Init()
        {
            mSourceControl.LocalFolder = WorkSpace.Instance.Solution.Folder;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ConfigureProxyCheckBox, CheckBox.IsCheckedProperty, mSourceControl, nameof(SourceControlBase.IsProxyConfigured));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ProxyAddressTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.ProxyAddress));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ProxyPortTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.ProxyPort));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlURLTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.URL));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlUserTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.Username));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlPassTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.Password));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xBranchesCombo, ComboBox.TextProperty, mSourceControl, nameof(SourceControlBase.BranchName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlBranchTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.BranchName));

        }


        private void FetchBranches_Click(object sender, RoutedEventArgs e)
        {
            loaderElement.Visibility = Visibility.Visible;
            mSourceControl.Password = SourceControlPassTextBox.Password;
            xBranchesCombo.ItemsSource = SourceControlIntegration.GetBranches(mSourceControl);
            if (xBranchesCombo.Items.Count > 0)
            {
                xBranchesCombo.SelectedIndex = 0;
            }
            else
            {
                mSourceControl.BranchName = "";
            }
            loaderElement.Visibility = Visibility.Collapsed;
        }



        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button testConnBtn = new Button
            {
                Content = "Upload Solution"
            };
            testConnBtn.Click += new RoutedEventHandler(UploadSolution_Click);

            loaderElement = new ImageMakerControl
            {
                Name = "xProcessingImage",
                Height = 30,
                Width = 30,
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing,
                Visibility = Visibility.Collapsed
            };

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, [testConnBtn], true, "Close", new RoutedEventHandler(Close_Click), false, loaderElement);
        }

        private void PopProcessIsBusyMsg()
        {
            Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
        }
        private async void UploadSolution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loaderElement.Visibility = Visibility.Visible;
                mSourceControl.Password = SourceControlPassTextBox.Password;
                if (SourceControlIntegration.BusyInProcessWhileDownloading)
                {
                    PopProcessIsBusyMsg();
                    return;
                }
                SourceControlIntegration.BusyInProcessWhileDownloading = true;
                if (TestSourceControlConnection())
                {
                    await Task.Run(() =>
                    {
                        UploadSolutionToSourceControl(sender, e);
                    });
                }
                loaderElement.Visibility = Visibility.Collapsed;
            }
            finally
            {
                loaderElement.Visibility = Visibility.Collapsed;
                SourceControlIntegration.BusyInProcessWhileDownloading = false;
            }
        }

        private bool TestSourceControlConnection()
        {
            bool result = SourceControlUI.TestConnection(mSourceControl, true);
            Mouse.OverrideCursor = null;
            return result;
        }

        private void UploadSolutionToSourceControl(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateBranchNameIsNotEmpty())
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlBranchNameEmpty);
                    return;
                }
                if (mSourceControl.InitializeRepository(mSourceControl.URL))
                {
                    Reporter.ToUser(eUserMsgKey.UploadSolutionInfo, "Solution was successfully uploaded into: '" + mSourceControl.URL + "'");
                    if (mSourceControl.URL.Contains(".git", StringComparison.OrdinalIgnoreCase))
                    {
                        mSourceControl.GetSourceControlType = SourceControlBase.eSourceControlType.GIT;
                    }
                    UpdateSourceControlDetails();
                    if (WorkSpace.Instance.Solution.SourceControl == null)
                    {
                        WorkSpace.Instance.Solution.SourceControl = mSourceControl;
                    }
                    SourceControlIntegration.BusyInProcessWhileDownloading = false;
                    App.MainWindow.Dispatcher.Invoke(() =>
                    {
                        Close_Click(sender, e);
                    });
                }
                else
                {
                    CleanSourceControlFolderUponFailure();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Upload Solution to Source Control", ex);
            }
        }
        private void UpdateSourceControlDetails()
        {
            UpdateSourceControlSolutionFolder();
            WorkSpace.Instance.UserProfile.SetSourceControlPropertyOnUserProfile(mSourceControl, WorkSpace.Instance.Solution.Guid);
        }

        private void UpdateSourceControlSolutionFolder()
        {
            if (String.IsNullOrEmpty(mSourceControl.SolutionFolder))
            {
                mSourceControl.SolutionFolder = WorkSpace.Instance.Solution.Folder;
            }
        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (SourceControlIntegration.BusyInProcessWhileDownloading)
            {
                PopProcessIsBusyMsg();
                return;
            }
            if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.SourceControl != null)
            {
                WorkSpace.Instance.UserProfile.SetSourceControlPropertyOnUserProfile(WorkSpace.Instance.Solution.SourceControl, WorkSpace.Instance.Solution.Guid);
                SourceControlIntegration.Disconnect(WorkSpace.Instance.Solution.SourceControl);
            }
            genWin.Close();
        }

        private void CleanSourceControlFolderUponFailure()
        {
            string sourceControlLocation = Path.Combine(WorkSpace.Instance.Solution.Folder, ".git");
            if (System.IO.Directory.Exists(sourceControlLocation))
            {
                var directory = new DirectoryInfo(sourceControlLocation) { Attributes = FileAttributes.Normal };

                foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    info.Attributes = FileAttributes.Normal;
                }

                directory.Delete(true);
            }
        }
        private void ConfigureProxyCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            ProxyAddressTextBox.IsEnabled = true;
            ProxyPortTextBox.IsEnabled = true;
        }

        private bool ValidateBranchNameIsNotEmpty()
        {
            if (String.IsNullOrEmpty(mSourceControl.BranchName))
            {
                return false;
            }
            return true;
        }
        private void ConfigureProxyCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            ProxyAddressTextBox.IsEnabled = false;
            ProxyPortTextBox.IsEnabled = false;
        }
        private void BranchesControl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)FetchBranchRadioBtn.IsChecked)
            {
                SP_CreateNewBranch.Visibility = Visibility.Collapsed;
                SP_FetchBranches.Visibility = Visibility.Visible;
                mSourceControl.BranchName = "";
            }
            if ((bool)CreateBranchRadioBtn.IsChecked)
            {
                SP_FetchBranches.Visibility = Visibility.Collapsed;
                SP_CreateNewBranch.Visibility = Visibility.Visible;
                mSourceControl.BranchName = "";
            }
        }
    }
}
