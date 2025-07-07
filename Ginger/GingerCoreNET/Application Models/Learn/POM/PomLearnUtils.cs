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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions.VisualTesting;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static GingerCore.Agent;

namespace Amdocs.Ginger.CoreNET.Application_Models
{
    public class PomLearnUtils
    {
        RepositoryFolder<ApplicationPOMModel> mPomModelsFolder;
        public ApplicationPOMModel POM;
        public ObservableList<UIElementFilter> AutoMapBasicElementTypesList = [];
        public ObservableList<UIElementFilter> AutoMapAdvanceElementTypesList = [];
        public ObservableList<UIElementFilter> SelectedElementTypesList = [];
        public ObservableList<ElementLocator> ElementLocatorsSettingsList = [];
        List<eLocateBy> mElementLocatorsList = [];
        public ObservableList<ElementInfo> mElementsList = [];
        public PomSetting pomSetting;
        public bool IsGeneratedByAI { get; set; } = false;

        bool mLearnOnlyMappedElements = true;
        public bool LearnOnlyMappedElements
        {
            get
            {
                return mLearnOnlyMappedElements;
            }
            set
            {
                mLearnOnlyMappedElements = value;
            }
        }

        bool mLearnScreenshotsOfElements = true;
        public bool LearnScreenshotsOfElements
        {
            get
            {
                return mLearnScreenshotsOfElements;
            }
            set
            {
                mLearnScreenshotsOfElements = value;
            }
        }

        private Agent mAgent = null;
        public Agent Agent
        {
            get
            {
                return mAgent;
            }
            set
            {
                mAgent = value;
            }
        }

        bool mLearnShadowDomElements = false;

        public bool LearnShadowDomElements
        {
            get
            {
                return mLearnShadowDomElements;
            }

            set
            {
                mLearnShadowDomElements = value;
            }
        }

        public IWindowExplorer IWindowExplorerDriver
        {
            get
            {
                if (Agent != null)
                {
                    return ((IWindowExplorer)(((AgentOperations)Agent.AgentOperations).Driver));
                }
                else
                {
                    return null;
                }
            }
        }

        public Bitmap ScreenShot { get; set; }
        public string SpecificFramePath { get; set; }

        public PomLearnUtils(ApplicationPOMModel pom, Agent agent = null, RepositoryFolder<ApplicationPOMModel> pomModelsFolder = null)
        {
            POM = pom;
            if (POM.PomSetting != null && POM.PomSetting.FilteredElementType != null && POM.PomSetting.FilteredElementType.Count > 0)
            {
                var elementList = PlatformInfoBase.GetPlatformImpl(ePlatformType.Web).GetUIElementFilterList();
                AutoMapBasicElementTypesList = elementList["Basic"];
                AutoMapAdvanceElementTypesList = elementList["Advanced"];
                SelectElementsToList(AutoMapBasicElementTypesList, POM.PomSetting.FilteredElementType);
                SelectElementsToList(AutoMapAdvanceElementTypesList, POM.PomSetting.FilteredElementType);
            }
            mAgent = agent;
            mPomModelsFolder = pomModelsFolder;
            StartLearningTime();
            mElementsList.CollectionChanged += ElementsListCollectionChanged;
        }

        public void SelectElementsToList(ObservableList<UIElementFilter> elements, ObservableList<UIElementFilter> filterList)
        {
            foreach (UIElementFilter element in elements)
            {
                var selectedFilter = filterList.FirstOrDefault(filter => filter.ElementType.Equals(element.ElementType));
                element.Selected = selectedFilter?.Selected ?? false;
            }
        }

        public void SaveLearnedPOM()
        {
            StopLearningTime();
            if (ScreenShot != null)
            {
                using (var ms = new MemoryStream())
                {
                    POM.ScreenShotImage = BitmapToBase64(ScreenShot);
                }
            }

            if (Agent != null)
            {
                POM.LastUsedAgent = Agent.Guid;
                POM.AIGenerated = IsGeneratedByAI;
            }

            if (mPomModelsFolder != null)
            {
                mPomModelsFolder.AddRepositoryItem(POM);
            }
            else
            {
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(POM);
            }
        }

        private string BitmapToBase64(Bitmap bImage)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] byteImage = ms.ToArray();
                return Convert.ToBase64String(byteImage); //Get Base64
            }
        }

        public void StartLearningTime()
        {
            POM.StartTimer();
        }
        public void StopLearningTime()
        {
            POM.StopTimer();
        }

        public void StopLearning()
        {
            if (mAgent != null && ((AgentOperations)mAgent.AgentOperations).Driver != null)
            {
                ((AgentOperations)mAgent.AgentOperations).Driver.StopProcess = true;
            }

        }

        public void ClearStopLearning()
        {
            if (mAgent != null && ((AgentOperations)mAgent.AgentOperations).Driver != null)
            {
                ((AgentOperations)mAgent.AgentOperations).Driver.StopProcess = false;
            }
        }

        public void PrepareLearningConfigurations()
        {

            ObservableList<UIElementFilter> uIElementList = new ObservableList<UIElementFilter>();

            // Adding items from AutoMapBasicElementTypesList
            uIElementList.AddRange(AutoMapBasicElementTypesList);

            // Adding items from AutoMapAdvanceElementTypesList
            uIElementList.AddRange(AutoMapAdvanceElementTypesList);

            // Filtering selected elements and getting their types
            SelectedElementTypesList = new ObservableList<UIElementFilter>();
            SelectedElementTypesList.AddRange(uIElementList.Where(x => x.Selected));

            if (POM.PomSetting != null)
            {
                mElementLocatorsList = POM.PomSetting.ElementLocatorsSettingsList != null ? POM.PomSetting.ElementLocatorsSettingsList.Select(x => x.LocateBy).ToList() : ElementLocatorsSettingsList.Select(x => x.LocateBy).ToList();

                POM.PomSetting.FilteredElementType = SelectedElementTypesList;
                POM.PomSetting.ElementLocatorsSettingsList = POM.PomSetting.ElementLocatorsSettingsList != null ? POM.PomSetting.ElementLocatorsSettingsList : ElementLocatorsSettingsList;
                POM.PomSetting.isPOMLearn = true;
                POM.PomSetting.RelativeXpathTemplateList = POM.PomSetting.RelativeXpathTemplateList != null ? POM.PomSetting.RelativeXpathTemplateList : GetRelativeXpathTemplateList();
                POM.PomSetting.SpecificFramePath = POM.PomSetting.SpecificFramePath != null ? POM.PomSetting.SpecificFramePath : SpecificFramePath;
                POM.PomSetting.LearnScreenshotsOfElements = POM.PomSetting.LearnScreenshotsOfElements ? POM.PomSetting.LearnScreenshotsOfElements : LearnScreenshotsOfElements;
                POM.PomSetting.LearnShadowDomElements = POM.PomSetting.LearnShadowDomElements ? POM.PomSetting.LearnShadowDomElements : LearnShadowDomElements;
                POM.PomSetting = POM.PomSetting;
            }
            else
            {

                pomSetting = new PomSetting();
                mElementLocatorsList = ElementLocatorsSettingsList.Select(x => x.LocateBy).ToList();

                pomSetting.FilteredElementType = SelectedElementTypesList;
                pomSetting.ElementLocatorsSettingsList = ElementLocatorsSettingsList;
                pomSetting.isPOMLearn = true;
                pomSetting.RelativeXpathTemplateList = GetRelativeXpathTemplateList();
                pomSetting.SpecificFramePath = SpecificFramePath;
                pomSetting.LearnScreenshotsOfElements = LearnScreenshotsOfElements;
                pomSetting.LearnShadowDomElements = LearnShadowDomElements;
                POM.PomSetting = pomSetting;
            }
        }

        public void LearnScreenShot()
        {
            IWindowExplorerDriver.UnHighLightElements();
            ScreenShot = ((IVisualTestingDriver)((AgentOperations)Agent.AgentOperations).Driver).GetScreenShot(null, true);
        }

        public async Task Learn()
        {
            using IFeatureTracker featureTracker = Reporter.StartFeatureTracking(FeatureId.POMLearning);
            featureTracker.Metadata.Add("Platform", Agent.Platform.ToString());
            featureTracker.Metadata.Add("DriverType", Agent.DriverType.ToString());

            ClearStopLearning();
            PrepareLearningConfigurations();
            LearnScreenShot();
            POM.PageURL = ((AgentOperations)Agent.AgentOperations).Driver.GetURL();
            if (Agent.Platform == ePlatformType.Web)
            {
                try
                {
                    Uri uri = new Uri(POM.PageURL);
                    if (uri.IsFile && File.Exists(uri.AbsolutePath))
                    {
                        string normalizedPageUrl = Path.GetFullPath(new Uri(POM.PageURL).LocalPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        if (normalizedPageUrl.Contains(WorkSpace.Instance.SolutionRepository.SolutionFolder))
                        {
                            POM.PageURL = GingerCoreNET.GeneralLib.General.SetupRelativePath(normalizedPageUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, $"Invalid URI format in POM.PageURL: {POM.PageURL}", ex);
                }
            }
            POM.Name = IWindowExplorerDriver.GetActiveWindow().Title;
            // appending Specific frame title in POM name
            if (!string.IsNullOrEmpty(SpecificFramePath))
            {
                var frame = IWindowExplorerDriver.GetWindowAllFrames().FirstOrDefault(x => x.Path.Equals(SpecificFramePath));

                if (frame != null)
                {
                    POM.Name = string.Concat(POM.Name, " : ", frame.Title);
                }
            }

            POM.MappedUIElements.Clear();
            POM.UnMappedUIElements.Clear();
            mElementsList.Clear();
            if (LearnOnlyMappedElements)
            {
                if (SelectedElementTypesList.Count > 0)
                {
                    await IWindowExplorerDriver.GetVisibleControls(POM.PomSetting, mElementsList, POM.ApplicationPOMMetaData, ScreenShot);

                }
            }
            else
            {
                POM.PomSetting.FilteredElementType = null;
                await IWindowExplorerDriver.GetVisibleControls(POM.PomSetting, mElementsList, POM.ApplicationPOMMetaData, ScreenShot);
            }

            featureTracker.Metadata.Add("MappedElementCount", POM.MappedUIElements != null ? POM.MappedUIElements.Count.ToString() : "");
            featureTracker.Metadata.Add("UnmappedElementCount", POM.UnMappedUIElements != null ? POM.UnMappedUIElements.Count.ToString() : "");
        }

        private ObservableList<CustomRelativeXpathTemplate> GetRelativeXpathTemplateList()
        {
            var customRelXpathTemplateList = new ObservableList<CustomRelativeXpathTemplate>();
            if (POM.PomSetting != null && POM.PomSetting.RelativeXpathTemplateList != null)
            {
                foreach (var item in POM.PomSetting.RelativeXpathTemplateList)
                {
                    customRelXpathTemplateList.Add(item);
                }
            }
            return customRelXpathTemplateList;
        }

        private void ElementsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    ElementInfo learnedElement = (ElementInfo)e.NewItems[0];

                    SetLearnedElementDetails(learnedElement);
                    learnedElement.ParentGuid = POM.Guid;
                    //add to relevent group
                    if (SelectedElementTypesList.Any(x => x.ElementType.Equals(learnedElement.ElementTypeEnum)))
                    {
                        POM.MappedUIElements.Add(learnedElement);
                    }
                    else
                    {
                        POM.UnMappedUIElements.Add(learnedElement);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "POM: Learned Element Info from type was failed to be added to Page Elements", ex);
            }
        }

        public void SetLearnedElementDetails(ElementInfo element)
        {
            //set locators
            List<ElementLocator> orderedLocatorsList = element.Locators.OrderBy(m => mElementLocatorsList.IndexOf(m.LocateBy)).ToList();
            foreach (ElementLocator elemLoc in orderedLocatorsList)
            {
                elemLoc.Active = POM.PomSetting.ElementLocatorsSettingsList.Any(m => m.LocateBy == elemLoc.LocateBy) && POM.PomSetting.ElementLocatorsSettingsList.FirstOrDefault(m => m.LocateBy == elemLoc.LocateBy).Active;
            }
            element.Locators = new ObservableList<ElementLocator>(orderedLocatorsList);

            //set name
            UpdateElementInfoName(element);
        }

        /// <summary>
        /// This method is used to update the element name by filtering the specia characters and checking the duplicate names
        /// </summary>
        /// <param name="curElement"></param>
        private void UpdateElementInfoName(ElementInfo curElement)
        {
            try
            {
                if (curElement != null)
                {
                    //remove invalid chars
                    string name = curElement.ElementName.Trim().Replace(".", "").Replace("?", "").Replace("\n", "").Replace("\r", "").Replace("#", "").Replace("!", " ").Replace(",", " ").Replace("   ", "");
                    foreach (char chr in Path.GetInvalidFileNameChars())
                    {
                        name = name.Replace(chr.ToString(), string.Empty);
                    }

                    //set max name length to 60
                    if (name.Length > 60)
                    {
                        name = name[..60];
                    }

                    //make sure name is unique                    
                    name = GetUniqueName(POM.MappedUIElements, name);
                    name = GetUniqueName(POM.UnMappedUIElements, name);
                    curElement.ElementName = name;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Updating POM Element Name", ex);
            }
        }

        /// <summary>
        /// This method is used to get the uniquename for the element
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetUniqueName(ObservableList<ElementInfo> elements, string name)
        {
            string uname = name;
            try
            {
                if (elements.Any(p => p.ElementName == name))
                {
                    bool isFound = false;
                    int count = 2;
                    while (!isFound)
                    {
                        string postfix = string.Format("{0}_{1}", name, count);
                        if (elements.Any(p => p.ElementName == postfix))
                        {
                            count++;
                        }
                        else
                        {
                            uname = postfix;
                            isFound = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Updating POM Element Name", ex);
            }
            return uname;
        }

    }
}
