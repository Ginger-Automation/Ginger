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

namespace Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol
{
    //TODO: move all messages hard coded string to here

    public static class SocketMessages
    {
        public const string Register = "Register";

        public const string Reserve = "Reserve";

        public const string Unregister = "Unregister";

        public const string ActionOutputValue = "O"; // Keep it short as used many times

        public const string FindNode = "FindNode";

        public const string SendToNode = "SendToNode";



    }
}
