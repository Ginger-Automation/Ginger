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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GingerPluginCoreTest.CommunicationProtocol.WebPlatformServiceFakeLib
{
    public class WebTextBoxFake : ITextBox
    {
        string txt;
        public object Element { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void ClearValue()
        {
            throw new NotImplementedException();
        }

        public void DragAndDrop(string DragDropType, IGingerWebElement Element)
        {
            throw new NotImplementedException();
        }

        public string GetAttribute(string attributeName)
        {
            throw new NotImplementedException();
        }

        public string GetFont()
        {
            throw new NotImplementedException();
        }

        public int GetHeight()
        {
            throw new NotImplementedException();
        }

        public Size GetSize()
        {
            throw new NotImplementedException();
        }

        public string GetStyle()
        {
            throw new NotImplementedException();
        }

        public string GetText()
        {
            return txt;
        }

        public int GetTextLength()
        {
            throw new NotImplementedException();
        }

        public string GetValue()
        {
            throw new NotImplementedException();
        }

        public int GetWidth()
        {
            throw new NotImplementedException();
        }

        public void Hover()
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled()
        {
            throw new NotImplementedException();
        }

        public bool IsValuePopulated()
        {
            throw new NotImplementedException();
        }

        public bool IsVisible()
        {
            throw new NotImplementedException();
        }

        public void RightClick()
        {
            throw new NotImplementedException();
        }

        public string RunJavascript(string Script)
        {
            throw new NotImplementedException();
        }

        public void ScrollToElement()
        {
            throw new NotImplementedException();
        }

        public void SendKeys(string keys)
        {
            throw new NotImplementedException();
        }

        public void SetFocus()
        {
            throw new NotImplementedException();
        }

        public void SetText(string Text)
        {
            txt = Text;
        }

        public void SetValue(string Text)
        {
            throw new NotImplementedException();
        }
    }
}
