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

using Ginger.Plugin.Platform.Web.Elements;

namespace GingerPluginCoreTest.CommunicationProtocol.WebPlatformServiceFakeLib
{
    internal class LocateWebElementFake : ILocateWebElement
    {
        public IGingerWebElement LocateElementByCss(eElementType elementType, string LocateValue)
        {
            throw new System.NotImplementedException();
        }


        WebTextBoxFake userTextBox;
        public IGingerWebElement LocateElementByID(eElementType elementType, string id)
        {
            if (elementType == eElementType.Button && id == "button1")
            {
                WebButtonFake gingerWebElement = new WebButtonFake();
                return gingerWebElement;
            }

            if (elementType == eElementType.TextBox && id == "user")
            {
                // Cache it so we can do get/set value on same object
                if (userTextBox == null)
                {
                    userTextBox = new WebTextBoxFake();
                }
                return userTextBox;
            }

            return null;
        }

        public IGingerWebElement LocateElementByLinkTest(eElementType elementType, string LocateValue)
        {
            throw new System.NotImplementedException();
        }

        public IGingerWebElement LocateElementByName(eElementType elementType, string locateByValue)
        {
            throw new System.NotImplementedException();
        }

        public IGingerWebElement LocateElementByPartiallinkText(eElementType elementType, string LocateValue)
        {
            throw new System.NotImplementedException();
        }

        public IGingerWebElement LocateElementByTag(eElementType elementType, string LocateValue)
        {
            throw new System.NotImplementedException();
        }

        public IGingerWebElement LocateElementByXPath(eElementType elementType, string xpath)
        {
            throw new System.NotImplementedException();
        }
    }
}