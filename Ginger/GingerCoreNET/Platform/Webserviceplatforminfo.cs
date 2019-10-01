﻿using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Platform
{
  public  class Webserviceplatforminfo : IPlatformPluginPostRun
    {
       private bool SaveRequest;
        private bool SaveResponse;
        private string PathToSave;
        public void PostExecute(Agent agent, Act actPlugin)
        {


            foreach (DriverConfigParam DCP in agent.DriverConfiguration)
           {
                switch (DCP.Parameter)
                {

                    case "Save Request":
                        Boolean.TryParse(DCP.Value,out SaveRequest);
                        break;
                    case "Save Response":
                        Boolean.TryParse(DCP.Value, out SaveResponse);
                        break;
                    case "Path To Save":
                        PathToSave = DCP.Value;
                        break;
                }
            }
            if(PathToSave.StartsWith(@"~\"))
            {
                PathToSave = Path.Combine(WorkSpace.Instance.Solution.ContainingFolderFullPath, PathToSave.Remove(0, 2));
            }

            String FileContent;
            if(SaveRequest)
            {
                FileContent= actPlugin.ReturnValues.Where(x => x.Param == "Request:").FirstOrDefault().Actual;
                SaveToFile("Request", FileContent, PathToSave,(ActWebAPIBase) actPlugin);
            }
            if(SaveResponse)
            {

                FileContent = actPlugin.ReturnValues.Where(x => x.Param == "Response:").FirstOrDefault().Actual;
                SaveToFile("Response", FileContent, PathToSave, (ActWebAPIBase)actPlugin);
            }



        }
        public static string SaveToFile(string fileType, string fileContent, string saveDirectory, ActWebAPIBase mAct)
        {
            string extension = string.Empty;
            string contentType = string.Empty;
            string actName = string.Empty;

            if (fileType == "Request")
            {
                contentType = mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType);
            }
            else if (fileType == "Response")
            {
                contentType = mAct.GetInputParamValue(ActWebAPIRest.Fields.ResponseContentType);
            }

            if (contentType == ApplicationAPIUtils.eContentType.XML.ToString())
            {
                extension = "xml";
            }
            else if (contentType == ApplicationAPIUtils.eContentType.JSon.ToString())
            {
                extension = "json";
            }
            else if (contentType == ApplicationAPIUtils.eContentType.PDF.ToString())
            {
                extension = "pdf";
            }
            else
            {
                extension = "txt";
            }

            string directoryFullPath = Path.Combine(saveDirectory.Replace("~//", WorkSpace.Instance.Solution.ContainingFolderFullPath), fileType + "s");
            if (!Directory.Exists(directoryFullPath))
            {
                Directory.CreateDirectory(directoryFullPath);
            }

            String timeStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
            actName = PathHelper.CleanInValidPathChars(mAct.Description);
            string fullFileName = Path.Combine(directoryFullPath, actName + "_" + timeStamp + "_" + fileType + "." + extension);

            if (contentType != ApplicationAPIUtils.eContentType.PDF.ToString())
            {
                File.WriteAllText(fullFileName, fileContent);
            }
            else
            {
                byte[] bytes = Encoding.Default.GetBytes(fileContent);
                File.WriteAllBytes(fullFileName, bytes);
            }

            return fullFileName;
        }





    }
}
