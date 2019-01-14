using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common
{
    public class UserMsg
    {
        public UserMsg(eUserMsgType MessageType, string Caption, string Message, eUserMsgOption SelectionOptions, eUserMsgSelection DefualtSelectionOption)
        {
            this.MessageType = MessageType;
            this.Caption = Caption;
            this.Message = Message;
            this.SelectionOptions = SelectionOptions;
            this.DefualtSelectionOption = DefualtSelectionOption;
        }

        public eUserMsgType MessageType { get; set; }
        public string Caption { get; set; }
        public string Message { get; set; }
        public eUserMsgOption SelectionOptions { get; set; }
        public eUserMsgSelection DefualtSelectionOption { get; set; }
    }
}
