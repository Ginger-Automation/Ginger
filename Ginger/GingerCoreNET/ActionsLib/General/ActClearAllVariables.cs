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

using Amdocs.Ginger.Repository;
using System.Collections.Generic;
using GingerCore.Helpers;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions
{
    public class ActClearAllVariables : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Clear All " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Action"; } }
        public override string ActionUserDescription { get { return "Resets all " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " value"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to reset all Business Flow " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " values.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To reset " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " values click on run action button.");
        }        

        public override string ActionEditPage { get { return "ActClearAllVariablesEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

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

        public override string ActionType
        {
            get { return "Clear All " + GingerDicser.GetTermResValue(eTermResKey.Variables); }
        }

        public string VariableName 
        {
            get
            {
                return GetOrCreateInputParam(nameof(VariableName)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(VariableName), value);
            }
        }

        public override void Execute()
        {
            foreach (VariableBase vb in RunOnBusinessFlow.GetBFandCurrentActivityVariabeles())
            {
                if (vb.GetType() == typeof(VariableString) && SetEmptyValue)
                    vb.Value = "";
                else
                    vb.ResetValue();
            }
        }        

        public bool SetEmptyValue
        {
            get
            {
                bool value = false;
                if (bool.TryParse(GetInputParamValue(nameof(SetEmptyValue)), out value) == true)
                    return value;
                else
                    return true;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetEmptyValue), value.ToString());
                OnPropertyChanged(nameof(SetEmptyValue));
            }
        }
    }
}
