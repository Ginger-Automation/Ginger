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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SourceControl
{
    /// <summary>
    /// Interaction logic for CheckInWindow.xaml
    /// </summary>
    public partial class CreateNewBranch : Page
    {
        GenericWindow genWin = null;
        ImageMakerControl loaderElement;
        private SourceControlBase mSourceControl = null;
        public CreateNewBranch()
        {
            InitializeComponent();
            SourceControlInit();
            Init();
        }
        private void SourceControlInit()
        {
            mSourceControl = new GITSourceControl();
            WorkSpace.Instance.UserProfile.GetSourceControlPropertyFromUserProfile(mSourceControl, WorkSpace.Instance.Solution.Guid);
        }
        private void Init()
        {
            mSourceControl.LocalFolder = WorkSpace.Instance.Solution.Folder;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xURLTextBox, TextBox.TextProperty, mSourceControl, nameof(GITSourceControl.URL));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xUserTextBox, TextBox.TextProperty, mSourceControl, nameof(GITSourceControl.Username));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xPassTextBox, PasswordBox.PasswordCharProperty, mSourceControl, nameof(GITSourceControl.Password));
            xPassTextBox.Password = mSourceControl.Password;
            ShowExistingBranch();
        }
        List<string> AllLocalBranchNames = [];
        private void ShowExistingBranch()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    AllLocalBranchNames = mSourceControl.GetLocalBranches();
                    xCurrentWorkingBranch.Text = mSourceControl.GetCurrentWorkingBranch();

                });
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.ToString());
            }
        }
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            ObservableList<Button> windowBtnsList = [];

            Button createBranch = new Button
            {
                Content = "Create Branch"
            };
            createBranch.Click += CreateNewBranch_ClickAsync;

            windowBtnsList.Add(createBranch);
            loaderElement = new ImageMakerControl
            {
                Name = "xProcessingImage",
                Height = 30,
                Width = 30,
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing,
                Visibility = Visibility.Collapsed
            };

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, windowBtnsList, true, "Close", new RoutedEventHandler(Close_Click), false, loaderElement);
        }
        private void CloseWindow(object sender, EventArgs e)
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            genWin.Close();
        }
        private async void CreateNewBranch_ClickAsync(object sender, RoutedEventArgs e)
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

                if (string.IsNullOrEmpty(mSourceControl.AuthorEmail) && string.IsNullOrEmpty(mSourceControl.AuthorName))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Author name or Author email not found.");
                    return;
                }

                await Task.Run(() =>
                {
                    CreateNewSourceControlBranch(sender, e);
                });

                loaderElement.Visibility = Visibility.Collapsed;
            }
            finally
            {
                loaderElement.Visibility = Visibility.Collapsed;
                SourceControlIntegration.BusyInProcessWhileDownloading = false;
            }
        }
        private void PopProcessIsBusyMsg()
        {
            Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
        }

        private void CreateNewSourceControlBranch(object sender, RoutedEventArgs e)
        {
            try
            {
                string TextBoxBranch = string.Empty;
                Dispatcher.Invoke(() =>
                {
                    TextBoxBranch = SourceControlBranchTextBox.Text;
                });
                bool result = false;
                string newBranchName = string.Empty;
                string error = string.Empty;

                if (!string.IsNullOrEmpty(TextBoxBranch))
                {
                    result = mSourceControl.CreateBranch(TextBoxBranch, ref error);
                    if (!string.IsNullOrEmpty(error))
                    {
                        ShowErrorMsg(error);
                        return;
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            xErrorMsg.Visibility = Visibility.Collapsed;
                        });
                    }
                    newBranchName = TextBoxBranch;
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlBranchEmptyOrAlreadyExists);
                    return;
                }
                if (result == true && !string.IsNullOrEmpty(newBranchName))
                {
                    mSourceControl.Branch = newBranchName;
                    UpdateSourceControlDetails();
                    ShowExistingBranch();
                    Reporter.ToUser(eUserMsgKey.SourceControlBranchCreated);
                    Close_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create branch", ex);
            }
        }
        private void UpdateSourceControlDetails()
        {
            WorkSpace.Instance.UserProfile.SetSourceControlPropertyOnUserProfile(mSourceControl, WorkSpace.Instance.Solution.Guid);
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                genWin.Close();
            });

        }

        private void SourceControlBranchTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var result = AllLocalBranchNames.Any(branch => branch.Equals(SourceControlBranchTextBox.Text, StringComparison.OrdinalIgnoreCase));
            if (result)
            {
                ShowErrorMsg("This branch is already exists.");
            }
            else
            {
                xErrorMsg.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowErrorMsg(string message)
        {
            Dispatcher.Invoke(() =>
            {
                xErrorMsg.Visibility = Visibility.Visible;
                xErrorMsg.Content = message;
            });
        }
    }
}