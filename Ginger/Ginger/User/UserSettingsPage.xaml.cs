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
using Amdocs.Ginger.Core;
using GingerCore;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using amdocs.ginger.GingerCoreNET;
namespace Ginger.User
{
    /// <summary>
    /// Interaction logic for UserSettings.xaml
    /// </summary>
    public partial class UserSettingsPage : Page
    {
        GenericWindow _pageGenericWin;
        readonly GingerCore.eTerminologyType mOriginalTerminologyType;

        public UserSettingsPage()
        {
            InitializeComponent();

            mOriginalTerminologyType =  WorkSpace.Instance.UserProfile.TerminologyDictionaryType;
            xTerminologyTypeComboBox.BindControl( WorkSpace.Instance.UserProfile, nameof(UserProfile.TerminologyDictionaryType));
            xTerminologyTypeNoteLbl.Visibility = Visibility.Collapsed;

            xLoggingLevelComboBox.BindControl( WorkSpace.Instance.UserProfile, nameof(UserProfile.AppLogLevel));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAutoLoadLastSolutionCheckBox, CheckBox.IsCheckedProperty,  WorkSpace.Instance.UserProfile, nameof(UserProfile.AutoLoadLastSolution));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAskToUpgradeSolutionCheckBox, CheckBox.IsCheckedProperty,  WorkSpace.Instance.UserProfile, nameof(UserProfile.DoNotAskToUpgradeSolutions));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAskToRecoverSolutionCheckBox, CheckBox.IsCheckedProperty,  WorkSpace.Instance.UserProfile, nameof(UserProfile.DoNotAskToRecoverSolutions));            
        }

        private void xTerminologyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((GingerCore.eTerminologyType)xTerminologyTypeComboBox.SelectedValue != mOriginalTerminologyType)
            {
                xTerminologyTypeNoteLbl.Visibility = Visibility.Visible;
            }
            else
            {
                xTerminologyTypeNoteLbl.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
             WorkSpace.Instance.UserProfile.SaveBackup();

            ObservableList<Button> winButtons = new ObservableList<Button>();
            Button saveBtn = new Button();
            saveBtn.Content = "Save";
            saveBtn.Click += new RoutedEventHandler(saveBtn_Click);
            winButtons.Add(saveBtn);

            Button undoBtn = new Button();
            undoBtn.Content = "Undo & Close";
            undoBtn.Click += new RoutedEventHandler(UndoBtn_Click);
            winButtons.Add(undoBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Edit User Settings", this, winButtons, false, "Undo & Close", CloseWinClicked, startupLocationWithOffset: startupLocationWithOffset);
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
             WorkSpace.Instance.UserProfile.SaveUserProfile();
            _pageGenericWin.Close();
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.AskIfToUndoChanges) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                UndoChangesAndClose();
            }
        }

        private void UndoChangesAndClose()
        {
             WorkSpace.Instance.UserProfile.RestoreFromBackup(true);
            _pageGenericWin.Close();
        }
    }
}
