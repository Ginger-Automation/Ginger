#region License
/*
Copyright � 2014-2022 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.BusinessFlowWindows;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for SelectAppFolderWizardPage.xaml
    /// </summary>
    public partial class POMGeneralDetailsWizardPage : Page, IWizardPage
    {
        BasePOMWizard mBasePOMWizard;
        ScreenShotViewPage mScreenshotPage;
        ucBusinessFlowMap mBusinessFlowControl;

        public POMGeneralDetailsWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                     mBasePOMWizard = (BasePOMWizard)WizardEventArgs.Wizard;
                     xNameTextBox.BindControl(mBasePOMWizard.mPomLearnUtils.POM, nameof(ApplicationPOMModel.Name));
                     xNameTextBox.AddValidationRule(new POMNameValidationRule());
                     xNameTextBox.Focus();

                     xURLTextBox.BindControl(mBasePOMWizard.mPomLearnUtils.POM, nameof(ApplicationPOMModel.PageURL));

                     xDescriptionTextBox.BindControl(mBasePOMWizard.mPomLearnUtils.POM, nameof(ApplicationPOMModel.Description));
                     xTagsViewer.Init(mBasePOMWizard.mPomLearnUtils.POM.TagsKeys);

                     mBusinessFlowControl = new ucBusinessFlowMap(mBasePOMWizard.mPomLearnUtils.POM, nameof(mBasePOMWizard.mPomLearnUtils.POM.MappedBusinessFlow), false);
                     xFrameBusinessFlowControl.ClearAndSetContent(mBusinessFlowControl);
                    break;
                case EventType.Active:
                    SetDefaultPage();
                    ShowScreenShot();
                    if (mBasePOMWizard.ManualElementConfiguration)
                    {
                        xTakeScreenShotLoadButton.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        xTakeScreenShotLoadButton.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }

        private void SetDefaultPage()
        {
            xPageUrlRadioBtn.IsChecked = true;

            mBusinessFlowControl.TargetApplication = mBasePOMWizard.mPomLearnUtils.POM.TargetApplicationKey.ItemName;

            ePlatformType mAppPlatform = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mBasePOMWizard.mPomLearnUtils.POM.TargetApplicationKey);

            PlatformInfoBase platformInfoBase = PlatformInfoBase.GetPlatformImpl(mAppPlatform);
            if (platformInfoBase != null)
            {
                xPageUrlRadioBtn.Content = platformInfoBase.GetPageUrlRadioLabelText();
            }

        }

        public void ShowScreenShot()
        {
            mScreenshotPage = new ScreenShotViewPage(mBasePOMWizard.mPomLearnUtils.POM.Name, mBasePOMWizard.mPomLearnUtils.ScreenShot);
            mScreenshotPage.xZoomSlider.Value = 0.5;
            xScreenShotFrame.ClearAndSetContent(mScreenshotPage);
        }

        private void xTakeScreenShotLoadButton_Click(object sender, RoutedEventArgs e)
        {
            mBasePOMWizard.mPomLearnUtils.LearnScreenShot();
            ShowScreenShot();

        }

        private void xBrowseImageLoadButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog
            {
                Title = "Select a picture",
                Filter = "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg"
            };
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fileLength = new FileInfo(op.FileName).Length;
                if (fileLength <= 500000)
                {
                    if ((op.FileName != null) && (op.FileName != string.Empty))
                    {
                        BitmapImage bi = new BitmapImage(new Uri(op.FileName));
                        mBasePOMWizard.mPomLearnUtils.ScreenShot = Ginger.General.BitmapImage2Bitmap(bi);
                        mBasePOMWizard.mPomLearnUtils.POM.ScreenShotImage = Ginger.General.BitmapToBase64(mBasePOMWizard.mPomLearnUtils.ScreenShot);
                        mScreenshotPage = new ScreenShotViewPage(mBasePOMWizard.mPomLearnUtils.POM.Name, mBasePOMWizard.mPomLearnUtils.ScreenShot);
                        xScreenShotFrame.ClearAndSetContent(mScreenshotPage);
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.ImageSize, "500");
                }
            }
        }

        private void xRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(xPageUrlRadioBtn.IsChecked))
            {
                mBasePOMWizard.mPomLearnUtils.POM.PageLoadFlow = ApplicationPOMModel.ePageLoadFlowType.PageURL;
                xURLTextBox.Visibility = Visibility.Visible;
                xFrameBusinessFlowControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                mBasePOMWizard.mPomLearnUtils.POM.PageLoadFlow = ApplicationPOMModel.ePageLoadFlowType.BusinessFlow;
                xFrameBusinessFlowControl.Visibility = Visibility.Visible;
                xURLTextBox.Visibility = Visibility.Collapsed;
            }

        }
    }
}
