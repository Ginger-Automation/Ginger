using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Drivers;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    /// <summary>
    /// Interaction logic for POMDeltaPage.xaml
    /// </summary>
    public partial class PomDeltaElementComparePage : Page, IWizardPage
    {
        PomDeltaWizard mWizard;
        PomDeltaViewPage mPomDeltaViewPage = null;       

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
                mPomDeltaViewPage.SetAgent(mWizard.Agent);
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

                    //await Task.Run(() => GetUnifiedPomElementsListForDeltaUse());
                    await Task.Run(() => mWizard.IWindowExplorerDriver.CollectOriginalElementsDataForDeltaCheck(mWizard.mPOMAllOriginalElements));
                    await Task.Run(() => mWizard.IWindowExplorerDriver.GetVisibleControls(null, mWizard.mPOMLatestElements, true));

                    SetDeletedElementsDeltaDetails();//TODO:also do async?
                    DoEndOfRelearnElementsSorting();//TODO:also do async?
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

        //public void GetUnifiedPomElementsListForDeltaUse()
        //{
        //    mWizard.mDeltaViewElements.Clear();
        //    //TODO: check if should be done using Parallel.ForEach
        //    foreach (ElementInfo mappedElement in mWizard.mPOM.MappedUIElements)
        //    {
        //        DeltaElementInfo deltaElement = ConvertElementInfoToDelta(mappedElement);
        //        deltaElement.ElementGroup = ApplicationPOMModel.eElementGroup.Mapped;
        //        mWizard.mDeltaViewElements.Add(deltaElement);
        //    }
        //    foreach (ElementInfo unmappedElement in mWizard.mPOM.UnMappedUIElements)
        //    {
        //        DeltaElementInfo deltaElement = ConvertElementInfoToDelta(unmappedElement);
        //        deltaElement.ElementGroup = ApplicationPOMModel.eElementGroup.Unmapped;
        //        mWizard.mDeltaViewElements.Add(deltaElement);
        //    }
        //}

        //private DeltaElementInfo ConvertElementInfoToDelta(ElementInfo element)
        //{
        //    //copy element and convert it to Delta
        //    DeltaElementInfo deltaElement = (DeltaElementInfo)element.CreateCopy(false);//keeping original GUI            
        //    //convert Locators to Delta
        //    List<DeltaElementLocator> deltaLocators = deltaElement.Locators.Cast<DeltaElementLocator>().ToList();
        //    deltaElement.Locators.Clear();
        //    foreach (DeltaElementLocator deltaLocator in deltaLocators)
        //    {
        //        deltaElement.Locators.Add(deltaLocator);
        //    }
        //    //convert properties to Delta
        //    List<DeltaControlProperty> deltaProperties = deltaElement.Properties.Cast<DeltaControlProperty>().ToList();
        //    deltaElement.Properties.Clear();
        //    foreach (DeltaControlProperty deltaPropery in deltaProperties)
        //    {
        //        deltaElement.Properties.Add(deltaPropery);
        //    }
        //    return deltaElement;
        //}

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
            if (mPomDeltaViewPage == null)
            {
                mPomDeltaViewPage = new PomDeltaViewPage(mWizard.mDeltaViewElements);
                xPomElementsMappingPageFrame.Content = mPomDeltaViewPage;
            }
        }

        private void ElementsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {                
                ElementInfo latestElement = ((ObservableList<ElementInfo>)sender).Last();
                ElementInfo matchingOriginalElement = (ElementInfo)mWizard.IWindowExplorerDriver.GetMatchingElement(latestElement, mWizard.mPOMAllOriginalElements);

                if (matchingOriginalElement == null)//New element
                {
                    //copy element and convert it to Delta
                    DeltaElementInfo newDeltaElement = new DeltaElementInfo();
                    newDeltaElement.OriginalElementInfo = latestElement;//setting to Original because it is new
                    //newDeltaElement.LatestMatchingElementInfo = latestElement;
                    latestElement.ElementStatus = ElementInfo.eElementStatus.Passed;
                    newDeltaElement.DeltaStatus = eDeltaStatus.New;
                    newDeltaElement.SelectedElementGroup = ApplicationPOMModel.eElementGroup.Mapped;
                    newDeltaElement.IsSelected = true;
                    foreach (ElementLocator locator in latestElement.Locators)
                    {
                        DeltaElementLocator deltaLocator = new DeltaElementLocator();
                        deltaLocator.OriginalElementLocator= locator;//setting to Original because it is new
                        //deltaLocator.LatestMatchingElementLocator = locator;
                        deltaLocator.OriginalElementLocator.LocateStatus = ElementLocator.eLocateStatus.Passed;
                        deltaLocator.DeltaStatus = eDeltaStatus.New;
                        newDeltaElement.Locators.Add(deltaLocator);                        
                    }
                    foreach (ControlProperty propery in latestElement.Properties)
                    {
                        DeltaControlProperty deltaPropery = new DeltaControlProperty();
                        deltaPropery.OriginalElementProperty = propery;//setting to Original because it is new
                        //deltaPropery.LatestMatchingElementProperty = propery;
                        deltaPropery.DeltaStatus = eDeltaStatus.New;
                        newDeltaElement.Properties.Add(deltaPropery);
                    }
                    mWizard.mDeltaViewElements.Add(newDeltaElement);
                }
                else
                {                    
                    SetMatchingElementDeltaDetails(matchingOriginalElement, latestElement);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "POM Delta- failed to compare new learned element with existing elements", ex);
            }
        }

        private void SetMatchingElementDeltaDetails(ElementInfo existingElement, ElementInfo latestElement)
        {
            DeltaElementInfo matchedDeltaElement = new DeltaElementInfo();
            matchedDeltaElement.OriginalElementInfo = existingElement;
            matchedDeltaElement.LatestMatchingElementInfo = latestElement;

            ////////------------------ Locators
            foreach (ElementLocator latestLocator in latestElement.Locators)
            {
                DeltaElementLocator deltaLocator = new DeltaElementLocator();
                ElementLocator matchingExistingLocator = existingElement.Locators.Where(x => x.IsAutoLearned == true && x.LocateBy == latestLocator.LocateBy).FirstOrDefault();
                if (matchingExistingLocator != null)
                {
                    deltaLocator.OriginalElementLocator = matchingExistingLocator;
                    deltaLocator.LatestMatchingElementLocator = latestLocator;
                    if (matchingExistingLocator.LocateValue == latestLocator.LocateValue)//Unchanged
                    {
                        deltaLocator.DeltaStatus = eDeltaStatus.Unchanged;                        
                    }
                    else//Changed
                    {
                        deltaLocator.DeltaStatus = eDeltaStatus.Changed;
                        deltaLocator.DeltaExtraDetails = string.Format("Previous value was '{0}'", matchingExistingLocator.LocateValue);
                        matchingExistingLocator.LocateValue = latestLocator.LocateValue;
                    }
                }
                else//new locator
                {
                    deltaLocator.OriginalElementLocator = latestLocator;
                    //deltaLocator.LatestMatchingElementLocator = latestLocator;
                    deltaLocator.DeltaStatus = eDeltaStatus.New;                    
                }
                matchedDeltaElement.Locators.Add(deltaLocator);
            }
            //deleted Locators
            List<ElementLocator> deletedLocators= existingElement.Locators.Where(x => x.IsAutoLearned == true && matchedDeltaElement.Locators.Where(y=>y.OriginalElementLocator.Guid == x.Guid).FirstOrDefault() == null).ToList();
            foreach (ElementLocator deletedlocator in deletedLocators)
            {
                DeltaElementLocator deltaLocator = new DeltaElementLocator();
                deltaLocator.OriginalElementLocator = deletedlocator;
                deltaLocator.DeltaStatus = eDeltaStatus.Deleted;
                matchedDeltaElement.Locators.Add(deltaLocator);
            }

            ////////--------------- Properties
            foreach (ControlProperty latestProperty in latestElement.Properties)
            {
                DeltaControlProperty deltaProperty = new DeltaControlProperty();
                ControlProperty matchingExistingProperty = existingElement.Properties.Where(x => x.Name == latestProperty.Name).FirstOrDefault();
                if (matchingExistingProperty != null)
                {
                    deltaProperty.OriginalElementProperty = matchingExistingProperty;
                    deltaProperty.LatestMatchingElementProperty = latestProperty;
                    if (matchingExistingProperty.Value == latestProperty.Value)//Unchanged
                    {
                        deltaProperty.DeltaStatus = eDeltaStatus.Unchanged;
                    }
                    else//Changed
                    {
                        deltaProperty.DeltaStatus = eDeltaStatus.Changed;
                        deltaProperty.DeltaExtraDetails = string.Format("Previous value was '{0}'", matchingExistingProperty.Value);
                        matchingExistingProperty.Value = latestProperty.Value;
                    }
                }
                else//new locator
                {
                    deltaProperty.OriginalElementProperty = latestProperty;
                    //deltaLocator.LatestMatchingElementLocator = latestLocator;
                    deltaProperty.DeltaStatus = eDeltaStatus.New;
                }
                matchedDeltaElement.Properties.Add(deltaProperty);
            }
            //deleted Properties
            List<ControlProperty> deletedProperties = existingElement.Properties.Where(x => matchedDeltaElement.Properties.Where(y => y.OriginalElementProperty.Guid == x.Guid).FirstOrDefault() == null).ToList();
            foreach (ControlProperty deletedProperty in deletedProperties)
            {
                DeltaControlProperty deltaProp = new DeltaControlProperty();
                deltaProp.OriginalElementProperty = deletedProperty;
                deltaProp.DeltaStatus = eDeltaStatus.Deleted;
                matchedDeltaElement.Properties.Add(deltaProp);
            }

            //------------ General Status set     
            List<DeltaElementLocator> modifiedLocatorsList = matchedDeltaElement.Locators.Where(x => x.DeltaStatus != eDeltaStatus.Unchanged && x.DeltaStatus != eDeltaStatus.Unknown).ToList();
            List<DeltaControlProperty> modifiedPropertiesList = matchedDeltaElement.Properties.Where(x => x.DeltaStatus != eDeltaStatus.Unchanged && x.DeltaStatus != eDeltaStatus.Unknown).ToList();
            if (modifiedLocatorsList.Count > 0 || modifiedPropertiesList.Count > 0)
            {
                matchedDeltaElement.DeltaStatus = eDeltaStatus.Changed;
                matchedDeltaElement.IsSelected = true;
                if (modifiedLocatorsList.Count > 0 && modifiedPropertiesList.Count > 0)
                {
                    matchedDeltaElement.DeltaExtraDetails = DeltaElementInfo.eDeltaExtraDetails.LocatorsAndPropertiesChanged;
                }
                else if (modifiedLocatorsList.Count > 0)
                {
                    matchedDeltaElement.DeltaExtraDetails = DeltaElementInfo.eDeltaExtraDetails.LocatorsChanged;
                }
                else if (modifiedPropertiesList.Count > 0)
                {
                    matchedDeltaElement.DeltaExtraDetails = DeltaElementInfo.eDeltaExtraDetails.PropertiesChanged;
                }
            }
            else
            {
                matchedDeltaElement.DeltaStatus = eDeltaStatus.Unchanged;
                matchedDeltaElement.IsSelected = false;
            }

            mWizard.mDeltaViewElements.Add(matchedDeltaElement);
        }

        private void SetDeletedElementsDeltaDetails()
        {
            List<ElementInfo> deletedElements = mWizard.mPOMAllOriginalElements.Where(x => mWizard.mDeltaViewElements.Where(y=>y.OriginalElementInfo.Guid == x.Guid).FirstOrDefault() == null).ToList();
            foreach(ElementInfo deletedElement in deletedElements)
            {
                DeltaElementInfo deletedDeltaElement = new DeltaElementInfo();
                deletedDeltaElement.OriginalElementInfo = deletedElement;
                deletedDeltaElement.DeltaStatus = eDeltaStatus.Deleted;
                deletedDeltaElement.IsSelected = true;
            }
        }

        private void DoEndOfRelearnElementsSorting()
        {            
            List<DeltaElementInfo> deletedMappedElements = mWizard.mDeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Deleted && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<DeltaElementInfo> modifiedMappedElements = mWizard.mDeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Changed && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<DeltaElementInfo> newMappedElements = mWizard.mDeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.New && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();

            List<DeltaElementInfo> deletedUnMappedElements = mWizard.mDeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Deleted && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();
            List<DeltaElementInfo> modifiedUnMappedElements = mWizard.mDeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Changed && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();
            List<DeltaElementInfo> newUnMappedElements = mWizard.mDeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.New && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();

            List<DeltaElementInfo> unchangedMapped = mWizard.mDeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Unchanged && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<DeltaElementInfo> unchangedUnmapped = mWizard.mDeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Unchanged && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();

            List<List<DeltaElementInfo>> ElementsLists = new List<List<DeltaElementInfo>>() { deletedMappedElements, modifiedMappedElements, newMappedElements, deletedUnMappedElements, modifiedUnMappedElements, newUnMappedElements, unchangedMapped, unchangedUnmapped };
            mWizard.mDeltaViewElements.Clear();

            foreach (List<DeltaElementInfo> elementsList in ElementsLists)
            {
                foreach (DeltaElementInfo EI in elementsList)
                {
                    mWizard.mDeltaViewElements.Add(EI);
                }
            }
        }
    }
}
