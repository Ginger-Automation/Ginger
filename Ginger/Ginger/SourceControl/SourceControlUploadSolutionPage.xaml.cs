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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.UserControls;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;

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
            if (mSourceControl != null)
            {
                WorkSpace.Instance.UserProfile.SourceControlType = mSourceControl.GetSourceControlType;
                mSourceControl.SourceControlURL = "";
                mSourceControl.SourceControlUser = WorkSpace.Instance.UserProfile.SourceControlUser;
                mSourceControl.SourceControlPass = WorkSpace.Instance.UserProfile.SourceControlPass;
                mSourceControl.SourceControlLocalFolder = WorkSpace.Instance.UserProfile.SourceControlLocalFolder;
                mSourceControl.SourceControlBranch = WorkSpace.Instance.UserProfile.SourceControlBranch;

                mSourceControl.SourceControlConfigureProxy = WorkSpace.Instance.UserProfile.SolutionSourceControlConfigureProxy;
                mSourceControl.SourceControlProxyAddress = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress;
                mSourceControl.SourceControlProxyPort = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort;

                if (WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout == 0)
                {
                    WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout = 80;
                }
                mSourceControl.SourceControlTimeout = WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout;
                mSourceControl.PropertyChanged += SourceControl_PropertyChanged;

                FetchBranchRadioBtn.IsChecked = true;
            }
        }

        private static void SourceControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            WorkSpace.Instance.UserProfile.SourceControlType = mSourceControl.GetSourceControlType;
            WorkSpace.Instance.UserProfile.SourceControlURL = mSourceControl.SourceControlURL;
            WorkSpace.Instance.UserProfile.SourceControlUser = mSourceControl.SourceControlUser;
            WorkSpace.Instance.UserProfile.SourceControlPass = mSourceControl.SourceControlPass;
            WorkSpace.Instance.UserProfile.SourceControlLocalFolder = mSourceControl.SourceControlLocalFolder;
            WorkSpace.Instance.UserProfile.SourceControlBranch = mSourceControl.SourceControlBranch;

            WorkSpace.Instance.UserProfile.SolutionSourceControlConfigureProxy = mSourceControl.SourceControlConfigureProxy;
            WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress = mSourceControl.SourceControlProxyAddress;
            WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort = mSourceControl.SourceControlProxyPort;
            WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout = mSourceControl.SourceControlTimeout;
        }

        private void Init()
        {
            if (WorkSpace.Instance.UserProfile.SourceControlURL == null)
            {

                WorkSpace.Instance.UserProfile.SourceControlURL = "";
            }
            mSourceControl.SourceControlLocalFolder = WorkSpace.Instance.Solution.Folder;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ConfigureProxyCheckBox, CheckBox.IsCheckedProperty, mSourceControl, nameof(SourceControlBase.SourceControlConfigureProxy));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ProxyAddressTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlProxyAddress));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ProxyPortTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlProxyPort));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlURLTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlURL));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlUserTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlUser));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlPassTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlPass));
            SourceControlPassTextBox.Password = mSourceControl.SourceControlPass;

            //xBranchesCombo.ItemsSource = SourceControlIntegration.GetBranches(mSourceControl);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xBranchesCombo, ComboBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlBranch));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SourceControlBranchTextBox, TextBox.TextProperty, mSourceControl, nameof(SourceControlBase.SourceControlBranch));

            SourceControlPassTextBox.PasswordChanged += SourceControlPassTextBox_PasswordChanged;    
        }

        private void SourceControlPassTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            mSourceControl.SourceControlPass = ((PasswordBox)sender).Password;
            SourceControlIntegration.Init(mSourceControl);
        }

        private void SourceControlUserTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceControlIntegration.Init(mSourceControl);
        }

        private void FetchBranches_Click(object sender, RoutedEventArgs e)
        {
            loaderElement.Visibility = Visibility.Visible;
            xBranchesCombo.ItemsSource = SourceControlIntegration.GetBranches(mSourceControl);
            if (xBranchesCombo.Items.Count > 0)
            {
                xBranchesCombo.SelectedIndex = 0;
            }
            else
            {
                mSourceControl.SourceControlBranch = "";
            }
            loaderElement.Visibility = Visibility.Collapsed;
        }

        private void SourceControlURLTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceControlIntegration.Init(mSourceControl);
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button testConnBtn = new Button();
            testConnBtn.Content = "Upload Solution";
            testConnBtn.Click += new RoutedEventHandler(UploadSolution_Click);

            loaderElement = new ImageMakerControl();
            loaderElement.Name = "xProcessingImage";
            loaderElement.Height = 30;
            loaderElement.Width = 30;
            loaderElement.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing;
            loaderElement.Visibility = Visibility.Collapsed;

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { testConnBtn }, true, "Close", new RoutedEventHandler(Close_Click), false, loaderElement);
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
            if(!ValidateBranchNameIsNotEmpty())
            {
                Reporter.ToUser(eUserMsgKey.SourceControlBranchNameEmpty);
                return;
            }
            if (mSourceControl.InitializeRepository(mSourceControl.SourceControlURL))
            {
                Reporter.ToUser(eUserMsgKey.UploadSolutionInfo, "Solution was successfully uploaded into: '" + mSourceControl.SourceControlURL + "'");
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
        private void UpdateSourceControlDetails()
        {
            UpdateSourceControlSolutionFolder();
            UpdateSourceControlAuthorDetails();
        }

        private void UpdateSourceControlSolutionFolder()
        {
            if(String.IsNullOrEmpty(mSourceControl.SolutionFolder))
            {
                mSourceControl.SolutionFolder = WorkSpace.Instance.Solution.Folder;
            }
        }

        private void UpdateSourceControlAuthorDetails()
        {
            if (String.IsNullOrEmpty(mSourceControl.SolutionSourceControlAuthorEmail))
            {
                UpdateSourceControlAuthorEmail();
            }
            if (String.IsNullOrEmpty(mSourceControl.SolutionSourceControlAuthorName))
            {
                UpdateSourceControlAuthorName();
            }
        }
        private void UpdateSourceControlAuthorEmail()
        {
            if (!String.IsNullOrEmpty(WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail))
            {
                mSourceControl.SolutionSourceControlAuthorEmail = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail;
            }
            else
            {
                mSourceControl.SolutionSourceControlAuthorEmail = mSourceControl.SourceControlUser;
            }
        }

        private void UpdateSourceControlAuthorName()
        {
            if (!String.IsNullOrEmpty(WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName))
            {
                mSourceControl.SolutionSourceControlAuthorName = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName;
            }
            else
            {
                mSourceControl.SolutionSourceControlAuthorName = mSourceControl.SourceControlUser;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if(SourceControlIntegration.BusyInProcessWhileDownloading)
            {
                PopProcessIsBusyMsg();
                return;
            }
            if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.SourceControl != null)
            {
                WorkSpace.Instance.UserProfile.SolutionSourceControlUser = WorkSpace.Instance.Solution.SourceControl.SourceControlUser;
                WorkSpace.Instance.UserProfile.SolutionSourceControlPass = WorkSpace.Instance.Solution.SourceControl.SourceControlPass;
                WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName = WorkSpace.Instance.Solution.SourceControl.SolutionSourceControlAuthorName;
                WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail = WorkSpace.Instance.Solution.SourceControl.SolutionSourceControlAuthorName;
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
            if(String.IsNullOrEmpty(mSourceControl.SourceControlBranch))
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
                mSourceControl.SourceControlBranch = "";
            }
            if((bool)CreateBranchRadioBtn.IsChecked)
            {
                SP_FetchBranches.Visibility = Visibility.Collapsed;
                SP_CreateNewBranch.Visibility = Visibility.Visible;
                mSourceControl.SourceControlBranch = "";
            }
        }
    }
}
