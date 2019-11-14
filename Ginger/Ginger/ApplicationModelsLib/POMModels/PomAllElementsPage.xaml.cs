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
using Ginger.ApplicationModelsLib.POMModels.POMWizardLib;
using Ginger.UserControls;
using GingerCore;
using GingerWPF.WizardLib;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for PomAllElementsPage.xaml
    /// </summary>
    public partial class PomAllElementsPage : Page
    {
        ApplicationPOMModel mPOM;
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

        public Agent mAgent;

        public enum eAllElementsPageContext
        {
            POMEditPage,
            AddPOMWizard
        }

        public eAllElementsPageContext mContext;

        public PomElementsPage mappedUIElementsPage;
        public PomElementsPage unmappedUIElementsPage;


        public PomAllElementsPage(ApplicationPOMModel POM, eAllElementsPageContext context)
        {
            InitializeComponent();
            mPOM = POM;
            mContext = context;

            if (mContext == eAllElementsPageContext.AddPOMWizard)
            {
                xReLearnElements.Visibility = Visibility.Collapsed;
            }

            mPOM.MappedUIElements.CollectionChanged += MappedUIElements_CollectionChanged;
            mPOM.UnMappedUIElements.CollectionChanged += UnMappedUIElements_CollectionChanged;

            mappedUIElementsPage = new PomElementsPage(mPOM, eElementsContext.Mapped);
            xMappedElementsFrame.Content = mappedUIElementsPage;

            unmappedUIElementsPage = new PomElementsPage(mPOM, eElementsContext.Unmapped);
            xUnMappedElementsFrame.Content = unmappedUIElementsPage;

            UnMappedUIElementsUpdate();
            MappedUIElementsUpdate();
        }

        private void UnMappedUIElements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UnMappedUIElementsUpdate();
        }        

        private void UnMappedUIElementsUpdate()
        {
            Dispatcher.Invoke(() =>
            {
                xUnMappedElementsTextBlock.Text = string.Format("Unmapped Elements ({0})", mPOM.UnMappedUIElements.Count);
            });
        }

        private void MappedUIElements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MappedUIElementsUpdate();
        }

        public Visibility ShowTestAllElementsButton
        {
            get { return xTestAllElements.Visibility; }
            set { xTestAllElements.Visibility = value; }
        }

        private void MappedUIElementsUpdate()
        {
            Dispatcher.Invoke(() =>
            {
                xMappedElementsTextBlock.Text = string.Format("Mapped Elements ({0})", mPOM.MappedUIElements.Count);
            });
        }

        public void SetAgent(Agent agent)
        {
            mAgent = agent;
            mappedUIElementsPage.SetAgent(mAgent);
            unmappedUIElementsPage.SetAgent(mAgent);
        }

        private void CreateNewElemetClicked(object sender, RoutedEventArgs e)
        {
            mSpyElement.IsAutoLearned = false;
            mPOM.MappedUIElements.Add(mSpyElement);
            mPOM.MappedUIElements.CurrentItem = mSpyElement;
            mappedUIElementsPage.MainElementsGrid.ScrollToViewCurrentItem();
            xCreateNewElement.Visibility = Visibility.Collapsed;
            xStatusLable.Content = "Element added to the list";
        }


        System.Windows.Threading.DispatcherTimer mDispatcherTimer = null;

        public void StopSpy()
        {
            if (mDispatcherTimer != null)
            {
                mDispatcherTimer.IsEnabled = false;
            }
        }

        private void LiveSpyHandler(object sender, RoutedEventArgs e)
        {
            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKey.POMAgentIsNotRunning);
                LiveSpyButton.IsChecked = false;
                return;
            }

            if (mAgent.Driver.IsDriverBusy)
            {
                Reporter.ToUser(eUserMsgKey.POMDriverIsBusy);
                LiveSpyButton.IsChecked = false;
                return;
            }

            if (LiveSpyButton.IsChecked == true)
            {
                mWinExplorer.StartSpying();
                xStatusLable.Content = "Spying is On";
                if (mDispatcherTimer == null)
                {
                    mDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    mDispatcherTimer.Tick += new EventHandler(timenow);
                    mDispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                }

                mDispatcherTimer.IsEnabled = true;
            }
            else
            {
                xCreateNewElement.Visibility = Visibility.Collapsed;
                xStatusLable.Content = "Spying is Off";
                mDispatcherTimer.IsEnabled = false;
            }
        }

        private void StopSpying()
        {
            xCreateNewElement.Visibility = Visibility.Collapsed;
            xStatusLable.Content = "Spying is Off";
            mDispatcherTimer.IsEnabled = false;
        }

        ElementInfo mSpyElement;

        private void timenow(object sender, EventArgs e)
        {
            // Get control info only if control key is pressed
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                xStatusLable.Content = "Spying element, please wait...";
                xCreateNewElement.Visibility = Visibility.Collapsed;
                GingerCore.General.DoEvents();
                mSpyElement = mWinExplorer.GetControlFromMousePosition();
                if (mSpyElement != null)
                {

                    mSpyElement.WindowExplorer = mWinExplorer;
                    mSpyElement.IsAutoLearned = true;
                    xStatusLable.Content = "Element found";
                    FocusSpyItemOnElementsGrid();
                    mWinExplorer.HighLightElement(mSpyElement);
                }
                else
                {
                    xStatusLable.Content = "Failed to spy element.";
                    GingerCore.General.DoEvents();
                }
            }
        }

        private void FocusSpyItemOnElementsGrid()
        {
            if (mSpyElement == null)
            {
                return;
            }

            ElementInfo matchingOriginalElement = (ElementInfo)mWinExplorer.GetMatchingElement(mSpyElement, mPOM.GetUnifiedElementsList());

            if (matchingOriginalElement == null)
            {
                mWinExplorer.LearnElementInfoDetails(mSpyElement);
                matchingOriginalElement = (ElementInfo)mWinExplorer.GetMatchingElement(mSpyElement, mPOM.GetUnifiedElementsList());
            }


            if (mPOM.MappedUIElements.Contains(matchingOriginalElement))
            {
                xMappedElementsTab.Focus();
                mPOM.MappedUIElements.CurrentItem = matchingOriginalElement;
                mappedUIElementsPage.MainElementsGrid.ScrollToViewCurrentItem();
                return;
            }

            if (mPOM.UnMappedUIElements.Contains(matchingOriginalElement))
            {
                xUnmappedElementsTab.Focus();
                mPOM.UnMappedUIElements.CurrentItem = matchingOriginalElement;
                unmappedUIElementsPage.MainElementsGrid.ScrollToViewCurrentItem();
                return;
            }

            xStatusLable.Content = "Found element is not included in below elements list, click here to add it ";
            xCreateNewElement.Visibility = Visibility.Visible;
        }

        private void TestAllElementsClicked(object sender, RoutedEventArgs e)
        {
            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKey.POMAgentIsNotRunning);
                return;
            }

            if (mAgent != null && mAgent.Driver.IsDriverBusy)
            {
                Reporter.ToUser(eUserMsgKey.POMDriverIsBusy);
                return;
            }

            if (xMappedElementsTab.IsSelected)
            {
                mappedUIElementsPage.MainElementsGrid.ChangeGridView(GridViewDef.DefaultViewName);
            }
            else if (xUnmappedElementsTab.IsSelected)
            {
                unmappedUIElementsPage.MainElementsGrid.ChangeGridView(GridViewDef.DefaultViewName);
            }
            
            TestAllElementsAsync();
        }


        public async void TestAllElementsAsync()
        {
            xTestAllElements.Visibility = Visibility.Collapsed;
            xStopTestAllElements.Visibility = Visibility.Visible;
            mStopProcess = false;

            if (xMappedElementsTab.IsSelected)
            {
                await Task.Run(() => TestAllElements(mPOM.MappedUIElements));
            }
            else if (xUnmappedElementsTab.IsSelected)
            {
                await Task.Run(() => TestAllElements(mPOM.UnMappedUIElements));
            }

            xTestAllElements.Visibility = Visibility.Visible;
            xStopTestAllElements.Visibility = Visibility.Collapsed;
        }

        private void TestAllElements(ObservableList<ElementInfo> Elements)
        {
            int TotalElements = Elements.Count;
            int TotalFails = 0;
            
            bool WarnErrorOccured = false;
            foreach (ElementInfo EI in Elements)
            {
                EI.ElementStatus = ElementInfo.eElementStatus.Pending;
            }

            foreach (ElementInfo EI in Elements)
            {
                if (mStopProcess)
                {
                    return;
                }

                if (mWinExplorer.TestElementLocators(EI,true))
                {
                    EI.ElementStatus = ElementInfo.eElementStatus.Passed;
                }
                else
                {
                    TotalFails++;
                    EI.ElementStatus = ElementInfo.eElementStatus.Failed;
                }

                if (!WarnErrorOccured && ((double)TotalFails / TotalElements) > 0.2)
                {
                    WarnErrorOccured = true;
                    if (Reporter.ToUser(eUserMsgKey.POMNotOnThePageWarn, TotalFails, TotalElements) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                    {
                        return;
                    }
                }
            }
        }

        private bool mStopProcess = false;

        private void StopTestAllElementsClicked(object sender, RoutedEventArgs e)
        {
            mStopProcess = true;
            xStopTestAllElements.Visibility = Visibility.Collapsed;
        }

        public void FinishEditInAllGrids()
        {
            mappedUIElementsPage.FinishEditInGrids();
            unmappedUIElementsPage.FinishEditInGrids();
        }

        private void POMModelTabsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xMappedElementsTab.IsSelected)
            {
                xTestAllElements.ButtonText = "Test All Mapped Elements";
            }
            else if (xUnmappedElementsTab.IsSelected)
            {
                xTestAllElements.ButtonText = "Test All Unmapped Elements";
            }

            //set the selected tab text style
            try
            {
                if (xPOMModelTabs.SelectedItem != null)
                {
                    foreach (TabItem tab in xPOMModelTabs.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (xPOMModelTabs.SelectedItem == tab)
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
                Reporter.ToLog(eLogLevel.ERROR, "Error in POM All Elements Page tabs style", ex);
            }
        }

        private void ReLearnClicked(object sender, RoutedEventArgs e)
        {
            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKey.POMAgentIsNotRunning);
                return;
            }

            if (mAgent.Driver.IsDriverBusy)
            {
                Reporter.ToUser(eUserMsgKey.POMDriverIsBusy);
                return;
            }


            WizardWindow.ShowWizard(new PomDeltaWizard(mPOM, mAgent),1600,800, true);
        }



    }
}
