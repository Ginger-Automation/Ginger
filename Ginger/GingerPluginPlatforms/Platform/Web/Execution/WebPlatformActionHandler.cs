using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.DriversLib;
using System.Reflection;

namespace Ginger.Plugin.Platform.Web.Execution
{
    /// <summary>
    /// Default Implementation of IPlatformActionHandler for WebPlatform. A Plugin Implementing IwebPlatform can use this.
    /// </summary>
    public class WebPlatformActionHandler : IPlatformActionHandler
    {
        /// <summary>
        /// It Takes an instance of IWebPlatform(ehich extends IPlatformService ) and Action payload and call the required functions for execution.
        /// Supported actions are Browser Action and Ui Element action with Page object Model Support
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="ActionPayload"></param>
        /// <returns></returns>
        public NewPayLoad HandleRunAction(IPlatformService service, NewPayLoad ActionPayload)
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
