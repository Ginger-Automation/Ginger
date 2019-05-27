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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.Agents;
using Ginger.BusinessFlowWindows;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.VisualTesting;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Ginger.General;

namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for POMEditPage.xaml
    /// </summary>
    public partial class POMEditPage : Page
    {
        ApplicationPOMModel mPOM;
        ScreenShotViewPage mScreenShotViewPage;
        GenericWindow mWin;
        public bool IsPageSaved = false;
        public RepositoryItemPageViewMode mEditMode { get; set; }
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
                if (mAgent != null)
                {
                    mPOM.LastUsedAgent = mAgent.Guid;
                }
            }
        }

        ucBusinessFlowMap mBusinessFlowControl;

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
                    if (mAgent != null)
                    {
                        mAgent.Close();
                    }
                    return null;
                }
            }
        }
        
        // ScreenShotViewPage pd;

        readonly PomAllElementsPage mPomAllElementsPage;
        public POMEditPage(ApplicationPOMModel POM, RepositoryItemPageViewMode editMode = RepositoryItemPageViewMode.View)
        {
            InitializeComponent();
            mPOM = POM;
            mEditMode = editMode;
            
            mBusinessFlowControl = new ucBusinessFlowMap(mPOM, nameof(mPOM.MappedBusinessFlow));
            xFrameBusinessFlowControl.Content = mBusinessFlowControl;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xNameTextBox, TextBox.TextProperty, mPOM, nameof(mPOM.Name));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xDescriptionTextBox, TextBox.TextProperty, mPOM, nameof(mPOM.Description));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xPageURLTextBox, TextBox.TextProperty, mPOM, nameof(mPOM.PageURL));
            
            FillTargetAppsComboBox();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xTargetApplicationComboBox, ComboBox.SelectedValueProperty, mPOM, nameof(ApplicationPOMModel.TargetApplicationKey));
            xTagsViewer.Init(mPOM.TagsKeys);
            
            BitmapSource source = null;
            if (mPOM.ScreenShotImage != null)
            {
                source = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(mPOM.ScreenShotImage.ToString()));
            }

            mScreenShotViewPage = new ScreenShotViewPage(mPOM.Name, source);
            xScreenShotFrame.Content = mScreenShotViewPage;
            
            mPomAllElementsPage = new PomAllElementsPage(mPOM, PomAllElementsPage.eAllElementsPageContext.POMEditPage);
            xUIElementsFrame.Content = mPomAllElementsPage;

            UIElementTabTextBlockUpdate();

            ePlatformType mAppPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(POM.TargetApplicationKey);
            ObservableList<Agent> optionalAgentsList = GingerCore.General.ConvertListToObservableList((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Platform == mAppPlatform select x).ToList());
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAgentControlUC, ucAgentControl.SelectedAgentProperty, this, nameof(Agent));
            xAgentControlUC.Init(optionalAgentsList, mPOM.LastUsedAgent);

            SetDefaultPage();
        }

        private void SetDefaultPage()
        {
            if (mPOM.PageLoadFlow == ApplicationPOMModel.ePageLoadFlowType.PageURL)
            {
                xPageUrlRadioBtn.IsChecked = true;
            }
            else if (mPOM.PageLoadFlow == ApplicationPOMModel.ePageLoadFlowType.BusinessFlow)
            {
                xBusinessFlowRadioBtn.IsChecked = true;
            }
        }

        private void FillTargetAppsComboBox()
        {
            //get key object 
            if (mPOM.TargetApplicationKey != null)
            {
                RepositoryItemKey key =  WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.Guid == mPOM.TargetApplicationKey.Guid).Select(x => x.Key).FirstOrDefault();
                if (key != null)
                {
                    mPOM.TargetApplicationKey = key;
                }
                else
                {                    
                    Reporter.ToUser(eUserMsgKey.MissingTargetApplication, "The mapped" + mPOM.Key.ItemName + "Target Application was not found, please select new Target Application");

                }
            }
            xTargetApplicationComboBox.ItemsSource =  WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x=> ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList();
            xTargetApplicationComboBox.SelectedValuePath = nameof(ApplicationPlatform.Key);
            xTargetApplicationComboBox.DisplayMemberPath = nameof(ApplicationPlatform.AppName);

             WorkSpace.Instance.Solution.ApplicationPlatforms.CollectionChanged += ApplicationPlatforms_CollectionChanged;
        }

        private void ApplicationPlatforms_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            xTargetApplicationComboBox.ItemsSource = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList();
        }

        private void xTargetApplicationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xTargetApplicationComboBox.SelectedValue != null)
            {
                mBusinessFlowControl.TargetApplication = Convert.ToString(((Amdocs.Ginger.Repository.RepositoryItemKey)xTargetApplicationComboBox.SelectedValue).ItemName); 
            }
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
                Reporter.ToUser(eUserMsgKey.POMAgentIsNotRunning);
                return;
            }

            mWinExplorer.UnHighLightElements();
            Bitmap ScreenShotBitmap = ((IVisualTestingDriver)mAgent.Driver).GetScreenShot(new Tuple<int,int>(ApplicationPOMModel.cLearnScreenWidth, ApplicationPOMModel.cLearnScreenHeight));
            mPOM.ScreenShotImage = Ginger.General.BitmapToBase64(ScreenShotBitmap);
            mScreenShotViewPage = new ScreenShotViewPage(mPOM.Name, ScreenShotBitmap);
            xScreenShotFrame.Content = mScreenShotViewPage;
        }

        private void BrowseImageButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg";
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
                                BitmapImage bi = new BitmapImage(new Uri(op.FileName));
                                Bitmap ScreenShotBitmap = Ginger.General.BitmapImage2Bitmap(bi);
                                mPOM.ScreenShotImage = Ginger.General.BitmapToBase64(ScreenShotBitmap);
                                mScreenShotViewPage = new ScreenShotViewPage(mPOM.Name, ScreenShotBitmap);
                                xScreenShotFrame.Content = mScreenShotViewPage;
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

        private void xPageURLBtn_Click(object sender, RoutedEventArgs e)
        {
            GoToPageURL();
        }

        public void GoToPageURL()
        {
            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKey.POMAgentIsNotRunning);
                return;
            }

            ActGotoURL act = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = mPOM.PageURL, ValueForDriver = mPOM.PageURL, Active = true };
            mAgent.Driver.RunAction(act);
        }

        private void AgentStartedHandler()
        {
            GoToPageURL();
        }

        private void xPomTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set the selected tab text style
            try
            {
                if (xPomTabs.SelectedItem != null)
                {
                    foreach (TabItem tab in xPomTabs.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (xPomTabs.SelectedItem == tab)
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$Color_DarkBlue");

                                ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in POM Edit Page tabs style", ex);
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.FreeMaximized)
        {            
            mPOM.SaveBackup();            
            IsPageSaved = false;
            if (mPOM.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoTracked)
            {
                mPOM.StartDirtyTracking();
            }

            Button saveButton = new Button();
            saveButton.Content = "Save";
            saveButton.Click += SaveButton_Click;

            Button undoButton = new Button();
            undoButton.Content = "Undo & Close";
            undoButton.Click += UndoButton_Click;

            this.Height = 800;
            this.Width = 800;
            GingerCore.General.LoadGenericWindow(ref mWin, App.MainWindow, windowStyle, mPOM.Name + " Edit Page", this, new ObservableList<Button> { saveButton, undoButton });
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            IsPageSaved = true;
            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mPOM);
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
            // Logic to be added
            mWin.Close();
        }

        private void UndoChangesAndClose()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                mPOM.RestoreFromBackup(true);
                mWin.Close();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        
        private void xRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(xPageUrlRadioBtn.IsChecked))
            {
                mPOM.PageLoadFlow = ApplicationPOMModel.ePageLoadFlowType.PageURL;
                xPageUrlStackPanel.Visibility = Visibility.Visible;
                xFrameBusinessFlowControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                mPOM.PageLoadFlow = ApplicationPOMModel.ePageLoadFlowType.BusinessFlow;
                xPageUrlStackPanel.Visibility = Visibility.Collapsed;
                xFrameBusinessFlowControl.Visibility = Visibility.Visible;
            }
        }
    }
}
