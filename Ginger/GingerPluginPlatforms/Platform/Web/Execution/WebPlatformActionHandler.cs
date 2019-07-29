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
            // add try catch !!!!!!!!!

            IWebPlatform webPlatformService = (IWebPlatform)service;


            switch (platformAction.ActionHandler)
            {
                case "BrowserActions":
                    //TODO: cache
                    BrowserActionhandler Handler = new BrowserActionhandler(webPlatformService);
                    Handler.ExecuteAction(ref platformAction);
                    break;
                case "UIElementAction":                    
                    UIELementActionHandler Handler2 = new UIELementActionHandler(webPlatformService);                    
                    Handler2.ExecuteAction(ref platformAction);
                    break;
                default:
                    platformAction.error += "HandleRunAction: handler not found: " + platformAction.ActionHandler;
                    break;
            }

            // using reflection get the attr and run
            // get the relevant handle from the service which will run the action
            //PropertyInfo pi = service.GetType().GetProperty(platformAction.ActionHandler);
            //object obj = pi.GetValue(service);

            //IActionHandler platformActionHandler = (IActionHandler)obj;
            //platformActionHandler.ExecuteAction(ref platformAction);
            
        }

     

        private NewPayLoad CreateActionResult(NodePlatformAction platformAction)
        {
            return GingerNode.CreateActionResult(platformAction.exInfo, platformAction.error, null);              // platformAction.outputValues
        }

    }
}
