#region License
/*
Copyright © 2014-2024 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.Application_Models
{
    public static class ModelParamUtils
    {
        public static void SetUniquePlaceHolderName(GlobalAppModelParameter newModelGlobalParam, bool isCopy = false)
        {
            var mModelsGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();
            if (isCopy)
            {
                newModelGlobalParam.PlaceHolder = "{" + (!string.IsNullOrEmpty(newModelGlobalParam.PlaceHolder) ? newModelGlobalParam.PlaceHolder.Replace("{", "").Replace("}", "") : newModelGlobalParam.PlaceHolder) + "_Copy}";
            }
            else if (string.IsNullOrEmpty(newModelGlobalParam.PlaceHolder))
            {
                newModelGlobalParam.PlaceHolder = "{NewGlobalParameter}";
            }

            if (mModelsGlobalParamsList.FirstOrDefault(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder) == null)
            {
                return;
            }

            List<GlobalAppModelParameter> samePlaceHolderList = mModelsGlobalParamsList.Where(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder).ToList<GlobalAppModelParameter>();
            if (samePlaceHolderList.Count == 1 && samePlaceHolderList[0] == newModelGlobalParam)
            {
                return; //Same internal object
            }

            //Set unique name
            if (isCopy)
            {
                if ((mModelsGlobalParamsList.FirstOrDefault(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder)) != null)
                {
                    int counter = 2;
                    while ((mModelsGlobalParamsList.FirstOrDefault(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder + counter)) != null)
                    {
                        counter++;
                    }

                    newModelGlobalParam.PlaceHolder = newModelGlobalParam.PlaceHolder + counter;
                }
            }
            else
            {
                int counter = 2;
                while ((mModelsGlobalParamsList.FirstOrDefault(x => x.PlaceHolder == "{NewGlobalParameter_" + counter.ToString() + "}")) != null)
                {
                    counter++;
                }

                if (newModelGlobalParam.PlaceHolder.Contains(newModelGlobalParam.PlaceHolder))
                {
                    while ((mModelsGlobalParamsList.FirstOrDefault(x => x.PlaceHolder == "{" + (!string.IsNullOrEmpty(newModelGlobalParam.PlaceHolder) ? newModelGlobalParam.PlaceHolder.Replace("{", "").Replace("}", "") : newModelGlobalParam.PlaceHolder) + counter.ToString() + "}")) != null)
                    {
                        counter++;
                    }
                    newModelGlobalParam.PlaceHolder = "{" + (!string.IsNullOrEmpty(newModelGlobalParam.PlaceHolder) ? newModelGlobalParam.PlaceHolder.Replace("{", "").Replace("}", "") : newModelGlobalParam.PlaceHolder) + counter.ToString() + "}";
                }
                else
                {
                    newModelGlobalParam.PlaceHolder = "{NewGlobalParameter_" + counter.ToString() + "}";
                }


            }
        }

        public static void AddGlobalParametertoAPIGlobalParameterList(ObservableList<GlobalAppModelParameter> APIGlobalParamList, GlobalAppModelParameter GAMP)
        {
            GlobalAppModelParameter newAPIGlobalParam = new GlobalAppModelParameter
            {
                Guid = GAMP.Guid,
                CurrentValue = GAMP.CurrentValue,
                PlaceHolder = GAMP.PlaceHolder,
                Description = GAMP.Description
            };
            foreach (OptionalValue ov in GAMP.OptionalValuesList)
            {
                OptionalValue newOV = new OptionalValue
                {
                    Guid = ov.Guid,
                    Value = ov.Value,
                    IsDefault = ov.IsDefault
                };
                newAPIGlobalParam.OptionalValuesList.Add(newOV);
            }
            APIGlobalParamList.Add(newAPIGlobalParam);
        }
    }
}
