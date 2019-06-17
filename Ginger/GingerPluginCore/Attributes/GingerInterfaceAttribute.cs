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

using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{

    public class GingerInterfaceAttribute : Attribute
    {
        string mID;
        string mDescription;
        /// <summary>
        /// Define the interace as Ginger service/actions interface
        /// </summary>
        /// <param name="Id">Service Id is kept in the XML and should never change</param>
        /// <param name="Description">Description is displayed to the user when he select Service</param>
        /// 


        public GingerInterfaceAttribute(string Id, string Description)
        {
            mID = Id;
            mDescription = Description;
        }

      

        public string Id { get { return mID; } set { mID = value; } }
        public string Description { get { return mDescription; } set { mDescription = value; } }

        public override string ToString()
        {
            return "GingerInterface";
        }
    }
}
