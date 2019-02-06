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
    public partial class PomDeltaElementComparePage : Page, IWizardPage
    {
        PomDeltaWizard mWizard;
        PomElementsPage mPomElementsPage = null;       

        public PomDeltaElementComparePage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            if (WizardEventArgs.EventType == EventType.Init)
            {
                mWizard = (PomDeltaWizard)WizardEventArgs.Wizard;
                mWizard.mPOMLatestElements.CollectionChanged += ElementsListCollectionChanged;
                InitilizePomElementsMappingPage();
                mPomElementsPage.SetAgent(mWizard.Agent);
                xReLearnButton.Visibility = Visibility.Visible;
                Learn();
            }
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

                    await Task.Run(() => GetUnifiedPomElementsListForDeltaUse());
                    await Task.Run(() => mWizard.IWindowExplorerDriver.CollectOriginalElementsDataForDeltaCheck(mWizard.mPOMCurrentElements));
                    await Task.Run(() => mWizard.IWindowExplorerDriver.GetVisibleControls(null, mWizard.mPOMLatestElements, true));

                    SetDeletedElementDeltaDetails();
                    DoEndOfRelearnElementsSorting();
                    mWizard.IsLearningWasDone = true;
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

        public void GetUnifiedPomElementsListForDeltaUse()
        {
            mWizard.mPOMCurrentElements.Clear();
            //TODO: check if should be done using Parallel.ForEach
            foreach (ElementInfo mappedElement in mWizard.mPOM.MappedUIElements)
            {
                DeltaElementInfo deltaElement = ConvertElementInfoToDelta(mappedElement);
                deltaElement.ElementGroup = ApplicationPOMModel.eElementGroup.Mapped;
                mWizard.mPOMCurrentElements.Add(deltaElement);
            }
            foreach (ElementInfo unmappedElement in mWizard.mPOM.UnMappedUIElements)
            {
                DeltaElementInfo deltaElement = ConvertElementInfoToDelta(unmappedElement);
                deltaElement.ElementGroup = ApplicationPOMModel.eElementGroup.Unmapped;
                mWizard.mPOMCurrentElements.Add(deltaElement);
            }
        }

        private DeltaElementInfo ConvertElementInfoToDelta(ElementInfo element)
        {
            //copy element and convert it to Delta
            DeltaElementInfo deltaElement = (DeltaElementInfo)element.CreateCopy(false);//keeping original GUI            
            //convert Locators to Delta
            List<DeltaElementLocator> deltaLocators = deltaElement.Locators.Cast<DeltaElementLocator>().ToList();
            deltaElement.Locators.Clear();
            foreach (DeltaElementLocator deltaLocator in deltaLocators)
            {
                deltaElement.Locators.Add(deltaLocator);
            }
            //convert properties to Delta
            List<DeltaControlProperty> deltaProperties = deltaElement.Properties.Cast<DeltaControlProperty>().ToList();
            deltaElement.Properties.Clear();
            foreach (DeltaControlProperty deltaPropery in deltaProperties)
            {
                deltaElement.Properties.Add(deltaPropery);
            }
            return deltaElement;
        }

        //private async void CollectOriginalElementsData()
        //{
        //    await Task.Run(() => mWizard.IWindowExplorerDriver.CollectOriginalElementsDataForDeltaCheck(mWizard.mPOMCurrentElements));
        //}

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
                mPomElementsPage = new PomElementsPage(mWizard.mPOM, eElementsContext.DeltaElements, mWizard.mPOMCurrentElements);
                xPomElementsMappingPageFrame.Content = mPomElementsPage;
            }
        }

        private void ElementsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {                
                ElementInfo latestElement = ((ObservableList<ElementInfo>)sender).Last();
                DeltaElementInfo CurrentElement = (DeltaElementInfo)mWizard.IWindowExplorerDriver.GetMatchingElement(latestElement, mWizard.mPOMCurrentElements);

                if (CurrentElement == null)//New element
                {
                    //copy element and convert it to Delta
                    DeltaElementInfo newDeltaElement = (DeltaElementInfo)latestElement.CreateCopy(false);
                    newDeltaElement.ElementStatus = ElementInfo.eElementStatus.Passed;
                    newDeltaElement.DeltaStatus = eDeltaStatus.New;
                    newDeltaElement.ElementGroup = ApplicationPOMModel.eElementGroup.Mapped;
                    newDeltaElement.IsSelected = true;
                    List<DeltaElementLocator> deltaLocators = newDeltaElement.Locators.Cast<DeltaElementLocator>().ToList();
                    newDeltaElement.Locators.Clear();
                    foreach (DeltaElementLocator deltaLocator in deltaLocators)
                    {
                        deltaLocator.LocateStatus = ElementLocator.eLocateStatus.Passed;
                        deltaLocator.DeltaStatus = eDeltaStatus.New;
                        newDeltaElement.Locators.Add(deltaLocator);                        
                    }
                    List<DeltaControlProperty> deltaProperties = newDeltaElement.Properties.Cast<DeltaControlProperty>().ToList();
                    newDeltaElement.Properties.Clear();
                    foreach (DeltaControlProperty deltaPropery in deltaProperties)
                    {
                        deltaPropery.DeltaStatus = eDeltaStatus.New;
                        newDeltaElement.Properties.Add(deltaPropery);
                    }
                    newDeltaElement.LatestMatchingElementInfo = latestElement;
                    mWizard.mPOMCurrentElements.Add(newDeltaElement);
                }
                else
                {
                    CurrentElement.LatestMatchingElementInfo = latestElement;
                    SetMatchingElementDeltaDetails(CurrentElement, latestElement);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "POM Delta- failed to compare new learned element with existing elements", ex);
            }
        }

        private void SetMatchingElementDeltaDetails(ElementInfo existingElement, ElementInfo latestElement)
        {
            ////////------------------ Locators
            foreach (ElementLocator latestLocator in latestElement.Locators)
            {               
                ElementLocator matchingExistingLocator = existingElement.Locators.Where(x => x.IsAutoLearned == true && x.LocateBy == latestLocator.LocateBy).FirstOrDefault();
                if (matchingExistingLocator != null)
                {
                    if (matchingExistingLocator.LocateValue == latestLocator.LocateValue)//Unchanged
                    {
                        ((DeltaElementLocator)matchingExistingLocator).DeltaStatus = eDeltaStatus.Unchanged;
                    }
                    else//Changed
                    {
                        ((DeltaElementLocator)matchingExistingLocator).DeltaStatus = eDeltaStatus.Changed;
                        ((DeltaElementLocator)matchingExistingLocator).DeltaExtraDetails = string.Format("Previous value was '{0}'", matchingExistingLocator.LocateValue);
                        matchingExistingLocator.LocateValue = latestLocator.LocateValue;
                    }
                }
                else//new locator
                {
                    DeltaElementLocator newElementLocator = (DeltaElementLocator)latestLocator.CreateCopy(false);
                    newElementLocator.DeltaStatus = eDeltaStatus.New;
                    existingElement.Locators.Add(newElementLocator);
                }                
            }
            List<ElementLocator> deletedLocators= existingElement.Locators.Where(x => x.IsAutoLearned == true && ((DeltaElementLocator)x).DeltaStatus== eDeltaStatus.Unknown).ToList();
            foreach (ElementLocator deletedLocator in deletedLocators)
            {
                ((DeltaElementLocator)deletedLocator).DeltaStatus = eDeltaStatus.Deleted;
            }

            ////////--------------- Properties
            foreach (ControlProperty latestProperty in latestElement.Properties)
            {
                ControlProperty matchingExistingProperty = existingElement.Properties.Where(x => x.Name == latestProperty.Name).FirstOrDefault();
                if (matchingExistingProperty != null)
                {
                    if (matchingExistingProperty.Value == latestProperty.Value)//Unchanged
                    {
                        ((DeltaControlProperty)matchingExistingProperty).DeltaStatus = eDeltaStatus.Unchanged;
                    }
                    else//Changed
                    {
                        ((DeltaControlProperty)matchingExistingProperty).DeltaStatus = eDeltaStatus.Changed;
                        ((DeltaControlProperty)matchingExistingProperty).DeltaExtraDetails = string.Format("Previous value was '{0}'", matchingExistingProperty.Value);
                        matchingExistingProperty.Value = latestProperty.Value;
                    }
                }
                else//new property
                {
                    DeltaControlProperty newElementProperty = (DeltaControlProperty)latestProperty.CreateCopy(false);
                    newElementProperty.DeltaStatus = eDeltaStatus.New;
                    existingElement.Properties.Add(newElementProperty);
                }
            }
            List<ControlProperty> deletedProperties = existingElement.Properties.Where(x => ((DeltaControlProperty)x).DeltaStatus == eDeltaStatus.Unknown).ToList();
            foreach (ControlProperty deletedProperty in deletedProperties)
            {
                ((DeltaControlProperty)deletedProperty).DeltaStatus = eDeltaStatus.Deleted;
            }

            //------------ General Status set
            DeltaElementInfo deltaExistingElement = (DeltaElementInfo)existingElement;         
            List<ElementLocator> modifiedLocatorsList = existingElement.Locators.Where(x => ((DeltaElementLocator)x).DeltaStatus != eDeltaStatus.Unchanged && ((DeltaElementLocator)x).DeltaStatus != eDeltaStatus.Unknown).ToList();
            List<ControlProperty> modifiedPropertiesList = existingElement.Properties.Where(x => ((DeltaControlProperty)x).DeltaStatus != eDeltaStatus.Unchanged && ((DeltaControlProperty)x).DeltaStatus != eDeltaStatus.Unknown).ToList();
            if (modifiedLocatorsList.Count > 0 || modifiedPropertiesList.Count > 0)
            {
                deltaExistingElement.DeltaStatus = eDeltaStatus.Changed;
                existingElement.IsSelected = true;
                if (modifiedLocatorsList.Count > 0 && modifiedPropertiesList.Count > 0)
                {
                    deltaExistingElement.DeltaExtraDetails = DeltaElementInfo.eDeltaExtraDetails.LocatorsAndPropertiesChanged;
                }
                else if (modifiedLocatorsList.Count > 0)
                {
                    deltaExistingElement.DeltaExtraDetails = DeltaElementInfo.eDeltaExtraDetails.LocatorsChanged;
                }
                else if (modifiedPropertiesList.Count > 0)
                {
                    deltaExistingElement.DeltaExtraDetails = DeltaElementInfo.eDeltaExtraDetails.PropertiesChanged;
                }
            }
            else
            {
                deltaExistingElement.DeltaStatus = eDeltaStatus.Unchanged;
                existingElement.IsSelected = false;
            }
        }

        private void SetDeletedElementDeltaDetails()
        {
            List<ElementInfo> deletedElements = mWizard.mPOMCurrentElements.Where(x => ((DeltaElementInfo)x).DeltaStatus == eDeltaStatus.Unknown).ToList();
            foreach(ElementInfo deletedElement in deletedElements)
            {
                ((DeltaElementInfo)deletedElement).DeltaStatus = eDeltaStatus.Deleted;
                deletedElement.IsSelected = false;
            }
        }

        private void DoEndOfRelearnElementsSorting()
        {            
            List<ElementInfo> deletedMappedElements = mWizard.mPOMCurrentElements.Where(x => ((DeltaElementInfo)x).DeltaStatus == eDeltaStatus.Deleted && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<ElementInfo> modifiedMappedElements = mWizard.mPOMCurrentElements.Where(x => ((DeltaElementInfo)x).DeltaStatus == eDeltaStatus.Changed && x.ElementGroup.ToString() == ApplicationPOMModel.eElementGroup.Mapped.ToString()).ToList();
            List<ElementInfo> newMappedElements = mWizard.mPOMCurrentElements.Where(x => ((DeltaElementInfo)x).DeltaStatus == eDeltaStatus.New && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();

            List<ElementInfo> deletedUnMappedElements = mWizard.mPOMCurrentElements.Where(x => ((DeltaElementInfo)x).DeltaStatus == eDeltaStatus.Deleted && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();
            List<ElementInfo> modifiedUnMappedElements = mWizard.mPOMCurrentElements.Where(x => ((DeltaElementInfo)x).DeltaStatus == eDeltaStatus.Changed && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();
            List<ElementInfo> newUnMappedElements = mWizard.mPOMCurrentElements.Where(x => ((DeltaElementInfo)x).DeltaStatus == eDeltaStatus.New && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();

            List<ElementInfo> unchangedMapped = mWizard.mPOMCurrentElements.Where(x => (((DeltaElementInfo)x).DeltaStatus == eDeltaStatus.Unchanged) && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<ElementInfo> unchangedUnmapped = mWizard.mPOMCurrentElements.Where(x => (((DeltaElementInfo)x).DeltaStatus == eDeltaStatus.Unchanged) && (ApplicationPOMModel.eElementGroup)x.ElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();

            List<List<ElementInfo>> ElementsLists = new List<List<ElementInfo>>() { deletedMappedElements, modifiedMappedElements, newMappedElements, deletedUnMappedElements, modifiedUnMappedElements, newUnMappedElements, unchangedMapped, unchangedUnmapped };
            mWizard.mPOMCurrentElements.Clear();

            foreach (List<ElementInfo> elementsList in ElementsLists)
            {
                foreach (ElementInfo EI in elementsList)
                {
                    mWizard.mPOMCurrentElements.Add(EI);
                }
            }
        }
    }
}
