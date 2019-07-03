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