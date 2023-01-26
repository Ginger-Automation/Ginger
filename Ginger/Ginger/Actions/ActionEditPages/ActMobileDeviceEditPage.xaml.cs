#region License
/*
Copyright © 2014-2022 European Support Limited

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

using GingerCore.Actions;
using System.Windows.Controls;
using System.Windows;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System.Windows.Media.Imaging;
using System.IO;
using System;
using Ginger.Actions.UserControls;
using System.Drawing;
using System.Linq;
using amdocs.ginger.GingerCoreNET;
using GingerCore;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActMobileDeviceEditPage.xaml
    /// </summary>
    public partial class ActMobileDeviceEditPage : Page
    {

        ActMobileDevice mAct;
        Context mContext;
        bool isValueExpression;

        public ActMobileDeviceEditPage(ActMobileDevice Act)
        {
            InitializeComponent();

            mAct = Act;
            mContext = Context.GetAsContext(Act.Context);

            BindControls();
            SetControlsView();
        }

        private void BindControls()
        {
            xOperationNameComboBox.Init(mAct, nameof(mAct.MobileDeviceAction), typeof(ActMobileDevice.eMobileDeviceAction), ActionNameComboBox_SelectionChanged);

            xKeyPressComboBox.Init(mAct, nameof(mAct.MobilePressKey), typeof(ActMobileDevice.ePressKey));

            xX1TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.X1, nameof(ActInputValue.Value));
            xY1TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.Y1, nameof(ActInputValue.Value));
            xX2TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.X2, nameof(ActInputValue.Value));
            xY2TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.Y2, nameof(ActInputValue.Value));

            xPhotoSumilationTxtBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActMobileDevice.SimulatedPhotoPath)), true, true, UCValueExpression.eBrowserType.File, "*", ValueTextBox_LostFocus);

            UpdateBaseLineImage(true);

            xPhotoSumilationTxtBox.ValueTextBox.LostFocus -= ValueTextBox_LostFocus;
            xPhotoSumilationTxtBox.ValueTextBox.LostFocus += ValueTextBox_LostFocus;

            xPhotoSumilationTxtBox.OpenExpressionEditorButton.Click -= ValueTextBox_LostFocus;
            xPhotoSumilationTxtBox.OpenExpressionEditorButton.Click += ValueTextBox_LostFocus;
        }

        private void ProcessInputForDriver()
        {
            if (mContext != null)
            {
                mContext.Runner.ProcessInputValueForDriver(mAct);
            }
        }

        private void ImportPhotoToSolutionFolder(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || filePath.Substring(0, 1) == "~")
            {
                return;
            }
            if (Reporter.ToUser(eUserMsgKey.AskIfToImportFile) == eUserMsgSelection.No)
            {
                return;
            }

            ProcessInputForDriver();

            string SolutionFolder = WorkSpace.Instance.Solution.Folder;
            string targetPath = System.IO.Path.Combine(SolutionFolder, @"Documents\MobileSimulations\Photos");
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }

            string fileName = System.IO.Path.GetFileName(filePath);
            string destFile = System.IO.Path.Combine(targetPath, fileName);

            int fileNum = 1;
            string copySufix = "_Copy";
            while (System.IO.File.Exists(destFile))
            {
                fileNum++;
                string newFileName = System.IO.Path.GetFileNameWithoutExtension(destFile);
                if (newFileName.IndexOf(copySufix) != -1)
                {
                    newFileName = newFileName.Substring(0, newFileName.IndexOf(copySufix));
                }
                newFileName = newFileName + copySufix + fileNum.ToString() + System.IO.Path.GetExtension(destFile);
                destFile = System.IO.Path.Combine(targetPath, newFileName);
            }

            System.IO.File.Copy(filePath, destFile, true);
            if (!isValueExpression)
            {
                xPhotoSumilationTxtBox.ValueTextBox.Text = @"~\Documents\MobileSimulations\Photos\" + System.IO.Path.GetFileName(destFile);
            }
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string filePath = UpdateBaseLineImage();
            ImportPhotoToSolutionFolder(filePath);
        }

        private string UpdateBaseLineImage(bool firstTime = false)
        {
            string FileName = General.GetFullFilePath(xPhotoSumilationTxtBox.ValueTextBox.Text);
            BitmapImage b = null;
            if (File.Exists(FileName))
            {
                b = GetFreeBitmapCopy(FileName, firstTime);
                isValueExpression = false;
            }
            else
            {

                ValueExpression ve = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, Context.GetAsContext(mAct.Context), null);
                FileName = ve.Calculate(FileName);
                if (File.Exists(FileName))
                {
                    b = GetFreeBitmapCopy(FileName, firstTime);
                }
                isValueExpression = true;
            }
            // send with null bitmap will show image not found
            ScreenShotViewPage p = new ScreenShotViewPage("Baseline Image", b);
            SimulatedPhotoFrame.Content = p;
            //if b is null then photo is not in required format, so return empty string to not try to import photo
            if (b == null)
            {
                return string.Empty;
            }
            return FileName;
        }
        private BitmapImage GetFreeBitmapCopy(String filePath, bool firstTime = false)
        {
            // make sure we load bitmap and the file is readonly not get locked
            try
            {
                Bitmap bmp = new Bitmap(filePath);
                BitmapImage bi = BitmapToImageSource(bmp);
                bmp.Dispose();
                return bi;
            }
            catch (Exception)
            {
                if (!firstTime)
                {
                    Reporter.ToUser(eUserMsgKey.FileExtensionNotSupported, "jpg/jpeg/png");
                }
                UserMsg messageToShow = null;
                if ((Reporter.UserMsgsPool != null) && Reporter.UserMsgsPool.Keys.Contains(eUserMsgKey.FileExtensionNotSupported))
                {
                    messageToShow = Reporter.UserMsgsPool[eUserMsgKey.FileExtensionNotSupported];
                }
                if (messageToShow != null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format(messageToShow.Message, "jpg/jpeg/png"));
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "File extention not supported. Supported extentions: jpg/jpeg/png");
                }
                return null;
            }

        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void ActionNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetControlsView();
        }

        private void SetControlsView()
        {
            xKeyPressPnl.Visibility = Visibility.Collapsed;
            xXY1Pnl.Visibility = Visibility.Collapsed;
            xXY2Pnl.Visibility = Visibility.Collapsed;
            xPhotoSimulationPnl.Visibility = Visibility.Collapsed;


            switch (mAct.MobileDeviceAction)
            {
                case ActMobileDevice.eMobileDeviceAction.PressKey:
                case ActMobileDevice.eMobileDeviceAction.LongPressKey:
                    xKeyPressPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.PressXY:
                case ActMobileDevice.eMobileDeviceAction.LongPressXY:
                case ActMobileDevice.eMobileDeviceAction.TapXY:
                    xXY1Pnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.DragXYXY:
                case ActMobileDevice.eMobileDeviceAction.SwipeByCoordinates:
                    xXY1Pnl.Visibility = Visibility.Visible;
                    xXY2Pnl.Visibility = Visibility.Visible;
                    break;
                case ActMobileDevice.eMobileDeviceAction.SimulatePhoto:
                    xPhotoSimulationPnl.Visibility = Visibility.Visible;
                    break;
            }
        }


    }
}
