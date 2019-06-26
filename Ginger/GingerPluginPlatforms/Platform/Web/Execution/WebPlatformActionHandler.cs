using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.DriversLib;

namespace Ginger.Plugin.Platform.Web.Execution
{
    public class WebPlatformActionHandler : IPlatformActionHandler
    {
        public void HandleRunAction(IPlatformService service, ref NodePlatformAction platformAction)
        {
            IWebPlatform webPlatformService = (IWebPlatform)service;
            
            if (platformAction.ActionHandler == "BrowserActions")
            {
                //TODO: cache
                BrowserActionhandler Handler = new BrowserActionhandler(webPlatformService);                    
                Handler.ExecuteAction(ref platformAction);                    
            }


            // get the relevant handle from the service which will run the action
            //PropertyInfo pi = service.GetType().GetProperty(platformAction.ActionHandler);
            //object obj = pi.GetValue(service);

            //IPlatformActionHandler platformActionHandler = (IPlatformActionHandler)obj;
            //NewPayLoad actionResultPayload = platformActionHandler.HandleRunAction(service, platformAction);            
            //return actionResultPayload;


            //if (platformAction.ActionType == "BrowserAction")
            //{
            //    BrowserActionhandler Handler = new BrowserActionhandler(PlatformService, platformAction);
            //    Handler.ExecuteAction();
            //    NewPayLoad actionResultPayload = CreateActionResult(Handler.ExecutionInfo, Handler.Error, Handler.outputValues);
            //    return actionResultPayload;
            //}

            //if (platformAction.ActionType == "UIElementAction")
            //{
            //    try
            //    {
            //        UIELementActionHandler Handler = new UIELementActionHandler(PlatformService, platformAction);
            //        // Handler.PrepareforExecution(PomPayload);
            //        Handler.ExecuteAction();
            //        NewPayLoad PLRC = CreateActionResult(Handler.ExecutionInfo, Handler.Error, Handler.AOVs);
            //        return PLRC;

            //    }
            //    catch (Exception ex)
            //    {
            //        NewPayLoad newPayLoad = NewPayLoad.Error(ex.Message);
            //        return newPayLoad;
            //    }
            //}

            //NewPayLoad err = NewPayLoad.Error("RunPlatformAction: Unknown action type: " + platformAction.ActionType);
            //return err;

        }

     

        private NewPayLoad CreateActionResult(NodePlatformAction platformAction)
        {
            return GingerNode.CreateActionResult(platformAction.exInfo, platformAction.error, null);              // platformAction.outputValues
        }

    }
}
