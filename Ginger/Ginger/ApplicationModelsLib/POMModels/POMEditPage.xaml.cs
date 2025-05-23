#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.Agents;
using Ginger.BusinessFlowWindows;
using Ginger.Repository;
using Ginger.UserControlsLib;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.VisualTesting;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Ginger.General;

namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for POMEditPage.xaml
    /// </summary>
    public partial class POMEditPage : GingerUIPage
    {
        ApplicationPOMModel mPOM;
        ScreenShotViewPage mScreenShotViewPage;
        ActivitiesRepositoryPage mActivitiesRepositoryViewPage;
        GenericWindow mWin;
        public bool IsPageSaved = false;
        public eRIPageViewMode mEditMode { get; set; }
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
                mPomAllElementsPage?.SetAgent(mAgent);
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
                if (mAgent != null && ((AgentOperations)mAgent.AgentOperations).Status == Agent.eStatus.Running)
                {
                    return ((AgentOperations)mAgent.AgentOperations).Driver as IWindowExplorer;
                }
                else
                {
                    if (mAgent != null)
                    {
                        mAgent.AgentOperations.Close();
                    }
                    return null;
                }
            }
        }

        // ScreenShotViewPage pd;

        readonly PomAllElementsPage mPomAllElementsPage;
        ePlatformType mAppPlatform;
        public POMEditPage(ApplicationPOMModel POM, eRIPageViewMode editMode, bool ignoreValidationRules = false)
        {
            InitializeComponent();
            mPOM = POM;
            CurrentItemToSave = mPOM;
            mEditMode = editMode;

            mBusinessFlowControl = new ucBusinessFlowMap(mPOM, nameof(mPOM.MappedBusinessFlow));
            xFrameBusinessFlowControl.Content = mBusinessFlowControl;

            xShowIDUC.Init(mPOM);
            xFirstLabel.Text = mPOM.Name;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xNameTextBox, TextBox.TextProperty, mPOM, nameof(mPOM.Name));
            if (!ignoreValidationRules)
            {
                xNameTextBox.AddValidationRule(new AddEditPOMWizardLib.POMNameValidationRule());
            }
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xDescriptionTextBox, TextBox.TextProperty, mPOM, nameof(mPOM.Description));
            xPageURLTextBox.Init(null, mPOM, nameof(mPOM.PageURL));

            xTAlabel.Content = $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}:";

            mAppPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(POM.TargetApplicationKey);

            mPomAllElementsPage = new PomAllElementsPage(mPOM, PomAllElementsPage.eAllElementsPageContext.POMEditPage, editMode: mEditMode);
            xUIElementsFrame.ClearAndSetContent(mPomAllElementsPage);
            mPomAllElementsPage.raiseUIElementsCountUpdated += UIElementCountUpdatedHandler;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAgentControlUC, ucAgentControl.SelectedAgentProperty, this, nameof(Agent));
            ObservableList<Agent> optionalAgentsList = SupportedAgents();
            foreach (Agent agent in optionalAgentsList)
            {
                agent.AgentOperations ??= new AgentOperations(agent);
            }
            xAgentControlUC.Init(optionalAgentsList, mPOM.LastUsedAgent);

            FillTargetAppsComboBox();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xTargetApplicationComboBox, ComboBox.SelectedValueProperty, mPOM, nameof(ApplicationPOMModel.TargetApplicationKey));
            xTagsViewer.Init(mPOM.TagsKeys);
            BitmapSource source = null;
            if (mPOM.ScreenShotImage != null)
            {
                source = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(mPOM.ScreenShotImage.ToString()));
            }
            mScreenShotViewPage = new ScreenShotViewPage(mPOM.Name, source, ImageMaxHeight: 550, ImageMaxWidth: 750);
            xScreenShotFrame.ClearAndSetContent(mScreenShotViewPage);

            UIElementTabTextBlockUpdate();
            SetDefaultPage();

            if (mEditMode is eRIPageViewMode.View or eRIPageViewMode.ViewAndExecute)
            {
                xScreenshotOperationBtns.IsEnabled = false;
            }
            else
            {
                xScreenshotOperationBtns.IsEnabled = true;
            }
            SetIconImageType();
        }

        private void SetDefaultPage()
        {
            if (mPOM.PageLoadFlow == ApplicationPOMModel.ePageLoadFlowType.PageURL)
            {
                xPageUrlRadioBtn.IsChecked = true;

                mAppPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey);

                PlatformInfoBase platformInfoBase = PlatformInfoBase.GetPlatformImpl(mAppPlatform);
                if (platformInfoBase != null)
                {
                    xPageUrlRadioBtn.Content = platformInfoBase.GetPageUrlRadioLabelText();
                    xPageURLBtn.ToolTip = platformInfoBase.GetNextBtnToolTip();
                }
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
                RepositoryItemKey key = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.Guid == mPOM.TargetApplicationKey.Guid).Select(x => x.Key).FirstOrDefault();
                if (key != null)
                {
                    mPOM.TargetApplicationKey = key;
                }
                else if (mPOM.TargetApplicationKey.ItemName != null && key == null)//if POM is imported/copied from other solution
                {
                    var platform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey);
                    if (platform != ePlatformType.NA)
                    {
                        mPOM.TargetApplicationKey = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.Platform == platform).Select(x => x.Key).FirstOrDefault();
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.MissingTargetApplication, "The mapped " + mPOM.Key.ItemName + " Target Application was not found, please select new Target Application");
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.MissingTargetApplication, "The mapped " + mPOM.Key.ItemName + " Target Application was not found, please select new Target Application");
                }
            }
            xTargetApplicationComboBox.ItemsSource = null;
            SupportedTargetApplication();
            CollectionChangedEventManager.AddHandler(source: WorkSpace.Instance.Solution.ApplicationPlatforms, handler: ApplicationPlatforms_CollectionChanged);

        }
        /// <summary>
        /// Sets the supported target applications for the current POM based on the agent's configuration and platform type.
        /// </summary>
        private void SupportedTargetApplication()
        {
            var targetPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey);

            if (targetPlatform != ePlatformType.NA)
            {
                xTargetApplicationComboBox.ItemsSource = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.Platform == targetPlatform).ToList();
            }
            else
            {
                xTargetApplicationComboBox.ItemsSource = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList();
            }
            xTargetApplicationComboBox.SelectedValuePath = nameof(ApplicationPlatform.Key);
            xTargetApplicationComboBox.DisplayMemberPath = nameof(ApplicationPlatform.AppName);
        }

        /// <summary>
        /// Returns a list of supported agents based on the application platform and agent type.
        /// </summary>
        /// <returns>ObservableList of supported agents.</returns>
        private ObservableList<Agent> SupportedAgents()
        {
            ObservableList<Agent> optionalAgentsList;

            var agents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();

            if (mAppPlatform == ePlatformType.Web)
            {
                List<Agent> list = agents.Where(x => x.Platform == ePlatformType.Web).ToList();

                foreach (var agent in agents.Where(a => a.Platform == ePlatformType.Mobile))
                {
                    var appType = agent.DriverConfiguration.FirstOrDefault(p => string.Equals(p.Parameter, "AppType", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;

                    if (string.Equals(appType, nameof(eAppType.Web), StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(agent);
                    }
                }

                optionalAgentsList = GingerCore.General.ConvertListToObservableList(list);
            }
            else
            {
                optionalAgentsList = GingerCore.General.ConvertListToObservableList(agents.Where(x => x.Platform == mAppPlatform).ToList());
            }

            return optionalAgentsList;
        }


        private void ApplicationPlatforms_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SupportedTargetApplication();
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

        private void UIElementCountUpdatedHandler(object sender, EventArgs e)
        {
            UIElementTabTextBlockUpdate();
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
            Bitmap ScreenShotBitmap = ((IVisualTestingDriver)((AgentOperations)mAgent.AgentOperations).Driver).GetScreenShot(new Tuple<int, int>(ApplicationPOMModel.cLearnScreenWidth, ApplicationPOMModel.cLearnScreenHeight));
            mPOM.ScreenShotImage = Ginger.General.BitmapToBase64(ScreenShotBitmap);
            mScreenShotViewPage = new ScreenShotViewPage(mPOM.Name, ScreenShotBitmap);
            xScreenShotFrame.ClearAndSetContent(mScreenShotViewPage);
        }

        private void BrowseImageButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
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
                                BitmapImage bi = new BitmapImage(new Uri(op.FileName));
                                Bitmap ScreenShotBitmap = Ginger.General.BitmapImage2Bitmap(bi);
                                mPOM.ScreenShotImage = Ginger.General.BitmapToBase64(ScreenShotBitmap);
                                mScreenShotViewPage = new ScreenShotViewPage(mPOM.Name, ScreenShotBitmap);
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

            var VE = new ValueExpression(WorkSpace.Instance.GetRecentEnvironment(), new Context(), null);
            GoToPage(VE.Calculate(mPOM.PageURL));
        }

        private void GoToPage(string calculatedValue)
        {
            Act act = null;
            switch (mAppPlatform)
            {
                case ePlatformType.Web:
                case ePlatformType.Mobile:
                    act = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = calculatedValue, ValueForDriver = calculatedValue, Active = true };
                    break;
                case ePlatformType.Java:
                    act = new ActSwitchWindow { LocateBy = eLocateBy.ByTitle, Value = calculatedValue, ValueForDriver = calculatedValue, Active = true, WaitTime = 5 };
                    break;
                case ePlatformType.Windows:
                    act = new ActSwitchWindow { LocateBy = eLocateBy.ByTitle, LocateValue = calculatedValue, LocateValueCalculated = calculatedValue, Active = true, WaitTime = 5 };
                    break;
            }
            if (act != null)
            {
                ((AgentOperations)mAgent.AgentOperations).Driver.RunAction(act);

                if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && act.Error.Contains("not support"))
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Navigating to Native Application not supported");
                }
                else if (!string.IsNullOrEmpty(act.Error))
                {
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, act.Error);
                }
            }
        }

        private void AgentStartedHandler()
        {
            if (string.IsNullOrEmpty(mPOM.PageURL) == false && mPOM.PageLoadFlow == ApplicationPOMModel.ePageLoadFlowType.PageURL)
            {
                GoToPageURL();
            }
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
                        {
                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (xPomTabs.SelectedItem == tab)
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                }
                                else
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$PrimaryColor_Black");
                                } ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
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

            Button saveButton = new Button
            {
                Content = "Save"
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: saveButton, eventName: nameof(ButtonBase.Click), handler: SaveButton_Click);


            Button undoButton = new Button
            {
                Content = "Undo & Close"
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: undoButton, eventName: nameof(ButtonBase.Click), handler: UndoButton_Click);

            // creating this event handler for stopping the spy
            RoutedEventHandler closeButtonHandler = (_, e) =>
            {
                mPomAllElementsPage.StopSpying();
                mWin.Close();
            };

            this.Height = 800;
            this.Width = 800;
            GingerCore.General.LoadGenericWindow(ref mWin, App.MainWindow, windowStyle, mPOM.Name + " Edit Page", this, [saveButton, undoButton], closeEventHandler: closeButtonHandler);
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
                mPomAllElementsPage.StopSpying();
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

        private void xEditPageExpander_Expanded(object sender, RoutedEventArgs e)
        {
            FirstRow.Height = new GridLength(170, GridUnitType.Pixel);
            SecondRow.Height = new GridLength(100, GridUnitType.Star);
        }

        private void xEditPageExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            FirstRow.Height = new GridLength(6, GridUnitType.Star);
        }

        /// <summary>
        /// Sets the icon image type based on the POM model's item image type.
        /// Ensures the update happens on the UI thread using the dispatcher.
        /// </summary>
        private void SetIconImageType()
        {
            Dispatcher.Invoke(() =>
            {
                xIconImage.ImageType = mPOM.ItemImageType;
            });
        }
    }
}
