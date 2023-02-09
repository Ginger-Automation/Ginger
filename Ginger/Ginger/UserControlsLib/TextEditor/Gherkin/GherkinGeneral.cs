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

using System.Linq;

namespace Ginger.GherkinLib
{
    public class GherkinGeneral
    {
        //TODO: make %p constant to use where needed, might decide on better naming for params, so user can change

        public static string GetActivityGherkinName(string Name)
        {
            string s = Name;
            //TODO: regex on the name to make it common for similar 
            int i = 0;
            while (s.Contains('"'))
            {
                i++;
                string num = General.GetStringBetween(s, "\"", "\"");
                if (num.StartsWith("<"))
                    s = s.Replace('"' + num + '"', num);
                else
                    s = s.Replace('"' + num + '"', "%p" + i);
            }
            return s;
        }
    }
}
