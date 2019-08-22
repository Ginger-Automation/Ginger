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

using GingerCore.Helpers;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions.WebServices
{
    public class ActWebAPISoap : ActWebAPIBase
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

        public new static partial class Fields
        {
            public static string SOAPAction = "SOAPAction";
        }
    }
}
