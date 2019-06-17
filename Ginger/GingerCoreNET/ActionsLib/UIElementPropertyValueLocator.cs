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

namespace GingerCore.Actions.Common
{
    //TODO  change to ElementProprty  and put attr values

    public class UIElementPropertyValueLocator
    {
        public enum eLocatorOpertor
        {
            [EnumValueDescription("=")]
            EQ,
            [EnumValueDescription("Contains")]
            Contains,
            [EnumValueDescription("Regex")]
            Regex,
            [EnumValueDescription("Start With")]
            StartWith,
            [EnumValueDescription("End With")]            
            EndWith            
        }

        [IsSerializedForLocalRepository]
        public string Property;

        [IsSerializedForLocalRepository]
        public eLocatorOpertor Opertor;
   
        [IsSerializedForLocalRepository]
        public string Value;
    }
}
