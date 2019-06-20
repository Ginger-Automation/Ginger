using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using Ginger.Plugin.Platform.Web;
using Ginger.Plugin.Platform.Web.Elements;
using Ginger.Plugin.Platform.Web.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerPluginCoreTest.CommunicationProtocol.WebPlatformServiceFakeLib
{
    [GingerService(Id: "WebPlatformServiceFake", Description: "Web Platform Service Fake for testing")]
    public class WebPlatformServiceFake : IWebPlatform, IServiceSession
    {
        public bool AutomaticallyShiftIframe { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IBrowserActions BrowserActions { get; set; } = new BrowserActionsFake();

        public ILocateWebElement LocateWebElement { get; set; } = new LocateWebElementFake();

        public IAlerts Alerts => throw new NotImplementedException();

        //Y.W: if we use the generic then move from here !!!
        public IPlatformActionHandler PlatformActionHandler { get; set; } = new WebPlatformActionHandler();


        public void StartSession()
        {
            //throw new NotImplementedException();
        }

        public void StopSession()
        {
            //throw new NotImplementedException();
        }
    }
}
