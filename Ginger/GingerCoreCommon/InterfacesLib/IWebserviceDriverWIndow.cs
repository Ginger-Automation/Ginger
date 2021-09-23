using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IWebserviceDriverWindow
    {
        IDispatcher GingerDispatcher { get; }
        void ShowDriverWindow();
        void Close();
        void UpdateResponseTextBox(string responseCode);
        void updateResponseXMLText(string v);
        void UpdateStatusLabel(string status);
        void UpdateRequestParams(string uRL, string value, string mRequest);
    }
}
