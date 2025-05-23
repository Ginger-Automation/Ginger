#region License
/*
Copyright © 2014-2025 European Support Limited

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
