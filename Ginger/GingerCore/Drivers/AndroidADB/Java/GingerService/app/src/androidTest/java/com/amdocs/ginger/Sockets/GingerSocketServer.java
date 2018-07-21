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

package com.amdocs.ginger.Sockets;

import android.util.Log;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.net.SocketAddress;
import java.util.*;

/**
 * Created on 2/10/2017.
 */

public class GingerSocketServer {

    ArrayList<MessageEvent> eventHappenedObservers = new ArrayList<MessageEvent>();

    ServerSocket m_Socket = null;
    Socket clientSocket;

    public void AddMEssageEventObserver(MessageEvent observer){
        Log.d("Ginger", "AddMEssageEventObserver");
        eventHappenedObservers.add(observer);
    }

    PayLoad RaiseEvent(GingerSocket.eProtocolMessageType ProtocolMessageType, Object obj)
    {
            // We pass the message to all observers, usually will be one client but can be used later for broadcast for multi clients
            // We return the Payload from the first responder, so we support currently only one client
            // if needed we can expand to support multiple clients, in this case we will need to keep the client who sent the event
            Log.d("Ginger", "RaiseEvent Sending to listeners");
            for (MessageEvent eventHappenedObserver:eventHappenedObservers) {
                Log.d("Ginger", "RaiseEvent Sending to listener #1");
                PayLoad PLRC =  eventHappenedObserver.GingerSocketMessage(GingerSocket.eProtocolMessageType.PayLoad , obj);
                return  PLRC;
            }
            return null;
    }

    public void StartServer()
    {
        try {
            //TODO: remove Android specific
            Log.d("Ginger" , "Before create ServerSocket");

            //TODO: config
            m_Socket = new ServerSocket(7878, 10);
            Log.d("Ginger" , "m_Socket " + m_Socket.toString() );

            while (true) {

                Log.d("Ginger" , "GingerAgent: Waiting for Clients to connect");

                clientSocket = m_Socket.accept();
                Log.d("Ginger" ,"Got new client from IP = " + clientSocket.getInetAddress().toString() );


                SocketAddress sa = m_Socket.getLocalSocketAddress ();

                Log.d("Ginger" ,"sa = " + sa.toString());

                Log.d("Ginger" ,"GingerAgent: Post Accept");


                InputStream is;
                OutputStream out;

                //TODO: check also client is still connected othwerwise will throw erros in loop - fix also in Java driver the same

                is = clientSocket.getInputStream();
                out = clientSocket.getOutputStream();

                while (clientSocket.isConnected() ) {
                    Log.d("Ginger", "Waiting for commands");

                    byte[] lenBytes = new byte[4];
                    //The next read command will block until we get 4 bytes into input stream
                    is.read(lenBytes, 0, 4);

                    // & 0xFF since java byte is unsigned so convert to int and shift
                    int len = ((lenBytes[0] & 0xFF) << 24) + ((lenBytes[1] & 0xFF) << 16) + ((lenBytes[2] & 0xFF) << 8) + (lenBytes[3] & 0xFF);

                    Log.d("Ginger", "New Packet arrived Len = " + len);

                    Log.d("Ginger", "Len = " + len);

                    byte[] receivedBytes = new byte[len+4];
                    receivedBytes[0] = lenBytes[0];
                    receivedBytes[1] = lenBytes[1];
                    receivedBytes[2] = lenBytes[2];
                    receivedBytes[3] = lenBytes[3];

                    int bytesReceived = 0;
                    //Loop until we get all the packet
                    //TODO: enable timeout

                    while (bytesReceived < len )
                    {
                        bytesReceived += is.read(receivedBytes, bytesReceived + 4, len - bytesReceived);
                        Thread.sleep(1);
                    }

                    // Run the command and send back the response
                    if (bytesReceived != 0) {
                        Log.d("Ginger", "Before processInput");
                        processInput(receivedBytes);
                    }
                    else
                        break;
                }
            }
        } catch (IOException e) {
            Log.e("Ginger", e.getMessage());
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }

    public void processInput(byte[] bytes)
    {
        try
        {
            PayLoad pl = new PayLoad(bytes);
			
            Log.d("Ginger", "Before RaiseEvent");
            PayLoad Response = RaiseEvent(GingerSocket.eProtocolMessageType.PayLoad, pl);
            Log.d("Ginger", "After RaiseEvent");
            sendResponse(Response);

        }
        catch(Exception e)
        {
            Log.d("Ginger", "Exception: " + e.getMessage());
            String ErrMSG = e.getMessage();
            PayLoad ResponseERR = PayLoad.Error(ErrMSG);
            sendResponse(ResponseERR);
        }
    }

    public void sendResponse(PayLoad output)
    {
        if (clientSocket == null || output == null)
            return;

        try
        {
            OutputStream os = clientSocket.getOutputStream();
            if (os != null)
            {
                os.write(output.GetPackage());
                os.flush();
            }
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }
}
