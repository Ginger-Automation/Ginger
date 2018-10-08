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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.Agents;
using GingerCore;
using GingerCore.Actions.VisualTesting;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.BindingLib;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for POMEditPage.xaml
    /// </summary>
    public partial class POMEditPage : Page
    {
        ApplicationPOMModel mPOM;
        ScreenShotViewPage mScreenShotViewPage;


        private Agent mAgent;
        public Agent Agent
        {
            get
            {
                return mAgent;
            }
            set
            {
                mAgent = value;
                mPomAllElementsPage.SetAgent(mAgent);
            }
        }

        public IWindowExplorer mWinExplorer
        {
            get
            {
                if (mAgent != null && mAgent.Status == Agent.eStatus.Running)
                {
                    return mAgent.Driver as IWindowExplorer;
                }
                else
                {
                    return null;
                }
            }
        }


        ScreenShotViewPage pd;

        readonly PomAllElementsPage mPomAllElementsPage;
        public POMEditPage(ApplicationPOMModel POM)
        {
            InitializeComponent();
            mPOM = POM;
            ControlsBinding.ObjFieldBinding(xNameTextBox, TextBox.TextProperty, mPOM, nameof(mPOM.Name));
            ControlsBinding.ObjFieldBinding(xDescriptionTextBox, TextBox.TextProperty, mPOM, nameof(mPOM.Description));

            xTargetApplicationComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            FillTargetAppsComboBox();
            xTargetApplicationComboBox.Init(mPOM, nameof(ApplicationPOMModel.TargetApplicationKey));
            xTagsViewer.Init(mPOM.TagsKeys);

            ePlatformType mAppPlatform = App.UserProfile.Solution.GetTargetApplicationPlatform(POM.TargetApplicationKey);
            ObservableList<Agent>  optionalAgentsList = GingerCore.General.ConvertListToObservableList((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Platform == mAppPlatform select x).ToList());
            xAgentControlUC.Init(optionalAgentsList);
            App.ObjFieldBinding(xAgentControlUC, ucAgentControl.SelectedAgentProperty, this, nameof(Agent));

            BitmapSource source = null;
            if (mPOM.ScreenShotImage != null)
            {
                source = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetImageStream(Ginger.Reports.GingerExecutionReport.ExtensionMethods.Base64ToImage(mPOM.ScreenShotImage.ToString()));
            }

            mScreenShotViewPage = new ScreenShotViewPage(mPOM.Name, source);
            xScreenShotFrame.Content = mScreenShotViewPage;

            mPomAllElementsPage = new PomAllElementsPage(mPOM);
            xUIElementsFrame.Content = mPomAllElementsPage;

            UIElementTabTextBlockUpdate();

        }




        private void FillTargetAppsComboBox()
        {
            //get key object 
            if (mPOM.TargetApplicationKey != null)
            {
                RepositoryItemKey key = App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.Guid == mPOM.TargetApplicationKey.Guid).Select(x => x.Key).FirstOrDefault();
                if (key != null)
                {
                    mPOM.TargetApplicationKey = key;
                }
                else
                {                    
                    Reporter.ToUser(eUserMsgKeys.MissingTargetApplication, "The mapped" + mPOM.Key.ItemName + "Target Application was not found, please select new Target Application");

                }
            }
            xTargetApplicationComboBox.ComboBox.ItemsSource = App.UserProfile.Solution.ApplicationPlatforms;
            xTargetApplicationComboBox.ComboBox.SelectedValuePath = nameof(ApplicationPlatform.Key);
            xTargetApplicationComboBox.ComboBox.DisplayMemberPath = nameof(ApplicationPlatform.AppName);
        }

        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        private void UIElementTabTextBlockUpdate()
        {
            Dispatcher.Invoke(() =>
            {
                xUIElementTabTextBlock.Text = string.Format("UI Elements ({0})", mPOM.MappedUIElements.Count + mPOM.UnMappedUIElements.Count);
            });
        }

        private void TakeScreenShotButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowScreenShot();
        }

        public void ShowScreenShot()
        {

            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKeys.POMAgentIsNotRunning);
                return;
            }

            mWinExplorer.UnHighLightElements();
            Bitmap ScreenShotBitmap = ((IVisualTestingDriver)mAgent.Driver).GetScreenShot();
            mPOM.ScreenShotImage = Ginger.Reports.GingerExecutionReport.ExtensionMethods.BitmapToBase64(ScreenShotBitmap);
            mScreenShotViewPage = new ScreenShotViewPage(mPOM.Name, ScreenShotBitmap);
            xScreenShotFrame.Content = mScreenShotViewPage;
        }

        private void BrowseImageButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg";
            op.ShowDialog();
            if (!string.IsNullOrEmpty(op.FileName))
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
                            Bitmap ScreenShotBitmap = Ginger.Reports.GingerExecutionReport.ExtensionMethods.BitmapImage2Bitmap(bi_resized);
                            mPOM.ScreenShotImage = Ginger.Reports.GingerExecutionReport.ExtensionMethods.BitmapToBase64(ScreenShotBitmap);
                            mScreenShotViewPage = new ScreenShotViewPage(mPOM.Name, ScreenShotBitmap);
                            xScreenShotFrame.Content = mScreenShotViewPage;
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
