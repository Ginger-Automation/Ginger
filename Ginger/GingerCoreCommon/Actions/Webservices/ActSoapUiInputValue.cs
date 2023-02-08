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

namespace GingerCore.Actions.WebServices
{
    public class ActSoapUiInputValue : ActInputValue
    {
       
        public enum ePropertyType
        {
            System,
            Global,
            Project,
            TestSuite,
            TestCase,
            TestStep
        }
        [IsSerializedForLocalRepository]
        public string Type { get; set; }
       public ActSoapUiInputValue(ePropertyType properTpe, ActInputValue actInputValue)
        {
            this.Type = properTpe.ToString();
            this.Param = actInputValue.Param;
            this.Value = actInputValue.Value;
            this.ValueForDriver = actInputValue.Value;
        }
       public ActSoapUiInputValue() { }
    }
}
