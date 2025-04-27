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
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Mobile;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using static Amdocs.Ginger.CoreNET.ActionsLib.UI.Mobile.MobileTouchOperation;


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

            
              
            xInputVE.Init(Context.GetAsContext(mAct.Context), mAct.ActionInput, nameof(ActInputValue.Value));

            xAuthResultComboBox.Init(mAct, nameof(mAct.AuthResultSimulation), typeof(ActMobileDevice.eAuthResultSimulation), AuthResultComboBox_SelectionChanged);

            xKeyPressComboBox.Init(mAct, nameof(mAct.MobilePressKey), typeof(ActMobileDevice.ePressKey));

            xX1TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.X1, nameof(ActInputValue.Value));
            xY1TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.Y1, nameof(ActInputValue.Value));
            xX2TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.X2, nameof(ActInputValue.Value));
            xY2TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.Y2, nameof(ActInputValue.Value));

            xPhotoSumilationTxtBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActMobileDevice.SimulatedPhotoPath)), true, true, UCValueExpression.eBrowserType.File, "*");

            xDeviceRotateComboBox.Init(mAct, nameof(mAct.RotateDeviceState), typeof(ActMobileDevice.eRotateDeviceState), ActionNameComboBox_SelectionChanged);

            xDataTypeComboBox.Init(mAct, nameof(mAct.PerformanceTypes), typeof(ActMobileDevice.ePerformanceTypes), ActionNameComboBox_SelectionChanged);

            xFilePathTextBox.Init(Context.GetAsContext(mAct.Context), mAct.FilePathInput, nameof(ActInputValue.Value), true, true, UCValueExpression.eBrowserType.File, "*");

            xFolderPathTxtBox.Init(Context.GetAsContext(mAct.Context), mAct.FolderPathInput, nameof(ActMobileDevice.Value), true, true, UCValueExpression.eBrowserType.Folder, "*");

            xFolderPathTextBox.Init(Context.GetAsContext(mAct.Context), mAct.LocalFolderPathInput, nameof(ActMobileDevice.Value), true, true, UCValueExpression.eBrowserType.Folder, "*");

            xAppPackageVE.Init(Context.GetAsContext(mAct.Context), mAct.ActionAppPackage, nameof(ActInputValue.Value));

            xPressDurationTxtBox.Init(Context.GetAsContext(mAct.Context), mAct.PressDuration, nameof(ActInputValue.Value));
            xDragDurationTxtBox.Init(Context.GetAsContext(mAct.Context), mAct.DragDuration, nameof(ActInputValue.Value));
            xSwipeScaleTxtBox.Init(Context.GetAsContext(mAct.Context), mAct.SwipeScale, nameof(ActInputValue.Value));
            xSwipeDurationTxtBox.Init(Context.GetAsContext(mAct.Context), mAct.SwipeDuration, nameof(ActInputValue.Value));

            SetMultiTouchGridView();
            xMultiTouchGrid.DataSourceList = mAct.MobileTouchOperations;
            xMultiTouchGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddTouchOperation));

            UpdateBaseLineImage(true);

            WeakEventManager<UIElement, RoutedEventArgs>.RemoveHandler(source: xPhotoSumilationTxtBox.ValueTextBox, eventName: nameof(UIElement.LostFocus), handler: ValueTextBox_LostFocus);
            WeakEventManager<UIElement, RoutedEventArgs>.AddHandler(source: xPhotoSumilationTxtBox.ValueTextBox, eventName: nameof(UIElement.LostFocus), handler: ValueTextBox_LostFocus);

            WeakEventManager<ButtonBase, RoutedEventArgs>.RemoveHandler(source: xPhotoSumilationTxtBox.OpenExpressionEditorButton, eventName: nameof(ButtonBase.Click), handler: ValueTextBox_LostFocus);
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: xPhotoSumilationTxtBox.OpenExpressionEditorButton, eventName: nameof(ButtonBase.Click), handler: ValueTextBox_LostFocus);
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
            if (string.IsNullOrEmpty(filePath) || filePath[..1] == "~" || filePath.Contains(WorkSpace.Instance.Solution.Folder))
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
        


        private void BrowseButton_Click(object sender, RoutedEventArgs e)
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
            SimulatedPhotoFrame.ClearAndSetContent(p);
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

        private void AuthResultComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeAuthResultDetailsComboBox();
        }

        private void ChangeAuthResultDetailsComboBox()
        {
            switch (mAct.AuthResultSimulation)
            {
                case ActMobileDevice.eAuthResultSimulation.Success:
                    {
                        xAuthResultDetailsLbl.Visibility = Visibility.Collapsed;
                        xAuthResultDetailsComboBox.Visibility = Visibility.Collapsed;
                        break;
                    }
                case ActMobileDevice.eAuthResultSimulation.Failure:
                    {
                        xAuthResultDetailsLbl.Visibility = Visibility.Visible;
                        xAuthResultDetailsComboBox.Visibility = Visibility.Visible;
                        xAuthResultDetailsComboBox.Init(mAct, nameof(mAct.AuthResultDetailsFailureSimulation), typeof(ActMobileDevice.eAuthResultDetailsFailureSimulation));
                        break;
                    }
                case ActMobileDevice.eAuthResultSimulation.Cancel:
                    {
                        xAuthResultDetailsComboBox.Visibility = Visibility.Visible;
                        xAuthResultDetailsComboBox.Init(mAct, nameof(mAct.AuthResultDetailsCancelSimulation), typeof(ActMobileDevice.eAuthResultDetailsCancelSimulation));
                        break;
                    }
                default:
                    xAuthResultDetailsLbl.Visibility = Visibility.Collapsed;
                    xAuthResultDetailsComboBox.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void SetControlsView()
        {
            xKeyPressPnl.Visibility = Visibility.Collapsed;
            xXY1Pnl.Visibility = Visibility.Collapsed;
            xXY2Pnl.Visibility = Visibility.Collapsed;
            xPhotoSimulationPnl.Visibility = Visibility.Collapsed;
            xAuthSimulationPnl.Visibility = Visibility.Collapsed;
            xAppPnl.Visibility = Visibility.Collapsed;
            xPressPnl.Visibility = Visibility.Collapsed;
            xDragPnl.Visibility = Visibility.Collapsed;
            xSwipePnl.Visibility = Visibility.Collapsed;
            xInputPnl.Visibility = Visibility.Collapsed;
            xFileTransferPnl.Visibility = Visibility.Collapsed;
            xSpecificPerformanceDataPnl.Visibility = Visibility.Collapsed;
            xDeviceRotationPnl.Visibility = Visibility.Collapsed;
            xMultiTouchGrid.Visibility = Visibility.Collapsed;
            xFolderTransferPnl.Visibility = Visibility.Collapsed;
            

            switch (mAct.MobileDeviceAction)
            {
                case ActMobileDevice.eMobileDeviceAction.PressKey:
                case ActMobileDevice.eMobileDeviceAction.LongPressKey:
                    xKeyPressPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.TapXY:
                case ActMobileDevice.eMobileDeviceAction.DoubleTapXY:
                case ActMobileDevice.eMobileDeviceAction.LongPressXY:
                    xXY1Pnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.PressXY:
                    xXY1Pnl.Visibility = Visibility.Visible;
                    xPressPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.DragXYXY:
                    xXY1Pnl.Visibility = Visibility.Visible;
                    xXY2Pnl.Visibility = Visibility.Visible;
                    xPressPnl.Visibility = Visibility.Visible;
                    xDragPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.SwipeByCoordinates:
                    xXY1Pnl.Visibility = Visibility.Visible;
                    xXY2Pnl.Visibility = Visibility.Visible;
                    xSwipePnl.Visibility = Visibility.Visible;
                    xSwipeScalePnl.Visibility = Visibility.Collapsed;
                    break;

                case ActMobileDevice.eMobileDeviceAction.SwipeDown:
                case ActMobileDevice.eMobileDeviceAction.SwipeUp:
                case ActMobileDevice.eMobileDeviceAction.SwipeLeft:
                case ActMobileDevice.eMobileDeviceAction.SwipeRight:
                    xSwipePnl.Visibility = Visibility.Visible;
                    xSwipeScalePnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.SimulatePhoto:
                case ActMobileDevice.eMobileDeviceAction.SimulateBarcode:
                    xPhotoSimulationPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.SimulateBiometrics:
                    xAuthSimulationPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.OpenDeeplink:
                    xAppPnl.Visibility = Visibility.Visible;
                    xInputLabelVE.Content = "Link:";
                    xInputTextBlock.Text = "";
                    xInputPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.CloseApp:
                case ActMobileDevice.eMobileDeviceAction.OpenApp:
                case ActMobileDevice.eMobileDeviceAction.IsAppInstalled:
                case ActMobileDevice.eMobileDeviceAction.RemoveApp:
                case ActMobileDevice.eMobileDeviceAction.QueryAppState:
                case ActMobileDevice.eMobileDeviceAction.ClearAppdata:
                    xAppPnl.Visibility = Visibility.Visible;
                    break;
                    
                case ActMobileDevice.eMobileDeviceAction.SetContext:
                    xInputLabelVE.Content = "Context to Set:";
                    xInputTextBlock.Text = "";
                    xInputPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.RunScript:
                    xInputLabelVE.Content = "Script:";
                    xInputTextBlock.Text = "";
                    xInputPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.StartRecordingScreen:
                    xInputLabelVE.Content = "Note: Max duration recording: 30 min."; 
                    xInputTextBlock.Text = "";
                    xInputVE.Visibility = Visibility.Collapsed;
                    xInputPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.GetDeviceLogs:
                case ActMobileDevice.eMobileDeviceAction.StopRecordingScreen:
                    xFilePathLbl.Visibility = Visibility.Collapsed;
                    xFilePathTextBox.Visibility = Visibility.Collapsed;
                    xFolderPathLbl.Content = "Save to Folder\\File:";
                    xFileTransferPnl.Visibility = Visibility.Visible;
                    xFolderPathTxtBox.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.PushFileToDevice:
                    xFilePathLbl.Content = "Local File to Push:";
                    xFolderPathLbl.Content = "Device Target Folder:";
                    xFilePathLbl.Visibility = Visibility.Visible;
                    xFilePathTextBox.Visibility = Visibility.Visible;
                    xFolderPathLbl.Visibility = Visibility.Visible;
                    xFolderPathTxtBox.Visibility = Visibility.Visible;
                    xFileTransferPnl.Visibility = Visibility.Visible;          
                    break;

                case ActMobileDevice.eMobileDeviceAction.PullFileFromDevice:
                    xFolderPathLbl.Content = "Device File to Pull:";
                    xFilePathLbl.Content = "Local Target to Push:";
                    xFilePathLbl.Visibility = Visibility.Visible;
                    xFilePathTextBox.Visibility = Visibility.Visible;
                    xFolderPathLbl.Visibility = Visibility.Visible;
                    xFolderPathTxtBox.Visibility = Visibility.Visible;
                    xFileTransferPnl.Visibility = Visibility.Visible;              
                    break;

                case ActMobileDevice.eMobileDeviceAction.SetClipboardText:
                    xInputLabelVE.Content = "Text:";
                    xInputTextBlock.Text = "";
                    xInputPnl.Visibility = Visibility.Visible;
                    break;
                case ActMobileDevice.eMobileDeviceAction.TypeUsingIOSkeyboard:
                    xInputLabelVE.Content = "Text:";
                    xInputTextBlock.Text = "Note: For IOS, Keyboard Should be Open.";
                    xInputPnl.Visibility = Visibility.Visible;               
                    break;

                case ActMobileDevice.eMobileDeviceAction.GetSpecificPerformanceData:
                    xAppPnl.Visibility = Visibility.Visible;
                    xSpecificPerformanceDataPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.RotateSimulation:
                    xDeviceRotationPnl.Visibility = Visibility.Visible;
                    break;
                case ActMobileDevice.eMobileDeviceAction.LockForDuration:
                    xInputLabelVE.Content = "Set a Time Duration for Lock(Seconds):";
                    xInputTextBlock.Text = "";
                    xInputPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.PerformMultiTouch:
                    xMultiTouchGrid.Visibility = Visibility.Visible;
                    break;
                case ActMobileDevice.eMobileDeviceAction.GrantAppPermission:
                    xAppPnl.Visibility = Visibility.Visible;
                    xInputLabelVE.Content = "Permission:";
                    xInputTextBlock.Text = ""; 
                    xInputPnl.Visibility = Visibility.Visible;
                    break;
                //case ActMobileDevice.eMobileDeviceAction.PushFolder:                  
                //    xFolderPathLbl.Content = "Local Folder to Push:";
                //    xFolderTransferPnl.Visibility = Visibility.Visible;
                //    xFileTransferPnl.Visibility = Visibility.Visible;
                //    xFilePathTextBox.Visibility = Visibility.Collapsed;
                //    xFilePathLbl.Visibility = Visibility.Collapsed;
                //    xFolderTextBlock.Text = "";
                //    break;

                case ActMobileDevice.eMobileDeviceAction.SendAppToBackground:
                    xInputLabelVE.Content = "Set a Time Duration for Backgroud(Seconds):";
                    xInputTextBlock.Text = "";
                    xInputPnl.Visibility = Visibility.Visible; 
                    break;
                case ActMobileDevice.eMobileDeviceAction.StartActivity:
                    xAppPnl.Visibility = Visibility.Visible;
                    xInputLabelVE.Content = "Activity:";
                    xInputTextBlock.Text = "";
                    xInputPnl.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SetMultiTouchGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(MobileTouchOperation.OperationType), Header = "Operation Type", WidthWeight = 25,StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList=GingerCore.General.GetEnumValuesForCombo(typeof(eFingerOperationType)) },
                new GridColView() { Field = nameof(MobileTouchOperation.MoveXcoordinate), Header = "X Coordinate (Move Only)", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text },
                new GridColView() { Field = nameof(MobileTouchOperation.MoveYcoordinate), Header = "Y Coordinate (Move Only)", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text },
                new GridColView() { Field = nameof(MobileTouchOperation.OperationDuration), Header = "Duration Milliseconds (Move & Pause)", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text },
            ]
            };
            xMultiTouchGrid.btnRefresh.Visibility = Visibility.Collapsed;
            xMultiTouchGrid.btnEdit.Visibility = Visibility.Collapsed;
            xMultiTouchGrid.SetAllColumnsDefaultView(view);
            xMultiTouchGrid.InitViewItems();
        }

        private void AddTouchOperation(object sender, RoutedEventArgs e)
        {
            MobileTouchOperation tuchOperation = new MobileTouchOperation
            {
                OperationType = MobileTouchOperation.eFingerOperationType.FingerMove,
                MoveXcoordinate = 0,
                MoveYcoordinate = 0,
                OperationDuration = 200
            };
            mAct.MobileTouchOperations.Add(tuchOperation);
        }
    }
}
