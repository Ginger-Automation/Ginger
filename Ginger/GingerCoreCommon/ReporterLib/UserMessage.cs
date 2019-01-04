using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common
{
    public class UserMessage
    {
        public UserMessage(eMessageType MessageType, string Caption, string Message, MessageBoxButton ButtonsType, MessageBoxResult DefualtResualt)
        {
            this.MessageType = MessageType;
            this.Caption = Caption;
            this.Message = Message;
            this.ButtonsType = ButtonsType;
            this.DefualtResualt = DefualtResualt;
        }

        public eMessageType MessageType { get; set; }
        public string Caption { get; set; }
        public string Message { get; set; }
        public MessageBoxButton ButtonsType { get; set; }
        public MessageBoxResult DefualtResualt { get; set; }
    }
}
