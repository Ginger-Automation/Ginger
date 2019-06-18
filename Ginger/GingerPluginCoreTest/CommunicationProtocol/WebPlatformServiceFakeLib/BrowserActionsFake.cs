using Ginger.Plugin.Platform.Web;
using Ginger.Plugin.Platform.Web.Elements;
using System;
using System.Collections.Generic;

namespace GingerPluginCoreTest.CommunicationProtocol.WebPlatformServiceFakeLib
{
    public class BrowserActionsFake : IBrowserActions
    {

        string mURL;

        public void AcceptMessageBox()
        {
            // dummy change 7 check AppVeyor VS 2019
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

        public string GetCurrentUrl()
        {
            return mURL;
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

        public void SetAlertBoxText(string value)
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
