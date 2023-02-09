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

using Gherkin.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.UserControlsLib.TextEditor.Gherkin
{
    public class GherkinTag
    {
        Tag mGherkinTag;
        public GherkinTag(Tag GT)
        {
            mGherkinTag = GT;            
        }
        public string Name { get { return mGherkinTag.Name; } }

        public int Line { get { return mGherkinTag.Location.Line ; } }

        public int Column { get { return mGherkinTag.Location.Column; } }

        public static class Fields
        {
            public static string Name = "Name";
            public static string Line = "Line";
            public static string Column = "Column";
        }
    }
}
