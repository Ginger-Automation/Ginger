#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using GingerCore;
using GingerCore.Variables;
using System;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    public static class ParameterConfigHelper
    {

        public static void ValidateParameterConfig(ParameterConfig Parameter)
        {
            if (Parameter == null || string.IsNullOrEmpty(Parameter.Value) || string.IsNullOrEmpty(Parameter.Name))
            {
                throw new ArgumentException($"{nameof(Parameter)} Name or Value cannot be null or empty");
            }
        }

        public static VariableBase CreateParameterFromConfig(ParameterConfig Parameter)
        {
            ValidateParameterConfig(Parameter);
            if (EncryptionHandler.IsStringEncrypted(Parameter.Value))
            {
                Parameter.Value = EncryptionHandler.DecryptwithKey(Parameter.Value);
            }
            return new VariableDynamic()
            {
                ValueExpression = Parameter.Value,
                Name = Parameter.Name
            };
        }
        public static void UpdateParameterFromConfig(ParameterConfig ParameterConfig, ref VariableBase ParameterFromGinger)
        {
            ValidateParameterConfig(ParameterConfig);

            ParameterFromGinger.Name = ParameterConfig.Name;

            if (ParameterFromGinger is VariablePasswordString varpPassword)
            {
                if (!EncryptionHandler.IsStringEncrypted(ParameterConfig.Value))
                {
                    varpPassword.SetInitialValue(EncryptionHandler.EncryptwithKey(ParameterConfig.Value));
                }
                else
                {
                    varpPassword.SetInitialValue(ParameterConfig.Value);
                }
            }
            else
            {
                ParameterFromGinger.SetValue(ParameterConfig.Value);
            }
        }
    }
}
