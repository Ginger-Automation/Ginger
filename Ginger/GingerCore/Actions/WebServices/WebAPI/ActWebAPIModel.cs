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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Helpers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions.WebServices.WebAPI
{
    public class ActWebAPIModel : Act
    {
        public override String ActionType
        {
            get
            {
                return "ActWebAPIModel";
            }
        }

        public override string ActionDescription { get { return "Web API Model Action"; } }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

        public override bool ValueConfigsNeeded { get { return false; } }

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.WebServices);
                }
                return mPlatforms;
            }
        }

        public override string ActionEditPage { get { return "WebServices.ActWebAPIModelEditPage"; } }

        public override string AddActionWizardPage { get { return "Ginger.ApiModelsFolder.AddApiModelActionWizardPage"; } }

        public override string ActionUserDescription { get { return "Uses Application API Model template to performs SOAP/REST action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to use Application API Model template to perform a SOAP/REST Action.");
            TBH.AddLineBreak();
            TBH.AddText("Add your Application API Model you want to use and populate the placeholder grid according to the action you want to run");
            TBH.AddLineBreak();
            TBH.AddText("In order to create API Model please navigate to 'Resources' tab then select 'Application Models' Sub Option then select 'API Models' Sub Option then right click on the 'Application API Models' folder and select one of the option to add API's manually or from a document.");
        }

        [IsSerializedForLocalRepository]
        public Guid APImodelGUID
        {
            get
            {
                if (string.IsNullOrEmpty(GetOrCreateInputParam(nameof(APImodelGUID)).Value))
                    return new Guid();
                else   
                    return Guid.Parse(GetOrCreateInputParam(nameof(APImodelGUID)).Value);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(APImodelGUID), value.ToString());

            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<EnhancedActInputValue> APIModelParamsValue = new ObservableList<EnhancedActInputValue>();

        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = new List<ObservableList<ActInputValue>>();
            List<ActInputValue> AIVList = APIModelParamsValue.Cast<ActInputValue>().ToList();
            list.Add(General.ConvertListToObservableList(AIVList));
            return list;
        }

        public ObservableList<AppModelParameter> ActAppModelParameters;

        public override void CalculateModelParameterExpectedValue(ActReturnValue actReturnValue)
        {            
            if (actReturnValue.ExpectedCalculated.Contains("AppModelParam"))
            {                
                List<AppModelParameter> usedParams = ActAppModelParameters.Where(x => actReturnValue.ExpectedCalculated.Contains(x.PlaceHolder)).ToList();
                foreach (AppModelParameter param in usedParams)
                {
                    actReturnValue.ExpectedCalculated = actReturnValue.ExpectedCalculated.Replace(("{AppModelParam Name = " + param.PlaceHolder + "}"), param.ExecutionValue);
                }
            }
        }
    }
}
