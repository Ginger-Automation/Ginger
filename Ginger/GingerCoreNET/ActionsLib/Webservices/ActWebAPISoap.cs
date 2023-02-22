#region License
/*
Copyright © 2014-2023 European Support Limited

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

using GingerCore.Helpers;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run;
using GingerCore.Platforms;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;

namespace GingerCore.Actions.WebServices
{
    public class ActWebAPISoap : ActWebAPIBase, IActPluginExecution
    {
        public override string ActionDescription { get { return "WebAPI SOAP Action"; } }
        public override string ActionUserDescription { get { return "Performs SOAP action"; } }
        public override string ActionEditPage { get { return "WebServices.ActWebAPIEditPage"; } }
        public override bool ValueConfigsNeeded { get { return false; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool IsSelectableAction { get { return true; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH) 
        {
            TBH.AddText("Use this action in case you want to perform  a SOAP Action.");
            TBH.AddLineBreak();
            TBH.AddText("Enter End Point URL, SOAP Action, File location/Request Body, select security level and authorization if required.");
        }

        public PlatformAction GetAsPlatformAction()
        {
            PlatformAction platformAction = new PlatformAction(this);


            string SoapAction = this.GetInputParamCalculatedValue(Fields.SOAPAction);

            foreach (ActInputValue aiv in this.InputValues)
            {

                string ValueforDriver = aiv.ValueForDriver;
                if (!platformAction.InputParams.ContainsKey(aiv.Param) && !String.IsNullOrEmpty(ValueforDriver))
                {
                    platformAction.InputParams.Add(aiv.Param, ValueforDriver);
                }
            }
            if (platformAction.InputParams.ContainsKey("RequestBody"))
            {
                platformAction.InputParams["RequestBody"] = GetCalulatedRequestBodyString();

            }
            else
            {
                platformAction.InputParams.Add("RequestBody", GetCalulatedRequestBodyString());
            }

            Dictionary<string, string> sHttpHeaders = new Dictionary<string, string>();

            sHttpHeaders.Add("SOAPAction", SoapAction);
            foreach (ActInputValue header in this.HttpHeaders)
            {


                sHttpHeaders.Add(header.Param, header.ValueForDriver);

            }

            platformAction.InputParams.Add("Headers", sHttpHeaders);
     


           


            return platformAction;
        }

        public string GetName()
        {
            return "ActWebAPISoap";
        }

        public new static partial class Fields
        {
            public static string SOAPAction = "SOAPAction";
        }
    }
}
