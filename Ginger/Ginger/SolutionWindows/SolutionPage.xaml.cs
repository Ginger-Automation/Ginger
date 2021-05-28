#region License
/*
Copyright © 2014-2021 European Support Limited

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for SolutionPage.xaml
    /// </summary>
    public partial class SolutionPage : Page
    {
        GenericWindow _pageGenericWin;
        Solution mSolution;
        bool IsValidEncryptionKeyAdded = false;
        public SolutionPage()
        {
            InitializeComponent();
            
             WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;
            Init();
        }

        private void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(WorkSpace.Solution))
            {
                Init();
            }
        }

        private void Init()
        {
            if ( WorkSpace.Instance.Solution != null)
            {
                mSolution =  WorkSpace.Instance.Solution;
            }
            else
            {
                mSolution = null;
            }

            if (mSolution != null)
            {
                xLoadSolutionlbl.Visibility = Visibility.Collapsed;
                xSolutionDetailsStack.Visibility = Visibility.Visible;
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SolutionNameTextBox, TextBox.TextProperty, mSolution, nameof(Solution.Name));
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SolutionFolderTextBox, TextBox.TextProperty, mSolution, nameof(Solution.Folder));
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AccountTextBox, TextBox.TextProperty, mSolution, nameof(Solution.Account));
                EncryptionKeyPasswordBox.Password = mSolution.EncryptionKey;
            }
            else
            {
                xLoadSolutionlbl.Visibility = Visibility.Visible;
                xSolutionDetailsStack.Visibility = Visibility.Collapsed;
            }
        }
        private void ShowPassword_PreviewMouseDown(object sender, MouseButtonEventArgs e) => ShowPasswordFunction();
        private void ShowPassword_PreviewMouseUp(object sender, MouseButtonEventArgs e) => HidePasswordFunction();
        private void ShowPassword_MouseLeave(object sender, MouseEventArgs e) => HidePasswordFunction();

        private void ShowPasswordFunction()
        {
            ShowPassword.Text = "HIDE";
            EncryptionKeyTextBox.Visibility = Visibility.Visible;
            EncryptionKeyPasswordBox.Visibility = Visibility.Hidden;
            EncryptionKeyTextBox.Text = EncryptionKeyPasswordBox.Password;
        }

        private void HidePasswordFunction()
        {
            ShowPassword.Text = "SHOW";
            EncryptionKeyTextBox.Visibility = Visibility.Hidden;
            EncryptionKeyPasswordBox.Visibility = Visibility.Visible;
        }
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            SolutionFolderTextBox.IsReadOnly = false;
            SolutionNameTextBox.IsReadOnly = false;
            AccountTextBox.IsReadOnly = false;
            EncryptionKeyTextBox.IsReadOnly = true;
            EncryptionKeyPasswordBox.IsEnabled = false;
            mSolution.SaveBackup();

            ObservableList<Button> winButtons = new ObservableList<Button>();
            Button SaveBtn = new Button();
            SaveBtn.Content = "Save";
            SaveBtn.Click += new RoutedEventHandler(SaveBtn_Click);
            winButtons.Add(SaveBtn);
            Button undoBtn = new Button();
            undoBtn.Content = "Undo & Close";
            undoBtn.Click += new RoutedEventHandler(UndoBtn_Click);
            winButtons.Add(undoBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Solution Details", this, winButtons, startupLocationWithOffset: startupLocationWithOffset);
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            mSolution.RestoreFromBackup(true);
            _pageGenericWin.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            mSolution.SaveSolution(true, Solution.eSolutionItemToSave.GeneralDetails);
        }

        public bool ShowAsWindow(bool ValidateEncryptionKey, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            SolutionFolderTextBox.IsReadOnly = true;
            SolutionNameTextBox.IsReadOnly = true;
            AccountTextBox.IsReadOnly = true;
            EncryptionKeyTextBox.IsReadOnly = false;
            EncryptionKeyPasswordBox.IsEnabled = true;
            mSolution.SaveBackup();

            ObservableList<Button> winButtons = new ObservableList<Button>();
            Button ValidateBtn = new Button();
            ValidateBtn.Content = "Validate";
            ValidateBtn.Click += new RoutedEventHandler(ValidateBtn_Click);
            winButtons.Add(ValidateBtn);
            Button uSaveKeyBtn = new Button();
            uSaveKeyBtn.Content = "Save Key";
            uSaveKeyBtn.Click += new RoutedEventHandler(SaveKeyBtn_Click);
            winButtons.Add(uSaveKeyBtn);
            Button replaceKeyBtn = new Button();
            replaceKeyBtn.Content = "Replace Key";
            replaceKeyBtn.Click += new RoutedEventHandler(ReplaceKeyBtn_Click);
            winButtons.Add(replaceKeyBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Solution Details", this, winButtons,true, "Close Solution",CloseBtn_Click);
            return IsValidEncryptionKeyAdded;
        }

        private void ValidateBtn_Click(object sender, RoutedEventArgs e)
        {
            mSolution.EncryptionKey = EncryptionKeyPasswordBox.Password;
            if (mSolution.ValidateKey())
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage,"Enterred Encryption Key is Valid.");
            else
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Enterred Encryption Key is Invalid.");
        }

        private void ReplaceKeyBtn_Click(object sender, RoutedEventArgs e)
        {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Not implemented yet.");
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            IsValidEncryptionKeyAdded = false;            
        }

        private void SaveKeyBtn_Click(object sender, RoutedEventArgs e)
        {
            mSolution.EncryptionKey = EncryptionKeyPasswordBox.Password;
            if (!mSolution.ValidateKey())
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Enterred Encryption Key is Invalid.");
            else
            {
                mSolution.SaveEncryptionKey();
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Enterred Encryption Key is Saved on Solution.");
                IsValidEncryptionKeyAdded = true;
                _pageGenericWin.Close();
            }
        }
    }
}
