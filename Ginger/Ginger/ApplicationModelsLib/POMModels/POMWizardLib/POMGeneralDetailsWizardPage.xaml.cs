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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Actions.UserControls;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Actions.VisualTesting;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    xNameTextBox.BindControl(mWizard.POM, nameof(ApplicationPOMModel.Name));
                    xNameTextBox.AddValidationRule(new POMNameValidationRule());
                    xNameTextBox.Focus();

                    xDescriptionTextBox.BindControl(mWizard.POM, nameof(ApplicationPOMModel.Description));
                    xTagsViewer.Init(mWizard.POM.TagsKeys);
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
            mScreenshotPage = new ScreenShotViewPage(mWizard.POM.Name, mWizard.ScreenShot);
            xScreenShotFrame.Content = mScreenshotPage;
        }

        private void xTakeScreenShotLoadButton_Click(object sender, RoutedEventArgs e)
        {
            mWizard.ScreenShot = ((IVisualTestingDriver)mWizard.Agent.Driver).GetScreenShot();
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
                if (fileLength <= 30000)
                {
                    if ((op.FileName != null) && (op.FileName != string.Empty))
                    {
                        using (var ms = new MemoryStream())
                        {
                            BitmapImage bi = new BitmapImage(new Uri(op.FileName));
                            Tuple<int, int> sizes = Ginger.Reports.GingerExecutionReport.ExtensionMethods.RecalculatingSizeWithKeptRatio(bi, Ginger.Reports.GingerExecutionReport.GingerExecutionReport.logoWidth, Ginger.Reports.GingerExecutionReport.GingerExecutionReport.logoHight);

                            BitmapImage bi_resized = new BitmapImage();
                            bi_resized.BeginInit();
                            bi_resized.UriSource = new Uri(op.FileName);
                            bi_resized.DecodePixelHeight = sizes.Item2;
                            bi_resized.DecodePixelWidth = sizes.Item1;
                            bi_resized.EndInit();
                            mWizard.ScreenShot = Ginger.Reports.GingerExecutionReport.ExtensionMethods.BitmapImage2Bitmap(bi_resized);
                            mWizard.POM.ScreenShotImage = Ginger.Reports.GingerExecutionReport.ExtensionMethods.BitmapToBase64(mWizard.ScreenShot);
                            mScreenshotPage = new ScreenShotViewPage(mWizard.POM.Name, mWizard.ScreenShot);
                            xScreenShotFrame.Content = mScreenshotPage;
                        }
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKeys.ImageSize);
                }
            }
        }
    }
}
