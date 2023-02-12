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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.WebServicesDriver
{
    public class WebserviceDriverConsoleReporter : IWebserviceDriverWindow
    {
        public IDispatcher GingerDispatcher { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
