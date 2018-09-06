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
                pomAllElementsPage.SetAgent(mAgent);
            }
        }

        public IWindowExplorer mWinExplorer
        {
            get
            {
                if (mAgent != null && mAgent.Status == Agent.eStatus.Running)
                    return mAgent.Driver as IWindowExplorer;
                else
                    return null;
            }

        }


        ScreenShotViewPage pd;
        PomAllElementsPage pomAllElementsPage;
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

            //xAgentControlUC.xAgentStatusBtn.Click += StartAgentButtonClicked;

            BitmapSource source = null;
            if (mPOM.ScreenShotImage != null)
            {
                source = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetImageStream(Ginger.Reports.GingerExecutionReport.ExtensionMethods.Base64ToImage(mPOM.ScreenShotImage.ToString()));
            }

            //Bitmap ScreenShot = BitmapFromSource(Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetImageStream(Ginger.Reports.GingerExecutionReport.ExtensionMethods.Base64ToImage(mPOM.LogoBase64Image.ToString())));

            mScreenShotViewPage = new ScreenShotViewPage(mPOM.Name, source);
            xScreenShotFrame.Content = mScreenShotViewPage;

            //PomElementsMappingPage mappedUIElementsPage = new PomElementsMappingPage(mPOM,null);
            pomAllElementsPage = new PomAllElementsPage(mPOM);
            //pomAllElementsPage.mappedUIElementsPage.MainElementsGrid.ValidationRules.Add(ucGrid.eUcGridValidationRules.CantBeEmpty);
            //pomAllElementsPage.unmappedUIElementsPage.MainElementsGrid.ValidationRules.Add(ucGrid.eUcGridValidationRules.CantBeEmpty);
            xUIElementsFrame.Content = pomAllElementsPage;

            UIElementTabTextBlockUpdate();

            //PageNameTextBox.BindControl(mApplicationPOM, nameof(ApplicationPOM.Name));

            //ObservableList<ApplicationPlatform> Apps = App.UserProfile.Solution.ApplicationPlatforms;
            //ApplicationPlatform AP = (from x in Apps where x.Guid == mApplicationPOM.TargetApplicationKey.Guid select x).FirstOrDefault();
            //mApplicationPOM.TargetApplicationKey = AP.Key;  // Create new binding and get updates if exist as key app name might changed

            //Yuval Uncomment
            //TargetApplicationComboBox.BindControl<ApplicationPlatform>(mApplicationPOM, nameof(mApplicationPOM.TargetAppplicationKey), Apps, nameof(ApplicationPlatform.AppName), nameof(ApplicationPlatform.Key));

            //TODO: lazy load based on tabs with cache
            //if (mPOM.ScreenShot == null)
            //{
            //    string filename = mPOM.FileName.Replace("xml", "Screenshot.bmp");  // TODO: use same const
            //    //mPom.ScreenShot = General.LoadBitmapFromFile(filename);
            //}


            //POMSimulatorFrame.Content = new POMSimulatorPage(mPom);


            //LearnWizardPage p2 = new LearnWizardPage(mPom);
            //UIElementsFrame.Content = p2;

            //MappingFrame.Content = new MapUIElementsWizardPage(mPom);

            //NaviagtionsFrame.Content = new NavigationsPage(mPom);

            ////TODO: create a page and move code to design page
            //pd = new ScreenShotViewPage(mPom.Name, mPom.ScreenShot);
            //pd.MouseClickOnScreenshot += MouseClickOnScreenshot;
            //pd.MouseUpOnScreenshot += MouseUpOnScreenshot;
            //pd.MouseMoveOnScreenshot += MouseMoveOnScreenshot;
            //DesignFrame.Content = pd;

            //InitControlPropertiesGrid();
        }



        //private void StartAgentButtonClicked(object sender, RoutedEventArgs e)
        //{
        //    pomAllElementsPage.SetWindowExplorer(xAgentControlUC.IWindowExplorerDriver);
        //}

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
                    //TODO: Fix with New Reporter (on GingerWPF)
                    System.Windows.MessageBox.Show(string.Format("The mapped '{0}' Target Application was not found, please select new Target Application", mPOM.Key.ItemName), "Missing Target Application", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.OK);
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //private void InitControlPropertiesGrid()
        //{
        //    string[] list = new string[] {"Name=ABC", "Mandatory=No", "FontColor=Black" };
        //    ControlPropertiesGrid.ItemsSource = list;
        //}

        //double MX = -1;
        //double MY = -1;

        //private void MouseClickOnScreenshot(object arg1, MouseclickonScrenshot arg2)
        //{
        //    MX = arg2.X;
        //    MY = arg2.Y;            
        //}

        //private void MouseMoveOnScreenshot(object arg1, MouseMoveonScrenshot arg2)
        //{
        //    if (MX != -1 && MY != -1)
        //    {
        //        Rectangle r = GetMouseRectangle(MX, MY, arg2.X, arg2.Y);
        //        pd.HighLight(r);
        //    }
        //}


        // Return correct rectangle when x, Y is always top left, user can draw rectangle in different direction, below function avoid get width/height in minus
        //Rectangle GetMouseRectangle(double X1, double Y1, double X2, double Y2)
        //{
        //    Rectangle r = new Rectangle();

        //    if (X1 < X2)
        //    {
        //        r.X = (int)X1;
        //    }
        //    else
        //    {
        //        r.X = (int)X2 + 1;  // + 1 to make sure our highlighert is not under mouse and we will not get th mouse events, help when drawing back or up
        //    }

        //    if (Y1 < Y2)
        //    {
        //        r.Y = (int)Y1;
        //    }
        //    else
        //    {
        //        r.Y = (int)Y2 + 1; // + 1 to make sure our highlighert is not under mouse and we will not get th mouse events, help when drawing back or up
        //    }

        //    r.Width = (int)System.Math.Abs(X1 - X2);
        //    r.Height = (int)System.Math.Abs(Y1 - Y2);

        //    return r;
        //}


        //private void MouseUpOnScreenshot(object arg1, MouseUponScrenshot arg2)
        //{
        //    Rectangle r = GetMouseRectangle(MX, MY, arg2.X, arg2.Y);
        //    MX = -1;
        //    MY = -1;
        //    CreateControl(r);
        //}

        //private void CreateControl(Rectangle r)
        //{
        //    // ******************************************************************************
        //    //DO NOT  DELETE Temp commented for moving to GingerCoreCommon
        //    // ******************************************************************************
        //    //if (ControlsListBox.SelectedItem == null) return;
        //    //var c = ((ListBoxItem)ControlsListBox.SelectedItem).Content;

        //    //Control control = null;

        //    //if (c is Label)
        //    //{
        //    //    Label l = new Label();                
        //    //    l.Content = "Label";
        //    //    l.MouseDown += Label_MouseDown;                
        //    //    control = l;

        //    //    //TODO: create at the end for all the same
        //    //    ElementInfo EI = new ElementInfo();
        //    //    EI.ElementName = "New Label";
        //    //    EI.X = r.X;
        //    //    EI.Y = r.Y;
        //    //    EI.Width = (int)l.Width;
        //    //    EI.Height = (int)l.Height;
        //    //    EI.ElementType = "Label";
        //    //    EI.Active = true;
        //    //    mApplicationPOM.UIElements.Add(EI);
        //    //    l.Tag = EI;
        //    //}

        //    //if (c is Button)
        //    //{
        //    //    Button b = new Button();                
        //    //    b.Content = "Button";
        //    //    b.Click += B_Click;
        //    //    control = b;
        //    //}

        //    //if (c is TextBox)
        //    //{
        //    //    TextBox TB = new TextBox();
        //    //    control = TB;
        //    //}

        //    //if (c is ComboBox)
        //    //{
        //    //    ComboBox CB = new ComboBox();                
        //    //    CB.ItemsSource = new string[] { "A", "B", "C" };
        //    //    control = CB;
        //    //}

        //    //if (control != null)
        //    //{
        //    //    control.Width = r.Width;
        //    //    control.Height = r.Height;
        //    //    pd.AddControl(control, r.X, r.Y);
        //    //}

        //    ////TODO: common Setcontrol location for all C
        //}

        //private void B_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //}

        //private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //}

        //private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    pd.ClearControls();
        //}
    }
}
