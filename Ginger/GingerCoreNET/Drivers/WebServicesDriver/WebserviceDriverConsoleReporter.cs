using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.WebServicesDriver
{
    public class WebserviceDriverConsoleReporter : IWebserviceDriverWindow
    {
        public IDispatcher GingerDispatcher => throw new NotImplementedException();

        public void Close()
        {
            //not required for console
        }

        public void ShowDriverWindow()
        {//not required for console

        }

        public void UpdateRequestParams(string uRL, string value, string mRequest)
        {

            Reporter.ToConsole(eLogLevel.INFO, String.Format("Sending Execution on {0},{1},value {2},{3}, request body {4}.{5}", uRL, Environment.NewLine, value, Environment.NewLine, mRequest, Environment.NewLine));
        }

        public void UpdateResponseTextBox(string responseCode)
        {
            Reporter.ToConsole(eLogLevel.INFO, String.Format("Webservice driver Status Code {0},{1}", responseCode, Environment.NewLine));
        }

        public void updateResponseXMLText(string v)
        {
            Reporter.ToConsole(eLogLevel.INFO, String.Format("Webservice driver:Response Body {0},{1}", v, Environment.NewLine));
        }

        public void UpdateStatusLabel(string status)
        {
            Reporter.ToConsole(eLogLevel.INFO, String.Format("Webservice driver:Request Status {0},{1}", status, Environment.NewLine));
        }
    }
}
