#region License
/*
Copyright © 2014-2018 European Support Limited

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

using GingerPlugIns.ActionsLib;
using PactNet.Mocks.MockHttpService.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace GingerPACTPlugIn.ActionsLib
{
    public class PACTActions : PlugInActionsBase
    {
        const string cStart = "Start";
        const string cStop = "Stop";
        const string cAddInteraction = "AddInteraction";
        const string cLoadInteractionsFile = "LoadInteractionsFile";
        const string cLoadInteractionsFolder = "LoadInteractionsFolder";
        const string cClearInteractions = "ClearInteractions";
        const string cSaveInteractions = "SaveInteractions";

        ServiceVirtualization SV;

        public override List<PlugInAction> Actions()
        {
            List<PlugInAction> list = new List<PlugInAction>();

            list.Add(new PlugInAction() { ID = cStart , Description = "Start Mock Server", EditPage = "GingerPACTPlugIn.ActionEditPages.Start", UserDescription = "Start the PACT Server, Pact server is a stand-alone interactions reorder and verifier", UserRecommendedUseCase = "Should be used for starting the PACT server in order for it to be ready to get Interactions load." });

            list.Add(new PlugInAction() { ID = cLoadInteractionsFile, Description = "Load Interactions from JSON File", UserRecommendedUseCase = "Load Interaction in order to fire real API requests against it.", EditPage = "GingerPACTPlugIn.ActionEditPages.LoadInteractionsFile", UserDescription = "Loading a Pact server Interactions (expected requests & responses) from JSON format file" });

            list.Add(new PlugInAction() { ID = cClearInteractions, UserRecommendedUseCase = "Should be used for clearing all existing Interactions before loading new ones.", Description = "Clear Server Interactions", UserDescription = "Clear all pre-defined PACT server Interactions" });   // no edit page simple action

            list.Add(new PlugInAction() { ID = cStop,  Description = "Stop Mock Server", UserRecommendedUseCase = "Should be used for stopping the PACT server", UserDescription = "Stops the PACT Server" });
            
            return list;
        }

        public override void RunAction(GingerAction act)
        {
            switch (act.ID)
            {
                case cStart:
                    if (SV != null)
                    {
                        SV.MockProviderService.Stop();
                    }
                    string port = "0";
                    if (!Boolean.Parse(act.GetOrCreateParam("DynamicPort").Value.ToString()))
                        port = act.GetOrCreateParam("Port").GetValueAsString();
                    SV = new ServiceVirtualization(int.Parse(port));

                    act.ExInfo = "Mock Service Started: " + SV.MockProviderServiceBaseUri;
                    
                    break;
                case cStop:
                    if (SV == null)
                    {
                        act.Error += "Error: Service Virtualization not started yet";
                        return;
                    }
                    
                    SV.MockProviderService.Stop();
                    SV = null;
                    act.ExInfo += "Mock Server stopped";
                    break;
                case cLoadInteractionsFile:
                    if (SV == null)
                    {
                        act.Error += "Error: Service Virtualization not started yet";
                        return;
                    }
                    string s = act.GetOrCreateParam("FileName").GetValueAsString();
                    if (s.StartsWith("~"))
                        s = Path.GetFullPath(s.Replace("~", act.SolutionFolder));
                    if (File.Exists(s))
                    {
                        int count = SV.LoadInteractions(s);
                        act.ExInfo += "Interaction file loaded: '" + s + "', " + count + " Interactions loaded";
                        act.AddOutput("Interaction Loaded", count + "");
                    }
                    else
                    {
                        act.Error += "Interaction file not found - " + s;
                    }
                    break;
                case cLoadInteractionsFolder:
                    if (SV == null)
                    {
                        act.Error += "Error: Service Virtualization not started yet";
                        return;
                    }
                    break;
                case cClearInteractions:
                    if (SV == null)
                    {
                        act.Error += "Error: Service Virtualization not started yet";
                        return;
                    }
                    SV.ClearInteractions();
                    act.ExInfo += "Interactions cleared";
                    break;
                case cAddInteraction:
                    if (SV == null)
                    {
                        act.Error += "Error: Service Virtualization not started yet";
                        return;
                    }

                    //TODO: get it from the edit page
                    ProviderServiceInteraction PSI = new ProviderServiceInteraction();
                    PSI.ProviderState = act.GetOrCreateParam("ProviderState").GetValueAsString();
                    PSI.Description = act.GetOrCreateParam("Description").GetValueAsString();
                    PSI.Request = new ProviderServiceRequest();
                    

                    string HTTPType = act.GetOrCreateParam("RequestType").GetValueAsString();
                    switch (HTTPType)
                    {
                        case "Get":
                            PSI.Request.Method = HttpVerb.Get;
                            break;
                        case "Post":
                            PSI.Request.Method = HttpVerb.Post;
                            break;
                                case "PUT":
                            PSI.Request.Method = HttpVerb.Put;
                            break;

                            //TODO: add all the rest and include default for err
                    }

                    PSI.Request.Path = act.GetOrCreateParam("Path").GetValueAsString();
                    Dictionary<string, string> d = new Dictionary<string, string>();
                    d.Add("Accept", "application/json");  //TODO: fixme
                    PSI.Request.Headers = d;
                    PSI.Response = new ProviderServiceResponse();
                    PSI.Response.Status = 200;  //TODO: fixme
                    Dictionary<string, string> r = new Dictionary<string, string>();
                    r.Add("Content-Type", "application/json; charset=utf-8");   //TODO: fixme
                    PSI.Response.Headers = r;
                    PSI.Response.Body = act.GetOrCreateParam("ResposneBody").GetValueAsString();

                    SV.AddInteraction(PSI);

                    act.ExInfo += "Interaction added";

                    break;
                case cSaveInteractions:
                    SV.SaveInteractions();
                    act.ExInfo += "Interactions saved to file";
                    // TODO: add file name to ex info
                    break;
                default:
                    act.Error += "Unknown PlugIn action ID - " + act.ID;
                    break;
            }
        }
    }
}
