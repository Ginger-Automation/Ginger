/*
Copyright � 2014-2018 European Support Limited

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
import java.awt.Toolkit;
import java.awt.datatransfer.Clipboard;
import java.awt.datatransfer.StringSelection;
import java.io.File;
import java.io.IOException;
import java.lang.instrument.Instrumentation;
import java.lang.management.ManagementFactory;
import java.util.Set;

import javax.swing.JFrame;
import javax.swing.JOptionPane;
import com.sun.tools.attach.AgentInitializationException;
import com.sun.tools.attach.AgentLoadException;
import com.sun.tools.attach.AttachNotSupportedException;
import com.sun.tools.attach.VirtualMachine;

import sun.awt.AppContext;

public class GingerAgent  {

	public static final String GINGER_JAVA_AGENT_VERSION="3.0.6.1";
	
    private static Thread t1 = null;
    private static Instrumentation mInstrumentation = null;
    static GingerJavaSocketServer srv;    	
	static GingerAgentFrame GAF = null;
	private static boolean bDone = false;
	
	public static int SOCKET_ACCEPT_PORT=8888;	
	public static boolean SHOW_AGENT=true;
	public static String TESTED_APPLICATION_PID;	
	 private static Instrumentation instrumentation;
	 	 
	 //Main is called when launching as stand alone jar
	 public static void main(String[] args) 
	 {	
		 validateFolderName();
		 if (args.length > 0)
		 	{
		 		String AgentArgs = "";
		 		String PID = "";
		 		String GingerJarPath = "";
		 		
		 		for (String s : args)
		 		{		
		 			//pulling the args needed for the attaching GingerAgent 
		 			if (s.startsWith("PID="))
		 			{
		 				String[] a= s.split("=");
		 				PID=a[1];
		 			}
		 			else if (s.startsWith("AgentJarPath="))
		 			{
		 				String[] a= s.split("=");
		 				GingerJarPath=a[1];
		 				continue;//no need to add to AgentArgs
		 			}
		 			
			 			// Pass the args to GingerAgnet load (PID and other will be used for info on console)
			 			if (AgentArgs.length() > 0) 
			 				AgentArgs += ",";		 			
			 			AgentArgs += s;	
		 		}
		 				 		
		 		if (GingerJarPath.length() == 0)
		 		{	
		 			GingerJarPath = GetJarPath();
		 		}
		 		
		 		if (PID.length() > 0)
		 		{		
		 			Attach(GingerJarPath, AgentArgs, PID);
		 		}
		 		else
		 		{
		 			JOptionPane.showMessageDialog(null, "Please specify PID");
		 		}		 		
		 	}
		 	else
		 	{			
				GingerAgentStarterFrame frame=new GingerAgentStarterFrame();
				frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
				frame.setTitle("Ginger Starter");
			    frame.pack();
			    frame.setVisible(true);
		 	}		
	}
	
	 private static void ParseArgs(String args) {
		if(args!=null && args.length() > 0) 
 		{  
 			String params[]=args.split(",");
 			
 			//TODO: use continue and err if param not recorgnized
 			for (String ParamVal: params)
 			{
 				SetParamVal(ParamVal); 				
 			}
		}		
	}

	private static void SetParamVal(String paramVal) {			
			String aParVal[] = paramVal.split("=");
			
			//TODO: add check that there are param=val pair
			
			String Param = aParVal[0];
			String Val = aParVal[1];
			
			if ("ShowGingerAgentConsole".equals(Param))
			{    					
				SHOW_AGENT=Boolean.parseBoolean(Val);
			}
			else if ("Port".equals(Param))
			{
				SOCKET_ACCEPT_PORT=Integer.parseInt(Val);
			} 		
			else if ("PID".equals(Param))
			{
				TESTED_APPLICATION_PID=Val;
			} 	
			else
			{
			}		
	}

	//Attach the GingerAgent to a running JVM process based on proces ID
	public static void Attach(String GingerAgentJar, String AgentArgs, String PID)
	{			
		if (validateFolderName())
			{	
				try {					
					VirtualMachine vm = VirtualMachine.attach(PID);				
					vm.loadAgent(GingerAgentJar, AgentArgs);				
					vm.detach();
					
					
				} catch (AttachNotSupportedException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				} catch (AgentLoadException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				} catch (AgentInitializationException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
		}
		else
		{
			System.exit(0);
		}
	}	
	
    public static String getFolderName(String directoryName){
        File directory = new File(directoryName);
        //get all the files from a directory
        String rName = null;
        File[] fList = directory.listFiles();
        for (File file : fList){
        	if (file.getName().contains("hsperfdata_"))
        	{
        		rName = file.getName().toString();
        	}
        }
        return rName;
    }
    
	private static boolean validateFolderName()
	{
		String folder = null ;
		String hsperfdata = System.getProperty("java.io.tmpdir");
		String JavaFolderName = getFolderName(hsperfdata);
		String user =  System.getProperty("user.name");
		
		String expectedFolderName="hsperfdata_"+user;
		
		if (!expectedFolderName.equals(JavaFolderName))
		{
			
			String Message = "Attach cannot be done!\n For allowing successful attach to process please do the following:\n\n"
							+"1. Close all running Java Applications\n"
							+"2. Go to C:\\Users\\"+user+"\\AppData\\Local\\Temp\n"
							+"3. Find folder name'"+ JavaFolderName +"'"+"\n"
							+"4. Rename the folder name from: "+JavaFolderName+" To: "+expectedFolderName+"\n"
							+"5. restart all Java processes and try to attach the GingerAgent again\n\n"
							+ "Click 'Yes' to copy this instructions to Clipboard";
			
			int input = JOptionPane.showOptionDialog(null, Message,"Java Folder are not Set",JOptionPane.OK_OPTION, JOptionPane.INFORMATION_MESSAGE, null, null, null);
			
			if (input == JOptionPane.OK_OPTION)
			{
				StringSelection stringSelection = new StringSelection (Message);
				Clipboard clpbrd = Toolkit.getDefaultToolkit ().getSystemClipboard ();
				clpbrd.setContents (stringSelection, null);
			}
			
			return false;
		}
		return true;
	}
	 
	// agentmain is called after the GingerAgent is attached to the JVM
	public static void agentmain(String args, Instrumentation inst)
    {		
		ParseArgs(args);
		instrumentation = inst;		
		StartGingerAgent();	   
    }
	
	// premain is called when GingerAgent is part of the app launch using -javaagent
	 public static void premain(String args, Instrumentation inst)
	 {
		 ParseArgs(args);
		 instrumentation = inst;		
		 StartGingerAgent();		 	
	 }	
	 
	 private static void StartGingerAgent() {

    	 // use for wait for idle
    	 
         try {

        	 //Define task to be executed on the correct AppContext
        	 	Runnable task = new Runnable() {
        		 
	        	 public void run() {	
	        	
    				 if(SHOW_AGENT) 
	        			 ShowAgent();
    				 StartServer();
	        		 
  				 	// use later for instrumentation
//	        		 ClassLogger transformer = new ClassLogger();
//		     	     instrumentation.addTransformer(transformer, true);
//	    	        		        			
	        	 	}
	        	 };	        	 
        	 
	        	 // We need to search for the correct AppContext to start Ginger Agent, so it will find the correct app window
	        	   Set <AppContext> apps =  AppContext.getAppContexts();
	        	   
        	       boolean bFound = false;        	       
	               for (AppContext app : apps)
	               {	            	               	  
	            	   bFound = IsThreadGoupToAttach(app);	            	   
	            	   
	            	   if (bFound)
	            	   {
	            		   sun.awt.SunToolkit.invokeLaterOnAppContext(app, task);            		   
	            		   break;
	            	   }	            	 
	               }        	
	        	 } catch (Exception ex) {
	        		 ex.printStackTrace();        		  
        	 }		
	}	 
    
    private static boolean IsThreadGoupToAttach(AppContext app) {
    	
    	//TODO: need to find a way to return true on good AppContext: should work on JNPL like CRM, stand alone and for Applet
    	// any changes in this fund please validate on all 3: Stand-alone, JNLP and Applet
    	    	
    	String threadGroupName = app.getThreadGroup().getName();
    	
    	Boolean bFound = true;
    	if ("main".equals(threadGroupName))
		   {
    		bFound= false;
		   }
    	
    	if ("Plugin Thread Group".equals(threadGroupName))
		   {
    		bFound =false;
		   }
    	
    	if ("javawsSecurityThreadGroup".equals(threadGroupName))
		   {
    			bFound =false;
		   }

    	// JAVAWS - JNLP
 	   if ("javawsApplicationThreadGroup".equals(threadGroupName))
		   {
 		   		bFound =true;
		   }  
		   
 	   // Stand alone app
 	  if ("system".equals(threadGroupName))
	   {
 		 bFound =  true;
	   }
 	  if (bFound)
 	  {
 	  } 	  
		return bFound;
	}

	public static void WriteLog(String TXT) {        	    
    		System.out.println(TXT);    	
	}
    
	public static void StartServer() {	
		
    	bDone = true;
		
    	 t1 = new Thread(new Runnable() {     	    
			public void run()
     	    {
     	    	try {
						if(SHOW_AGENT)
						{
							WriteLog("Ginger Agent Started");
							WriteLog("---------------------------------");	
							WriteLog("Process ID of Attached Application = " + TESTED_APPLICATION_PID);
							WriteLog("Port = " + SOCKET_ACCEPT_PORT);
							WriteLog("JAVA_HOME: " + System.getenv("JAVA_HOME"));
							WriteLog("JAVA_HOME: " + System.getenv("JAVA_HOME"));
							WriteLog("Package Version: " + Runtime.class.getPackage().getImplementationVersion());														
							WriteLog("java.version: " + System.getProperty("java.version"));
							WriteLog("ManagementFactory Version: " + ManagementFactory.getRuntimeMXBean().getSpecVersion());
							WriteLog("Ginger GINGER_JAVA_AGENT_VERSION: " + GINGER_JAVA_AGENT_VERSION );							
							WriteLog("---------------------------------");													
						}						
						
						WriteLog("Ginger: Starting Socket Server");
						srv = new GingerJavaSocketServer(SOCKET_ACCEPT_PORT);					
						srv.listen();
						
						WriteLog("==============================================================");
						WriteLog("Ginger: srv.listen() done = Socket is Closed !");
						WriteLog("==============================================================");
												
					} catch (Exception e) {
						WriteLog("GingerAgent ERROR: " + e.getMessage());
						// TODO Auto-generated catch block
						e.printStackTrace();
					}     	    	
     	    }});  
     	    t1.start();		
	}
	
    static void ShowAgent()
    {
    	try
    	{
    		WriteLog("Show Ginger Agent Console");   	
    		GAF = new GingerAgentFrame();
    		GAF.setVisible(true);
    	}
    	catch (Exception e)
    	{
    	}
    }

	

	public static String GetJarPath() {
		String GingerJarPath = "";
		try {		
			GingerJarPath =new File(".").getCanonicalPath();
			GingerJarPath = GingerJarPath + "\\GingerAgent.jar";
			//TODO: mark it red + message
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		return GingerJarPath;
	}
    
	public static void setStatus(String txt)
	{
		if (GAF != null)
		{
			GAF.setStatus(txt);
		}
	} 
}