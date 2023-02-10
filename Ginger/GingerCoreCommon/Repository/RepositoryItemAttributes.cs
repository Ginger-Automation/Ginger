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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Repository
{
    class RepositoryItemAttributes
    {
        
    }

    public class IsSerializedForLocalRepositoryAttribute : Attribute
    {
        object mDefaultValue = null;

        public IsSerializedForLocalRepositoryAttribute()
        {

        }

        /// <summary>
        /// If Default value is set and the value in the attr is the same as default it will not be save to the xml, save space and load time
        /// </summary>
        /// <param name="DefaultValue">True or False</param>
        public IsSerializedForLocalRepositoryAttribute(bool DefaultValue)
        {
            mDefaultValue = DefaultValue;
        }

        /// <summary>
        /// If Default value is set and the value in the attr is the same as default it will not be save to the xml, save space and load time
        /// </summary>
        /// <param name="DefaultValue">Can be Enum value or any object which can be default for the attr</param>
        public IsSerializedForLocalRepositoryAttribute(object DefaultValue)
        {
            mDefaultValue = DefaultValue;
        }

        public override string ToString()
        {
            return "Is Serialized For Local Repository";
        }

        public object GetDefualtValue()
        {
            return mDefaultValue;
        }
    }

    

}
