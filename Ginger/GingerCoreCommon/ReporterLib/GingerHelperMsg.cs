using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common
{
    public class GingerHelperMsg
    {
        public GingerHelperMsg(eGingerHelperMsgType MessageType, string MsgHeader, string MsgContent, bool ShowBtn = false, string BtnContent = "")
        {
            this.MessageType = MessageType;
            this.MsgHeader = MsgHeader;
            this.MsgContent = MsgContent;
            this.ShowBtn = ShowBtn;
            this.BtnContent = BtnContent;
        }



        public eGingerHelperMsgType MessageType { get; set; }
        public string MsgHeader { get; set; }
        public string MsgContent { get; set; }
        public bool ShowBtn { get; set; }
        public string BtnContent { get; set; }
    }
}
