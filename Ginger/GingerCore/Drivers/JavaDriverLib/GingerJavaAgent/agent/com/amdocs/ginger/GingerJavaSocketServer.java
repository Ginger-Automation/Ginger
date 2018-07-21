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

package com.amdocs.ginger;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.net.UnknownHostException;

public class GingerJavaSocketServer 
{
	//TODO: fix hard coded port
	public static int SOCKET_ACCEPT_PORT = 8888;
    private int m_iSocketAcceptPort = SOCKET_ACCEPT_PORT;	
    private Socket m_Socket = null;
    private	ServerSocket m_ServerSocket = null;
    private String m_strExceptionMessage = null;    
    private JavaDriver mJavaDriver = new JavaDriver();
    
    /**
     * If this constructor is used, the default socket port used is 8888.
     */
	public GingerJavaSocketServer()
	{
	}
	
	/**
	 * Sets the local port number to use for binding to a socket that will be used for communications.
	 * 
	 * @param socketAcceptPort the port number, or 0 to use a port number that is automatically allocated
	 */
	public GingerJavaSocketServer(int socketAcceptPort)
	{
		m_iSocketAcceptPort = socketAcceptPort;
	}
	
	/**
	 * Implementation-specific method for running a command.
	 * Depending on the <code>command</code> to be executed,
	 * some of the parameters may be null.
	 * 
	 * @param input	 the input sent through the socket, comprised of the command parts delimited by '~'
	 * @param command
	 * @param locateBy
	 * @param locateValue
	 * @param propertyName
	 * @param value
	 * @return	the run result, either "OK<|...>", "Error|...", or "NA"
	 * @see GridCellSelector
	 */
	public String runCommand(String input, String command, String locateBy, String locateValue, String propertyName, String value, final boolean waitForIdle)
	{
		return "aa";
	}
	
	/**
	 * This method is called before waiting for a socket connection.
	 * Does nothing and throws away any exceptions thrown by derived classes.
	 */
	public void preAccept() {}
	
	/**
	 * This method is called after a successful connection and creation of a socket.
	 * Does nothing and throws away any exceptions thrown by derived classes.
	 */
	public void postAccept() {}
	
	/**
	 * Returns the port number that Ginger is listening to for connections.
	 * 
	 * @return port number
	 */
	public int getListenPort()
	{
		return m_iSocketAcceptPort;
	}
	
	/**
	 * Returns true if there is an existing socket connection being used to send commands.
	 * 
	 * @return
	 */
	public boolean isConnected()
	{
		if (m_Socket != null && ! m_Socket.isClosed())
			return true;
		else
			return false;
	}
	
	/**
	 * Closes the server socket and established socket connection, if any.
	 * Resets the request and response counters.
	 */
	public void reset()
	{		
		try
		{
			
			if (m_Socket != null && ! m_Socket.isClosed())
			{
				m_Socket.shutdownInput();
				m_Socket.shutdownOutput();
				m_Socket.close();
				m_Socket = null;
			}
			
			if (m_ServerSocket != null && ! m_ServerSocket.isClosed())
			{				
				m_ServerSocket.close();
				m_ServerSocket = null;
			}
		} 
		catch (IOException e) 
		{
			e.printStackTrace();
		}	
	}
	
	/**
	 * Listens for connections to be made to this socket and accepts it.
	 * Once a socket connection is established, no other connections are accepted 
	 * until the connection is closed by a "bye" command, or forcibly closed.
	 */
	public void listen()
	{
		try 
		{
			GingerAgent.WriteLog("GingerAgent is configure to listen on port: " + m_iSocketAcceptPort);
			
			//10 is backlog
			m_ServerSocket = new ServerSocket(m_iSocketAcceptPort, 10);
			
	        while (true)
	        {
		        try
		        {
		        	preAccept();
		        }
		        catch (Throwable t)
		        {
		        	// ignore exceptions occurring in derived classes
		        }
		        
		        System.out.println("GingerAgent: Before Accept");
		        
		        m_Socket = m_ServerSocket.accept();
		        
		        System.out.println("GingerAgent: After Accept");
		        try
		        {
		        	postAccept();
		        }
		        catch (Throwable t)
		        {
		        	// ignore exceptions occurring in derived classes
		        }
		        		        
		        InputStream is = m_Socket.getInputStream();
				
		        //
				// Multiple commands may be sent.  Continue until they say "bye".
		        //
				while (true)
				{
					try
					{
				        // Receive an incoming command in the format of
				        byte[] lenBytes = new byte[4];			 
				        is.read(lenBytes, 0, 4);
				        
				        // & 0xFF since java byte is unsigned so convert to int and shift
				        int len = ((lenBytes[0] & 0xFF) << 24) + ((lenBytes[1] & 0xFF) << 16) + ((lenBytes[2] & 0xFF) << 8) + (lenBytes[3] & 0xFF);				       
				        
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
				        if (bytesReceived != 0)
				        	processInput(receivedBytes);
				        else
				        	break;						     			    
				    }
					catch (Exception e)
					{						
						String ErrMSG = "Ginger Agent Exception: " + e.getMessage();
						GingerAgent.WriteLog(ErrMSG);						
						e.printStackTrace();						
						sendResponse(PayLoad.Error(ErrMSG));
					}
				}
				
				//TODO: close the socket on bye command
				m_Socket.close();
	        }
		} 
		catch (Throwable t) 
		{
			setExceptionMessage(t.toString());
			t.printStackTrace();
		}
	}
	
	/**
	 * Examines the Ginger protocol formatted input, runs the command and send the result back.
	 * Does nothing if the client sent a "bye" command.
	 * 
	 * @param input  the Ginger protocol formatted request sent by the client
	 * @return  false if the client sent a "bye" command, else true
	 */
	public void processInput(byte[] bytes)
	{		
		try
		{		
			PayLoad pl = new PayLoad(bytes);
			PayLoad Response = mJavaDriver.ProcessCommand(pl);
			sendResponse(Response);	    	
		}
		catch(Exception e)
		{
			GingerAgent.WriteLog("Error:"+e.getMessage());
			String ErrMSG = "Error:"+ e.getMessage();		
			PayLoad ResponseERR = PayLoad.Error(ErrMSG);
			sendResponse(ResponseERR);
		}
	}
	
	/**
	 * Sends <code>output</code> through the socket.
	 * Does nothing if the output is null.
	 * 
	 * @param command execution output message to send through the socket
	 */
	public void sendResponse(PayLoad output)
	{
		if (m_Socket == null || output == null)
			return;
		
		try 
		{
			OutputStream os = m_Socket.getOutputStream();
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
	
	/**
	 * Returns the IP address where this socket server is running.
	 * 
	 * @return the IP address where this socket server is running.
	 */
	public String getClientIpAddress() 
	{
		String strGingerToolBoxIpAddress = null;
		
		try 
		{
			InetAddress inetAddress = InetAddress.getLocalHost();
			strGingerToolBoxIpAddress = inetAddress.getHostAddress();
		} 
		catch (UnknownHostException e) 
		{
			e.printStackTrace();
		}	
		return strGingerToolBoxIpAddress;
	}

	/**
	 * Returns the IP address of the other end of the connected socket.
	 * 
	 * @return the IP address of the other end of the connected socket, or empty String if it is not connected yet
	 */
	public String getRemoteIpAddress() 
	{
		String strDriverIpAddress = "";
		if (isConnected())
			strDriverIpAddress = m_Socket.getRemoteSocketAddress().toString().substring(1, m_Socket.getRemoteSocketAddress().toString().indexOf(":")); 
		return strDriverIpAddress;
	}
	
	/**
	 * Sets the exception message that occurred when opening the socket, if any.
	 * 
	 * @param exceptionMessage the exception message to set
	 */
	public void setExceptionMessage(String exceptionMessage) 
	{
		m_strExceptionMessage = exceptionMessage;
	}

	/**
	 * Returns the exception message that occurred when opening the socket, if any.
	 * 
	 * @return socket open exception message, if any
	 */
	public String getExceptionMessage() 
	{
		return m_strExceptionMessage;
	}
}
