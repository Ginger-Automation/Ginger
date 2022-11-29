using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using OpenQA.Selenium;
using System.Collections;
using System.Collections.Generic;

namespace GingerCoreNET.Drivers.CommonLib
{
    public class FriendlyLocatorElement
    {
        public string position { get; set; }

        public IWebElement FriendlyElement { get; set; }
    }
}
