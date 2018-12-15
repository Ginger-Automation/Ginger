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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amdocs.Ginger.Repository;
using System.Linq;

namespace Amdocs.Ginger.Common.GeneralLib
{
    public static class General
    {
        public static string LocalUserApplicationDataFolderPath
        {
            get
            {
                //TODO: check where it goes - not roaming,.,
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                appDataFolder = Path.Combine(appDataFolder, @"Amdocs\Ginger");

                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                }

                return appDataFolder;
            }
        }

        public static string DefualtUserLocalWorkingFolder
        {
            get
            {
                string workingFolder = Path.Combine(LocalUserApplicationDataFolderPath, @"\WorkingFolder");

                if (!Directory.Exists(workingFolder))
                {
                    Directory.CreateDirectory(workingFolder);
                }

                return workingFolder;
            }
        }

        /// <summary>
        /// Should use the function temporary till solution will be implemented for VE fields search
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool IsFieldToAvoidInVeFieldSearch(string fieldName)
        {
            if (fieldName == "BackupDic" || fieldName == "GetNameForFileName" || fieldName == "FilePath" || fieldName == "FileName" ||
                fieldName == "ObjFileExt" || fieldName == "ItemNameField" || fieldName == "ItemImageType" || fieldName == "ItemName" ||
                fieldName == "RelativeFilePath" ||
                fieldName == "ObjFolderName" || fieldName == "ContainingFolder" || fieldName == "ContainingFolderFullPath" ||
                fieldName == "ActInputValues" || fieldName == "ActReturnValues" || fieldName == "ActFlowControls" ||
                fieldName == "ScreenShots" ||
                fieldName == "ListStringValue" || fieldName == "ListDynamicValue")
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public static void AddOrUpdateInputParamValue(string Param, string Value, ObservableList<ActInputValue> actInputValue)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActInputValue AIV = (from aiv in actInputValue where aiv.Param == Param select aiv).FirstOrDefault();
            if (AIV == null)
            {
                AIV = new ActInputValue();
                AIV.Param = Param;
                actInputValue.Add(AIV);
            }

            AIV.Value = Value;
        }       
    }
}
