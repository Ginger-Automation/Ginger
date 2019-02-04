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
            if (WizardEventArgs.EventType == EventType.Init)
            {
                mWizard = (PomRelearnWizard)WizardEventArgs.Wizard;
                mPOM = mWizard.mOriginalPOM;
                mElementsList.CollectionChanged += ElementsListCollectionChanged;
                InitilizePomElementsMappingPage();
                mAppPlatform = WorkSpace.UserProfile.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey);
                SetAutoMapElementTypes();
                mPomElementsPage.SetAgent(mWizard.Agent);
                PopulateDuplicatedUnienedElementsList();
                mOriginalList = mWizard.CopiedUnienedList;
                xReLearnButton.Visibility = Visibility.Visible;
                Learn();
            }
        }

        public void PopulateDuplicatedUnienedElementsList()
        {
            mWizard.CopiedUnienedList.Clear();
            foreach (ElementInfo EI in mPOM.MappedUIElements)
            {
                ElementInfo DuplicatedEI = (ElementInfo)EI.CreateCopy(false);
                DuplicatedEI.ElementGroup = ApplicationPOMModel.eElementGroup.Mapped;
                CorrectControlPropertyTypes(DuplicatedEI);
                mWizard.CopiedUnienedList.Add(DuplicatedEI);
            }
            foreach (ElementInfo EI in mPOM.UnMappedUIElements)
            {
                ElementInfo DuplicatedEI = (ElementInfo)EI.CreateCopy(false);
                DuplicatedEI.ElementGroup = ApplicationPOMModel.eElementGroup.Unmapped;
                CorrectControlPropertyTypes(DuplicatedEI);
                mWizard.CopiedUnienedList.Add(DuplicatedEI);
            }
        }

        private void CorrectControlPropertyTypes(ElementInfo EI)
        {
            ObservableList<ControlProperty> newProperties = new ObservableList<ControlProperty>();

            foreach (ControlProperty property in EI.Properties)
            {
                if (property is HTMLElementProperty)
                {
                    return;
                }
                else
                {
                    HTMLElementProperty hTMLElementProperety = new HTMLElementProperty() { Name = property.Name, Value = property.Value };
                    newProperties.Add(hTMLElementProperety);
                }
            }

            EI.Properties = newProperties;
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

                    await Task.Run(() => CollectOriginalElementsData());

                    await Task.Run(() => mWizard.IWindowExplorerDriver.GetVisibleControls(null, mElementsList, true));

                    mWizard.IsLearningWasDone = true;
                    mPomElementsPage.DoEndOfRelearnElementsSorting();
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.POMWizardFailedToLearnElement, ex.Message);
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

        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            xStopLoadButton.ButtonText = "Stopping...";
            xStopLoadButton.IsEnabled = false;
            ((DriverBase)mWizard.Agent.Driver).mStopProcess = true;
        }


        private void ReLearnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.POMDeltaWizardReLearnWillEraseModification) == eUserMsgSelection.Yes)
            {
                mWizard.IsLearningWasDone = false;
                Learn();
            }
        }

        private void InitilizePomElementsMappingPage()
        {
            if (mPomElementsPage == null)
            {
                mPomElementsPage = new PomElementsPage(mPOM,eElementsContext.AllDeltaElements,mWizard.CopiedUnienedList);
                xPomElementsMappingPageFrame.Content = mPomElementsPage;
            }
        }

        private void SetAutoMapElementTypes()
        {
            if (mSelectedElementTypesList.Count == 0)
            {

                if (mAppPlatform == ePlatformType.Web)
                {
                    foreach (PlatformInfoBase.ElementTypeData elementTypeOperation in new WebPlatform().GetPlatformElementTypesData().ToList())
                    {
                        if (elementTypeOperation.IsCommonElementType)
                        {
                            mSelectedElementTypesList.Add(elementTypeOperation.ElementType);
                        }
                    }
                }
            }
        }


        private void ElementsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                

                ElementInfo latestElementInfo = ((ObservableList<ElementInfo>)sender).Last();
                ElementInfo originalElementInfo = mWizard.IWindowExplorerDriver.GetMatchingElement(latestElementInfo, mOriginalList);

                if (originalElementInfo == null)
                {
                    latestElementInfo.ElementStatus = ElementInfo.eElementStatus.Passed;
                    latestElementInfo.DeltaStatus = ElementInfo.eDeltaStatus.New;
                    latestElementInfo.ElementGroup = ApplicationPOMModel.eElementGroup.Mapped;
                    latestElementInfo.IsSelected = true;
                    mWizard.CopiedUnienedList.Add(latestElementInfo);
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
            List<ElementLocator> originalLocatorsList = originalElelemnt.Locators.ToList();

            foreach (ElementLocator latestEL in latestElement.Locators)
            {
               
                ElementLocator originalModifiedEL = originalLocatorsList.Where(x => x.LocateBy == latestEL.LocateBy && x.LocateValue != latestEL.LocateValue).FirstOrDefault();
                ElementLocator ExistingEL = originalLocatorsList.Where(x => x.IsAutoLearned == true && x.LocateBy == latestEL.LocateBy).FirstOrDefault();
                if (originalModifiedEL != null)
                {
                    originalModifiedEL.DeltaStatus = ElementInfo.eDeltaStatus.Modified;
                    originalModifiedEL.DeltaExtraDetails = "Locate value changed to: " + latestEL.LocateValue;
                    originalModifiedEL.UpdatedValue = latestEL.LocateValue;
                }
                else if (ExistingEL == null)
                {
                    latestEL.DeltaStatus = ElementInfo.eDeltaStatus.New;
                    originalElelemnt.Locators.Add(latestEL);
                }
                else
                {
                    ExistingEL.DeltaStatus = ElementInfo.eDeltaStatus.Unchanged;
                }
            }

            foreach (ElementLocator originalEL in originalLocatorsList)
            {
                if (originalEL.IsAutoLearned)
                {
                    ElementLocator latestExistedEL = originalLocatorsList.Where(x => x.LocateBy == originalEL.LocateBy && x.LocateValue == originalEL.LocateValue).FirstOrDefault();
                    if (latestExistedEL == null)
                    {
                        originalEL.DeltaStatus = ElementInfo.eDeltaStatus.Deleted;
                    }
                }
            }

            List<ControlProperty> originalPropertiesList = originalElelemnt.Properties.ToList();

            foreach (ControlProperty latestCP in latestElement.Properties)
            {
                ControlProperty originalModifiedCP = originalPropertiesList.Where(x => x.Name == latestCP.Name && ( x.Value != latestCP.Value) && !(string.IsNullOrEmpty(x.Value) && string.IsNullOrEmpty(latestCP.Value))).FirstOrDefault();
                ControlProperty ExistingCP = originalElelemnt.Properties.Where(x => x.Name == latestCP.Name).FirstOrDefault();
                if (originalModifiedCP != null)
                {
                    ((HTMLElementProperty)originalModifiedCP).DeltaStatus = ElementInfo.eDeltaStatus.Modified;
                    ((HTMLElementProperty)originalModifiedCP).DeltaExtraDetails = "Property value changed to: " + latestCP.Value;
                    ((HTMLElementProperty)originalModifiedCP).UpdatedValue = latestCP.Value;
                }
                else if (ExistingCP == null)
                {
                    ((HTMLElementProperty)latestCP).DeltaStatus = ElementInfo.eDeltaStatus.New;
                    originalElelemnt.Properties.Add(latestCP);
                }
                else
                {

                    ((HTMLElementProperty)ExistingCP).DeltaStatus = ElementInfo.eDeltaStatus.Unchanged;
                }
            }

            foreach (ControlProperty originalEL in originalPropertiesList)
            {
                ControlProperty latestExistedEL = originalPropertiesList.Where(x => x.Name == originalEL.Name && x.Value == originalEL.Value).FirstOrDefault();

                if (latestExistedEL == null)
                {
                    ((HTMLElementProperty)originalEL).DeltaStatus = ElementInfo.eDeltaStatus.Deleted;
                }
            }


            List<ElementLocator> ModifiedElementsLocatorsList = originalElelemnt.Locators.Where(x => x.DeltaStatus != ElementInfo.eDeltaStatus.Unchanged).ToList();
            List<ControlProperty> ModifiedControlPropertiesList = originalElelemnt.Properties.Where(x => ((HTMLElementProperty)x).DeltaStatus != ElementInfo.eDeltaStatus.Unchanged).ToList();
            if (ModifiedElementsLocatorsList.Count > 0 && ModifiedControlPropertiesList.Count > 0)
            {
                originalElelemnt.DeltaStatus = ElementInfo.eDeltaStatus.Modified;
                originalElelemnt.IsSelected = true;
                originalElelemnt.DeltaExtraDetails = ElementInfo.eDeltaExtraDetails.LocatorsAndPropertiesChanged;
            }
            else if (ModifiedElementsLocatorsList.Count > 0)
            {
                originalElelemnt.DeltaStatus = ElementInfo.eDeltaStatus.Modified;
                originalElelemnt.IsSelected = true;
                originalElelemnt.DeltaExtraDetails = ElementInfo.eDeltaExtraDetails.LocatorsChanged;
            }
            else if (ModifiedControlPropertiesList.Count > 0)
            {
                originalElelemnt.DeltaStatus = ElementInfo.eDeltaStatus.Modified;
                originalElelemnt.IsSelected = true;
                originalElelemnt.DeltaExtraDetails = ElementInfo.eDeltaExtraDetails.PropertiesChanged;

            }
        }
    }
}
