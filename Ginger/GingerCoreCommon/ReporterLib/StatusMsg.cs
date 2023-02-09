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

using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common
{
    public class StatusMsg
    {
        public StatusMsg(eStatusMsgType MessageType, string MsgHeader, string MsgContent, bool ShowBtn = false, string BtnContent = "")
        {
            this.MessageType = MessageType;
            this.MsgHeader = MsgHeader;
            this.MsgContent = MsgContent;
            this.ShowBtn = ShowBtn;
            this.BtnContent = BtnContent;
        }

        public eStatusMsgType MessageType { get; set; }
        public string MsgHeader { get; set; }
        public string MsgContent { get; set; }
        public bool ShowBtn { get; set; }
        public string BtnContent { get; set; }
    }
}
