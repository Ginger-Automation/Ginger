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
            try
            {
                string actionType = ActionPayload.GetValueString();

                // TODO: split to class and functions, or we use smart reflection to redirect the action
                Dictionary<string, string> InputParams = new Dictionary<string, string>();
                List<NewPayLoad> FieldsandParams = ActionPayload.GetListPayLoad();

                NewPayLoad PomPayload = null;
                int i = 0;
                if (FieldsandParams[0].Name == "POMPayload")
                {
                    i = 1;
                    PomPayload = FieldsandParams[0];
                }

                for (; i < FieldsandParams.Count; i++)
                {
                    NewPayLoad Np = FieldsandParams[i];
                    string Name = Np.GetValueString();

                    string Value = Np.GetValueString();
                    if (!InputParams.ContainsKey(Name))
                    {
                        InputParams.Add(Name, Value);
                    }
                }
                //foreach (NewPayLoad Np in FieldsandParams)
                //{
                //    string Name = Np.GetValueString();

                //    string Value = Np.GetValueString();
                //    if (!InputParams.ContainsKey(Name))
                //    {
                //        InputParams.Add(Name, Value);
                //    }
                //}
                IWebPlatform PlatformService = null;
                if (service is IWebPlatform Mservice)
                {
                    PlatformService = Mservice;
                }


                if (actionType == "BrowserAction")
                {



                    BrowserActionhandler Handler = new BrowserActionhandler(PlatformService, InputParams);


                    Handler.ExecuteAction();


                    NewPayLoad PLRC = CreateActionResult(Handler.ExecutionInfo, Handler.Error, Handler.AOVs);
                    return PLRC;
                }

                if (actionType == "UIElementAction")
                {
                    try
                    {




                        UIELementActionHandler Handler = new UIELementActionHandler(PlatformService, InputParams);

                        Handler.PrepareforExecution(PomPayload);

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
            catch(Exception ex)
            {
                return NewPayLoad.Error(ex.Message + System.Environment.NewLine +ex.StackTrace);
            }

        }

     

        private NewPayLoad CreateActionResult(string exInfo, string error, List<NodeActionOutputValue> outputValues)
        {
            return GingerNode.CreateActionResult(exInfo, error, outputValues);
            
        }

    }
}
