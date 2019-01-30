#region License
/*
Copyright © 2014-2018 European Support Limited

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
    // Processing status 
    // in no communication it is Ready
    // When sending request it is:
    // 1. SendingRequest
    // 2. WaitingForResponse
    // 3. ResponseStarted
    // 4. ResponseCompleted
    // Back to Ready

    // When request arrived
    // 1. ReceviedRequest
    // 1. ProcessingRequest
    // 1. SendingResponse
    // Back to Ready

    public enum eProcessingStatus
    {
        Ready,

        // When client send to server
        SendingRequest,
        WaitingForResponse,
        ResponseStarted,   // We got some part of the response waiting for more - len of bytes receveid is smaller than header length
        ResponseCompleted,
        // Back to Ready

        // when server send to client
        ReceviedRequest,
        ProcessingRequest,
        SendingResponse        
        // Back to Ready
    }
    
    public enum eGingerSocketProtocolMessageType
    {
        // First we do handshake
        GetVersion,

        // then we can send/recv Payloads
        PayLoad,
        CloseConnection,
        LostConnection,
        CommunicationError
    }
}
