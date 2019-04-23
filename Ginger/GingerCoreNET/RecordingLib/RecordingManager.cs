using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET
{
    public class RecordingManager
    {       
        private bool CreatePOM { get; set; }

        public List<POMObjectRecordingHelper> ListPOMObjectHelper { get; set; }
        
        public Context Context { get; set; }

        public IRecord PlatformDriver { get; set; }

        public ApplicationPOMModel CurrentPOM { get; set; }

        public IPlatformInfo PlatformInfo { get; set; }

        public RecordingManager(List<ApplicationPOMModel> lstApplicationPOM, Context context, IRecord platformDriver, IPlatformInfo pInfo)
        {
            try
            {
                PlatformInfo = pInfo;
                PlatformDriver = platformDriver;
                //if lstApplicationPOM == null then dont create POM or if applicationPOM.Name has some value then use the existing POM
                //or else create new POM
                if (lstApplicationPOM == null)
                {
                    PlatformDriver.LearnAdditionalDetails = false;
                    CreatePOM = false;
                    CurrentPOM = null;
                }
                else
                {
                    PlatformDriver.LearnAdditionalDetails = true;
                    CreatePOM = true;
                    CurrentPOM = lstApplicationPOM[0];
                    ListPOMObjectHelper = new List<POMObjectRecordingHelper>();
                    foreach (var cPom in lstApplicationPOM)
                    {
                        ListPOMObjectHelper.Add(new POMObjectRecordingHelper() { PageTitle = cPom.ItemName, PageURL = cPom.PageURL, ApplicationPOM = cPom });
                    }
                    PlatformDriver.PageChanged += PlatformDriver_PageChanged;
                }

                Context = context;
                PlatformDriver.ElementRecorded += PlatformDriver_ElementRecorded;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Recording Manager while instantiating", ex);
            }
        }

        private POMObjectRecordingHelper GetRecordingHelperObject(string pageTitle, string pageURL, string screenShot)
        {
            POMObjectRecordingHelper recordingHelper = new POMObjectRecordingHelper();
            try
            {
                if (CreatePOM)
                {
                    CurrentPOM = new ApplicationPOMModel();
                }
                CurrentPOM.FileName = pageTitle;
                CurrentPOM.FilePath = pageTitle;
                CurrentPOM.Name = pageTitle;
                CurrentPOM.Guid = new Guid();
                CurrentPOM.ItemName = pageTitle;
                CurrentPOM.PageURL = pageURL;
                CurrentPOM.ScreenShotImage = screenShot;
                CurrentPOM.MappedUIElements = new ObservableList<ElementInfo>();

                recordingHelper.PageTitle = pageTitle;
                recordingHelper.PageURL = pageURL;
                recordingHelper.ApplicationPOM = CurrentPOM;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in creating Recording object", ex);
            }
            return recordingHelper;
        }

        private void PlatformDriver_PageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                if (ListPOMObjectHelper != null && ListPOMObjectHelper.Count > 0)
                {
                    var obj = ListPOMObjectHelper.FirstOrDefault(s => s.PageTitle == e.PageTitle && s.PageURL == e.PageURL);
                    if (obj == null && !string.IsNullOrEmpty(e.PageTitle) && !string.IsNullOrEmpty(e.PageURL))
                    {
                        ListPOMObjectHelper.Add(GetRecordingHelperObject(e.PageTitle, e.PageURL, e.ScreenShot));
                    }
                    else if (!(CurrentPOM.PageURL == obj.PageURL && CurrentPOM.Name == obj.PageTitle))
                    {
                        CurrentPOM = obj.ApplicationPOM;
                    }
                }
                else
                {
                    ListPOMObjectHelper = new List<POMObjectRecordingHelper>();
                    ListPOMObjectHelper.Add(GetRecordingHelperObject(e.PageTitle, e.PageURL, e.ScreenShot));
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Page change event handler while recording", ex);
            }
        }

        private void PlatformDriver_ElementRecorded(object sender, ElementActionCongifuration e)
        {
            try
            {
                Act actUI;
                ElementInfo einfo = null;
                if (e.LearnedElementInfo != null)
                {
                    einfo = (ElementInfo)e.LearnedElementInfo;
                    e.AddPOMToAction = CreatePOM;
                    e.POMGuid = CurrentPOM.Guid.ToString();
                    e.ElementGuid = einfo.Guid.ToString();

                    actUI = PlatformInfo.GetPlatformAction(einfo, e);                    
                }
                else
                {
                    actUI = PlatformInfo.GetPlatformAction(null, e);
                }
                if (actUI != null)
                {
                    if (CurrentPOM != null && einfo != null)
                    {
                        CurrentPOM.MappedUIElements.Add(einfo);
                    }
                    Context.BusinessFlow.AddAct(actUI);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Element recording event handler while recording", ex);
            }
        }

        public void StartRecording()
        {
            try
            {
                PlatformDriver.StartRecording();
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
                        if (!string.IsNullOrEmpty(cPom.PageTitle) && !string.IsNullOrEmpty(cPom.PageURL))
                        {
                            try
                            {
                                PomLearnUtils utils = new PomLearnUtils(cPom.ApplicationPOM);
                                utils.SaveLearnedPOM();
                            }
                            catch (Exception)
                            {
                                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(cPom.ApplicationPOM);
                            }                            
                        }
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
