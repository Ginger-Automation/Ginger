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
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Amdocs.Ginger.CoreNET
{
    public class RecordingManager
    {       
        private bool CreatePOM { get; set; }

        ObservableList<ApplicationPOMModel> mApplicationPOMList;

        public List<POMObjectRecordingHelper> ListPOMObjectHelper { get; set; }
        
        public Context Context { get; set; }

        public BusinessFlow BusinessFlow { get; set; }

        public IRecord PlatformDriver { get; set; }

        public ApplicationPOMModel CurrentPOM { get; set; }

        public IPlatformInfo PlatformInfo { get; set; }

        public bool LearnAdditionalDetails { get; set; }

        public RecordingManager(ObservableList<ApplicationPOMModel> lstApplicationPOM, BusinessFlow bFlow, Context context, IRecord platformDriver, IPlatformInfo pInfo)
        {
            try
            {
                PlatformInfo = pInfo;
                PlatformDriver = platformDriver;
                mApplicationPOMList = lstApplicationPOM;
                //if lstApplicationPOM == null then dont create POM or if applicationPOM.Name has some value then use the existing POM
                //or else create new POM
                if (mApplicationPOMList == null)
                {
                    LearnAdditionalDetails = false;
                    CreatePOM = false;
                    CurrentPOM = null;
                }
                else
                {
                    LearnAdditionalDetails = true;
                    CreatePOM = true;                    
                    if (mApplicationPOMList.Count > 0)
                    {
                        CurrentPOM = mApplicationPOMList[0]; 
                    }
                    else
                    {
                        CurrentPOM = new ApplicationPOMModel();
                    }
                    ListPOMObjectHelper = new List<POMObjectRecordingHelper>();
                    foreach (var cPom in mApplicationPOMList)
                    {
                        ListPOMObjectHelper.Add(new POMObjectRecordingHelper() { PageTitle = cPom.Name, PageURL = cPom.PageURL, ApplicationPOM = cPom });
                    }
                }

                BusinessFlow = bFlow;
                Context = context;
                PlatformDriver.ResetRecordingEventHandler();
                PlatformDriver.RecordingEvent += PlatformDriver_RecordingEvent;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Recording Manager while instantiating", ex);
            }
        }

        /// <summary>
        /// This method is used to get the new POM object for new creation while recording
        /// </summary>
        /// <param name="pageTitle"></param>
        /// <param name="pageURL"></param>
        /// <param name="screenShot"></param>
        /// <returns></returns>
        private POMObjectRecordingHelper GetNewPOM(string pageTitle, string pageURL, string screenShot)
        {
            POMObjectRecordingHelper recordingHelper = new POMObjectRecordingHelper();
            try
            {                
                ApplicationPOMModel newPOM = new ApplicationPOMModel();
                string uniquTitle = GetUniquePOMName(pageTitle);
                newPOM.Name = uniquTitle;                
                newPOM.PageURL = pageURL;
                newPOM.ScreenShotImage = screenShot;
                newPOM.MappedUIElements = new ObservableList<ElementInfo>();
                if (WorkSpace.Instance.Solution != null)//check for unit tests
                {
                    RepositoryItemKey tAppkey = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == Context.Target.Name).Select(x => x.Key).FirstOrDefault();
                    if (tAppkey != null)
                    {
                        newPOM.TargetApplicationKey = tAppkey;
                    }
                    else
                    {
                        newPOM.TargetApplicationKey = Context.Target.Key;
                    }
                }

                //Save new POM
                if (WorkSpace.Instance.SolutionRepository != null)//check for unit tests
                {
                    RepositoryFolder<ApplicationPOMModel> repositoryFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>();
                    repositoryFolder.AddRepositoryItem(newPOM);
                }

                if (mApplicationPOMList != null)
                {
                    mApplicationPOMList.Add(newPOM);//adding so user will notice it was added during recording
                }

                recordingHelper.PageTitle = pageTitle;
                recordingHelper.PageURL = pageURL;
                recordingHelper.ApplicationPOM = newPOM;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in creating Recording object", ex);
            }
            return recordingHelper;
        }

        /// <summary>
        /// This method is used to get the unique POM name for new POM creation
        /// </summary>
        /// <param name="pageTitle"></param>
        /// <returns></returns>
        private string GetUniquePOMName(string pageTitle)
        {
            string uniqueName = string.Empty;
            try
            {
                uniqueName = pageTitle;
                int appendCount = 2;
                while (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>().Where(x => x.Name.Trim().ToLower() == uniqueName.Trim().ToLower()).FirstOrDefault() != null)
                {
                    uniqueName = string.Format("{0}_{1}", pageTitle, appendCount);
                    appendCount++;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in getting the unique name while POM creation", ex);
            }
            return uniqueName;
        }

        private void PlatformDriverPageChangedHandler(PageChangedEventArgs args)
        {
            try
            {
                POMObjectRecordingHelper newPOMHelper = null;
                if (ListPOMObjectHelper != null && ListPOMObjectHelper.Count > 0)
                {
                    var obj = ListPOMObjectHelper.FirstOrDefault(s => s.PageURL == args.PageURL);
                    if (obj == null && !string.IsNullOrEmpty(args.PageTitle) && !string.IsNullOrEmpty(args.PageURL))
                    {
                        newPOMHelper = GetNewPOM(args.PageTitle, args.PageURL, args.ScreenShot);                        
                        ListPOMObjectHelper.Add(newPOMHelper);
                        CurrentPOM = newPOMHelper.ApplicationPOM;
                    }
                    else if (!(CurrentPOM.PageURL == obj.PageURL))
                    {
                        CurrentPOM = obj.ApplicationPOM;
                    }                    
                }
                else
                {
                    newPOMHelper = GetNewPOM(args.PageTitle, args.PageURL, args.ScreenShot);
                    CurrentPOM = newPOMHelper.ApplicationPOM;
                    ListPOMObjectHelper = new List<POMObjectRecordingHelper>();
                    ListPOMObjectHelper.Add(newPOMHelper);                    
                }
                AddBrowserAction(args.PageTitle, args.PageURL);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Page change event handler while recording", ex);
            }
        }

        private void AddBrowserAction(string pageTitle, string pageURL)
        {
            try
            {
                ElementActionCongifuration actConfig = new ElementActionCongifuration()
                {
                    Description = "Go to Url - " + pageTitle,
                    Operation = "GotoURL",
                    ElementValue = pageURL,
                    LocateBy = "NA"
                };

                ElementInfo einfo = new ElementInfo();
                einfo.ElementTypeEnum = eElementType.Iframe;
                Act actUI = PlatformInfo.GetPlatformAction(einfo, actConfig);
                if (actUI != null)
                {
                    AddActionToBusinessFlow(actUI, null);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while adding browser action", ex);
            }
        }

        private void PlatformDriver_RecordingEvent(object sender, RecordingEventArgs args)
        {
            try
            {
                switch (args.EventType)
                {
                    case eRecordingEvent.ElementRecorded:
                        ElementActionCongifuration elementAction = (ElementActionCongifuration)args.EventArgs;
                        ElementRecordedHandler(elementAction);
                        break;
                    case eRecordingEvent.PageChanged:
                        PageChangedEventArgs pageChanged = (PageChangedEventArgs)args.EventArgs;
                        PlatformDriverPageChangedHandler(pageChanged);
                        break;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Element recording event handler while recording", ex);
            }
        }

        private void ElementRecordedHandler(ElementActionCongifuration args)
        {
            try
            {
                Act actUI;
                ElementInfo einfo = null;
                if (args.LearnedElementInfo != null)
                {
                    einfo = (ElementInfo)args.LearnedElementInfo;
                    args.AddPOMToAction = CreatePOM;
                    args.POMGuid = CurrentPOM.Guid.ToString();
                    args.ElementGuid = einfo.Guid.ToString();

                    actUI = PlatformInfo.GetPlatformAction(einfo, args);
                }
                else
                {
                    actUI = PlatformInfo.GetPlatformAction(null, args);
                }
                AddActionToBusinessFlow(actUI, einfo);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Element recording event handler while recording", ex);
            }
        }

        private void AddActionToBusinessFlow(Act actUI, ElementInfo einfo)
        {
            try
            {
                if (actUI != null)
                {
                    if (CurrentPOM != null && einfo != null)
                    {
                        CurrentPOM.MappedUIElements.Add(einfo);
                    }
                    actUI.Context = Context;
                    BusinessFlow.AddAct(actUI);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while adding action to business flow", ex);
            }
        }

        public void StartRecording()
        {
            try
            {
                PlatformDriver.StartRecording(LearnAdditionalDetails);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Start recording", ex);
            }
        }

        public void StopRecording()
        {
            try
            {
                PlatformDriver.StopRecording();                
                if (ListPOMObjectHelper != null)
                {                    
                    foreach (var cPom in ListPOMObjectHelper)
                    {                        
                        //if (!string.IsNullOrEmpty(cPom.PageTitle) && !string.IsNullOrEmpty(cPom.PageURL))
                        //{
                        //    try
                        //    {
                        //        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(cPom.ApplicationPOM);
                        //    }
                        //    catch (Exception e)
                        //    {                                
                        //        Reporter.ToLog(eLogLevel.ERROR, "Error while saving the POM", e);
                        //    }                            
                        //}
                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(cPom.ApplicationPOM);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Stop recording", ex);
            }
        }
    }
}
