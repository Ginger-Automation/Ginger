using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Application_Models;
using Ginger.Actions.UserControls;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for UploadMockUpWizardPage.xaml
    /// </summary>
    public partial class UploadMockUpWizardPage : Page, IWizardPage
    {
        private AddPOMFromScreenshotWizard mWizard;
        private ePlatformType mAppPlatform;
        ScreenShotViewPage mScreenShotViewPage;
        public UploadMockUpWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMFromScreenshotWizard)WizardEventArgs.Wizard;
                    break;
            }
        }


        private void BrowseImageButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            string fileName = string.Empty;
            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog
            {
                Title = "Select a picture",
                Filter = "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg"
            };
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(op.FileName))
                {
                    var fileLength = new FileInfo(op.FileName).Length;
                    if (fileLength <= 500000)
                    {
                        if ((op.FileName != null) && (op.FileName != string.Empty))
                        {
                            using (var ms = new MemoryStream())
                            {
                                xURLTextBox.Text = op.FileName;
                                mWizard.ScreenShotImagePath = xURLTextBox.Text;
                                fileName = Path.GetFileName(op.FileName);
                                BitmapImage bi = new BitmapImage(new Uri(op.FileName));
                                Bitmap ScreenShotBitmap = Ginger.General.BitmapImage2Bitmap(bi);
                                mWizard.ScreenShotImage = Ginger.General.BitmapToBase64(ScreenShotBitmap);
                                BitmapSource source = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(mWizard.ScreenShotImage));

                                mScreenShotViewPage = new ScreenShotViewPage(fileName, source, ImageMaxHeight: 450, ImageMaxWidth: 550);
                                xScreenShotFrame.ClearAndSetContent(mScreenShotViewPage);

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
}
