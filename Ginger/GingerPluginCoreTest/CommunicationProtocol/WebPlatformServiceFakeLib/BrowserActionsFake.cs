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

using Ginger.Plugin.Platform.Web;
using Ginger.Plugin.Platform.Web.Elements;
using System;
using System.Collections.Generic;

namespace GingerPluginCoreTest.CommunicationProtocol.WebPlatformServiceFakeLib
{
    public class BrowserActionsFake : IBrowserActions
    {

        string mURL;

        public void AcceptAlert()
        {
            throw new NotImplementedException();
        }

        public void AcceptMessageBox()
        {
            // dummy change 25 check AppVeyor VS 2019
            throw new NotImplementedException();
        }

        public void CloseCurrentTab()
        {
            throw new NotImplementedException();
        }

        public void CloseWindow()
        {
            throw new NotImplementedException();
        }

        public void DeleteAllCookies()
        {
            throw new NotImplementedException();
        }

        public void DismissAlert()
        {
            throw new NotImplementedException();
        }

        public void DismissMessageBox()
        {
            throw new NotImplementedException();
        }

        public object ExecuteScript(string script)
        {
            throw new NotImplementedException();
        }

        public void FullScreen()
        {
            throw new NotImplementedException();
        }

        public string GetAlertText()
        {
            throw new NotImplementedException();
        }

        public string GetCurrentUrl()
        {
            return mURL;
        }

        public string GetPageSource()
        {
            throw new NotImplementedException();
        }

        public string GetTitle()
        {
            throw new NotImplementedException();
        }

        public string GetWindowHandle()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<string> GetWindowHandles()
        {
            throw new NotImplementedException();
        }

        public void Maximize()
        {
            throw new NotImplementedException();
        }

        public void Minimize()
        {
            throw new NotImplementedException();
        }

        public void Navigate(string url, string OpenIn)
        {
            mURL = url;
        }

        public void NavigateBack()
        {
            throw new NotImplementedException();
        }

        public void NavigateForward()
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void SendAlertText(string Text)
        {
            throw new NotImplementedException();
        }

        public void SetAlertBoxText(string value)
        {
            throw new NotImplementedException();
        }

        public void SwitchToDefaultContent()
        {
            throw new NotImplementedException();
        }

        public void SwitchToFrame(IGingerWebElement WebElement)
        {
            throw new NotImplementedException();
        }

        public void SwitchToParentFrame()
        {
            throw new NotImplementedException();
        }
    }
}
