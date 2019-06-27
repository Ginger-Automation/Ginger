using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.DriversLib;
using System.Reflection;

namespace Ginger.Plugin.Platform.Web.Execution
{
    public class WebPlatformActionHandler : IPlatformActionHandler
    {
        public void HandleRunAction(IPlatformService service, ref NodePlatformAction platformAction)
        {
            // add try catch !!!!!!!!!!


            IWebPlatform webPlatformService = (IWebPlatform)service;


            if (platformAction.ActionHandler == "BrowserActions")
            {
                //TODO: cache
                BrowserActionhandler Handler = new BrowserActionhandler(webPlatformService);
                Handler.ExecuteAction(ref platformAction);
            }

            // using reflection get the attr and run
            // get the relevant handle from the service which will run the action
            //PropertyInfo pi = service.GetType().GetProperty(platformAction.ActionHandler);
            //object obj = pi.GetValue(service);

            //IActionHandler platformActionHandler = (IActionHandler)obj;
            //platformActionHandler.ExecuteAction(ref platformAction);
            //NewPayLoad actionResultPayload = platformActionHandler.HandleRunAction(service, platformAction);            
            //return actionResultPayload;



            if (platformAction.ActionType == "UIElementAction")
            {
                    UIELementActionHandler Handler = new UIELementActionHandler(webPlatformService);
                    // Handler.PrepareforExecution(PomPayload);
                    Handler.ExecuteAction(ref platformAction);
                    // NewPayLoad PLRC = CreateActionResult(Handler.ExecutionInfo, Handler.Error, Handler.AOVs);
                    // return PLRC;
            }

            //NewPayLoad err = NewPayLoad.Error("RunPlatformAction: Unknown action type: " + platformAction.ActionType);
            //return err;

            //catch (Exception ex)
            //{
            //    NewPayLoad newPayLoad = NewPayLoad.Error(ex.Message);
            //    return newPayLoad;
            //}


        }

     

        private NewPayLoad CreateActionResult(NodePlatformAction platformAction)
        {
            return GingerNode.CreateActionResult(platformAction.exInfo, platformAction.error, null);              // platformAction.outputValues
        }

    }
}
