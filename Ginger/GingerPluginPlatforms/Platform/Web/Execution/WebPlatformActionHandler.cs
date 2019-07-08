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
        /// <param name="platformAction"></param>
        /// <returns></returns>
        public void HandleRunAction(IPlatformService service, ref NodePlatformAction platformAction)
        {
            // add try catch !!!!!!!!!!


            IWebPlatform webPlatformService = (IWebPlatform)service;



            switch (platformAction.ActionType)
            {
                case "BrowserAction":
                    //TODO: cache
                    BrowserActionhandler Handler = new BrowserActionhandler(webPlatformService);
                    Handler.ExecuteAction(ref platformAction);
                    break;
                case "UIElementAction":

                    UIELementActionHandler Handler2 = new UIELementActionHandler(webPlatformService);
                    Handler2.ExecuteAction(ref platformAction);
                    break;
                default:
                    platformAction.error += "HandleRunAction: handler not found: " ;
                    break;
            }


        }

     

        private NewPayLoad CreateActionResult(NodePlatformAction platformAction)
        {
            return GingerNode.CreateActionResult(platformAction.exInfo, platformAction.error, null);              // platformAction.outputValues
        }

    }
}
