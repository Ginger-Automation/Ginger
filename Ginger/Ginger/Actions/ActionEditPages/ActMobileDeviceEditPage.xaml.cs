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

            xPhotoSumilationTxtBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActMobileDevice.SimulatedPhotoPath)), true, true, UCValueExpression.eBrowserType.File, "*", ValueTextBox_ClickBrowse);

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
            if (string.IsNullOrEmpty(filePath) || filePath.Substring(0, 1) == "~" || filePath.Contains(WorkSpace.Instance.Solution.Folder))
            {
                return;
            }
            string SolutionFolder = WorkSpace.Instance.Solution.Folder;
            string targetPath = System.IO.Path.Combine(SolutionFolder, @"Documents\MobileSimulations\Photos");
            if (Reporter.ToUser(eUserMsgKey.AskIfToImportFile, targetPath) == eUserMsgSelection.No)
            {
                return;
            }

            ProcessInputForDriver();

            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }

            string fileName = System.IO.Path.GetFileName(filePath);
            string destFile = System.IO.Path.Combine(targetPath, fileName);

            System.IO.File.Copy(filePath, destFile, true);
            if (!isValueExpression)
            {
                xPhotoSumilationTxtBox.ValueTextBox.Text = @"~\Documents\MobileSimulations\Photos\" + System.IO.Path.GetFileName(destFile);
            }
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateBaseLineImage();
        }

        private void ValueTextBox_ClickBrowse(object sender, RoutedEventArgs e)
        {
            string filePath = UpdateBaseLineImage();
            ImportPhotoToSolutionFolder(filePath);
        }

        private string UpdateBaseLineImage(bool firstTime = false)
        {
            string FileName = General.GetFullFilePath(xPhotoSumilationTxtBox.ValueTextBox.Text);
            if (File.Exists(FileName))
            {
                isValueExpression = false;
            }
            else
            {

                ValueExpression ve = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, Context.GetAsContext(mAct.Context), null);
                FileName = ve.Calculate(FileName);
                isValueExpression = true;
            }
            // send with null bitmap will show image not found
            ScreenShotViewPage p = new ScreenShotViewPage("Baseline Image", FileName);
            SimulatedPhotoFrame.Content = p;
            if (p.BitmapImage == null)
            {
                return string.Empty;
            }
            return FileName;
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
