using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using Ginger.Plugin.Platform.Web.Elements;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.DriversLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Execution
{
    public class WebPlatformActionHandler : IPlatformActionHandler
    {
        public NewPayLoad HandleRunAction(IPlatformService service, PlatformActionData platformAction)
        {
            //NewPayLoad payload = new NewPayLoad("rc");
            //payload.ClosePackage();
            //return payload;


            //PlatformAction platformAction = 


            // NewPayLoad pl2 = new NewPayLoad(ActionPayload.GetBytes());

            // string actionType = ActionPayload.GetValueString();


            // TODO: split to class and functions, or we use smart reflection to redirect the action
            //Dictionary<string, string> InputParams = new Dictionary<string, string>();
            //List<NewPayLoad> FieldsandParams = ActionPayload.GetListPayLoad();

            //NewPayLoad PomPayload=null;
            //int i = 0;
            //if(FieldsandParams[0].Name== "POMPayload")
            //{
            //    i = 1;
            //    PomPayload = FieldsandParams[0];
            //}

            //for (; i < FieldsandParams.Count-1; i++)
            //{
            //    NewPayLoad Np =FieldsandParams[i];
            //    string Name = Np.GetValueString();

            //    string Value = Np.GetValueString();
            //    if (!InputParams.ContainsKey(Name))
            //    {
            //        InputParams.Add(Name, Value);
            //    }
            //}
            ////foreach (NewPayLoad Np in FieldsandParams)
            ////{
            ////    string Name = Np.GetValueString();

            ////    string Value = Np.GetValueString();
            ////    if (!InputParams.ContainsKey(Name))
            ////    {
            ////        InputParams.Add(Name, Value);
            ////    }
            ////}
            //IWebPlatform PlatformService = null;
            //if (service is IWebPlatform Mservice)
            //{
            //    PlatformService = Mservice;
            //}

            IWebPlatform PlatformService = null;
            if (service is IWebPlatform Mservice)
            {
                PlatformService = Mservice;
            }


            if (platformAction.ActionType == "BrowserAction")
            {
                BrowserActionhandler Handler = new BrowserActionhandler(PlatformService, platformAction);
                Handler.ExecuteAction();
                NewPayLoad PLRC = CreateActionResult(Handler.ExecutionInfo, Handler.Error, Handler.AOVs);
                return PLRC;
            }

            if (platformAction.ActionType == "UIElementAction")
            {
                try
                {
                    UIELementActionHandler Handler = new UIELementActionHandler(PlatformService, platformAction);
                    // Handler.PrepareforExecution(PomPayload);
                    Handler.ExecuteAction();
                    NewPayLoad PLRC = CreateActionResult(Handler.ExecutionInfo, Handler.Error, Handler.AOVs);
                    return PLRC;

                }
                catch (Exception ex)
                {
                    NewPayLoad newPayLoad = NewPayLoad.Error(ex.Message);
                    return newPayLoad;
                }
            }

            NewPayLoad err = NewPayLoad.Error("RunPlatformAction: Unknown action type: " + platformAction.ActionType);
            return err;

        }

     

        private NewPayLoad CreateActionResult(string exInfo, string error, List<NodeActionOutputValue> outputValues)
        {
            return GingerNode.CreateActionResult(exInfo, error, outputValues);            
        }

    }
}
