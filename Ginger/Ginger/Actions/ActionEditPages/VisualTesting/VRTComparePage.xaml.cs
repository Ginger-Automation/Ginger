#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.Actions.UserControls;
using GingerCore.Actions;
using GingerCore.Actions.VisualTesting;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace Ginger.Actions.VisualTesting
{
    /// <summary>
    /// Interaction logic for VRTComparePage.xaml
    /// </summary>
    public partial class VRTComparePage : Page
    {
        private GingerCore.Actions.ActVisualTesting mAct;
        public VisualCompareAnalyzerIntegration visualCompareAnalyzerIntegration = new VisualCompareAnalyzerIntegration();


        public VRTComparePage(GingerCore.Actions.ActVisualTesting mAct)
        {
            InitializeComponent();
            this.mAct = mAct;
            xVRTActionComboBox.Init(mAct.GetOrCreateInputParam(VRTAnalyzer.VRTAction, VRTAnalyzer.eVRTAction.Track.ToString()), typeof(VRTAnalyzer.eVRTAction), false);
            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: xVRTActionComboBox.ComboBox, eventName: nameof(Selector.SelectionChanged), handler: VRTAction_Changed);

            xActionByComboBox.Init(mAct.GetOrCreateInputParam(VRTAnalyzer.ActionBy, VRTAnalyzer.eActionBy.Window.ToString()), typeof(VRTAnalyzer.eActionBy), false);
            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: xActionByComboBox.ComboBox, eventName: nameof(Selector.SelectionChanged), handler: ActionBy_Changed);

            xVRTImageNameActionComboBox.Init(mAct.GetOrCreateInputParam(VRTAnalyzer.ImageNameBy, VRTAnalyzer.eImageNameBy.ActionName.ToString()), typeof(VRTAnalyzer.eImageNameBy), false);
            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: xVRTImageNameActionComboBox.ComboBox, eventName: nameof(Selector.SelectionChanged), handler: ImageNameBy_Changed);
            xImageNameUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(VRTAnalyzer.ImageName, mAct.Description));

            xBaselineImageRadioButton.Init(typeof(VRTAnalyzer.eBaselineImageBy), xBaselineImageRadioButtonPnl, mAct.GetOrCreateInputParam(VRTAnalyzer.BaselineImage, VRTAnalyzer.eBaselineImageBy.ActiveWindow.ToString()), BaselineImageButton_Clicked);

            VRTCurrentBaselineImagePathTxtBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(VRTAnalyzer.VRTSavedBaseImageFilenameString), true, true, UCValueExpression.eBrowserType.File, "png", BaseLineFileSelected_Click);
            WeakEventManager<TextBoxBase, TextChangedEventArgs>.AddHandler(source: VRTCurrentBaselineImagePathTxtBox.ValueTextBox, eventName: nameof(TextBoxBase.TextChanged), handler: ValueTextBox_TextChanged);
            UpdateBaseLineImage();
            GetBaseLineImage();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xCreateBaselineCheckbox, CheckBox.IsCheckedProperty, mAct, nameof(mAct.CreateBaselineImage));

            InitLayout();

            DiffTollerancePercentUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(VRTAnalyzer.VRTParamDiffTollerancePercent, "0.0"), true, false);

            string buildTestName = string.Empty;
            if (Context.GetAsContext(mAct.Context) != null && (Context.GetAsContext(mAct.Context)).Activity != null)
            {
                buildTestName = (Context.GetAsContext(mAct.Context)).Activity.ActivityName;
            }
            xTestNameUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(VRTAnalyzer.VRTParamBuildName, buildTestName), true, false);


            List<eLocateBy> locatorsTypeList = mAct.AvailableLocateBy().Where(e => e != eLocateBy.iOSClassChain && e != eLocateBy.iOSPredicateString).ToList();
            if (mAct.Platform == ePlatformType.NA)
            {
                mAct.Platform = GetActionPlatform();
            }
            PlatformInfoBase mPlatform = PlatformInfoBase.GetPlatformImpl(mAct.Platform);
            List<eLocateBy> LocateByList = mPlatform.GetPlatformUIElementLocatorsList();
            xElementLocateByComboBox.BindControl(mAct, Act.Fields.LocateBy, LocateByList);
            xLocateValueVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(Act.Fields.LocateValue));
            string allProperties = string.Empty;
            PropertyChangedEventManager.AddHandler(source: mAct, handler: mAct_PropertyChanged, propertyName: allProperties);
            SetLocateValueControls();
        }

        public void InitLayout()
        {
            VRTAnalyzer.eVRTAction vrtAction = (VRTAnalyzer.eVRTAction)Enum.Parse(typeof(VRTAnalyzer.eVRTAction), xVRTActionComboBox.ComboBox.SelectedValue.ToString(), true);
            VRTAnalyzer.eActionBy actionBy = (VRTAnalyzer.eActionBy)Enum.Parse(typeof(VRTAnalyzer.eActionBy), xActionByComboBox.ComboBox.SelectedValue.ToString(), true);
            VRTAnalyzer.eImageNameBy imageNameBy = (VRTAnalyzer.eImageNameBy)Enum.Parse(typeof(VRTAnalyzer.eImageNameBy), xVRTImageNameActionComboBox.ComboBox.SelectedValue.ToString(), true);
            VRTAnalyzer.eBaselineImageBy baselineImageBy = (VRTAnalyzer.eBaselineImageBy)Enum.Parse(typeof(VRTAnalyzer.eBaselineImageBy), mAct.GetInputParamValue(VRTAnalyzer.BaselineImage),true);
            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, eVisualTestingVisibility.Collapsed);
            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetBaselineSectionVisibility, eVisualTestingVisibility.Collapsed);
            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetTargetSectionVisibility, eVisualTestingVisibility.Collapsed);
            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetResultsSectionVisibility, eVisualTestingVisibility.Collapsed);
            xVRTNote.Visibility = Visibility.Visible;
            xCreateBaseline.Visibility = Visibility.Visible;
            xCreateBaselineCheckbox.Visibility = Visibility.Visible;
            xCreateBaselineNote.Visibility = Visibility.Visible;
            switch (vrtAction)
            {
                case VRTAnalyzer.eVRTAction.Start:
                    //diff
                    xDiffTollerancePercentLabel.Visibility = Visibility.Collapsed;
                    DiffTollerancePercentUCVE.Visibility = Visibility.Collapsed;
                    //test/build
                    xTestNameLabel.Visibility = Visibility.Visible;
                    xTestNameUCVE.Visibility = Visibility.Visible;
                    //locateby
                    xLocateByAndValuePanel.Visibility = Visibility.Collapsed;
                    //actionby
                    xVRTActionByLabel.Visibility = Visibility.Collapsed;
                    xActionByComboBox.Visibility = Visibility.Collapsed;
                    //imagenameby
                    xVRTImageNameActionLabel.Visibility = Visibility.Collapsed;
                    xVRTImageNameActionComboBox.Visibility = Visibility.Collapsed;
                    //image name
                    xImageNameLabel.Visibility = Visibility.Collapsed;
                    xImageNameUCVE.Visibility = Visibility.Collapsed;
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, eVisualTestingVisibility.Visible);
                    //Base line image
                    xBaselineImage.Visibility = Visibility.Collapsed;
                    xBaselineImageRadioButtonPnl.Visibility = Visibility.Collapsed;
                    VRTCurrentBaselineImagePathTxtBoxPnl.Visibility = Visibility.Collapsed;
                    VRTBaseImageFramePnl.Visibility = Visibility.Collapsed;
                    xBaselineImagePath.Visibility = Visibility.Collapsed;
                    xCreateBaseline.Visibility = Visibility.Collapsed;
                    xCreateBaselineCheckbox.Visibility = Visibility.Collapsed;
                    xPreviewImage.Visibility = Visibility.Collapsed;
                    xCreateBaselineNote.Visibility = Visibility.Collapsed;
                    xPreviewBaselineImage.Visibility = Visibility.Collapsed;
                    VRTPreviewBaselineImageFramePnl.Visibility = Visibility.Collapsed;
                    break;
                case VRTAnalyzer.eVRTAction.Track:
                    xDiffTollerancePercentLabel.Visibility = Visibility.Visible;
                    DiffTollerancePercentUCVE.Visibility = Visibility.Visible;
                    xTestNameLabel.Visibility = Visibility.Collapsed;
                    xTestNameUCVE.Visibility = Visibility.Collapsed;
                    xVRTActionByLabel.Visibility = Visibility.Visible;
                    xActionByComboBox.Visibility = Visibility.Visible;
                    if(!string.IsNullOrEmpty(mAct.previewBaselineImageName))
                    {
                        xPreviewBaselineImage.Visibility = Visibility.Visible;
                        VRTPreviewBaselineImageFramePnl.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        xPreviewBaselineImage.Visibility = Visibility.Collapsed;
                        VRTPreviewBaselineImageFramePnl.Visibility = Visibility.Collapsed;
                    }
                    
                    if (actionBy == VRTAnalyzer.eActionBy.Region)
                    {
                        xLocateByAndValuePanel.Visibility = Visibility.Visible;
                        SetLocateValueControls();
                    }
                    else
                    {
                        xLocateByAndValuePanel.Visibility = Visibility.Collapsed;
                    }
                    xVRTImageNameActionLabel.Visibility = Visibility.Visible;
                    xVRTImageNameActionComboBox.Visibility = Visibility.Visible;
                    if (imageNameBy == VRTAnalyzer.eImageNameBy.Custom)
                    {
                        xImageNameLabel.Visibility = Visibility.Visible;
                        xImageNameUCVE.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        xImageNameLabel.Visibility = Visibility.Collapsed;
                        xImageNameUCVE.Visibility = Visibility.Collapsed;
                    }
                    if (baselineImageBy == VRTAnalyzer.eBaselineImageBy.ImageFile)
                    {
                        VRTCurrentBaselineImagePathTxtBoxPnl.Visibility = Visibility.Visible;
                        VRTBaseImageFramePnl.Visibility = Visibility.Visible;
                        xBaselineImagePath.Visibility = Visibility.Visible;
                        xPreviewImage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        VRTCurrentBaselineImagePathTxtBoxPnl.Visibility = Visibility.Collapsed;
                        VRTBaseImageFramePnl.Visibility = Visibility.Collapsed;
                        xBaselineImagePath.Visibility = Visibility.Collapsed;
                        xPreviewImage.Visibility = Visibility.Collapsed;
                    }
                    if (xCreateBaselineCheckbox.IsChecked??false)
                    {
                        xBaselineImage.Visibility = Visibility.Visible;
                        xBaselineImageRadioButtonPnl.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        xBaselineImage.Visibility = Visibility.Collapsed;
                        xBaselineImageRadioButtonPnl.Visibility = Visibility.Collapsed;
                        VRTCurrentBaselineImagePathTxtBoxPnl.Visibility = Visibility.Collapsed;
                        VRTBaseImageFramePnl.Visibility = Visibility.Collapsed;
                        xBaselineImagePath.Visibility = Visibility.Collapsed;
                        xPreviewImage.Visibility = Visibility.Collapsed;
                    }
                    
                    break;

                case VRTAnalyzer.eVRTAction.Stop:
                    //diff
                    xDiffTollerancePercentLabel.Visibility = Visibility.Collapsed;
                    DiffTollerancePercentUCVE.Visibility = Visibility.Collapsed;
                    //test/build
                    xTestNameLabel.Visibility = Visibility.Collapsed;
                    xTestNameUCVE.Visibility = Visibility.Collapsed;
                    //locateby
                    xLocateByAndValuePanel.Visibility = Visibility.Collapsed;
                    //actionby
                    xVRTActionByLabel.Visibility = Visibility.Collapsed;
                    xActionByComboBox.Visibility = Visibility.Collapsed;
                    //imagenameby
                    xVRTImageNameActionLabel.Visibility = Visibility.Collapsed;
                    xVRTImageNameActionComboBox.Visibility = Visibility.Collapsed;
                    //image name
                    xImageNameLabel.Visibility = Visibility.Collapsed;
                    xImageNameUCVE.Visibility = Visibility.Collapsed;
                    //Base line image
                    xBaselineImage.Visibility = Visibility.Collapsed;
                    xBaselineImageRadioButtonPnl.Visibility = Visibility.Collapsed;
                    VRTCurrentBaselineImagePathTxtBoxPnl.Visibility = Visibility.Collapsed;
                    VRTBaseImageFramePnl.Visibility = Visibility.Collapsed;
                    xBaselineImagePath.Visibility = Visibility.Collapsed;
                    xCreateBaseline.Visibility = Visibility.Collapsed;
                    xCreateBaselineCheckbox.Visibility = Visibility.Collapsed;
                    xPreviewImage.Visibility = Visibility.Collapsed;
                    xCreateBaselineNote.Visibility = Visibility.Collapsed;
                    xPreviewBaselineImage.Visibility = Visibility.Collapsed;
                    VRTPreviewBaselineImageFramePnl.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void VRTAction_Changed(object sender, SelectionChangedEventArgs e)
        {
            InitLayout();
        }

        private void ActionBy_Changed(object sender, SelectionChangedEventArgs e)
        {
            InitLayout();
        }
        private void ImageNameBy_Changed(object sender, SelectionChangedEventArgs e)
        {
            InitLayout();
        }

        private void LocateBy_Changed(object sender, SelectionChangedEventArgs e)
        {
            InitLayout();
        }
        private void mAct_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ActVisualTesting.Fields.CompareResult)
            {
                if (mAct.CompareResult is string)
                {
                    Dispatcher.Invoke(() =>
                    {
                        mAct.AddOrUpdateReturnParamActual("ResultsURL", (string)mAct.CompareResult + "");
                    });

                }
            }
        }

        private void ElementLocateByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetLocateValueControls();
        }

        private void SetLocateValueControls()
        {
            if (xElementLocateByComboBox.SelectedItem == null)
            {
                xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            else
            {
                mAct.LocateBy = (eLocateBy)((ComboEnumItem)xElementLocateByComboBox.SelectedItem).Value;
            }

            eLocateBy SelectedLocType = (eLocateBy)((ComboEnumItem)xElementLocateByComboBox.SelectedItem).Value;

            switch (SelectedLocType)
            {
                case eLocateBy.POMElement:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Collapsed;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Visible;
                    Page p = new LocateByPOMElementPage(Context.GetAsContext(mAct.Context), null, null, mAct, nameof(ActBrowserElement.LocateValue));
                    xLocateValueEditFrame.ClearAndSetContent(p);
                    break;
                default:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }

        private ePlatformType GetActionPlatform()
        {
            ePlatformType platform;
            if (mAct.Context != null && (Context.GetAsContext(mAct.Context)).BusinessFlow != null)
            {
                string targetapp = (Context.GetAsContext(mAct.Context)).BusinessFlow.CurrentActivity.TargetApplication;
                platform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
            }
            else
            {
                platform = WorkSpace.Instance.Solution.ApplicationPlatforms[0].Platform;
            }
            return platform;
        }

        private void BaselineImageButton_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            InitLayout();
        }

        private void BaseLineFileSelected_Click(object sender, RoutedEventArgs e)
        {
            string BaselineFileSavingNameTypeAndPath = General.ConvertSolutionRelativePath(VRTCurrentBaselineImagePathTxtBox.ValueTextBox.Text);
            UpdateBaseLineImage();
        }

        private void UpdateBaseLineImage()
        {
            string FileName = General.GetFullFilePath(VRTCurrentBaselineImagePathTxtBox.ValueTextBox.Text);
            BitmapImage b = null;
            if (File.Exists(FileName) && new FileInfo(FileName).Length > 0)
            {
                b = GetFreeBitmapCopy(FileName);
            }
            // send with null bitmap will show image not found
            ScreenShotViewPage p = new ScreenShotViewPage("Preview Image", b);
            VRTBaseImageFrame.ClearAndSetContent(p);
        }

        private void GetBaseLineImage()
        {
            try
            {
                string previewBaselineImage = GingerCoreNET.GeneralLib.General.DownloadBaselineImage($"{WorkSpace.Instance.Solution.VRTConfiguration.ApiUrl}/{mAct.previewBaselineImageName}", mAct);
                string FileName = General.GetFullFilePath(previewBaselineImage);
                BitmapImage b = null;
                if (File.Exists(FileName) && new FileInfo(FileName).Length > 0)
                {
                    b = GetFreeBitmapCopy(FileName);
                }
                // send with null bitmap will show image not found
                ScreenShotViewPage p = new ScreenShotViewPage("Preview Baseline Image", b);
                VRTPreviewBaselineImageFrame.ClearAndSetContent(p);
            }
            catch(Exception ex) 
            {
                Reporter.ToLog(eLogLevel.ERROR, "unable to fetch the baseline image",ex);
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

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateBaseLineImage();
        }

        private void xCreateBaselineCheckbox_CheckChanged(object sender, RoutedEventArgs e)
        {
            InitLayout();
        }
    }
}