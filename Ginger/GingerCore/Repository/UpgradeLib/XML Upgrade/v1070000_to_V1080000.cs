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
    class v1070000_to_V1080000 : XMLConverterBase
    {
        public override void Convert()
        {
            string outputxml = "";
            UpdateXMLVersion("1.8.0.0");
            outputxml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + xmlDoc.ToString();
            UpdatedXML = outputxml;
        }
    }
}
