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
using System.Text;

namespace Amdocs.Ginger.Repository
{
    
    public class RepositoryItemHeader  
    {
        // Keep it first attr since it will help find object based on Guid without reading other attr, write it first!
        public Guid ItemGuid { get; set; }
        // We keep the class type of the content obj RI in the header so when we want to read only headers first we will know the type of the RI from header        
        public string ItemType { get; set; }

        /// <summary>
        /// Ginger version which this item was used to save
        /// </summary>
        public string GingerVersion { get; set; }


        // Version of this object, each time a save to file is done and the saved file is different/dirty then it increase +1
        // First version is 1
        public int Version { get; set; }


        // UserID create this RI
        public string CreatedBy { get; set; }


        // Created time in UTC
        public DateTime Created { get; set; }

        // Last user updated this RI
        public string LastUpdateBy { get; set; }

        // Last time this file was save when update was done
        public DateTime LastUpdate { get; set; }

        

        //TODO: External ID - for example to QC - need to be only in class itself not here, BF only?
        // public string ExternalID { get; set; }


    }
}
