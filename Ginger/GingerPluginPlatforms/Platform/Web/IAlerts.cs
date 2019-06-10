using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web
{
    public interface IAlerts
    {

        void DismissAlert();
        void AcceptAlert();
        string GetALertText();
        void SendAlertText(string Text);
    }
}
