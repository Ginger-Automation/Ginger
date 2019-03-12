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

namespace GingerCore.XMLConverters
{
    class v1050000_to_V1060000 : XMLConverterBase
    {
        /// <summary>
        /// Update v2 files as necessary 
        /// </summary>
        public override void Convert()
        {
            string outputxml = "";

            UpdateXMLVersion("1.6.0.0");

            #region parse using Linq
            switch (RepoType)
            {
                #region case GingerFileType.BusinessFlow:
                case eGingerFileType.BusinessFlow:
                    // ### Set all Activity's to active ###
                   
                    break;
                #endregion
                #region case GingerFileType.Activity
                case eGingerFileType.Activity:
                    
                    break;
                #endregion
            }
            #endregion

            outputxml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + xmlDoc.ToString();

            UpdatedXML = outputxml;
        }
    }
}
