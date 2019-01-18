using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
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
using static Ginger.ApplicationModelsLib.POMModels.PomAllElementsPage;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    /// <summary>
    /// Interaction logic for POMDeltaPage.xaml
    /// </summary>
    public partial class POMDeltaWizardPage : Page, IWizardPage
    {

        PomRelearnWizard mWizard;
        private ePlatformType mAppPlatform;
        ObservableList<ElementInfo> mElementsList = new ObservableList<ElementInfo>();
        ObservableList<ElementInfo> mOriginalList = new ObservableList<ElementInfo>();
        ApplicationPOMModel mPOM;
        PomElementsPage mPomElementsPage = null;
        List<eElementType> mSelectedElementTypesList = new List<eElementType>();

        public POMDeltaWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (PomRelearnWizard)WizardEventArgs.Wizard;
                    mPOM = mWizard.mDuplicatedPOM;
                    mElementsList.CollectionChanged += ElementsListCollectionChanged;
                    InitilizePomElementsMappingPage();
                    mAppPlatform = App.UserProfile.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey);
                    SetAutoMapElementTypes();
                    mPomElementsPage.SetAgent(mWizard.Agent);
                    mPOM.PopulateDuplicatedUnienedElementsList();
                    mOriginalList = mPOM.mCopiedUnienedList;
                    CollectOriginalElementsData();

                    xReLearnButton.Visibility = Visibility.Visible;
                    Learn();
                    break;
            }
        }


        private async void CollectOriginalElementsData()
        {
            await Task.Run(() => mWizard.IWindowExplorerDriver.CollectOriginalElementsDataForDeltaCheck(mOriginalList));
        }


        private async void Learn()
        {
            if (!mWizard.IsLearningWasDone)
            {
                try
                {
                    mWizard.ProcessStarted();
                    xReLearnButton.Visibility = Visibility.Collapsed;
                    xStopLoadButton.ButtonText = "Stop";
                    xStopLoadButton.IsEnabled = true;
                    ((DriverBase)mWizard.Agent.Driver).mStopProcess = false;
                    xStopLoadButton.Visibility = Visibility.Visible;

                    mWizard.IWindowExplorerDriver.UnHighLightElements();

                    await Task.Run(() => WaitUntilDriverWillBeFree());

                    //await Task.Run(() => CollectOriginalElementsData());

                    xPreperingDataLable.Content = "Learning";

                    await Task.Run(() => mWizard.IWindowExplorerDriver.GetVisibleControls(null, mElementsList, true));

                    xPreperingDataLable.Content = "Learning Finished";
                    mWizard.IsLearningWasDone = true;
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKeys.POMWizardFailedToLearnElement, ex.Message);
                    mWizard.IsLearningWasDone = false;
                }
                finally
                {
                    xStopLoadButton.Visibility = Visibility.Collapsed;
                    xReLearnButton.Visibility = Visibility.Visible;
                    mWizard.ProcessEnded();
                }

            }
        }

        private void WaitUntilDriverWillBeFree()
        {
            while (mWizard.Agent.Driver.IsDriverBusy)
            {
                Thread.Sleep(2000);
            }
        }

        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            xStopLoadButton.ButtonText = "Stopping...";
            xStopLoadButton.IsEnabled = false;
            ((DriverBase)mWizard.Agent.Driver).mStopProcess = true;
        }


        private void ReLearnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.POMWizardReLearnWillDeleteAllElements) == Amdocs.Ginger.Common.MessageBoxResult.Yes)
            {
                mWizard.IsLearningWasDone = false;
                Learn();
            }
        }

        private void InitilizePomElementsMappingPage()
        {
            if (mPomElementsPage == null)
            {
                mPomElementsPage = new PomElementsPage(mPOM,eElementsContext.AllDeltaElements);
                xPomElementsMappingPageFrame.Content = mPomElementsPage;
            }
        }

        private void SetAutoMapElementTypes()
        {
            if (mSelectedElementTypesList.Count == 0)
            {
                switch (mAppPlatform)
                {
                    case ePlatformType.Web:
                        foreach (PlatformInfoBase.ElementTypeData elementTypeOperation in new WebPlatform().GetPlatformElementTypesData().ToList())
                        {
                            if(elementTypeOperation.IsCommonElementType)
                                 mSelectedElementTypesList.Add(elementTypeOperation.ElementType);
                        }
                        break;
                }
            }
        }

        ObservableList<ElementInfo> mDeltaNewElementsList = new ObservableList<ElementInfo>();

        private void ElementsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                

                ElementInfo latestElementInfo = ((ObservableList<ElementInfo>)sender).Last();
                ElementInfo originalElementInfo = mWizard.IWindowExplorerDriver.GetMatchingElement(latestElementInfo, mOriginalList);

                if (originalElementInfo == null)
                {
                    latestElementInfo.DeltaStatus = ElementInfo.eDeltaStatus.New;
                    mDeltaNewElementsList.Add(latestElementInfo);
                }
                else
                {
                    SetElementDelta(originalElementInfo, latestElementInfo);
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "POM: Learned Element Info from type was failed to be added to Page Elements", ex);
            }

        }

        private void SetElementDelta(ElementInfo originalElelemnt, ElementInfo latestElement)
        {
            foreach (ElementLocator originalEL in originalElelemnt.Locators)
            {
                ElementLocator latestEL = latestElement.Locators.Where(x => x.LocateBy == originalEL.LocateBy && x.LocateValue == originalEL.LocateValue).FirstOrDefault();
                if (latestEL == null)
                {
                    originalEL.DeltaStatus = ElementInfo.eDeltaStatus.New;
                }
                else if (originalEL.IsAutoLearned = true && originalEL.LocateBy == latestEL.LocateBy && originalEL.LocateValue != latestEL.LocateValue)
                {
                    originalEL.DeltaStatus = ElementInfo.eDeltaStatus.Modified;
                }
                else
                {
                    originalEL.DeltaStatus = ElementInfo.eDeltaStatus.Equal;
                }
            }

            foreach (ControlProperty originalCP in originalElelemnt.Properties)
            {
                ControlProperty latestCP = latestElement.Properties.Where(x => x.Name == originalCP.Name && x.Value == originalCP.Value).FirstOrDefault();
                if (latestCP == null)
                {
                    originalCP.DeltaStatus = ElementInfo.eDeltaStatus.New;
                }
                else if (originalCP.Name == latestCP.Name && originalCP.Value != latestCP.Value)
                {
                    originalCP.DeltaStatus = ElementInfo.eDeltaStatus.Modified;
                }
                else
                {
                    originalCP.DeltaStatus = ElementInfo.eDeltaStatus.Equal;
                }
            }

            List<ElementLocator> ModifiedElementsLocatorsList = originalElelemnt.Locators.Where(x => x.DeltaStatus != ElementInfo.eDeltaStatus.Equal).ToList();
            List<ControlProperty> ModifiedControlPropertiesList = originalElelemnt.Properties.Where(x => x.DeltaStatus != ElementInfo.eDeltaStatus.Equal).ToList();
            if (ModifiedElementsLocatorsList.Count > 0 || ModifiedControlPropertiesList.Count > 0)
            {
                originalElelemnt.DeltaStatus = ElementInfo.eDeltaStatus.Modified;
                originalElelemnt.Update = true;
            }

        }
    }
}
