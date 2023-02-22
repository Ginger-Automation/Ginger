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
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions
{
    // This class is for Running functions    
    public class ActFunction : Act
    {
        public override string ActionDescription { get { return "Function Action (Obsolete - Do Not Use)"; } }
        public override string ActionUserDescription { get { return "Function Action (Obsolete - Do Not Use)"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("This action is used to test any function. Function Action (Obsolete - Do Not Use)");            
        }        

        public override string ActionEditPage { get { return null; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }
        
        public object returnValue;
        public object ReturnValue { get{return returnValue; } 
            set{                
                returnValue = value;
            } 
        }
        
        public override String ActionType
        {
            get
            {
                string Params = getParamsAsString();
                return "Function: " + LocateValue + "." + GetInputParamValue("Value") + "(" + Params + ")";
            }
        }

        private string getParamsAsString()
        {
            if (InputValues == null) return "";
            string s = "";
            foreach (var fp in InputValues)
            {
                if (s.Length > 0) s = s + ",";
                s = s + fp.Param + "=" + fp.Value;
            }
            return s;
        }

        public override bool IsSelectableAction { get { return false; } }

        internal object[] FuncParamsAsObjectsArray()
        {

            List<object> funcparams = new List<object>();
            if (InputValues == null) return funcparams.ToArray();

            foreach (var fp in InputValues)
            {               
                //TODO: assume string...
                funcparams.Add(fp.Value);
            }
            return funcparams.ToArray();
        }

    }
}
