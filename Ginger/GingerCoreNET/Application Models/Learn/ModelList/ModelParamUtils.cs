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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.UserControlsLib;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Amdocs.Ginger.CoreNET.Application_Models
{
    public class ModelParamUtils
    {

        public static ObservableList<GlobalAppModelParameter> mModelsGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();

        

        public static void SetUniquePlaceHolderName(GlobalAppModelParameter newModelGlobalParam, bool isCopy = false)
        {
            if (isCopy)
            {
                newModelGlobalParam.PlaceHolder = "{" + (!string.IsNullOrEmpty(newModelGlobalParam.PlaceHolder) ? newModelGlobalParam.PlaceHolder.Replace("{","").Replace("}","") : newModelGlobalParam.PlaceHolder) + "_Copy}";
            }
            else if(string.IsNullOrEmpty(newModelGlobalParam.PlaceHolder))
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

                if(newModelGlobalParam.PlaceHolder.Contains(newModelGlobalParam.PlaceHolder))
                {
                    newModelGlobalParam.PlaceHolder = "{" + (!string.IsNullOrEmpty(newModelGlobalParam.PlaceHolder) ? newModelGlobalParam.PlaceHolder.Replace("{", "").Replace("}", "") : newModelGlobalParam.PlaceHolder) + counter.ToString() + "}";
                }
                else
                {
                    newModelGlobalParam.PlaceHolder = "{NewGlobalParameter_" + counter.ToString() + "}";
                }

                
            }
        }
    }
}
