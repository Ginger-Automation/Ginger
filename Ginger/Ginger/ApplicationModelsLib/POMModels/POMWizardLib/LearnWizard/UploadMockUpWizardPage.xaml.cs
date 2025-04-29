using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.Agents;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Drivers.Common;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTest.WizardLib;
using GingerWPF.WizardLib;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DocumentFormat.OpenXml.Wordprocessing;
using GingerCore.Actions;
using org.apache.zookeeper;
using Microsoft.TeamFoundation.Build.WebApi;
using Newtonsoft.Json;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for POMScreenshotLearnConfigWizardPage.xaml
    /// </summary>
    public partial class UploadMockUpWizardPage : Page, IWizardPage
    {
        private AddPOMFromScreenshotWizard mWizard;
        private ePlatformType mAppPlatform;
        ScreenShotViewPage mScreenShotViewPage;
        ApiSettings ApiSettings { get; set; }
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
                case EventType.Active:
                    break;
                case EventType.LeavingForNextPage:
                    break;
            }
        }

        private void URLTextBoxTextChange(object sender, TextChangedEventArgs e)
        {

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
