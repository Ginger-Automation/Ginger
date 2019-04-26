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
using Amdocs.Ginger.UserControls;
using GingerCore;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using amdocs.ginger.GingerCoreNET;

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
            if (string.IsNullOrEmpty( WorkSpace.Instance.UserProfile.ProfileImage))
            {
                xProfileImageImgBrush.ImageSource = ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User, foreground: (System.Windows.Media.SolidColorBrush)FindResource("$BackgroundColor_DarkBlue"), width: 50);
            }
            else
            {
                xProfileImageImgBrush.ImageSource = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage( WorkSpace.Instance.UserProfile.ProfileImage));
            }

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xUserNameTxtBox, TextBox.TextProperty,  WorkSpace.Instance.UserProfile, nameof(UserProfile.UserName), BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xUserFirstNameTxtBox, TextBox.TextProperty,  WorkSpace.Instance.UserProfile, nameof(UserProfile.UserFirstName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xUserMiddleNameTxtBox, TextBox.TextProperty,  WorkSpace.Instance.UserProfile, nameof(UserProfile.UserMiddleName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xUserLastNameTxtBox, TextBox.TextProperty,  WorkSpace.Instance.UserProfile, nameof(UserProfile.UserLastName));
                      
            mOriginalUserType =  WorkSpace.Instance.UserProfile.UserType;
            xUserTypeComboBox.BindControl( WorkSpace.Instance.UserProfile, nameof(UserProfile.UserType));
            xUserTypeNoteLbl.Visibility = Visibility.Collapsed;
            xUserRoleComboBox.BindControl( WorkSpace.Instance.UserProfile, nameof(UserProfile.UserRole));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xUserDepartmentTxtBox, TextBox.TextProperty,  WorkSpace.Instance.UserProfile, nameof(UserProfile.UserDepartment));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xUserEmailAddressTxtBox, TextBox.TextProperty,  WorkSpace.Instance.UserProfile, nameof(UserProfile.UserEmail));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xUserPhoneTxtBox, TextBox.TextProperty,  WorkSpace.Instance.UserProfile, nameof(UserProfile.UserPhone));
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

                             WorkSpace.Instance.UserProfile.ProfileImage = Ginger.General.BitmapToBase64(Ginger.General.BitmapImage2Bitmap(bi_resized));
                        }
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.ImageSize, "50");
                }
            }
        }

        private void xProfileImageDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            xProfileImageImgBrush.ImageSource = ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User, width: 50);
             WorkSpace.Instance.UserProfile.ProfileImage = string.Empty;
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

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Edit User Profile", this, winButtons, false, "Undo & Close", CloseWinClicked, startupLocationWithOffset: startupLocationWithOffset);
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
