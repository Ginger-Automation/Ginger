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
using GingerCore;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserConfig
{
    /// <summary>
    /// Interaction logic for SetUserTypePage.xaml
    /// </summary>
    public partial class SetUserTypePage : Page
    {
        eUserType mInitialType;
        GenericWindow _pageGenericWin = null;

        public SetUserTypePage()
        {
            InitializeComponent();
            UserTypeCombo.ItemsSource = GingerCore.General.GetEnumValues(typeof(eUserType));
            App.ObjFieldBinding(UserTypeCombo, ComboBox.TextProperty, App.UserProfile, nameof(UserProfile.UserType));

            mInitialType = App.UserProfile.UserType;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            ObservableList<Button> winButtons = new ObservableList<Button>();
            Button okBtn = new Button();
            okBtn.Content = "Ok";
            okBtn.Click += new RoutedEventHandler(okBtn_Click);
            winButtons.Add(okBtn);

            Button undoBtn = new Button();
            undoBtn.Content = "Cancel";
            undoBtn.Click += new RoutedEventHandler(undoBtn_Click);
            winButtons.Add(undoBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, false, string.Empty, CloseWinClicked);
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            App.UserProfile.UserType = mInitialType;
            _pageGenericWin.Close();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (App.UserProfile.UserType != mInitialType)
            {
                Reporter.ToUser(eUserMsgKeys.SettingsChangeRequireRestart);
            }
            _pageGenericWin.Close();
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            App.UserProfile.UserType = mInitialType;
            _pageGenericWin.Close();
        }
    }
}