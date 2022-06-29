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

using Ginger.Actions.UserControls;
using GingerCore;
using GingerCore.Actions.VisualTesting;
using GingerWPF.WizardLib;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for MapUIElementsWizardPage.xaml
    /// </summary>
    public partial class POMScreenShotWizardPage : Page, IWizardPage
    {       
        AddPOMWizard mWizard;

        public POMScreenShotWizardPage()
        {
            InitializeComponent();
        }

        ScreenShotViewPage mScreenshotPage;

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    break;

                case EventType.Active:
                    ShowScreenShot();
                    break;
            }
        }

        public void ShowScreenShot()
        {
            mWizard.IWindowExplorerDriver.UnHighLightElements();
            mWizard.ScreenShot = ((IVisualTestingDriver)mWizard.Agent.Driver).GetScreenShot();
            mScreenshotPage = new ScreenShotViewPage(mWizard.POM.Name, mWizard.ScreenShot);
            MainFrame.Content = mScreenshotPage;
        }


        private void TakeScreenShotButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowScreenShot();
        }

        private void BrowseImageButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg";
            op.ShowDialog();
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
                        MainFrame.Content = mScreenshotPage;
                    }
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.ImageSize);
            }
        }

        /// <summary>
        /// This method is used to cehck whether alternate page is required to load
        /// </summary>
        /// <returns></returns>
        public bool IsAlternatePageToLoad()
        {
            return false;
        }
    }
}
