using Ginger.Plugin.Platform.Web.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerPluginCoreTest.CommunicationProtocol.WebPlatformServiceFakeLib
{
    public class WebButtonFake : GingerWebElementFake, IButton
    {
        public void Click()
        {
            
        }

        public void DoubleClick()
        {
            throw new NotImplementedException();
        }

        public string GetValue()
        {
            throw new NotImplementedException();
        }

        public void JavascriptClick()
        {
            throw new NotImplementedException();
        }

        public void MouseClick()
        {
            throw new NotImplementedException();
        }

        public void MultiClick()
        {
            throw new NotImplementedException();
        }

        public void Submit()
        {
            throw new NotImplementedException();
        }
    }
}
