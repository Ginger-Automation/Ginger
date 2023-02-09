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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.SolutionCategories;
using Ginger.SolutionGeneral;
using Ginger.UserControlsLib;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
        bool IsEncrytedStrAvailableOnSol = false;
        public SolutionPage()
        {
            InitializeComponent();

            WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;
            UCEncryptionKey.UpdateKey.PreviewMouseDown += ReplaceKeyBtn_Click;

            Init();
        }

        private void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                Init();
            }
        }


        private void Init(Solution solution = null)
        {
            if (WorkSpace.Instance.Solution != null)
            {
                mSolution = WorkSpace.Instance.Solution;
            }
            else if (solution != null)
            {
                mSolution = solution;
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
                UCEncryptionKey.EncryptionKeyPasswordBox.Password = mSolution.EncryptionKey;
            }
            else
            {
                xLoadSolutionlbl.Visibility = Visibility.Visible;
                xSolutionDetailsStack.Visibility = Visibility.Collapsed;
                xCategoriesExpander.Visibility = Visibility.Collapsed;
            }
        }
     
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            xCategoriesExpander.Visibility = Visibility.Visible;
            xCategoriesFrame.Content = new SolutionCategoriesPage(eSolutionCategoriesPageMode.OptionalValuesDefinition);

            SolutionFolderTextBox.IsReadOnly = false;
            SolutionNameTextBox.IsReadOnly = false;
            AccountTextBox.IsReadOnly = false;
            UCEncryptionKey.mSolution = WorkSpace.Instance.Solution;
            UCEncryptionKey.EncryptionKeyTextBox.IsReadOnly = true;
            UCEncryptionKey.EncryptionKeyPasswordBox.IsEnabled = false;
            UCEncryptionKey.ValidFlag.Visibility = Visibility.Collapsed;
            UCEncryptionKey.InvalidFlag.Visibility = Visibility.Collapsed;
            UCEncryptionKey.EncryptionKeyPasswordBox.Password = mSolution.EncryptionKey;
            UCEncryptionKey.CopyToClipboard.Visibility = Visibility.Visible;
            UCEncryptionKey.UpdateKey.Visibility = Visibility.Visible;
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
            //Button replaceKeyBtn = new Button();
            //replaceKeyBtn.Content = "Update Key";
            //replaceKeyBtn.Click += new RoutedEventHandler(ReplaceKeyBtn_Click);
            //winButtons.Add(replaceKeyBtn);

            this.Height = 600;
            this.Width = 800;

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Solution Details", this, winButtons, startupLocationWithOffset: startupLocationWithOffset);
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            mSolution.RestoreFromBackup(true);
            _pageGenericWin.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            mSolution.SolutionOperations.SaveSolution(true, Solution.eSolutionItemToSave.GeneralDetails);
        }

        public bool ShowAsWindow(Solution solution, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {

            Init(solution);
            UCEncryptionKey.mSolution = solution;
            IsEncrytedStrAvailableOnSol = !string.IsNullOrEmpty(mSolution.EncryptedValidationString);

            SolutionFolderTextBox.IsReadOnly = true;
            SolutionNameTextBox.IsReadOnly = true;
            AccountTextBox.IsReadOnly = true;
            UCEncryptionKey.EncryptionKeyTextBox.IsReadOnly = false;
            UCEncryptionKey.EncryptionKeyPasswordBox.IsEnabled = true;
            UCEncryptionKey.ValidFlag.Visibility = Visibility.Collapsed;
            UCEncryptionKey.InvalidFlag.Visibility = Visibility.Visible;
            //UCEncryptionKey.Validate.Visibility = Visibility.Visible;
            UCEncryptionKey.UpdateKey.Visibility = Visibility.Visible;
            //xInvalidKeyLabel.Visibility = Visibility.Visible;

            xCategoriesExpander.Visibility = Visibility.Collapsed;


            ObservableList<Button> winButtons = new ObservableList<Button>();
            Button uSaveKeyBtn = new Button();
            uSaveKeyBtn.Content = "Ok";
            uSaveKeyBtn.Click += new RoutedEventHandler(SaveKeyBtn_Click);
            winButtons.Add(uSaveKeyBtn);
            //Button replaceKeyBtn = new Button();
            //replaceKeyBtn.Content = "Update Key";
            //replaceKeyBtn.Click += new RoutedEventHandler(ReplaceKeyBtn_Click);
            //winButtons.Add(replaceKeyBtn);
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Solution Details", this, winButtons, true, "Cancel", CloseBtn_Click);
            return IsValidEncryptionKeyAdded;
        }

        private void ValidateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsEncrytedStrAvailableOnSol)
            {
                UCEncryptionKey.ValidateKey();               
            }
            else
            {
                UCEncryptionKey.CheckKeyCombination();
            }
        }


        private void ReplaceKeyBtn_Click(object sender, RoutedEventArgs e)
        {
            ReplaceEncryptionKeyPage replaceEncryptionKeyPage = new ReplaceEncryptionKeyPage();
            _pageGenericWin.Close();
            IsValidEncryptionKeyAdded = replaceEncryptionKeyPage.ShowAsWindow(mSolution);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            IsValidEncryptionKeyAdded = false;
        }

        private void SaveKeyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEncrytedStrAvailableOnSol && UCEncryptionKey.CheckKeyCombination())
            {
                mSolution.EncryptionKey = UCEncryptionKey.EncryptionKeyPasswordBox.Password;
                mSolution.NeedVariablesReEncryption = true;
                mSolution.SolutionOperations.SaveEncryptionKey();
                mSolution.SolutionOperations.SaveSolution(false);
                IsValidEncryptionKeyAdded = true;
                _pageGenericWin.Close();
            }
            else if (IsEncrytedStrAvailableOnSol && UCEncryptionKey.ValidateKey())
            {
                mSolution.SolutionOperations.SaveEncryptionKey();
                IsValidEncryptionKeyAdded = true;
                _pageGenericWin.Close();
            }
        }
    }
}
