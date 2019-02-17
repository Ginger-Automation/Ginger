using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
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

namespace Amdocs.Ginger.CoreNET.Application_Models
{
    public class PomLearnUtils
    {
        RepositoryFolder<ApplicationPOMModel> mPomModelsFolder;
        public ApplicationPOMModel POM;
        public ObservableList<UIElementFilter> AutoMapElementTypesList = new ObservableList<UIElementFilter>();
        public List<eElementType> SelectedElementTypesList = new List<eElementType>();
        public ObservableList<ElementLocator> AutoMapElementLocatorsList = new ObservableList<ElementLocator>();
        List<eLocateBy> mElementLocatorsList = new List<eLocateBy>();
        public ObservableList<ElementInfo> mElementsList = new ObservableList<ElementInfo>();
        

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
                    return ((IWindowExplorer)(Agent.Driver));
                else
                    return null;
            }
        }

        public Bitmap ScreenShot { get; set; }
       

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
            if (mPomModelsFolder != null)
                mPomModelsFolder.AddRepositoryItem(POM);
            else
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(POM);
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
            if (mAgent != null && mAgent.Driver != null)
            {
                mAgent.Driver.mStopProcess = true;
            }
        }

        public void ClearStopLearning()
        {
            if (mAgent != null && mAgent.Driver != null)
            {
                mAgent.Driver.mStopProcess = false;
            }
        }

        public void PrepareLearningConfigurations()
        {
            SelectedElementTypesList = AutoMapElementTypesList.Where(x => x.Selected == true).Select(x => x.ElementType).ToList();
            mElementLocatorsList = AutoMapElementLocatorsList.Select(x => x.LocateBy).ToList();           
        }

        public void LearnScreenShot()
        {
            IWindowExplorerDriver.UnHighLightElements();
            ScreenShot = ((IVisualTestingDriver)Agent.Driver).GetScreenShot(new Tuple<int, int>(ApplicationPOMModel.cLearnScreenWidth, ApplicationPOMModel.cLearnScreenHeight));
        }

        public void Learn()
        {
            ClearStopLearning();
            PrepareLearningConfigurations();
            LearnScreenShot();
            POM.PageURL = ((DriverBase)Agent.Driver).GetURL();
            POM.Name = IWindowExplorerDriver.GetActiveWindow().Title;
            POM.MappedUIElements.Clear();
            POM.UnMappedUIElements.Clear();
            mElementsList.Clear();
            IWindowExplorerDriver.GetVisibleControls(null, mElementsList, true);
        }

        private void ElementsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    ElementInfo learnedElement = (ElementInfo)e.NewItems[0];

                    SetLearnedElementDetails(learnedElement);

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
                elemLoc.Active = AutoMapElementLocatorsList.Where(m => m.LocateBy == elemLoc.LocateBy).FirstOrDefault().Active;
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
