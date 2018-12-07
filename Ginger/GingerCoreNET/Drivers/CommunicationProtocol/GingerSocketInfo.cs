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

using System;
using System.Net.Sockets;
using System.Threading;
using GingerCoreNET.Drivers.CommunicationProtocol;

namespace Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol
{
    public class GingerSocketInfo
    {
        private Socket mSocket = null;
        
        public Socket Socket { get { return mSocket; } set { mSocket = value; } }
        
        public const int InitialBufferSize = 1024; // Size of initial receive buffer.  

        // Unique session id
        public Guid SessionID { get; set; }

        // Receive buffer.  
        public byte[] buffer = new byte[InitialBufferSize];  // TODO: need to enable it to grow !!!!

        public int IncomingRequetsesCounter = 0;
        public int OutgoingRequetsesCounter = 0;        
        public int BytesIn = 0;
        public int bytesOut = 0;
        public int ResponseTimeoutMS = 1000*60*60;//1 hour
        
        // Class using it attach its handler to Action
        public Action<GingerSocketInfo> MessageHandler { get; set; }
        
        public NewPayLoad DataAsPayload
        {
            get
            {
                NewPayLoad rc = new NewPayLoad(buffer, true);                
                return rc;
            }
        }

        // We store the response here, TODO: why not to use only DataAsPayload or only this one - combine to best which is not creating new byte array        
        public NewPayLoad Response { get; set; }
        private int BufferPOS { get; set; }


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

        public eProcessingStatus mProcessingStatus = eProcessingStatus.Ready;
        private ManualResetEvent mRequestProcessingDone = new ManualResetEvent(false);
        private ManualResetEvent mSendDone = new ManualResetEvent(false);
        
        public NewPayLoad SendRequest(NewPayLoad payload)
        {
            // get stuck when Ginger close
            while (mProcessingStatus != eProcessingStatus.Ready && mProcessingStatus != eProcessingStatus.ResponseCompleted)
            {
                Thread.Sleep(1);  //TODO: add timeout!!! or??
            }            
            
            OutgoingRequetsesCounter++;
            mRequestProcessingDone.Reset();
            mSendDone.Reset();

            Response = null;
            
            mProcessingStatus = eProcessingStatus.SendingRequest;
            
            byte[] b = payload.GetPackage();            
            
            // This is where the actual Payload bytes start being sent
            mSocket.BeginSend(b, 0, b.Length, SocketFlags.None, SendCallback, this);
            bytesOut += b.Length;
            mProcessingStatus = eProcessingStatus.WaitingForResponse;
            mSendDone.WaitOne(); // blocking until send completed
                       
            bool bOK = mRequestProcessingDone.WaitOne(ResponseTimeoutMS);  // wait for response - blocking
            if (!bOK)
            {
                throw new Exception("Timeout waiting for response, Payload-" + payload.Name);
            }
            mProcessingStatus = eProcessingStatus.ResponseCompleted;
            mSocket.Blocking = false;
            return Response;
        }

        internal void Receive()
        {
            mSocket.BeginReceive(buffer, 0, GingerSocketInfo.InitialBufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), this);
        }

        int PayloadLen = 0;
        // Each time new data arrive we get callback
        private void ReceiveCallback(IAsyncResult ar)
        {
            Console.WriteLine("ReceiveCallback");
            while (mProcessingStatus == eProcessingStatus.SendingRequest)
            {
                Thread.Sleep(1);
            }
            
            try
            {
                // Retrieve the state object and the  socket   
                // from the asynchronous state object.  
                GingerSocketInfo gingerSocketInfo = (GingerSocketInfo)ar.AsyncState;
                Socket socket = gingerSocketInfo.Socket;

            // Read data from the socket
            int bytesRead = socket.EndReceive(ar);
            BytesIn += bytesRead;
            if (bytesRead > 0)
            {

                if (gingerSocketInfo.BufferPOS == 0) // do only once per Payload
                {
                    //TODO: There might be more data, so store the data received so far - need to veruf completion, create TC !!
                    byte[] rcvLenBytesBB = gingerSocketInfo.buffer;  // TODO: !!!!!!!!!!!!!temp do work direct - avoid creating a new byte array
                    PayloadLen = ((rcvLenBytesBB[0]) << 24) + (rcvLenBytesBB[1] << 16) + (rcvLenBytesBB[2] << 8) + rcvLenBytesBB[3];


                    if (PayloadLen > gingerSocketInfo.buffer.Length)
                    {
                        Array.Resize(ref gingerSocketInfo.buffer, PayloadLen + 4);   // Make sure we will have enough space  // Add 1024 !!!
                        // TODO: check if buffer is more than x size release it back....                        
                    }
                }

                gingerSocketInfo.BufferPOS += bytesRead;

                    if (gingerSocketInfo.BufferPOS == PayloadLen + 4)
                    {
                        NewPayLoad Resp = new NewPayLoad(gingerSocketInfo.buffer, true);

                        // We got the full packet, convert to Payload and process                        
                        if (Resp.PaylodType == NewPayLoad.ePaylodType.ResponsePayload || Resp.PaylodType == NewPayLoad.ePaylodType.SocketResponse)
                        {
                            mProcessingStatus = eProcessingStatus.ResponseStarted;
                            // Create new Payload from the buffer, ignoring the extra space at the end
                            Response = new NewPayLoad(gingerSocketInfo.buffer, true);
                            
                            mProcessingStatus = eProcessingStatus.ResponseCompleted;
                            // we are done with pair of Req/Resp - good job - mission completed
                            // signal receive is complete so ready for processing
                            mRequestProcessingDone.Set(); // !!!
                        }
                        else  // this is new request, we need to process and respond
                        {
                            IncomingRequetsesCounter++;
                            mProcessingStatus = eProcessingStatus.ProcessingRequest;
                            // This is a request  need to respond 
                            
                            switch (Resp.PaylodType)
                            {
                                case NewPayLoad.ePaylodType.RequestPayload:
                                    MessageHandler(gingerSocketInfo);
                                    gingerSocketInfo.Response.PaylodType = NewPayLoad.ePaylodType.ResponsePayload;
                                    break;
                                case NewPayLoad.ePaylodType.SocketRequest:
                                    SocketRequestHandler(gingerSocketInfo);
                                    break;
                                default:
                                    throw new Exception("Unknown Payload Type, Payload.Name: " + Resp.Name);
                            }
                            
                            mProcessingStatus = eProcessingStatus.SendingResponse;
                            byte[] bb = gingerSocketInfo.Response.GetPackage();
                            bytesOut += bb.Length;
                            mSocket.BeginSend(bb, 0, bb.Length, SocketFlags.None, SendCallback, this);
                            mSendDone.WaitOne();                           
                        }

                        gingerSocketInfo.BufferPOS = 0;

                        // be ready to accept more new request or incoming data
                        socket.BeginReceive(gingerSocketInfo.buffer, 0, gingerSocketInfo.buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), gingerSocketInfo);

                        mProcessingStatus = eProcessingStatus.Ready;
                    }
                    else
                    {
                        // we got a big package more than 1024, starting collecting the bytes until all received
                        socket.BeginReceive(gingerSocketInfo.buffer, gingerSocketInfo.BufferPOS, gingerSocketInfo.buffer.Length - gingerSocketInfo.BufferPOS, SocketFlags.None, new AsyncCallback(ReceiveCallback), gingerSocketInfo);
                    }
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    // ????????????????????????????????????????????????????????????????????
                    // Signal that all bytes have been received.  
                    mRequestProcessingDone.Set();    //move up like server ???
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Console.WriteLine("SocketException: " + e.Message);
                    //TODO: raise event 

                    // other side closed connection - mark socket as dead TODO !!!!!!!!!!!!!!!!!!!!!!!
                    return;
                }
                throw e;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Internal socket communication is handled here
        private void SocketRequestHandler(GingerSocketInfo gingerSocketInfo)
        {
            NewPayLoad Req = gingerSocketInfo.DataAsPayload;
            if (Req.Name == "GetSession")
            {
                gingerSocketInfo.Response = new NewPayLoad("SessionID", SessionID);
                gingerSocketInfo.Response.PaylodType = NewPayLoad.ePaylodType.SocketResponse;  
                return;
            }

            throw new Exception("SocketRequestHandler - Unknown Request: " + Req.Name);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                mSendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
