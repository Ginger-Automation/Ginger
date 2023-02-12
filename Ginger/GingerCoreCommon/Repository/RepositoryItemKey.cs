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

namespace Amdocs.Ginger.Repository
{
    //Class to create link to repository item by GUID - which is saved - serialized and contain also the item name in case needed for analyzer when target item Guid not found can auto repair

    public class RepositoryItemKey 
    {        
        public Guid Guid { get; set; }
        public string ItemName { get; set; }
                

        public string Key
        {
            get
            {
                return ItemName + "~" + Guid.ToString();
            }

            set
            {
                string[] a = value.Split('~');
                ItemName = a[0];
                Guid = new Guid(a[1]);

            }
        }

        public override string ToString()
        {
            return Key;
        }

        
    }
}
