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
using Amdocs.Ginger.UserControls;
using GingerCore;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Ginger.User
{
    /// <summary>
    /// Interaction logic for UserProfilePage.xaml
    /// </summary>
    public partial class UserProfilePage : Page
    {
        GenericWindow _pageGenericWin;
        readonly eUserType mOriginalUserType;

        public UserProfilePage()
        {
            InitializeComponent();

            //profile image
            if (string.IsNullOrEmpty(App.UserProfile.ProfileImage))
            {
                xProfileImageImgBrush.ImageSource = ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User, foreground: (System.Windows.Media.SolidColorBrush)FindResource("$BackgroundColor_DarkBlue"), width: 50);
            }
            else
            {
                xProfileImageImgBrush.ImageSource = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(App.UserProfile.ProfileImage));
            }

            App.ObjFieldBinding(xUserNameTxtBox, TextBox.TextProperty, App.UserProfile, nameof(UserProfile.UserName), BindingMode.OneWay);
            App.ObjFieldBinding(xUserFirstNameTxtBox, TextBox.TextProperty, App.UserProfile, nameof(UserProfile.UserFirstName));
            App.ObjFieldBinding(xUserMiddleNameTxtBox, TextBox.TextProperty, App.UserProfile, nameof(UserProfile.UserMiddleName));
            App.ObjFieldBinding(xUserLastNameTxtBox, TextBox.TextProperty, App.UserProfile, nameof(UserProfile.UserLastName));
                      
            mOriginalUserType = App.UserProfile.UserType;
            xUserTypeComboBox.BindControl(App.UserProfile, nameof(UserProfile.UserType));
            xUserTypeNoteLbl.Visibility = Visibility.Collapsed;
            xUserRoleComboBox.BindControl(App.UserProfile, nameof(UserProfile.UserRole));
            App.ObjFieldBinding(xUserDepartmentTxtBox, TextBox.TextProperty, App.UserProfile, nameof(UserProfile.UserDepartment));

            App.ObjFieldBinding(xUserEmailAddressTxtBox, TextBox.TextProperty, App.UserProfile, nameof(UserProfile.UserEmail));
            App.ObjFieldBinding(xUserPhoneTxtBox, TextBox.TextProperty, App.UserProfile, nameof(UserProfile.UserPhone));
        }

        private void xProfileImageBrowseBtn_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog();
            op.Title = "Select Image";
            op.Filter = "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg";
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fileLength = new FileInfo(op.FileName).Length;
                if (fileLength <= 50000)
                {
                    xProfileImageImgBrush.ImageSource = new BitmapImage(new Uri(op.FileName));
                    if ((op.FileName != null) && (op.FileName != string.Empty))
                    {
                        using (var ms = new MemoryStream())
                        {
                            BitmapImage bi = new BitmapImage(new Uri(op.FileName));
                            Tuple<int, int> sizes = Ginger.General.RecalculatingSizeWithKeptRatio(bi, 100, 100);

                            BitmapImage bi_resized = new BitmapImage();
                            bi_resized.BeginInit();
                            bi_resized.UriSource = new Uri(op.FileName);
                            bi_resized.DecodePixelHeight = sizes.Item2;
                            bi_resized.DecodePixelWidth = sizes.Item1;
                            bi_resized.EndInit();

                            App.UserProfile.ProfileImage = Ginger.General.BitmapToBase64(Ginger.General.BitmapImage2Bitmap(bi_resized));
                        }
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKeys.ImageSize, "50");
                }
            }
        }

        private void xProfileImageDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            xProfileImageImgBrush.ImageSource = ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User, width: 50);
            App.UserProfile.ProfileImage = string.Empty;
        }

        private void xUserTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((eUserType)xUserTypeComboBox.SelectedValue != mOriginalUserType)
            {
                xUserTypeNoteLbl.Visibility = Visibility.Visible;
            }
            else
            {
                xUserTypeNoteLbl.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            App.UserProfile.SaveBackup();

            ObservableList<Button> winButtons = new ObservableList<Button>();
            Button saveBtn = new Button();
            saveBtn.Content = "Save";
            saveBtn.Click += new RoutedEventHandler(saveBtn_Click);
            winButtons.Add(saveBtn);

            Button undoBtn = new Button();
            undoBtn.Content = "Undo & Close";
            undoBtn.Click += new RoutedEventHandler(UndoBtn_Click);
            winButtons.Add(undoBtn);            

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Edit User Profile", this, winButtons, false, "Undo & Close", CloseWinClicked, startupLocationWithOffset: startupLocationWithOffset);
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            App.UserProfile.SaveUserProfile();
            _pageGenericWin.Close();
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.AskIfToUndoChanges) == MessageBoxResult.Yes)
            {
                UndoChangesAndClose();
            }
        }

        private void UndoChangesAndClose()
        {
            App.UserProfile.RestoreFromBackup(true);
            _pageGenericWin.Close();
        }
    }
}
