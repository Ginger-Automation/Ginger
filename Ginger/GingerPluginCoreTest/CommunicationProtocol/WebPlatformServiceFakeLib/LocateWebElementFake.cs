using Ginger.Plugin.Platform.Web.Elements;

namespace GingerPluginCoreTest.CommunicationProtocol.WebPlatformServiceFakeLib
{
    internal class LocateWebElementFake : ILocateWebElement
    {
        public IGingerWebElement LocateElementByCss(eElementType elementType, string LocateValue)
        {
            throw new System.NotImplementedException();
        }

        public IGingerWebElement LocateElementByID(eElementType elementType, string id)
        {
            if (elementType == eElementType.Button && id == "button1")
            {
                WebButtonFake gingerWebElement = new WebButtonFake();
                return gingerWebElement;
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