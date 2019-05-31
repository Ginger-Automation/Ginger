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
        public NewPayLoad HandleRunAction(IPlatformService service, NewPayLoad ActionPayload)
        {
            string actionType = ActionPayload.GetValueString();

            // TODO: split to class and functions, or we use smart reflection to redirect the action
            Dictionary<string, string> InputParams = new Dictionary<string, string>();
            List<NewPayLoad> FieldsandParams = ActionPayload.GetListPayLoad();


            foreach (NewPayLoad Np in FieldsandParams)
            {
                string Name = Np.GetValueString();

                string Value = Np.GetValueString();
                if (!InputParams.ContainsKey(Name))
                {
                    InputParams.Add(Name, Value);
                }
            }
            IWebPlatform PlatformService = null;
            if (service is IWebPlatform Mservice)
            {
                PlatformService = Mservice;
            }
            if (actionType == "BrowserAction")
            {



                BrowserActionhandler Handler = new BrowserActionhandler(PlatformService, InputParams);
                Handler.ExecuteAction();


                NewPayLoad PLRC = CreateActionResult(Handler.ExecutionInfo,Handler.Error, Handler.AOVs);
                return PLRC;
            }

            if (actionType == "UIElementAction")
            {
                try
                {
                  


                 
                    UIELementActionHandler Handler = new UIELementActionHandler(PlatformService,InputParams);

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

            

            NewPayLoad err = NewPayLoad.Error("RunPlatformAction: Unknown action type: " + actionType);
            return err;


            
        }




        private NewPayLoad CreateActionResult(string exInfo, string error, List<NodeActionOutputValue> outputValues)
        {
            return GingerNode.CreateActionResult(exInfo, error, outputValues);
            
        }

    }
}
