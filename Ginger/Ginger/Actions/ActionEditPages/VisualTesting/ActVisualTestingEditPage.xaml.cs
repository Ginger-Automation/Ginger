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

using Amdocs.Ginger.Common;
using Ginger.Actions.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.VisualTesting;
using GingerCore.GeneralLib;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace Ginger.Actions.VisualTesting
{
    /// <summary>
    /// Interaction logic for ActVisualTestingEditPage.xaml
    /// </summary>
    public partial class ActVisualTestingEditPage : Page
    {
        private ActVisualTesting mAct;

        ApplitoolsComparePage mApplitoolsComparePage = null;
        BitmapPixelsComaprePage mBitmapPixelsComaprePage = null;
        UIElementsComparisonPage mUIElementsBitmapComparisonPage = null;
        VRTComparePage mVRtComparisonPage = null;
        public ActVisualTestingEditPage(GingerCore.Actions.ActVisualTesting Act)
        {
            InitializeComponent();
            this.mAct = Act;

            //Visual Testing Engine
            VisualTestingEngineComboBox.Init(mAct.GetOrCreateInputParam(ActVisualTesting.Fields.VisualAnalyzer, ActVisualTesting.eVisualTestingAnalyzer.VRT.ToString()), typeof(ActVisualTesting.eVisualTestingAnalyzer), false, new SelectionChangedEventHandler(VisualTestingEngineComboBox_SelectionChanged));

            //Saved baseline image path for that action
            CurrentBaselineImagePathTxtBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActVisualTesting.Fields.SavedBaseImageFilenameString), true, true, UCValueExpression.eBrowserType.File, "*", BaseLineFileSelected_Click);
            UpdateBaseLineImage();
            //TODO: hook value change and update 

            WeakEventManager<TextBoxBase, TextChangedEventArgs>.AddHandler(source: CurrentBaselineImagePathTxtBox.ValueTextBox, eventName: nameof(TextBoxBase.TextChanged), handler: ValueTextBox_TextChanged);
            //Saved Applitools baseline image path

            //Saved Target image file path
            TargetImageFileNameUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActVisualTesting.Fields.SavedTargetImageFilenameString), true, true, UCValueExpression.eBrowserType.File, "*", BrowseTargetImageFromFile_Click);
            UpdateTargetImage();

            ShowCompareResult();
            ChangeAppScreenSizeComboBox.Init(mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ChangeAppWindowSize, ActVisualTesting.eChangeAppWindowSize.None.ToString()), typeof(ActVisualTesting.eChangeAppWindowSize), false, new SelectionChangedEventHandler(ChangeAppWindowSize_Changed));

            WidthUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActVisualTesting.Fields.SetAppWindowWidth, "0"), true);
            HeightUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActVisualTesting.Fields.SetAppWindowHeight, "0"), true);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xFullPageScreenshotCheckbox, CheckBox.IsCheckedProperty, mAct, nameof(mAct.IsFullPageScreenshot));

            if (mAct.IsTargetSourceFromScreenshot)
            {
                TargetScreenShotRadioButton.IsChecked = true;
            }
            else
            {
                TargetImageFileRadioButton.IsChecked = true;
            }

            if (mAct.CreateBaselineAction)
            {
                CreateBaselineRadioButton.IsChecked = true;
            }
            else
            {
                CompareRadioButton.IsChecked = true;
            }
            string allProperties = string.Empty;
            PropertyChangedEventManager.AddHandler(source: mAct, handler: mAct_PropertyChanged, propertyName: allProperties);
        }

        private void VisualCompareAnalyzerIntegration_VisualTestingEvent(VisualTestingEventArgs EventArgs)
        {
            switch (EventArgs.EventType)
            {
                case VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility:
                    xSetApplicationScreenSize.Visibility = ConvertVisibility(EventArgs.visibility);
                    break;

                case VisualTestingEventArgs.eEventType.SetBaselineSectionVisibility:
                    xCompareOrCreateBaselinesRadioButtons.Visibility = ConvertVisibility(EventArgs.visibility);
                    xBaselineAndTargetImages.Visibility = ConvertVisibility(EventArgs.visibility);
                    if (ConvertVisibility(EventArgs.visibility) == Visibility.Visible)
                    {
                        xBaselineAndTargetImagesRow.Height = new GridLength(400, GridUnitType.Pixel);
                    }
                    else
                    {
                        xBaselineAndTargetImagesRow.Height = new GridLength(400, GridUnitType.Star);
                    }

                    break;

                case VisualTestingEventArgs.eEventType.SetTargetSectionVisibility:
                    xDiffrenceImageFrame.Visibility = ConvertVisibility(EventArgs.visibility);
                    if (ConvertVisibility(EventArgs.visibility) == Visibility.Visible)
                    {
                        xDiffrenceImageFrameRow.Height = new GridLength(500, GridUnitType.Pixel);
                    }
                    else
                    {
                        xDiffrenceImageFrameRow.Height = new GridLength(500, GridUnitType.Star);
                    }

                    break;

                case VisualTestingEventArgs.eEventType.SetResultsSectionVisibility:
                    xResultImageHeader.Visibility = ConvertVisibility(EventArgs.visibility);
                    if (ConvertVisibility(EventArgs.visibility) == Visibility.Visible)
                    {
                        xResultImageHeaderRow.Height = new GridLength(30, GridUnitType.Pixel);
                    }
                    else
                    {
                        xResultImageHeaderRow.Height = new GridLength(30, GridUnitType.Star);
                    }

                    break;
            }
        }

        private Visibility ConvertVisibility(eVisualTestingVisibility eVisualTestingVisibility)
        {
            return eVisualTestingVisibility switch
            {
                eVisualTestingVisibility.Visible => Visibility.Visible,
                eVisualTestingVisibility.Hidden => Visibility.Hidden,
                eVisualTestingVisibility.Collapsed => Visibility.Collapsed,
                _ => Visibility.Visible,
            };
        }

        private void ChangeAppWindowSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (mAct.ChangeAppWindowSize == ActVisualTesting.eChangeAppWindowSize.Custom)
            {
                WidthHeightStackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                WidthHeightStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void mAct_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ActVisualTesting.Fields.CompareResult)
            {

                this.Dispatcher.Invoke(() =>
                {

                    ShowCompareResult();

                    //TODO: this need to be done when the target is change, add prop change dedicated to it
                    UpdateTargetImage();
                });
            }
        }

        private void ShowCompareResult()
        {
            if (mAct.CompareResult is BitmapImage)
            {
                ScreenShotViewPage p = new ScreenShotViewPage("Compare Result", (BitmapImage)mAct.CompareResult);
                xDiffrenceImageFrame.ClearAndSetContent(p);
                return;
            }

            if (mAct.CompareResult is Bitmap)
            {
                ScreenShotViewPage p = new ScreenShotViewPage("Compare Result", (Bitmap)mAct.CompareResult);
                xDiffrenceImageFrame.ClearAndSetContent(p);
                return;
            }

            xDiffrenceImageFrame.ClearAndSetContent(null);
        }

        private void UpdateTargetImage()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (mAct.IsTargetSourceFromScreenshot)
                {
                    if (mAct.ScreenShots.Count >= 2)
                    {
                        ScreenShotViewPage p = new ScreenShotViewPage("Target Image", mAct.ScreenShots[1]); // TODO: get it from act as target image
                        TargetImageFrame.ClearAndSetContent(p);
                    }
                }
                else
                {
                    // assume file
                    string filename = General.GetFullFilePath(CurrentBaselineImagePathTxtBox.ValueTextBox.Text);
                    if (File.Exists(filename))
                    {
                        ScreenShotViewPage p = new ScreenShotViewPage("Target Image", mAct.TargetFileName);
                        TargetImageFrame.ClearAndSetContent(p);
                    }
                }
            });
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateBaseLineImage();
        }

        private void UpdateBaseLineImage()
        {
            string FileName = General.GetFullFilePath(CurrentBaselineImagePathTxtBox.ValueTextBox.Text);
            BitmapImage b = null;
            if (File.Exists(FileName))
            {
                b = GetFreeBitmapCopy(FileName);
            }
            // send with null bitmap will show image not found
            ScreenShotViewPage p = new ScreenShotViewPage("Baseline Image", b);
            BaseImageFrame.ClearAndSetContent(p);
        }

        private void VisualTestingEngineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO: use the IVisualAnalzyer to egt the Edit Page of the analyzer, like action
            switch (mAct.VisualTestingAnalyzer)
            {
                // We cache the page so if user switch engines he doesn't lose the settings
                case ActVisualTesting.eVisualTestingAnalyzer.Applitools:

                    if (mApplitoolsComparePage == null)
                    {
                        mApplitoolsComparePage = new ApplitoolsComparePage(mAct);
                        mApplitoolsComparePage.visualCompareAnalyzerIntegration.VisualTestingEvent += VisualCompareAnalyzerIntegration_VisualTestingEvent;
                    }
                    mApplitoolsComparePage.InitLayout();
                    EngineConfigFrame.ClearAndSetContent(mApplitoolsComparePage);
                    xFullPageScreenshotCheckbox.Visibility = Visibility.Collapsed;
                    break;

                case ActVisualTesting.eVisualTestingAnalyzer.BitmapPixelsComparison:
                    if (mBitmapPixelsComaprePage == null)
                    {
                        mBitmapPixelsComaprePage = new BitmapPixelsComaprePage(mAct);
                        mBitmapPixelsComaprePage.visualCompareAnalyzerIntegration.VisualTestingEvent += VisualCompareAnalyzerIntegration_VisualTestingEvent;
                    }
                    mBitmapPixelsComaprePage.InitLayout();
                    EngineConfigFrame.ClearAndSetContent(mBitmapPixelsComaprePage);
                    xFullPageScreenshotCheckbox.Visibility = Visibility.Visible;
                    break;

                case ActVisualTesting.eVisualTestingAnalyzer.UIElementsComparison:
                    if (mUIElementsBitmapComparisonPage == null)
                    {
                        mUIElementsBitmapComparisonPage = new UIElementsComparisonPage(mAct);
                        mUIElementsBitmapComparisonPage.visualCompareAnalyzerIntegration.VisualTestingEvent += VisualCompareAnalyzerIntegration_VisualTestingEvent;
                    }
                    mUIElementsBitmapComparisonPage.InitLayout();
                    EngineConfigFrame.ClearAndSetContent(mUIElementsBitmapComparisonPage);
                    xFullPageScreenshotCheckbox.Visibility = Visibility.Visible;
                    break;
                case ActVisualTesting.eVisualTestingAnalyzer.VRT:
                    if (mVRtComparisonPage == null)
                    {
                        mVRtComparisonPage = new VRTComparePage(mAct);
                        mVRtComparisonPage.visualCompareAnalyzerIntegration.VisualTestingEvent += VisualCompareAnalyzerIntegration_VisualTestingEvent;
                    }
                    mVRtComparisonPage.InitLayout();
                    EngineConfigFrame.ClearAndSetContent(mVRtComparisonPage);
                    xFullPageScreenshotCheckbox.Visibility = Visibility.Collapsed;

                    break;
                default:
                    EngineConfigFrame.ClearAndSetContent(null);
                    xSetApplicationScreenSize.Visibility = Visibility.Collapsed;
                    xCompareOrCreateBaselinesRadioButtons.Visibility = Visibility.Collapsed;
                    xBaselineAndTargetImages.Visibility = Visibility.Collapsed;
                    xBaselineAndTargetImagesRow.Height = new GridLength(400, GridUnitType.Star);
                    xDiffrenceImageFrameRow.Height = new GridLength(500, GridUnitType.Star);
                    xResultImageHeader.Visibility = Visibility.Collapsed;
                    xResultImageHeaderRow.Height = new GridLength(30, GridUnitType.Star);
                    xFullPageScreenshotCheckbox.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        //---------------------------Set Baseline Image Section-------------------------------
        //In baseline section - Clicked on Button "Browse" to upload a picture from file
        private void BaseLineFileSelected_Click(object sender, RoutedEventArgs e)
        {
            string BaselineFileSavingNameTypeAndPath = General.ConvertSolutionRelativePath(CurrentBaselineImagePathTxtBox.ValueTextBox.Text);
            UpdateBaseLineImage();
        }

        //-------------------------------Current Screen Section-----------------------------------
        //Get Target image from upload
        private void BrowseTargetImageFromFile_Click(object sender, RoutedEventArgs e)
        {
            // string staticFilePath = PathTargetImageTxtLine.ValueTextBox.Text;            
            UpdateTargetImage();
        }

        private IVisualTestingDriver GetVisualTestingDriver()
        {
            Context.GetAsContext(mAct.Context).Runner.SetCurrentActivityAgent();
            Agent a = (Agent)(Context.GetAsContext(mAct.Context)).BusinessFlow.CurrentActivity.CurrentAgent;

            if (((AgentOperations)a.AgentOperations).Driver is IVisualTestingDriver)
            {
                return (IVisualTestingDriver)((AgentOperations)a.AgentOperations).Driver;
            }
            else
            {
                return null;
            }
        }

        //Maybe we don't need this method - to check
        //Helper method - Convert Bitmap to ImageSource
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

        private BitmapImage GetFreeBitmapCopy(String filePath)
        {
            // make sure we load bitmap and the file is readonly not get locked
            Bitmap bmp = new Bitmap(filePath);
            BitmapImage bi = BitmapToImageSource(bmp);
            bmp.Dispose();
            return bi;
        }

        private void DeleteOldFile(String FileToDelete)
        {
            if (File.Exists(FileToDelete))
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                File.Delete(FileToDelete);
            }
        }

        private void TargetScreenShotRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            TargetScreenShotLabel.Visibility = Visibility.Visible;
            TargetImageFileNameUCVE.Visibility = Visibility.Collapsed;
            mAct.TargetFileName = null; // must clear, since Isscreen shot depends on it
            TargetImageFrame.ClearAndSetContent(null);
        }

        private void TargetImageFileRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            TargetScreenShotLabel.Visibility = Visibility.Collapsed;
            TargetImageFileNameUCVE.Visibility = Visibility.Visible;
            UpdateTargetImage();
        }

        private void CreateBaseline_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            BaseImageFrame.ClearAndSetContent(null);
            string FileName = General.GetFullFilePath(CurrentBaselineImagePathTxtBox.ValueTextBox.Text);

            //TODO: add try catch if delete failed
            try
            {
                if (File.Exists(FileName))
                {
                    DeleteOldFile(FileName);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.VisualTestingFailedToDeleteOldBaselineImage);
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }

            mAct.CreateBaseline(GetVisualTestingDriver());
            UpdateBaseLineImage();

            Mouse.OverrideCursor = null;
        }

        private void MaximizeScreenSizeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mAct.SetAppWindowWidth = 0;
            mAct.SetAppWindowHeight = 0;
            WidthHeightStackPanel.Visibility = Visibility.Collapsed;
        }

        private void MaximizeScreenSizeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            WidthHeightStackPanel.Visibility = Visibility.Visible;
            if (mAct.SetAppWindowWidth == 0 && mAct.SetAppWindowHeight == 0)
            {
                mAct.SetAppWindowWidth = 800;
                mAct.SetAppWindowHeight = 600;
            }
        }

        private void CompareRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mAct.CreateBaselineAction = false;
            TargetImageColumn.Width = BaselineImageColumn.Width;
        }

        private void CreateBaselineRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mAct.CreateBaselineAction = true;
            TargetImageColumn.Width = new GridLength(0);
        }
    }
}
