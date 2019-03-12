#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System.Runtime.InteropServices;
using GingerCore.Drivers.CommunicationProtocol;

namespace GingerCore
{

    [Guid("EAA4976A-45C7-4BC7-BC0B-E474F4C3C77F")]
    public interface IQTPConnector
    {
        void GetAction(out string Action, out string LocateBy, out string LocateVal, out string Value);
    }

    [Guid("7BD20046-DF8C-77A6-8F6B-687FAA26FA71"),
     InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IQTPConnectorEvents
    {
        void GetAction(out string Action, out string LocateBy, out string LocateVal, out string Value);
    }

    [ComVisible(true)]
    [Guid("0D53A3E8-E51A-47C7-977E-E72A2064F938")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(IQTPConnectorEvents))]
    public class QTPConnector : IQTPConnector
    {

        public static bool QTPConnected = false;

        private GingerSocketServer AL;

        [ComVisible(true)]
        public void Connect()
        {
            AL = new GingerSocketServer();
            AL.StratServer(7002);

            //TODO run on task since blocking... Task.Factory            
            QTPConnected = true;
        }

        [ComVisible(true)]
        public void Disconnect()
        {
            QTPConnected = false;
        }

        int i = 0;
        [ComVisible(true)]
        public void GetAction(out string Action, out string LocateBy, out string LocateVal, out string Value)
        {
            


                Action = "";
                LocateBy = "";
                LocateVal = "";
                Value = "";
                
                i++;
                
                if (i == 1)
                {
                    Action = "GotoURL";
                    LocateBy = "NA";
                    LocateVal = "";
                    Value = "http://www.Priceline.com";                        
                }
                if (i == 2)
                {
                    Action = "WebEdit";
                    LocateBy = "ID";
                    LocateVal = "hotel-dest";
                    Value = "Tel Aviv";                        
                }
                
                if (i == 4)
                {
                    Action = "Close";
                    LocateBy = "";
                    LocateVal = "";
                    Value = "";
                }
                if (i == 10) Action = "Bye";
        }
    }
}