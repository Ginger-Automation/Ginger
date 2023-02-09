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

using System.Drawing;
using Ginger.Plugin.Platform.Web.Elements;

namespace GingerPluginCoreTest.CommunicationProtocol.WebPlatformServiceFakeLib
{
    public class GingerWebElementFake : IGingerWebElement
    {
        public object Element { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public void DragAndDrop(string DragDropType, IGingerWebElement Element)
        {
            throw new System.NotImplementedException();
        }

        public string GetAttribute(string attributeName)
        {
            throw new System.NotImplementedException();
        }

        public int GetHeight()
        {
            throw new System.NotImplementedException();
        }

        public Size GetSize()
        {
            throw new System.NotImplementedException();
        }

        public string GetStyle()
        {
            throw new System.NotImplementedException();
        }

        public int GetWidth()
        {
            throw new System.NotImplementedException();
        }

        public void Hover()
        {
            throw new System.NotImplementedException();
        }

        public bool IsEnabled()
        {
            throw new System.NotImplementedException();
        }

        public bool IsVisible()
        {
            throw new System.NotImplementedException();
        }

        public void RightClick()
        {
            throw new System.NotImplementedException();
        }

        public string RunJavascript(string Script)
        {
            throw new System.NotImplementedException();
        }

        public void ScrollToElement()
        {
            throw new System.NotImplementedException();
        }

        public void SetFocus()
        {
            throw new System.NotImplementedException();
        }

       
    }
}