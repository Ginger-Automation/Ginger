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

namespace  Amdocs.Ginger.Plugin.Core
{
    // Annotation of [GingerAction] for function in driver we want to use as actions
    public class GingerActionAttribute : Attribute
    {
        string mId;
        string mDescription;

        /// <summary>
        /// Define the method Ginger action
        /// </summary>
        /// <param name="Id">Action Id is kept in the XML and should never change</param>
        /// <param name="Description">Description is displayed to the user when he select action to add</param>
        public GingerActionAttribute(string Id, string description)
        {
            mId = Id;
            mDescription = description;
        }

        public string Id {get {return mId; } set { mId = value; }  }
        public string Description { get { return mDescription; } set { mDescription = value; } }

        public override string ToString()
        {
            return "GingerAction";
        }
    }
}
