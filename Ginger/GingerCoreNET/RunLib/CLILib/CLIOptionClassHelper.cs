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

using CommandLine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIOptionClassHelper
    {
        public const string FILENAME = "filename";

        public static string GetClassVerb<T>()
        {
            VerbAttribute verbAttr = typeof(T).GetCustomAttribute<VerbAttribute>();
            return verbAttr.Name;
        }

        public static string GetAttrShorName<T>(string name)
        {
            OptionAttribute attrOption = typeof(T).GetProperty(name).GetCustomAttribute<OptionAttribute>();
            return attrOption.ShortName;
        }

        public static string GetAttrLongName<T>(string name)
        {
            OptionAttribute attrOption = typeof(T).GetProperty(name).GetCustomAttribute<OptionAttribute>();
            return attrOption.LongName;
        }
    }
}
