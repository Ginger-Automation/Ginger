#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions.VisualTesting;
using GingerCore.Drivers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Application_Models
{
    public class PomLearnUtils
    {
        RepositoryFolder<ApplicationPOMModel> mPomModelsFolder;
        public ApplicationPOMModel POM;
        public ObservableList<UIElementFilter> AutoMapBasicElementTypesList = new ObservableList<UIElementFilter>();
        public ObservableList<UIElementFilter> AutoMapAdvanceElementTypesList = new ObservableList<UIElementFilter>();
        public List<eElementType> SelectedElementTypesList = new List<eElementType>();
        public ObservableList<ElementLocator> ElementLocatorsSettingsList = new ObservableList<ElementLocator>();
        List<eLocateBy> mElementLocatorsList = new List<eLocateBy>();
        public ObservableList<ElementInfo> mElementsList = new ObservableList<ElementInfo>();
        public PomSetting pomSetting;

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

        public IWindowExplorer IWindowExplorerDriver
        {
            get
            {
                if (Agent != null)
                    return ((IWindowExplorer)(((AgentOperations)Agent.AgentOperations).Driver));
                else
                    return null;
            }
        }

        public Bitmap ScreenShot { get; set; }
        public string SpecificFramePath { get; set; }

        public PomLearnUtils(ApplicationPOMModel pom, Agent agent=null, RepositoryFolder<ApplicationPOMModel> pomModelsFolder = null)
        {
            POM = pom;
            mAgent = agent;
            mPomModelsFolder = pomModelsFolder;

            mElementsList.CollectionChanged += ElementsListCollectionChanged;
        }

        public void SaveLearnedPOM()
        {
            if (ScreenShot != null)
            {
                using (var ms = new MemoryStream())
                {
                    POM.ScreenShotImage = BitmapToBase64(ScreenShot);
                }
            }

            if(Agent != null)
            {
                POM.LastUsedAgent = Agent.Guid;
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
            pomSetting = new PomSetting();
            var uIElementList = new List<UIElementFilter>();
            uIElementList.AddRange(AutoMapBasicElementTypesList.ToList());
            uIElementList.AddRange(AutoMapAdvanceElementTypesList.ToList());

            SelectedElementTypesList = uIElementList.Where(x => x.Selected).Select(x => x.ElementType).ToList();
            mElementLocatorsList = ElementLocatorsSettingsList.Select(x => x.LocateBy).ToList();

            pomSetting.filteredElementType = SelectedElementTypesList;
            pomSetting.ElementLocatorsSettingsList = ElementLocatorsSettingsList;
            pomSetting.isPOMLearn = true;
            pomSetting.relativeXpathTemplateList = GetRelativeXpathTemplateList();
            pomSetting.SpecificFramePath = SpecificFramePath;
            pomSetting.LearnScreenshotsOfElements = LearnScreenshotsOfElements;
        }

        public void LearnScreenShot()
        {
            IWindowExplorerDriver.UnHighLightElements();
            ScreenShot = ((IVisualTestingDriver)((AgentOperations)Agent.AgentOperations).Driver).GetScreenShot(null, true);
        }

        public async Task Learn()
        {
            ClearStopLearning();
            PrepareLearningConfigurations();
            LearnScreenShot();
            POM.PageURL = ((DriverBase)((AgentOperations)Agent.AgentOperations).Driver).GetURL();
            POM.Name = IWindowExplorerDriver.GetActiveWindow().Title;
            
            // appending Specific frame title in POM name
            if (!string.IsNullOrEmpty(SpecificFramePath))
            {
                var frame = IWindowExplorerDriver.GetWindowAllFrames().Where(x => x.Path.Equals(SpecificFramePath)).FirstOrDefault();
                
                if(frame != null)
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
                    await IWindowExplorerDriver.GetVisibleControls(pomSetting,mElementsList, POM.ApplicationPOMMetaData);
                }
            }
            else
            {
                pomSetting.filteredElementType = null;
               await IWindowExplorerDriver.GetVisibleControls(pomSetting, mElementsList, POM.ApplicationPOMMetaData);
            }

        }

        private List<string> GetRelativeXpathTemplateList()
        {
            var customRelXpathTemplateList = new List<string>();

            foreach (var item in POM.RelativeXpathTemplateList)
            {
                customRelXpathTemplateList.Add(item.Value);
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
                    if (SelectedElementTypesList.Contains(learnedElement.ElementTypeEnum))
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
                elemLoc.Active = ElementLocatorsSettingsList.Any(m => m.LocateBy == elemLoc.LocateBy) && ElementLocatorsSettingsList.Where(m => m.LocateBy == elemLoc.LocateBy).FirstOrDefault().Active;
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
                        name = name.Substring(0, 60);
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
                if (elements.Where(p => p.ElementName == name).Count() > 0)
                {
                    bool isFound = false;
                    int count = 2;
                    while (!isFound)
                    {
                        string postfix = string.Format("{0}_{1}", name, count);
                        if (elements.Where(p => p.ElementName == postfix).Count() > 0)
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
