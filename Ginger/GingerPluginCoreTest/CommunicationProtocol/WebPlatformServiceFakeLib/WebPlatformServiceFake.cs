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
