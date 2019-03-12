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
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Actions.UserControls;
using GingerCore.Actions.VisualTesting;
using GingerWPF.WizardLib;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static Ginger.ExtensionMethods;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for SelectAppFolderWizardPage.xaml
    /// </summary>
    public partial class POMGeneralDetailsWizardPage : Page, IWizardPage
    {
        AddPOMWizard mWizard;
        ScreenShotViewPage mScreenshotPage;

        public POMGeneralDetailsWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    xNameTextBox.BindControl(mWizard.mPomLearnUtils.POM, nameof(ApplicationPOMModel.Name));
                    xNameTextBox.AddValidationRule(new POMNameValidationRule());
                    xNameTextBox.Focus();

                    xURLTextBox.BindControl(mWizard.mPomLearnUtils.POM, nameof(ApplicationPOMModel.PageURL));

                    xDescriptionTextBox.BindControl(mWizard.mPomLearnUtils.POM, nameof(ApplicationPOMModel.Description));
                    xTagsViewer.Init(mWizard.mPomLearnUtils.POM.TagsKeys);
                    break;
                case EventType.Active:
                    ShowScreenShot();
                    if (mWizard.ManualElementConfiguration)
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

        public void ShowScreenShot()
        {
            mScreenshotPage = new ScreenShotViewPage(mWizard.mPomLearnUtils.POM.Name, mWizard.mPomLearnUtils.ScreenShot);
            xScreenShotFrame.Content = mScreenshotPage;
        }

        private void xTakeScreenShotLoadButton_Click(object sender, RoutedEventArgs e)
        {
            mWizard.mPomLearnUtils.LearnScreenShot();
            ShowScreenShot();
        }

        private void xBrowseImageLoadButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg";
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fileLength = new FileInfo(op.FileName).Length;
                if (fileLength <= 500000)
                {
                    if ((op.FileName != null) && (op.FileName != string.Empty))
                    {
                        using (var ms = new MemoryStream())
                        {
                            BitmapImage bi = new BitmapImage(new Uri(op.FileName));
                            mWizard.mPomLearnUtils.ScreenShot = Ginger.General.BitmapImage2Bitmap(bi);
                            mWizard.mPomLearnUtils.POM.ScreenShotImage = Ginger.General.BitmapToBase64(mWizard.mPomLearnUtils.ScreenShot);
                            mScreenshotPage = new ScreenShotViewPage(mWizard.mPomLearnUtils.POM.Name, mWizard.mPomLearnUtils.ScreenShot);
                            xScreenShotFrame.Content = mScreenshotPage;
                        }
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.ImageSize, "500");
                }
            }
        }
    }
}
