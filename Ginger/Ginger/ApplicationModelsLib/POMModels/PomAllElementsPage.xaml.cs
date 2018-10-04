using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.UserControls;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                    return null;
                }
            }

        }

        public Agent mAgent;

        public enum eElementsContext
        {
            Mapped,
            Unmapped

        }

        public PomElementsPage mappedUIElementsPage;
        public PomElementsPage unmappedUIElementsPage;

        public PomAllElementsPage(ApplicationPOMModel POM)
        {
            InitializeComponent();
            mPOM = POM;
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


        System.Windows.Threading.DispatcherTimer dispatcherTimer = null;

        private void LiveSpyHandler(object sender, RoutedEventArgs e)
        {
            if (mAgent.Driver.IsDriverBusy)
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return;
            }

            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKeys.POMAgentIsNotRunning);
                return;
            }


            if (LiveSpyButton.IsChecked == true)
            {
                xStatusLable.Content = "Spying on";
                if (dispatcherTimer == null)
                {
                    dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(timenow);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                }

                dispatcherTimer.IsEnabled = true;
            }
            else
            {
                xCreateNewElement.Visibility = Visibility.Collapsed;
                xStatusLable.Content = "Spying off";
                dispatcherTimer.IsEnabled = false;
            }
        }

        private void StopSpying()
        {
            xCreateNewElement.Visibility = Visibility.Collapsed;
            xStatusLable.Content = "Spying off";
            dispatcherTimer.IsEnabled = false;
        }

        ElementInfo mSpyElement;

        private void timenow(object sender, EventArgs e)
        {
            // Get control info only if control key is pressed
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                xStatusLable.Content = "Spying Element, Please Wait...";
                xCreateNewElement.Visibility = Visibility.Collapsed;
                GingerCore.General.DoEvents();
                mSpyElement = mWinExplorer.GetControlFromMousePosition();

                mWinExplorer.UpdateElementInfoFields(mSpyElement);
                mSpyElement.Locators = mWinExplorer.GetElementLocators(mSpyElement);
                mSpyElement.Properties = mWinExplorer.GetElementProperties(mSpyElement);
                mSpyElement.WindowExplorer = mWinExplorer;
                mSpyElement.IsAutoLearned = true;

                if (mSpyElement != null)
                {
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
            bool elementfocused = false;
            if (mSpyElement == null)
            {
                return;
            } 
            foreach (ElementInfo EI in mPOM.MappedUIElements)
            {
                ///*mWinExplorer.UpdateElementInfoFields(EI);/*//Not sure if needed

                if (IsTheSameElement(EI, mSpyElement))
                {
                    xMappedElementsTab.Focus();
                    elementfocused = true;
                    mPOM.MappedUIElements.CurrentItem = EI;
                    mappedUIElementsPage.MainElementsGrid.ScrollToViewCurrentItem();
                    break;
                }
            }

            foreach (ElementInfo EI in mPOM.UnMappedUIElements)
            {
                //mWinExplorer.UpdateElementInfoFields(EI);//Not sure if needed

                if (IsTheSameElement(EI, mSpyElement))
                {
                    xUnmappedElementsTab.Focus();
                    elementfocused = true;
                    mPOM.UnMappedUIElements.CurrentItem = EI;
                    unmappedUIElementsPage.MainElementsGrid.ScrollToViewCurrentItem();
                    break;
                }
            }

            if (!elementfocused)
            {
                xStatusLable.Content = "Element has not been found on the list, Click here to create new Element ";
                xCreateNewElement.Visibility = Visibility.Visible;
            }
        }

        private bool IsTheSameElement(ElementInfo firstEI, ElementInfo secondEI)
        {
            bool HasSimilarXpath = firstEI.XPath == secondEI.XPath && firstEI.Path == secondEI.Path;

            bool HasSimilarLocators = true;
            foreach (ElementLocator EL in firstEI.Locators)
            {
                ElementLocator SimilarLocator = secondEI.Locators.Where(x => x.LocateBy == EL.LocateBy && x.LocateValue == EL.LocateValue).FirstOrDefault();
                if (SimilarLocator == null)
                    HasSimilarLocators = false;
            }

            if (HasSimilarXpath && HasSimilarLocators)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void TestAllElementsClicked(object sender, RoutedEventArgs e)
        {

            //Change Grid View
            
            if (mAgent.Driver.IsDriverBusy)
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return;
            }

            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKeys.POMAgentIsNotRunning);
                return;
            }

            mappedUIElementsPage.MainElementsGrid.ChangeGridView(GridViewDef.DefaultViewName);
            TestAllElementsAsync().ConfigureAwait(false);
        }


        public async Task TestAllElementsAsync()
        {
            xTestAllElements.Visibility = Visibility.Collapsed;
            xStopTestAllElements.Visibility = Visibility.Visible;
            mStopProcess = false;
            await Task.Run(() => TestAllElements()).ConfigureAwait(false);
            xTestAllElements.Visibility = Visibility.Visible;
            xStopTestAllElements.Visibility = Visibility.Collapsed;
        }

        private void TestAllElements()
        {
            int TotalElements = mPOM.MappedUIElements.Count;
            int TotalFails = 0;

            bool WarnErrorOccured = false;
            foreach (ElementInfo EI in mPOM.MappedUIElements)
            {
                EI.ElementStatus = ElementInfo.eElementStatus.Pending;
            }

            foreach (ElementInfo EI in mPOM.MappedUIElements)
            {
                if (mStopProcess)
                {
                    return;
                }
                    
                if (mWinExplorer.TestElementLocators(EI.Locators,true))
                {
                    //TODO: Add Error frm locators
                    //EI.ElementStatus = EI.Locators.Where(x=>x.StatusError)
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
                    if (Reporter.ToUser(eUserMsgKeys.POMNotOnThePageWarn, TotalFails, TotalElements) == MessageBoxResult.No)
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



    }
}
