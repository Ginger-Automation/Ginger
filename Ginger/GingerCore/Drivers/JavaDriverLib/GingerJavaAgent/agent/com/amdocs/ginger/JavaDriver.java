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
import java.awt.AWTException;
import java.awt.Color;
import java.awt.Component;
import java.awt.Container;
import java.awt.Dialog;
import java.awt.Frame;
import java.awt.MouseInfo;
import java.awt.Point;
import java.awt.Rectangle;
import java.awt.Robot;
import java.awt.TextArea;
import java.awt.Window;
import java.awt.event.ActionListener;
import java.awt.event.InputEvent;
import java.awt.event.KeyEvent;
import java.awt.event.MouseEvent;
import java.awt.event.WindowEvent;
import java.awt.image.BufferedImage;
import java.io.IOException;
import java.lang.management.ManagementFactory;
import java.lang.management.ThreadMXBean;
import java.lang.reflect.Field;
import java.lang.reflect.InvocationTargetException;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.Date;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Random;
import java.util.Set;
import java.util.Timer;
import java.util.TimerTask;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import javax.imageio.ImageIO;
import javax.swing.AbstractButton;
import javax.swing.BorderFactory;
import javax.swing.ComboBoxModel;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JComboBox;
import javax.swing.JComponent;
import javax.swing.JDialog;
import javax.swing.JEditorPane;
import javax.swing.JFrame;
import javax.swing.JInternalFrame;
import javax.swing.JLabel;
import javax.swing.JList;
import javax.swing.JMenu;
import javax.swing.JMenuItem;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JRadioButton;
import javax.swing.JScrollBar;
import javax.swing.JScrollPane;
import javax.swing.JTabbedPane;
import javax.swing.JTable;
import javax.swing.JTextArea;
import javax.swing.JTextField;
import javax.swing.JTextPane;
import javax.swing.JTree;
import javax.swing.JViewport;
import javax.swing.ListModel;
import javax.swing.ListSelectionModel;
import javax.swing.SwingUtilities;
import javax.swing.border.Border;
import javax.swing.table.TableCellEditor;
import javax.swing.table.TableModel;
import javax.swing.text.BadLocationException;
import javax.swing.text.Position;
import javax.swing.tree.TreePath;

import org.jsoup.nodes.Element;
import org.jsoup.select.Elements;

import com.amdocs.ginger.ASCFPack.ASCFHelper;

import sun.awt.SunToolkit;

//TODO:clean up

public class JavaDriver {
	
	private BrowserHelper mBrowserHelper = null;
	private List<BrowserHelper> lstmBrowser = new ArrayList<BrowserHelper>();//All the Widgets that already have been initialize
	private EditorHelper mEditorHelper=null;
	private volatile int mCommandTimeout=-1;

	private int implicitWait=30;
	private Component CurrentHighlighedComponent;
	private Border CurrentHighlighedComponentOriginalBorder;
	
	private ASCFHelper mASCFHelper = new ASCFHelper(); 
	private SwingHelper mSwingHelper= new SwingHelper();
    
	// Wait For Idle 
	private WaitForIdle mWaitForIdleHandler = new WaitForIdle();
	private String mWaitForIdle = null;
	private String mValue = null;
	private String mValueToSelect = null;
	private String mXCoordinate = null;
	private String mYCoordinate = null;
	private String mLocateColTitle = null;
	private String mLocateRowType = null;
	private String mLocateRowValue = null;
	
		
	HashMap<Component, Integer> mComponentDictionary=new HashMap<Component, Integer>();
	
	private Recorder mRecorder;
	private List<String> Scripts;
	private String recordingJSFile;
	   
    
	enum CommandType
	{
		 AgentOperation,
         WindowExplorerOperation,
         WidgetAction,
         SwingElementAction,
         WidgetExplorerOperation,
         UnitTest
	}
	
	enum AgentOperationType
	{
		SetCommandTimeout,
		GetVersion,
		AgentConfig
	}
	
	enum WindowExplorerOperationType
	{
		Highlight,
		GetCurrentWindowTitle,
		GetProperties,
		GetAllWindows,
		GetActiveWindow,
		GetCurrentWindowVisibleControls,
		GetContainerControls,
		GetComponentFromCursor		
	}
	
		

public PayLoad ProcessCommand(final PayLoad PL) {
    		
    	mWaitForIdleHandler.isCommandTimedOut=false;
          System.out.println("Before process");
        
	      final PayLoad[] response = new PayLoad[1];
	      response[0] = null;
	     
	     if ((CommandType.AgentOperation.toString().equals(PL.Name)|| CommandType.WindowExplorerOperation.toString().equals(PL.Name)|| CommandType.UnitTest.toString().equals(PL.Name)))
	  	 {	    	 
	        	GingerAgent.WriteLog("\n********************** Running command **********************");
	  		    response[0] = runCommand(PL);
	  	 }
	  	 else
	  	 {	  		
	  		GingerAgent.WriteLog("\n**********************\nRunning command in EDT\n**********************");
			
			Runnable r= new Runnable() {
				
				//@Override
				public void run() {					
					try {
						SwingUtilities.invokeAndWait(new Runnable() 
						{							
							//@Override
							public void run() 
							{							
								response[0] = runCommand(PL);
							}
						});
					} 
					catch (Exception e)
					{						
						response[0]=PayLoad.Error("Error:"+e.getMessage());
						e.printStackTrace();
					} 
				}
			};
			
			//TODO: Creating a new thread each time is expensive. Find a better way using Executor or Executor service
			Thread t= new Thread(r);
			t.start();

			
			  int iTimeout=0;
			    while (response[0] == null )
			    {	
			    	try 
			    	{
			    		Thread.sleep(1000);

			    		iTimeout =iTimeout +1;

			    		if(iTimeout>=mCommandTimeout)
			    		{
			    			mWaitForIdleHandler.isCommandTimedOut=true;	
							
			    			Thread.sleep(1000);			    			
			    			//executor.
			    			break;
			    		}
			    	} 
			    	catch (InterruptedException e) 
			    	{
			    		// TODO Auto-generated catch block
			    		e.printStackTrace();
			    	}
			    }			
			
	     }
	  					
		if (response[0]!=null && !response[0].IsErrorPayLoad())
		{
			
			if (!SwingHelper.isModalDialogShowing())
			{				
			// 	For any action we push one empty event which should be good for all actions, if nothing in Q there is minimal wait time, if there is it will wait for completion		
				SunToolkit.flushPendingEvents();	
			
				try
				{			
					mWaitForIdleHandler.pushEmptyEventAndWait();

					//ProcessCommand can return with decision if we need to wait for idle, currently only for Click
					if ( mWaitForIdle != null && !"None".equals(mWaitForIdle))
					{					
						mWaitForIdleHandler.DoWaitWaitIdle(mWaitForIdle);						
						SunToolkit.flushPendingEvents();
					}	
				}
				catch(Exception e)
				{
					GingerAgent.WriteLog("Exception after runcommand"+e.getMessage());
				}
			}
		}
		else			
		{
			if(response[0]==null)
			{
				response[0]=PayLoad.Error("Timeout after: "+mCommandTimeout+" secs");
			}			
		}

		
        mWaitForIdle = null;      
        SunToolkit.flushPendingEvents();    
		return response[0];
    }   

	private PayLoad runCommand(PayLoad PL) {
		
		GingerAgent.WriteLog("Starting to run Command: " + PL.Name);
				
		if(CommandType.AgentOperation.toString().equals(PL.Name))
		{	
			return HandleAgentOperation(PL);
		}
		else if (CommandType.WindowExplorerOperation.toString().equals(PL.Name))
		{
			return HandleWindowExplorerOperations(PL);
		}
		
		//TODO: put most common at the top - speed
		if ("UIElementAction".equals(PL.Name))
		{	
			return HandleUIElementAction(PL);
		}
		else if ("ElementAction".equals(PL.Name))
		{	
			String ControlAction = PL.GetValueString();
			mWaitForIdle = PL.GetValueEnum();
			String LocateBy = PL.GetValueString();
			String LocateValue = PL.GetValueString();			
			String Value = PL.GetValueString();
			GingerAgent.WriteLog("ProcessCommand 'ElementAction': "  
						+ "Wait for Idle = " + mWaitForIdle + ","
						+ "ControlAction = " + ControlAction + "," 
						+ "" + LocateBy + "," 
						+ "" + LocateValue + ","
						+ "" + Value);
			
				PayLoad plrc= HandleElementAction(LocateBy, LocateValue, ControlAction, Value, "", "", "");
			return plrc;			
		}		
		if(CommandType.UnitTest.toString().equals(PL.Name))
		{	
		
			String unitTestName= PL.GetValueString();
			
			if(unitTestName.equals("IntegerValueTest"))
			{
				
				int value=  PL.GetValueInt();
				
				PayLoad PLResp= new PayLoad("IntegerValueTestResponse");
				PLResp.AddValue(value);
				PLResp.ClosePackage();
				
				return PLResp;
			}

		}
		
		else if ("DialogElementAction".equals(PL.Name))
		{	
			String ControlAction = PL.GetValueString();
		    mWaitForIdle = PL.GetValueEnum();
			String LocateBy = PL.GetValueString();
			String LocateValue = PL.GetValueString();			
			String Value = PL.GetValueString();
			return HandleDialogAction(LocateBy, LocateValue, ControlAction, Value);								
		}			
		
		
		else if ("SwitchWindow".equals(PL.Name))
		{					
			GingerAgent.WriteLog("Switch Window");
			String Title = PL.GetValueString();
			if (mSwingHelper.SwitchWindow(Title))
			{
				GingerAgent.WriteLog("Switch Window Done");
				PayLoad pl2 = new PayLoad("Done");
				pl2.ClosePackage();
				return pl2;
			}
			else
			{
				return PayLoad.Error("Window not found title: " + Title);				
			}					
		}		
		else if("InitializeJEditorPane".equals(PL.Name))
		{
			GingerAgent.WriteLog("Initialize JEditor");
			
			String LocateBy = PL.GetValueString();
			String LocateValue = PL.GetValueString();
			
			Component jEditor = mSwingHelper.FindElement(LocateBy, LocateValue);
			
			if(jEditor!=null)
			{
				mEditorHelper= new EditorHelper(jEditor);	
				
				return PayLoad.OK("Active JEditor set to " +  LocateBy + " " + LocateValue);
			}
			else
			{					 
				return PayLoad.Error("JEditor Element not found - " + LocateBy + " " + LocateValue);
			}	
			
		}
		
		/////////////////////////////////////////////////////
		// Start of Widgets  Commands
		/////////////////////////////////////////////////////
		else if ("InitializeBrowser".equals(PL.Name))
		{					
			GingerAgent.WriteLog("Initialize Browser");
			
			String LocateBy = PL.GetValueString();
			String LocateValue = PL.GetValueString();
			int implicitBrowserWait = PL.GetValueInt();
			
			Scripts= PL.GetListString();
			
			return InitializeBrowser(LocateBy,LocateValue,implicitBrowserWait);
			
													
		}
		else if ("GetPageSource".equals(PL.Name))
		{	
				return mBrowserHelper.ExceuteJavaScriptPayLoad(PL);	
		}
		else if ("SwitchToDefaultFrame".equals(PL.Name))
		{
				return mBrowserHelper.ExceuteJavaScriptPayLoad(PL);	
		}
		else if ("GetPageURL".equals(PL.Name))
		{	
			return mBrowserHelper.ExceuteJavaScriptPayLoad(PL);							
		}

		else if ("HTMLElementAction".equals(PL.Name))
		{
			//TODO: add null check and browser valid check also in other places.
			if(mBrowserHelper != null) 
			{
				if (mBrowserHelper.isBrowserValid()) 
				{
					
					if(!mBrowserHelper.CheckIfScriptExist())
					{
					 GingerAgent.WriteLog("Trying to reinitialize browser");
					 PayLoad resp=	InitializeBrowser("ByXPath", mBrowserHelper.getmBrowserXPath(),-1);
					 
					 if(resp.IsErrorPayLoad())
					 {
						 return resp;
					 }						

					}
					
					return mBrowserHelper.ExceuteJavaScriptPayLoad(PL);
				}
				else 
				{
					return PayLoad.Error("Current browser is not valid. Please add Initializer Browser");
				}
			}
			else
			{
				return PayLoad.Error("Browser not initialized");
			}
		}
		else if("RunJavaScript".equals(PL.Name))
		{
			String LocateBy = PL.GetValueString();
			String LocateValue = PL.GetValueString();
			String Script=PL.GetValueString();
			Component browser = mSwingHelper.FindElement(LocateBy, LocateValue);	

			if (browser != null)
			{					
				GingerAgent.WriteLog("Inside Run Java Script::");
				mBrowserHelper = new BrowserHelper(browser);
			
				Object RC=mBrowserHelper.ExecuteScript(Script);
				if (RC!=null && RC.toString().startsWith("ERROR"))
			        	return PayLoad.Error(RC.toString()); 
				return PayLoad.OK("Java script executed");		
			}
			else
			{					 
				return PayLoad.Error("Browser Element not found - " + LocateBy + " " + LocateValue);
			}											
		}
		
		
		else if ("GetElementProperties".equals(PL.Name))
		{	
			return mBrowserHelper.ExceuteJavaScriptPayLoad(PL);							
		}		
		else if ("GetVisibleElements".equals(PL.Name))
		{	
			return mBrowserHelper.ExceuteJavaScriptPayLoad(PL);
		}
		else if("GetElementChildren".equals(PL.Name))
		{
				return mBrowserHelper.ExceuteJavaScriptPayLoad(PL);
		}
		else if("HighLightElement".equals(PL.Name))
		{
			return mBrowserHelper.ExceuteJavaScriptPayLoad(PL);
		}
		/////////////////////////////////////////////////////
		// End of Widgets  Commands
		/////////////////////////////////////////////////////
		else if("HighLightEditorElement".equals(PL.Name))
		{
			String LocateBy = PL.GetValueString();
			String LocateValue = PL.GetValueString();		
						
			mEditorHelper.HighlightElement(LocateValue);
			return PayLoad.OK("ok");
			
		}
		else if("GetEditorElementProperties".equals(PL.Name))
		{
			String LocateBy = PL.GetValueString();
			String LocateValue = PL.GetValueString();		
						
			Element el= mEditorHelper.FindEditorElement(LocateBy, LocateValue);
			
			PayLoad PLResp = new PayLoad("ControlProperties");
			List<PayLoad> list = mEditorHelper.GetElementProperties(el);
			PLResp.AddListPayLoad(list);
			PLResp.ClosePackage();
			return PLResp;
			
		}
		else if("GetEditorVisibleElements".equals(PL.Name))
		{
			
			List<PayLoad> Elements=getEditorComponents();
			PayLoad pl2 = new PayLoad("HTML Element Children");
			pl2.AddListPayLoad(Elements);
			pl2.ClosePackage();
			return pl2;	
			
		}		
	
		else if("TableAction".equals(PL.Name))
		{

			String ControlAction = PL.GetValueString();
			GingerAgent.WriteLog("ControlAction:" + ControlAction);
			String LocateBy = PL.GetValueString();
			GingerAgent.WriteLog("LocateBy:" + LocateBy);
			String LocateValue = PL.GetValueString();
			GingerAgent.WriteLog("LocateValue:" + LocateValue);
			String Value = PL.GetValueString();
			GingerAgent.WriteLog("Value:" + Value);
			List<String> CellLocator = PL.GetListString();	
			
			GingerAgent.WriteLog("Size:" + CellLocator.size());
			
			PayLoad response= HandleTableActions(LocateBy, LocateValue, CellLocator, ControlAction, Value);
			//TODO: handle it using ui element action
			//PayLoad response= mEditorHelper.HandleEditorTableActions(LocateBy, LocateValue, CellLocator, ControlAction, Value);
			//WaitForPageLoad();
			return response;
		}
		
		else if ("GetTableDetails".equals(PL.Name))
		{
			String ControlAction = PL.GetValueString();
			String LocateBy = PL.GetValueString();			
			String LocateValue = PL.GetValueString();			
			String Value = PL.GetValueString();			
			List<String> CellLocator= new ArrayList<String>();
			CellLocator.add(LocateValue);
			return HandleTableActions(LocateBy, LocateValue,CellLocator, ControlAction, Value);								
		}
		else if ("GetEditorTableDetails".equals(PL.Name))
		{
			String ControlAction = PL.GetValueString();
			String LocateBy = PL.GetValueString();			
			String LocateValue = PL.GetValueString();			
			String Value = PL.GetValueString();			
			List<String> CellLocator= new ArrayList<String>();
			CellLocator.add(LocateValue);
			return mEditorHelper.HandleEditorTableActions(LocateBy, LocateValue,CellLocator, ControlAction, Value);								
		}
		else if("Echo".equals(PL.Name))
		{
			String s=PL.GetValueString();
			GingerAgent.WriteLog("String Len=" + s.length());
			PayLoad resp=new PayLoad("EchoResponse");
			resp.AddValue(s);
			resp.ClosePackage();
			
			return resp;
		}
		else if("CheckJExplorerExists".equals(PL.Name))
		{			
			return CheckJExplorerExists();		
		}		
		else if("StartRecording".equals(PL.Name))
		{	
			//Getting JS files list and JS recorder file name
			//for JS injection any time if needed (popup + JExplorer)
			recordingJSFile = PL.GetValueString();
			Scripts	= PL.GetListString();
			
			if(mBrowserHelper != null){
				GingerAgent.WriteLog("StartRecording --> mBrowserHelper 'Not Null' ");
				mBrowserHelper.InjectRecordingScript(recordingJSFile);				
				PayLoad m = mBrowserHelper.ExceuteJavaScriptPayLoad(PL);
				GingerAgent.WriteLog("StartRecording ResponsePL.Name: " + m.Name);

			}
			else
				GingerAgent.WriteLog("StartRecording --> mBrowserHelper 'Null' ");
			
			// We go on the current window and attach listener event base on component type		
			
			mRecorder = new Recorder(this, mSwingHelper);
			
			mRecorder.Start(mSwingHelper.getCurrentWindow());
			
			//New tracker to check every 1 second if a pop-up was opened and
			//it has JExplorer inside for later JS injection.
			mRecorder.StartWindowTracker();
			
			//TODO: Add New window opneded listener, detect new window and do AddActionLsitenr on it
			PayLoad resp=new PayLoad("Recording Started");		
			resp.ClosePackage();			
			return resp;
		}
		
		else if("StopRecording".equals(PL.Name))
		{			
			//TODO: Cancel the timer started during Start recording
			RemoveActionListener(mSwingHelper.getCurrentWindow(), mRecorder);
			PayLoad resp=new PayLoad("Recording Stopped");		
			resp.ClosePackage();			
			return resp;
		}
			
		else if("GetRecording".equals(PL.Name))
		{										
			PayLoad resp=new PayLoad("Recordings");
			List<PayLoad> list = mRecorder.GetRecording();	
			
			if(mBrowserHelper != null){
				
				GingerAgent.WriteLog("GetRecording --> mBrowserHelper 'Not Null' ");
				PayLoad PLgerRC = mBrowserHelper.ExceuteJavaScriptPayLoad(PL);
				
				//If ERROR comes from ExceuteJavaScriptPayLoad means no JS injected in new page loaded
				if (PLgerRC.Name.equals("ERROR")){
					
					//JS re-injection as InitializeBrowser
					mBrowserHelper.InjectInitializationScripts(Scripts);	
					//JS re-injection for recording
					mBrowserHelper.InjectRecordingScript(recordingJSFile);		
					
					// Re-invoking StartRecording if there is a new page with JS re-injected.
					PayLoad startRec=new PayLoad("StartRecording");		
					startRec.ClosePackage();	
					PayLoad m = mBrowserHelper.ExceuteJavaScriptPayLoad(startRec);
					GingerAgent.WriteLog("ReInjectJavaScript(StartRecording)ResponsePL.Name: " + m.Name);					
				}	
				
				GingerAgent.WriteLog("GetRecording ResponsePL.Name: " + PLgerRC.Name);
				
				List<PayLoad> PLs = PLgerRC.GetListPayLoad();
				GingerAgent.WriteLog("GetRecording PLs Size: " + PLs.size());
				
                for (PayLoad PLR : PLs)
                {
					String LocateBy         = PLR.GetValueString();
                    String LocateValue      = PLR.GetValueString();
                    String ElemValue        = PLR.GetValueString();
                    String ControlAction    = PLR.GetValueString();
                    String Type             = PLR.GetValueString();
                    
                    GingerAgent.WriteLog("PLR LocatedBy - LocateValue - Type: " 
                    			+ LocateBy + " - " + LocateValue + " - " + Type);                
                    list.add(PLR);                    
                }
                
                String recordingStarted = PLgerRC.GetValueString();
                GingerAgent.WriteLog("GetRecording ResponsePL [recordingStarted ]: " + recordingStarted);
				
				}	
			else
				GingerAgent.WriteLog("GetRecording --> mBrowserHelper 'Null' ");
										
			resp.AddListPayLoad(list);
			mRecorder.ClearRecording();
			resp.ClosePackage();	
			
			//TODO:: Need to be handled only when required.			
			RemoveActionListener(mSwingHelper.getCurrentWindow(), mRecorder);
			mRecorder = new Recorder(this,mSwingHelper);
			mRecorder.Start(mSwingHelper.getCurrentWindow());				
			return resp;
		}
		else if("TakeScreenShots".equals(PL.Name))
		{
			PayLoad resp = new PayLoad("ScreenShot");
			String Type = PL.GetValueString();
			resp.AddListPayLoad(HandleScreenShots(Type));
			resp.ClosePackage();
			return resp;
		}
		
		else if ("isElementDisplayed".equals(PL.Name))
		{
			String LocateBy = PL.GetValueString();
			String LocateValue = PL.GetValueString();
			return HandleisElementDisplayed (LocateBy,LocateValue);
		}
		else if ("WindowAction".equals(PL.Name))
		{			 
			String ControlAction = PL.GetValueString();
			String LocateBy = PL.GetValueString();
			String LocateValue = PL.GetValueString();			
			GingerAgent.WriteLog("ProcessCommand 'WindowAction': "
						+ "ControlAction = " + ControlAction + "," 
						+ "" + LocateBy + "," 
						+ "" + LocateValue + ","
						);
			
			PayLoad plrc= HandleWindowAction(LocateBy, LocateValue, ControlAction);
			return plrc;			
		}							
		
		return PayLoad.Error("Unknown Package Type - " + PL.Name);
	}
	
	private PayLoad HandleAgentOperation(PayLoad PL)
	{
		String agentOperationType= PL.GetValueEnum();			
		
		
		if(AgentOperationType.SetCommandTimeout.toString().equals(agentOperationType))
		{
			mCommandTimeout=PL.GetValueInt();
			return PayLoad.OK("Command Timeout set to: "+mCommandTimeout);
		}
		else if (AgentOperationType.GetVersion.toString().equals(agentOperationType))
		{				
			PayLoad pl = new PayLoad("Version");
			pl.AddValue(GingerAgent.GINGER_JAVA_AGENT_VERSION);
			pl.ClosePackage();
			return pl;
		}
		else if (AgentOperationType.AgentConfig.toString().equals(agentOperationType))
		{		
			mCommandTimeout = PL.GetValueInt();
			implicitWait=PL.GetValueInt();
			PayLoad pl = new PayLoad("Status");
			pl.AddValue("Done");
			pl.ClosePackage();
			return pl;
		}
		else
		{
			return PayLoad.Error("Invalid Agent Operation Type: "+ agentOperationType);
		}
	}
	
	private PayLoad HandleWindowExplorerOperations(PayLoad PL)
	{
		
		String operationType= PL.GetValueEnum(); 
		
		 if(WindowExplorerOperationType.Highlight.toString().equals(operationType))
		{			
			String LocateBy = PL.GetValueString();
			String LocateValue = PL.GetValueString();		
						
			Component c = mSwingHelper.FindElement(LocateBy, LocateValue);
			if(c == null){
				System.out.println("Component null");
				return PayLoad.Error("Unable to find element to highlight");
			}
			return HighLightElement(c);
			
		}
		else if (WindowExplorerOperationType.GetCurrentWindowTitle.toString().equals(operationType))
		{
			if(mSwingHelper.getCurrentWindow()!=null)
			{
				String winTitleToAdd = "";
				Window winCurrentWindow = mSwingHelper.getCurrentWindow();
				do 
				{
					if (winCurrentWindow instanceof JFrame) 
					{
						winTitleToAdd = ((JFrame) winCurrentWindow).getTitle();								
						winCurrentWindow = (Window) ((JFrame) winCurrentWindow).getParent();
					}
					if (winCurrentWindow instanceof JDialog) 
					{
						winTitleToAdd = ((JDialog) winCurrentWindow).getTitle();								
						winCurrentWindow = (Window) ((JDialog) winCurrentWindow).getParent();								
					}
					GingerAgent.WriteLog("winTitleToAdd::"+ winTitleToAdd);
							
				} while ((winCurrentWindow instanceof JFrame) || (winCurrentWindow instanceof JDialog));
						
				
				if(winTitleToAdd!="")				
					return PayLoad.OK(winTitleToAdd);
				else
					return PayLoad.Error("Window title is empty");
			}
			else
				return PayLoad.Error("Current Windnow is null");
		}
		else if (WindowExplorerOperationType.GetProperties.toString().equals(operationType))
		{
			String LocateBy = PL.GetValueString();
			String LocateValue = PL.GetValueString();		
			Component c = mSwingHelper.FindElement(LocateBy, LocateValue);		
			
			PayLoad PLResp = new PayLoad("ControlProperties");
			List<PayLoad> list = GetComponentProperties(c);
			PLResp.AddListPayLoad(list);
			PLResp.ClosePackage();
			return PLResp;
		}
		else if (WindowExplorerOperationType.GetAllWindows.toString().equals(operationType))
		{			
			PayLoad pl = new PayLoad("AllWindows");
			List<String> list = mSwingHelper.GetAllWindows();
			pl.AddValue(list);									
			pl.ClosePackage();
			return pl;
		}
		else if (WindowExplorerOperationType.GetActiveWindow.toString().equals(operationType))
		{			
			
			//TODO: fixme try to get the real active window, meanwhile return 0			
			Window f =null;
			if(mSwingHelper.getCurrentWindow()!=null)		
			{
				f = mSwingHelper.getCurrentWindow();
			}				
			else
			{
				Window[] listOfWindows=SwingHelper.GetAllWindowsByReflection();
				if(listOfWindows.length!=0)
					f= listOfWindows[0];
			}
			if(f!=null)
			{
				PayLoad pl = new PayLoad("ActiveWindow");
				pl.AddValue(f.getName());						
				pl.ClosePackage();
				return pl;
			}
			else
			{
				return PayLoad.Error("Active window not found");
			}
			
		}		
		 
		else if (WindowExplorerOperationType.GetCurrentWindowVisibleControls.toString().equals(operationType))
		{					
			return HandleGetCurrentWindowVisibleControls();
							
		}

		else if (WindowExplorerOperationType.GetContainerControls.toString().equals(operationType))
		{
			String containerXPath = PL.GetValueString();
			return HandleGetContainerControls(containerXPath);
		}
		
		else if (WindowExplorerOperationType.GetComponentFromCursor.toString().equals(operationType))
		{						
			return GetComponentFromCursor();
		}
		else
		{
			return PayLoad.Error("Invalid Window Explorer Operation Type: "+ operationType);
		}
	}
	
	
	private PayLoad HandleUIElementAction(PayLoad PL)
	{
		String LocateBy = PL.GetValueString();
		String LocateValue = PL.GetValueString();
		String ElementType = PL.GetValueString();
		String ControlAction = PL.GetValueString();		
				
		HashMap pm = PL.GetParamHashMap(PL.GetListPayLoad());
		
		GingerAgent.WriteLog("ProcessCommand 'ElementAction': "  
				+ " LocateBy = " + LocateBy + ","
				+ " LocateValue = " + LocateValue + "," 
				+ " ElementType" + ElementType + "," 
				+ " ControlAction" + ControlAction + ","
				+ " HashMap = " + pm);			
		if (ElementType.equalsIgnoreCase("Table"))
		{
			GingerAgent.WriteLog("Inside Table");	

			List<String> CellLocator = new ArrayList<String>();
			String TableElementAction = PL.GetParamValue(pm, "ControlAction");
			CellLocator.add(PL.GetParamValue(pm, "LocateRowType"));	
			if (PL.GetParamValue(pm, "LocateRowType").equalsIgnoreCase("Row Number"))
			{
				CellLocator.add(PL.GetParamValue(pm, "LocateRowValue"));					
			}
			else if (PL.GetParamValue(pm, "LocateRowType").equalsIgnoreCase("Where"))
			{
				CellLocator.add(PL.GetParamValue(pm, "WhereColSelector"));	
				CellLocator.add(PL.GetParamValue(pm, "WhereColumnTitle"));	
				CellLocator.add(PL.GetParamValue(pm, "WhereProperty"));	
				CellLocator.add(PL.GetParamValue(pm, "WhereOperator"));
				CellLocator.add(PL.GetParamValue(pm, "WhereColumnValue"));	
			}
			CellLocator.add(PL.GetParamValue(pm, "ColSelectorValue"));	 
			CellLocator.add(PL.GetParamValue(pm, "LocateColTitle"));	
			
			mValue = PL.GetParamValue(pm, "ControlActionValue");
			GingerAgent.WriteLog(CellLocator.toString());				
			PayLoad plrc = HandleTableActions(LocateBy, LocateValue, CellLocator, TableElementAction, mValue);
			return plrc;
		}						
		else if (ElementType.equalsIgnoreCase("Window"))
		{
			GingerAgent.WriteLog("Inside Window action "  );
			GingerAgent.WriteLog("ProcessCommand 'WindowAction': "
						+ "ControlAction = " + ControlAction + "," 
						+ "" + LocateBy + "," 
						+ "" + LocateValue + ","
						);
			
			PayLoad plrc= HandleWindowAction(LocateBy, LocateValue, ControlAction);
			return plrc;			
		}
		else if (ElementType.equalsIgnoreCase("EditorPane"))
		{
			GingerAgent.WriteLog("Inside JEditorPane Condition");
			if (ControlAction.equals("InitializeJEditorPane"))
			{
				GingerAgent.WriteLog("Inside InitializeJEditorPane");
				
				Component jEditor = mSwingHelper.FindElement(LocateBy, LocateValue);
				
				if(jEditor!=null)
				{								
					mEditorHelper= new EditorHelper(jEditor);	
					
					return PayLoad.OK("Active JEditor set to " +  LocateBy + " " + LocateValue);
				}
				else
				{					 
					return PayLoad.Error("JEditor Element not found - " + LocateBy + " " + LocateValue);
				}	
				
			}
			else if (ControlAction.equalsIgnoreCase("JEditorPaneElementAction"))
			{
				GingerAgent.WriteLog("Inside JEditorPaneElementAction");
				String SubElementType = PL.GetParamValue(pm, "SubElementType");
				GingerAgent.WriteLog("SubElementType = "+ SubElementType);
				
				
				List<String> CellLocator = new ArrayList<String>();
				String TableElementAction = PL.GetParamValue(pm, "ControlAction");
				CellLocator.add(PL.GetParamValue(pm, "LocateRowType"));	
				if (PL.GetParamValue(pm, "LocateRowType").equalsIgnoreCase("Row Number"))
				{
					CellLocator.add(PL.GetParamValue(pm, "LocateRowValue"));					
				}
				else if (PL.GetParamValue(pm, "LocateRowType").equalsIgnoreCase("Where"))
				{
					CellLocator.add(PL.GetParamValue(pm, "WhereColSelector"));	
					CellLocator.add(PL.GetParamValue(pm, "WhereColumnTitle"));	
					CellLocator.add(PL.GetParamValue(pm, "WhereProperty"));	
					CellLocator.add(PL.GetParamValue(pm, "WhereOperator"));
					CellLocator.add(PL.GetParamValue(pm, "WhereColumnValue"));	
				}
				CellLocator.add(PL.GetParamValue(pm, "ColSelectorValue"));	 
				CellLocator.add(PL.GetParamValue(pm, "LocateColTitle"));	

				mValue = PL.GetParamValue(pm, "ControlActionValue");
				GingerAgent.WriteLog(CellLocator.toString());

				if (SubElementType.equalsIgnoreCase("HTMLTable"))
				{						
					PayLoad plrc = mEditorHelper.HandleEditorTableActions(LocateBy, LocateValue, CellLocator, TableElementAction, mValue);
					return plrc;
				}
				else 
				{
					return PayLoad.Error("Unknown sub-element type: "+ SubElementType);
				}
			}
			else
				return PayLoad.Error("Unknown EditorPane action");
		}
		else
		{
			GingerAgent.WriteLog("ProcessCommand 'ElementAction': "  
					+ " LocateBy = " + LocateBy + ","
					+ " ElementType =" + ElementType + ","
					+ " LocateValue = " + LocateValue + " ControlAction" + ControlAction ) ;
			
				mValue=PL.GetParamValue(pm, "Value");	
				mWaitForIdle = PL.GetParamValue(pm, "WaitforIdle");
				mValueToSelect = PL.GetParamValue(pm, "ValueToSelect");
				mXCoordinate = PL.GetParamValue(pm, "XCoordinate");
				mYCoordinate = PL.GetParamValue(pm, "YCoordinate");
				mLocateColTitle = PL.GetParamValue(pm, "LocateColTitle");
				mLocateRowType = PL.GetParamValue(pm, "LocateRowType");
				mLocateRowValue = PL.GetParamValue(pm, "LocateRowValue");
				
			GingerAgent.WriteLog("ProcessCommand 'ElementAction': "  
						+ "Wait for Idle = " + mWaitForIdle + ","
						+ "ControlAction = " + ControlAction + "," 
						+ "LocateBy = " + LocateBy + "," 
						+ "LocateValue = " + LocateValue + ","
						+ "Value = " + mValue
						+ "ValueToSelect = " + mValueToSelect
						+ "XCoordinate = " + mXCoordinate
						+ "YCoordinate = " + mYCoordinate
						+ "LocateColTitle" + mLocateColTitle
						+ "LocateRowType" + mLocateRowType
                        + "LocateRowValue" + mLocateRowValue);		
			
			PayLoad plrc = HandleElementAction(LocateBy, LocateValue, ControlAction, mValue, mValueToSelect, mXCoordinate, mYCoordinate);
			return plrc;
		}
	}
	
	private PayLoad InitializeBrowser(String LocateBy, String LocateValue, int implicitBrowserWait)
	{
		implicitBrowserWait = (implicitBrowserWait > 0) ? implicitBrowserWait : mCommandTimeout;
		
		Component browser = FindElementWithImplicitSync(LocateBy, LocateValue);
					
		if (browser != null)
		{
				// If we already added this browser and is valid - get it from the list
				mBrowserHelper = GetInitializedBrowser(LocateValue);
				if (mBrowserHelper == null)
				{
					mBrowserHelper = new BrowserHelper(browser,LocateValue);
					lstmBrowser.add(mBrowserHelper);

					//Scripts=PL.GetListString();

					if (IsBrowserBusyWithImplicitSync(implicitBrowserWait))
					{
						RemoveFromlstmBrowser(LocateValue);
						return PayLoad.Error("Browser not fully loaded");
					}
					mBrowserHelper.InjectInitializationScripts(Scripts);
					
					//Set the Implicit Wait time on Java script side
					PayLoad PLAgentConfig= new PayLoad("GingerAgentConfig");
					PLAgentConfig.AddValue(implicitWait+"");
					PLAgentConfig.ClosePackage();
					PayLoad agentConfigResponse=mBrowserHelper.ExceuteJavaScriptPayLoad(PLAgentConfig);
     				//TODO: validate agentConfigResponse
					
					return PayLoad.OK("Active Browser set to " +  LocateBy + " " + LocateValue + "");
				}
				else
				{
					GingerAgent.WriteLog("Browser is already initialized");
					return PayLoad.OK("Browser is already initialized");						
				}
		}
		else
		{					 
			return PayLoad.Error("Browser Element not found - " + LocateBy + " " + LocateValue);
		}
	}
	
	private PayLoad IsWindowExist(String LocateBy,String LocateValue )
	{
		//TODO: change the PayLoad to return boolean value instead of string
		
		String windowExist=mSwingHelper.isWindowExist(LocateBy, LocateValue).toString();
		
		PayLoad Response = new PayLoad("isWindowExist");
		Response.AddValue(windowExist);
		Response.ClosePackage();
		return Response;		
	}
	
	private PayLoad HandleisElementDisplayed(String LocateBy,String LocateValue )
	{		
		Component c = mSwingHelper.FindElement(LocateBy, LocateValue);	
	  	PayLoad PL=new PayLoad("isElementDisplayed");
		try 
		{
			if (c != null)
			{
				if (c.isVisible() && c.isEnabled() && c.isShowing())
				{
					PL.AddValue("True");
					PL.ClosePackage();
					return PL;
				}
			}
				PL.AddValue("False");
				PL.ClosePackage();
				return PL;								
		} 
		catch (Exception e) 
		{				
			PL.AddValue("false");
			PL.ClosePackage();
			return PL;
		}

	}
	
	private List<PayLoad> HandleScreenShots(String sType)
	{
		List<PayLoad> list = new ArrayList<PayLoad>() ;
		byte[] data = null;			
								
		
		if (sType.equals("OnlyActiveWindow"))
		{
			Window window = null;
			window = mSwingHelper.getCurrentWindow();
				
				data = ConvertWindowTobyte(window);		
				PayLoad f = new PayLoad("ScreenShot");
				f.AddBytes(data);
				f.ClosePackage();
				list.add(f);			
		}
	

		if (sType.equals("AllAvailableWindows"))
		{		
			Window[] sWindow = SwingHelper.GetAllWindowsByReflection(); 
			String title = null;
			
			for (Window a : sWindow )
			{	
				if (!a.isVisible() || a.getBounds().width<=0 || a.getBounds().height<=0)
					continue;
				
				if (a instanceof JFrame)
				{
					title = ((JFrame)a).getTitle();
				}
				if (a instanceof JDialog)
	    		{    		
					title= ((JDialog)a).getTitle();
	    		}
								
				if ((title!=GingerAgentFrame.GINGER_AGENT_CONSOLE))
				{				
					data = ConvertWindowTobyte(a);
					PayLoad f = new PayLoad("ScreenShot");
					f.AddBytes(data);
					f.ClosePackage();
					list.add(f);
				}
			}
		}
				
		if(mBrowserHelper !=null){

			if (mBrowserHelper.isBrowserValid()) 
			{
				GingerAgent.WriteLog("Inside Browser for SS"+ mBrowserHelper.toString());
				
				PayLoad s = mBrowserHelper.getScreenShot();
				
				if (s.Name.equalsIgnoreCase("ERROR")) {
				
					String errMsg = s.GetValueString();
					GingerAgent.WriteLog("Error:" + errMsg);
					
					if (errMsg.indexOf("Unknown runtime error") > -1) 
					{
						String winTitleToAdd = "";
						Window winCurrentWindow = mSwingHelper.getCurrentWindow();
						do 
						{
							if (winCurrentWindow instanceof JFrame) 
							{
								winTitleToAdd = ((JFrame) winCurrentWindow).getTitle() + "##" + winTitleToAdd;
								
								winCurrentWindow = (Window) ((JFrame) winCurrentWindow).getParent();

							}
							if (winCurrentWindow instanceof JDialog) 
							{
								winTitleToAdd = ((JDialog) winCurrentWindow).getTitle() + "##" + winTitleToAdd;
										
								winCurrentWindow = (Window) ((JDialog) winCurrentWindow).getParent();
										
							}
							GingerAgent.WriteLog("winTitleToAdd::"+ winTitleToAdd);
								
						} while ((winCurrentWindow instanceof JFrame)|| (winCurrentWindow instanceof JDialog));
								
						PayLoad plURL = new PayLoad("GetPageURL");
						
						PayLoad plURLResp = mBrowserHelper.ExceuteJavaScriptPayLoad(plURL);
						String sBrowserURL = plURLResp.GetValueString();
						winTitleToAdd = winTitleToAdd + sBrowserURL;
						GingerAgent.WriteLog("winTitleToAdd" + winTitleToAdd);
						list.add(PayLoad.Error("ERROR: Handle : "
								+ winTitleToAdd));
					} 
					else if(errMsg.contains("Failed"))
					{
						GingerAgent.WriteLog("screenshot failure failure:"+errMsg);
					}					
					else
					{
						mBrowserHelper = null;
					}
				} 
				else{
					list.add(s);
				}
					
			} 
			else 
			{
				mBrowserHelper = null;
			}
		}		
		if(list.size()>1)
			Collections.reverse(list);
		return list;
	}
	
	private byte[] ConvertWindowTobyte (final Window sWindow)
	{
		byte[] data = null;			
		Rectangle rec = null;
		final String[] resp= new String[1];
		resp[0]="false";
		
		rec = sWindow.getBounds();		
		final BufferedImage exportImage  = new BufferedImage(rec.width, rec.height, BufferedImage.TYPE_INT_ARGB);
		
		
		Runnable r = new Runnable() {
			public void run() 
			{				
				sWindow.paint(exportImage.getGraphics());
				resp[0]= "true";
			}
		};
		if (SwingUtilities.isEventDispatchThread())
		{
			GingerAgent.WriteLog("\n***************\nTake Screenshot-already in EDT\n***************");
			r.run();
		}
		else
		{
			GingerAgent.WriteLog("\n***************\nTake Screenshot-run in EDT\n***************");
			
			try {
				SunToolkit.flushPendingEvents();
				SunToolkit.executeOnEDTAndWait(sWindow, r);

			} catch (InvocationTargetException e) {
						GingerAgent.WriteLog("Inovation target exception while starting thread for click-"+e.getMessage());
						e.printStackTrace();
					} catch (InterruptedException e) {
						GingerAgent.WriteLog("InterruptedException while starting thread for click-"+e.getMessage());
						e.printStackTrace();						
			}
		}

		while (resp[0] == "false" && !mWaitForIdleHandler.isCommandTimedOut)
		{	
			try {
				Thread.sleep(1000);
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}					
		}	
		if (exportImage != null) 
		{
			java.io.ByteArrayOutputStream os = new java.io.ByteArrayOutputStream();
			try {
				ImageIO.write(exportImage, "png", os);
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			data = os.toByteArray();		
			try {
				os.close();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		 }
		return data;	
	}
	
	
	private void RemoveActionListener(Container container, Recorder recorder) {		
		//TODO: verify that this code is working
		Component[] list = container.getComponents();

    	for (Component c : list)
    	{   
    		if (c instanceof JComboBox)
    		{
    			//Removing all listeners for component
    			for (ActionListener al : ((JComboBox)c).getActionListeners()){
    				((JComboBox)c).removeActionListener(al);
    			}    			
    			((JComboBox)c).removeActionListener(mRecorder); 
    		}

			if (c instanceof JRadioButton)
    		{
    			//Removing all listeners for component
    			for (ActionListener al : ((JRadioButton)c).getActionListeners()){
    				((JRadioButton)c).removeActionListener(al);
    			}				
				((JRadioButton)c).removeActionListener(mRecorder);    			
    		}
			
			if (c instanceof JCheckBox)
    		{
    			//Removing all listeners for component
    			for (ActionListener al : ((JCheckBox)c).getActionListeners()){
    				((JCheckBox)c).removeActionListener(al);
    			}					
    			((JCheckBox)c).removeActionListener(mRecorder);    			
    		}			
			
			
			if (c instanceof JMenu)
    		{
    			//Removing all listeners for component
    			for (ActionListener al : ((JMenu)c).getActionListeners()){
    				((JMenu)c).removeActionListener(al);
    			}					
				JMenu m = ((JMenu)c);
				mRecorder.MenuRemoveActionListenerRecursive(m);								
    		}
												
			if (c instanceof JList)
    		{				
    			((JList)c).removeListSelectionListener(mRecorder);    			
    		}	

			if (c instanceof JTree)
    		{
					
    			((JTree)c).removeTreeSelectionListener(mRecorder);    			  			
    			((JTree)c).removeTreeExpansionListener(mRecorder);
    			
    		}	
    		
			if (c instanceof JTextArea)
    		{
				((JTextArea)c).removeFocusListener(mRecorder);    			
    		}

			if (c instanceof JTextPane)
    		{
				((JTextPane)c).removeFocusListener(mRecorder);    			
    		}	

			if (c instanceof JTable)
    		{
				//System.out.println( "JTable action Fired!");
				
				JTable table = ((JTable)c);
				//TODO Add proper modification for get cell data.
				//table.getModel().addTableModelListener(this);
				table.removeFocusListener(mRecorder);					
    		}	
			
    		//TODO: add other classes which are not covered, try to find the base class which impl addActionListener	
    		    		    	
			if (c instanceof JTabbedPane)
			{
				JTabbedPane jtp=(JTabbedPane)c;
				jtp.removeChangeListener(recorder);
				
			}
			    	
			if (c instanceof AbstractButton)
    		{			
    			((AbstractButton)c).removeActionListener(mRecorder);    			
    		}
    		
    		if (c instanceof JTextField)
    		{
    			for (ActionListener al : ((JTextField)c).getActionListeners()){
    				((JTextField)c).removeActionListener(al);
    			}    
    			((JTextField)c).removeActionListener(mRecorder);
    		}
    		
    		//TODO: add other classes which are not covered, try to find the base class which impl addActionListener	
    		
    		if (c instanceof Container)
    		{
    			//drill down recursive 
    			RemoveActionListener((Container)c, recorder);
    		}
    		
    	}
	}
	

	
	private Component FindElementWithImplicitSync(String LocateBy, String LocateValue)
	{

		Component comp=mSwingHelper.FindElement(LocateBy,LocateValue);
		
		boolean bStopWaiting=false;
		long start = System.currentTimeMillis();
		long elapsed = System.currentTimeMillis() - start;
		
		while(!bStopWaiting && !mWaitForIdleHandler.isCommandTimedOut)
		{
			if(elapsed> implicitWait*1000)
			{
				GingerAgent.WriteLog("*********Sync for Find element timeout after: "+elapsed);
				break;
			}
			
			if(comp==null)
			{
				comp=mSwingHelper.FindElement(LocateBy, LocateValue);			
			}
			else if (comp.isVisible() && comp.isEnabled() && comp.isShowing())
			{				
				bStopWaiting=true;
			}		
			
			try {
				Thread.sleep(500);
			} catch (InterruptedException e) {					
				e.printStackTrace();
			}
			
			elapsed = System.currentTimeMillis() - start;	
			//GingerAgent.WriteLog("Find Element Waiting for element to be ready:"+elapsed);
		}
		
		return comp;
		
	}
	
private PayLoad HandleElementAction(String locateBy, String locateValue,
			String controlAction, String Value, String ValueToSelect, String XCoordinate, String YCoordinate) 
	{	

		Component c=null;
		//If the action is for IsEnabled or IsVisible property then no need of sync		
		if (IsImplicitSyncRequired(controlAction, Value, ValueToSelect)) 
		{
			c = FindElementWithImplicitSync(locateBy, locateValue);
		} 
		else 
		{
			c = mSwingHelper.FindElement(locateBy, locateValue);
		}	
					
		// Handle Text Field		
		if (c!= null)
		{			
			GingerAgent.WriteLog("Element Found - " + locateValue);	
			GingerAgent.WriteLog("Control Action - " + controlAction);
			GingerAgent.WriteLog("Value To Select - " + ValueToSelect);
			GingerAgent.WriteLog("Value - " + Value);
			GingerAgent.WriteLog("XCoordinate - " + XCoordinate);
			GingerAgent.WriteLog("YCoordinate - " + YCoordinate);		

			if (controlAction.equals("Type"))
			{
				GingerAgent.WriteLog("Inside the HandleElementAction - Type");
				if(mSwingHelper.getCurrentWindow() instanceof JFrame)				
				{
					((JFrame)mSwingHelper.getCurrentWindow()).setExtendedState(Frame.MAXIMIZED_BOTH);
					((JFrame)mSwingHelper.getCurrentWindow()).requestFocus();
				}
				else if(mSwingHelper.getCurrentWindow() instanceof JDialog)
				{	
					((JFrame)mSwingHelper.getCurrentWindow()).setExtendedState(Frame.MAXIMIZED_BOTH);
					((JDialog)mSwingHelper.getCurrentWindow()).requestFocus();
				}
				try {
					Thread.sleep(500);
				} catch (InterruptedException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
				JTextField tf = (JTextField)c;
				GingerAgent.WriteLog("JTextField grabFocus");
				tf.grabFocus();
				
				try {

				    type(Value);

				} catch (Exception e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
					return PayLoad.Error(e.getMessage());
				}
				return PayLoad.OK("Type operation Passed");
				
			}

			if (controlAction.equals("SetFocus"))
			{
				return SetComponentFocus(c);
		    }
			if (controlAction.equals("SetValue"))
			{		
				//TODO: Need to fine a better way for sync. But this is helping to run flow smoothly for now
				GingerAgent.WriteLog("Inside the HandleElementAction - SetValue");
				return SetComponentValue(c, Value);
			}			
			if (controlAction.equals("SetDate"))
			{		
				//TODO: Need to fine a better way for sync. But this is helping to run flow smoothly for now
				return SetComponentDate(c, Value);
			}
			if (controlAction.equals("GetValue"))
			{
				GingerAgent.WriteLog("Inside GetValue");
				return GetComponentValue(c);
			}
			if (controlAction.equals("GetState"))
			{
				return GetComponentState(c);
			}
			if (controlAction.equals("GetAllValues"))
			{
				GingerAgent.WriteLog("Inside GetAllValues");
				return GetAllValues(c);
			}
			if (controlAction.equals("Click"))
			{	
			    GingerAgent.WriteLog("Coordinates = " + Value);
				GingerAgent.WriteLog("Before Click and Wait");
				GingerAgent.WriteLog("controlAction :: Click Value > " + Value);				
				
				PayLoad plrc = ClickComponent(c, Value, mCommandTimeout);
								
				GingerAgent.WriteLog("After Click and Wait");
				return plrc;
	
			}
			if (controlAction.equals("WinClick"))
			{					
				if (XCoordinate != null && !XCoordinate.isEmpty() && YCoordinate != null && !YCoordinate.isEmpty())		
				{
					Value = XCoordinate+","+ YCoordinate;
				}			
				PayLoad plrc =  WinClickComponent(c,Value,-2);
				GingerAgent.WriteLog("After Win Click and Wait");
				return plrc;				
			}
			if (controlAction.equals("winDoubleClick"))
			{				
				PayLoad plrc =  winDoubleClickComponent(c,Value,-2);
				GingerAgent.WriteLog("After Win Click and Wait");
				return plrc;				
			}
			if (controlAction.equals("MouseClick"))
			{	if (XCoordinate != null && !XCoordinate.isEmpty() && YCoordinate != null && !YCoordinate.isEmpty())		
				{
					Value = XCoordinate+","+ YCoordinate;
				}	
			    GingerAgent.WriteLog("Coordinates = " + Value);
				PayLoad plrc =  MouseClickComponent(c,Value,1);
				GingerAgent.WriteLog("After Mouse Click");
				return plrc;				
			}
			if (controlAction.equals("MousePressRelease"))
			{	if (XCoordinate != null && !XCoordinate.isEmpty() && YCoordinate != null && !YCoordinate.isEmpty())		
				{
					Value = XCoordinate+","+ YCoordinate;
				}	
			    GingerAgent.WriteLog("Coordinates = " + Value);
				GingerAgent.WriteLog("Inside Mouse Press/Release");
				PayLoad plrc =  MousePressReleaseComponent(c,Value,mCommandTimeout,1);
				GingerAgent.WriteLog("After Mouse Press/Release");
				return plrc;				
			}
			if (controlAction.equals("DoubleClick"))
			{	
			
				if(c instanceof JTree)  
				{
					Object treeNode=getTreeNodeFromPathAndSet((JTree) c, Value);
					if(treeNode == null)				
					{				
						return PayLoad.Error("Path " + Value + " not found");
					}
					((JTree)c).requestFocus();
					try {
						Thread.sleep(500);
					} catch (InterruptedException e) {						
						e.printStackTrace();
					}					
					TreePath p = null;
					String[] nodes = Value.split("/");					
					for (String node : nodes) {
						int row = (p == null ? 0 : ((JTree)c).getRowForPath(p));
						((JTree)c).expandRow(row);
						p = ((JTree)c).getNextMatch(node.trim(), row, Position.Bias.Forward);
					}				
				     Rectangle rect = ((JTree)c).getPathBounds(p);
				     ((JTree)c).scrollPathToVisible(p);				   				   
				     //Value = rect.x + "," + rect.y;
					 Value = (rect.x+rect.width/2) + "," + (rect.y+rect.height/2);
				}
				PayLoad plrc =  MousePressAndReleaseComponent(c,Value,mCommandTimeout,2);
				GingerAgent.WriteLog("After Mouse Double Click");
				return plrc;				
			}
			if (controlAction.equals("AsyncClick"))
			{				
				return ClickComponent(c,Value,-1);								
			}
			if(controlAction.equals("Toggle"))
			{
				return ToggleComponentValue(c);
			}
			if(controlAction.equals("Select"))
			{
				if(ValueToSelect != null && !ValueToSelect.isEmpty())
				{
					Value = ValueToSelect;
				}
				return SetComponentSelected(c,Value,mCommandTimeout);
			}
			if(controlAction.equals("AsyncSelect"))
			{
				if(ValueToSelect != null && !ValueToSelect.isEmpty())
				{
					Value = ValueToSelect;
				}
				return SetComponentSelected(c,Value,-1);
			}
			if(controlAction.equals("SelectByIndex"))
			{
				int index;
				if(ValueToSelect != null && !ValueToSelect.isEmpty())
				{
					index = Integer.parseInt(ValueToSelect);
				}
				else 
					index=Integer.parseInt(Value);
				
				GingerAgent.WriteLog("Value: " + Value);
				GingerAgent.WriteLog("ValueToSelect: " + ValueToSelect);
				GingerAgent.WriteLog("Index: " + index);
				
				
				GingerAgent.WriteLog("Inside Select By Index If condition");
				return SelectItemByIndex(c,index);
			}
			if(controlAction.equals("GetValueByIndex"))
			{
				int index=Integer.parseInt(Value);
				return GetValueByIndex(c,index);
			}
			if(controlAction.equals("IsChecked"))
			{
				return IsCheckboxChecked(c);
			}
			if(controlAction.equals("GetItemCount"))
			{
				GingerAgent.WriteLog("tetsing getitemcount");
				return GetItemCount(c);
			}
		    if(controlAction.equals("GetControlProperty"))
			{
		    	if(ValueToSelect != null && !ValueToSelect.isEmpty())
				{
		    		return getControlProperty(c, ValueToSelect);
				}
				else  
					return getControlProperty(c, Value);
			}			
		    if(controlAction.equals("IsEnabled") || controlAction.equals("IsVisible") || controlAction.equals("GetName") || controlAction.equals("IsMandatory"))
			{
				return getControlProperty(c, controlAction);
			}
			if(controlAction.equals("GetDialogText"))
			{
				return GetDialogComponentText(c);
			}
			
			else if (controlAction.startsWith("Scroll"))
			{
				return ScrollElement(c,controlAction,Value);
			}			
			else if (controlAction.equals("SendKeys"))
			{
				return SendKeys(c,Value);
			}	
			else if (controlAction.equals("SendKeyPressRelease"))
			{
				return SendKeyPressRelease(c);
			}		
			else 
			{
				return PayLoad.Error("Unknown Control Action - " + controlAction);
			}			
			
		}
		else if(controlAction.equals("GetControlProperty") && Value.equalsIgnoreCase("VISIBLE") )
		{
			PayLoad Response = new PayLoad("ComponentPropValue");
			Response.AddValue("false");		
			Response.ClosePackage();
			return Response;
		}	
		else
		{
			return PayLoad.Error("Element not found - " + locateBy + " " + locateValue);
		}
	}

	
	private Boolean IsImplicitSyncRequired(String controlAction, String Value, String ValueToSelect)
	{
		if(controlAction.equals("IsEnabled") || controlAction.equals("IsVisible"))
		{
			return false;
		}
		else if(controlAction.equals("GetControlProperty"))
		{
			if(Value!=null)
			{
				if( Value.equals("ISENABLED") || Value.equals("ISVISIBLE")|| Value.equals("VISIBLE")|| Value.equals("ENABLED"))
				{
					return false;
				}
			}
			else if(ValueToSelect!=null)
			{
				if( ValueToSelect.equals("ISENABLED") || ValueToSelect.equals("ISVISIBLE")|| ValueToSelect.equals("VISIBLE")|| ValueToSelect.equals("ENABLED"))
				{
					return false;
				}
			}
		}
		return true;
	}

	

	//TODO: fix coordinate to be better with X,Y not string...
	private PayLoad MousePressReleaseComponent(final Component c,final String Coordinate, final int Timeout,final int numOfClicks) {
		 final String[] response = new String[3];

		 response[0]="false";// Set it to true before any doclick method inside
		 response[1]="false";// to ensure the click passed and used to come out in case no response from application
		 response[2]=""; // to keep error message
		 
		Runnable r = new Runnable() {
			public void run() 
			{
				GingerAgent.WriteLog("Timeout: " + Timeout + ", Coordinate: " + Coordinate);
				GingerAgent.WriteLog("JComponent Info: " + c.getName() + "Height : "+ c.getHeight() + "Width : " + c.getWidth()+
						" Focus lis=" + c.getFocusListeners().length + " mouse lis=" + c.getMouseListeners().length +
						"Number of Clicks: " + numOfClicks);

				GingerAgent.WriteLog("Inside MousePressReleaseComponent");
				GingerAgent.WriteLog("Grab focus and doClick only");

				((JComponent)c).grabFocus();
				
				
				long when = System.currentTimeMillis();
				int x,y;
				
				 if (!Coordinate.isEmpty())
				 {
					 String[] sValue = Coordinate.split(",");
					 List<Integer> intlist =  ConvertListStringToInt(sValue);
					  x = intlist.get(0);
					  y = intlist.get(1);
				 }
				 else
				 {
					x = c.getWidth() /2;
					y = c.getHeight() /2;
				 }
							
				
					
					GingerAgent.WriteLog("Sending Mouse Press to C");
					
						MouseEvent me = new MouseEvent(c, MouseEvent.MOUSE_CLICKED, when, InputEvent.BUTTON1_DOWN_MASK , x, y, numOfClicks, false);
										
					response[0] = "true";	
					c.dispatchEvent(me);
					
					try {
						Thread.sleep(500);
					} catch (InterruptedException e) {					
						e.printStackTrace();
					}
					
					GingerAgent.WriteLog("Sending Mouse Release to C");
					response[1] = "true";
				    
			}
		};
		
		if(!(Timeout==-1))
		{
			//If the timeout is not -1 i.e. if is not asycn click
			//Then We use Suntoolkit.executeOnEDTAndWait. 
			//This executes a chunk of code on the Java event handler thread for the given target.  
			// And Waits for the execution to occur before returning to the caller.		
			if (SwingUtilities.isEventDispatchThread())
			{
				GingerAgent.WriteLog("\n***************\nMousePressReleaseComponent-already in EDT\n***************");
				r.run();
			}
			else
			{
				GingerAgent.WriteLog("\n***************\nMousePressReleaseComponent-run in EDT\n***************");
				try {
					SunToolkit.flushPendingEvents();
					SunToolkit.executeOnEDTAndWait(c, r);
					
				} catch (InvocationTargetException e) {
					GingerAgent.WriteLog("Inovation target exception while starting thread for click-"+e.getMessage());
					e.printStackTrace();
				} catch (InterruptedException e) {
					GingerAgent.WriteLog("InterruptedException while starting thread for click-"+e.getMessage());
					e.printStackTrace();						
				}
			}	
		}

		else
		{
			//If it is async Click then we do not use SunToolkit
			Thread t1= new Thread(r);			
			t1.start();
		}
		
		int iTimeout= -1;
		try 
		{				
			if(Timeout == -1) // Async Click
				while (response[0] != "true")					
					Thread.sleep(1);							
			else		
				while (response[1] == "false" && iTimeout<=Timeout)
				{	
					Thread.sleep(1000);
					iTimeout =iTimeout +1;				
				}			
		} 		
		
		catch (Exception e) {
			return PayLoad.Error(" PayLoad ClickComponent Error: " + e.getMessage());
		}		
		
		if (response[0] == "false")
			return PayLoad.Error("Fail to perform click operation");
		
		if (Timeout != -1 && response[1] == "false")
			return PayLoad.OK("Click Activity Passed after Timeout");
		
		if (response[2] != "")
			return PayLoad.Error(response[2]);
		else
			return  PayLoad.OK("Click Activity Passed");
	}

	private PayLoad MousePressAndReleaseComponent(final Component c,final String Coordinate, final int Timeout,final int numOfClicks) {
		 final String[] response = new String[3];

		 response[0]="false";// Set it to true before any doclick method inside
		 response[1]="false";// to ensure the click passed and used to come out in case no response from application
		 response[2]=""; // to keep error message
		 
		Runnable r = new Runnable() {
			public void run() 
			{
				GingerAgent.WriteLog("Timeout: " + Timeout + ", Coordinate: " + Coordinate);
				GingerAgent.WriteLog("JComponent Info: " + c.getName() + "Height : "+ c.getHeight() + "Width : " + c.getWidth()+
						" Focus lis=" + c.getFocusListeners().length + " mouse lis=" + c.getMouseListeners().length +
						"Number of Clicks: " + numOfClicks);
				
				GingerAgent.WriteLog("Inside MousePressAndReleaseComponent");
				GingerAgent.WriteLog("Grab focus and doClick only");

				((JComponent)c).grabFocus();
								

				long when = System.currentTimeMillis();
				int x,y;
				
				 if (!Coordinate.isEmpty())
				 {
					 String[] sValue = Coordinate.split(",");
					 List<Integer> intlist =  ConvertListStringToInt(sValue);
					  x = intlist.get(0);
					  y = intlist.get(1);
				 }
				 else
				 {
					x = c.getWidth() /2;
					y = c.getHeight() /2;
				 }
							
				
					
					GingerAgent.WriteLog("Sending Mouse Press to C");
					
						MouseEvent me = new MouseEvent(c, MouseEvent.MOUSE_PRESSED, when, InputEvent.BUTTON1_MASK , x, y, numOfClicks, false);
						c.dispatchEvent(me);
						try {
							Thread.sleep(1000);
						} catch (InterruptedException e) {					
							e.printStackTrace();
						}
						 me = new MouseEvent(c, MouseEvent.MOUSE_RELEASED, when, InputEvent.BUTTON1_MASK , x, y, numOfClicks, false);
						c.dispatchEvent(me);
									
					response[0] = "true";						
					try {
						Thread.sleep(1000);
					} catch (InterruptedException e) {					
						e.printStackTrace();
					}
					
					GingerAgent.WriteLog("Sending Mouse Release to C");
					response[1] = "true";

			}
		};
		
		if(!(Timeout==-1))
		{
			//If the timeout is not -1 i.e. if is not asycn click
			//Then We use Suntoolkit.executeOnEDTAndWait. 
			//This executes a chunk of code on the Java event handler thread for the given target.  
			// And Waits for the execution to occur before returning to the caller.
			if (SwingUtilities.isEventDispatchThread())
			{
				GingerAgent.WriteLog("\n***************\nMousePressAndReleaseComponent-already in EDT\n***************");
				r.run();
			}
			else
			{
				GingerAgent.WriteLog("\n***************\nMousePressAndReleaseComponent-run in EDT\n***************");
				try { 
					GingerAgent.WriteLog("Inside ");
					SunToolkit.flushPendingEvents();
					SunToolkit.executeOnEDTAndWait(c, r);
					GingerAgent.WriteLog("After executeOnEDTAndWait");
					
				} catch (InvocationTargetException e) {
					GingerAgent.WriteLog("Inovation target exception while starting thread for click-"+e.getMessage());
					e.printStackTrace();
				} catch (InterruptedException e) {
					GingerAgent.WriteLog("InterruptedException while starting thread for click-"+e.getMessage());
					e.printStackTrace();						
				}
			}
		}		
		
		else
		{
			//If it is async Click then we do not use SunToolkit
			Thread t1= new Thread(r);			
			t1.start();
		}
		GingerAgent.WriteLog("Inside 2" +  response[0]  + ":" +  response[1]);	
		int iTimeout= -1;
		try 
		{				
			if(Timeout == -1) // Async Click
				while (response[0] != "true")
				{
					GingerAgent.WriteLog("Inside 31" +  iTimeout  + ":" +  Timeout + ":"  + response[0]);	
					Thread.sleep(1);
				}										
			else		
				while (response[1] == "false" && iTimeout<=Timeout)
				{	
					GingerAgent.WriteLog("Inside 32" +  iTimeout  + ":" +  Timeout + ":" + response[1]);	
					Thread.sleep(1000);
					iTimeout =iTimeout +1;				
				}	
			GingerAgent.WriteLog("Inside 4" +  response[0]  + ":" +  response[1]);	
		} 		
		
		catch (Exception e) {
			return PayLoad.Error(" PayLoad ClickComponent Error: " + e.getMessage());
		}		
		
		if (response[0] == "false")
			return PayLoad.Error("Fail to perform click operation");
		
		if (Timeout != -1 && response[1] == "false")
			return PayLoad.OK("Click Activity Passed after Timeout");
		
		if (response[2] != "")
			return PayLoad.Error(response[2]);
		else
			return  PayLoad.OK("Click Activity Passed");
	}

	private PayLoad SendKeys(final Component c, final String value) {

		if (c instanceof JTextField) {
			final JTextField tf = (JTextField) c;

			Runnable r = new Runnable() {
				public void run() {
					tf.grabFocus();
					// TODO: clear text - check if when or not needed always?

					for (int i = 0; i < value.length(); i++) {
						char c1 = value.charAt(i);

						long when = System.currentTimeMillis();

						// sure it will be multi lang supported
						KeyEvent ke = new KeyEvent(tf, KeyEvent.KEY_TYPED, when, 0, KeyEvent.VK_UNDEFINED, c1);

						tf.dispatchEvent(ke);
					}

				}
			};

			if (SwingUtilities.isEventDispatchThread()) {
				GingerAgent.WriteLog("\n***************\nSendKeys-already in EDT\n***************");
				r.run();
			} else {
				GingerAgent.WriteLog("\n***************\nSendKeys-run in EDT\n***************");
				try {
					SunToolkit.executeOnEDTAndWait(c, r);
				} catch (InvocationTargetException e) {
					e.printStackTrace();
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
			}

			if (tf.getText().equals(value)) {
				return PayLoad.OK("JTextField value set to " + value);
			} else {
				return PayLoad.Error("JTextField value is '" + tf.getText() + "' instead of '" + value + "'");
			}
		} else {
			return PayLoad.Error("SendKeys - not supported for Component type " + c.getClass().getName());
		}

	}
	
	private PayLoad SendKeyPressRelease(final Component c) {
	
			final JTextField tf = (JTextField)c;
			
				Runnable r = new Runnable() {
				public void run() 
				{							
					tf.grabFocus();
					
					tf.dispatchEvent( new KeyEvent(tf, KeyEvent.KEY_PRESSED, System.currentTimeMillis(),
						    0, KeyEvent.VK_UP, (char)KeyEvent.VK_UP) );
				        
					tf.dispatchEvent( new KeyEvent(tf, KeyEvent.KEY_RELEASED, System.currentTimeMillis(),
						    0, KeyEvent.VK_UP, (char)KeyEvent.VK_UP) );
				        
				}
			};
			if (SwingUtilities.isEventDispatchThread())
			{
				GingerAgent.WriteLog("\n***************\nSendKeyPressRelease-already in EDT\n***************");
				r.run();
			}
			else
			{
				GingerAgent.WriteLog("\n***************\nSendKeyPressRelease-run in EDT\n***************");
				try {
					SunToolkit.executeOnEDTAndWait(c, r);
				} catch (InvocationTargetException e) {
					e.printStackTrace();
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
			}			

				return PayLoad.OK("JTextField tab key sent");	
		
	}

	private PayLoad MouseClickComponent(final Component c,final String Coordinate, final int NumOfClicks) 
	{
		Runnable r = new Runnable() {
			public void run() 
			{				
					JComponent b = (JComponent)c;
					
					GingerAgent.WriteLog("JComponent Info: " + b.getName() 
					+ " Actions lis=" + " Focus lis=" + b.getFocusListeners().length 
					+ " mouse lis=" + b.getMouseListeners().length
					+ " Coordinate=" + Coordinate);

					b.grabFocus();					
					long when = System.currentTimeMillis();
					int x,y;
					
						if (!Coordinate.isEmpty())
						 {
							 String[] sValue = Coordinate.split(",");
							 List<Integer> intlist =  ConvertListStringToInt(sValue);
							  x = intlist.get(0);
							  y = intlist.get(1);
						 }
						 else
						 {
							x = c.getWidth() /2;
							y = c.getHeight() /2;
						 }						
					
					boolean IsOfPopUp = false;					
										
					//TODO: verify correctness of params		
					
						GingerAgent.WriteLog("Mouse Event Click at point X:"+x+" Y:"+y);
						MouseEvent me = new MouseEvent(c, MouseEvent.MOUSE_CLICKED, when, MouseEvent.BUTTON1 , x, y, NumOfClicks, IsOfPopUp, 0);				
						c.dispatchEvent(me);																
			}
		};
		if (SwingUtilities.isEventDispatchThread())
		{
			GingerAgent.WriteLog("\n***************\nMouseClickComponent-already in EDT\n***************");
			r.run();
		}
		else
		{
			GingerAgent.WriteLog("\n***************\nMouseClickComponent-run in EDT\n***************");
			try {
				SunToolkit.flushPendingEvents();
				SunToolkit.executeOnEDTAndWait(c, r);
			} catch (InvocationTargetException e) {
				e.printStackTrace();
			} catch (InterruptedException e) {
				e.printStackTrace();
			}
		}		

		return PayLoad.OK("Done");
	}

	private PayLoad HandleWindowAction(String locateBy, String locateValue,
			String controlAction) 
	{		
 		if (controlAction.equals("IsExist"))
		{
		  return IsWindowExist(locateBy, locateValue);
		}
 		else if (controlAction.equals("Switch"))
 		{
 				if (mSwingHelper.SwitchWindow(locateValue))
 				{
 					return PayLoad.OK("Switch Window Performed");
 				}
 				else
 				{
 					return PayLoad.Error("Window not found with title : " + locateValue);				
 				}		
 		}
		else if (controlAction.equals("CloseWindow"))
		{	

			if (!mSwingHelper.SwitchWindow(locateValue))
			{
				return PayLoad.Error("Window not found with title : " + locateValue);
			}

			
			 Runnable r1 = new Runnable() {
					
					public void run() 
					{	
						try 
						{
							mSwingHelper.getCurrentWindow().dispatchEvent(new WindowEvent(mSwingHelper.getCurrentWindow(), WindowEvent.WINDOW_CLOSING));
							Thread.sleep(1000);
						}
						catch(Exception e){	
							
						}
					}		
				};
				
				Thread t1= new Thread(r1);			
				t1.start();
				

				return PayLoad.OK("Close Window Performed");
		}
		else 
		{
			return PayLoad.Error("Unknown Window Action - " + controlAction);
		}	
		
	}
		
	private PayLoad ScrollElement(Component c, String controlAction,String Value) {
	
		int iOrientation = 0;
		
		
		if (c != null)
		{
			
			if (c instanceof JScrollBar )
			{
				JScrollBar JSB = (JScrollBar)c;
				int CalcValue = 0;
				int sValue = JSB.getValue();
				CalcValue = CalcScrollByValue(c,Value);
	
				if(controlAction.equals("ScrollUp"))
				{
					sValue =sValue + CalcValue;
				}				
				else if(controlAction.equals("ScrollDown"))
				{
					sValue =sValue - CalcValue;
				}				
				else if(controlAction.equals("ScrollLeft"))
				{
					sValue =sValue - CalcValue;
				}				
				else if(controlAction.equals("ScrollRight"))
				{
					sValue =sValue + CalcValue;
				}
				else
				{
					return PayLoad.Error("Scroll operation is unknowen: " + controlAction);
				}
				
				GingerAgent.WriteLog("scroll value  before Scroll : " +JSB.getValue());				
				JSB.setValue(sValue);
				GingerAgent.WriteLog("scroll value  after Scroll : " +JSB.getValue());
				return PayLoad.OK(controlAction + " done successfully");
			}
		}	
		
		return PayLoad.Error("Control Type is not Supported: "+ c.getClass().toString());
	}
	
	private int CalcScrollByValue(Component c, String Value)
	{
		int ScrollValue = 0;
		if (Value.isEmpty())
		{
			Value = "0";
		}
		int iValue = Integer.parseInt(Value);
		
		if (c instanceof JScrollBar )
		{
			JScrollBar JSB = (JScrollBar)c;
			int Max = JSB.getMaximum();
				if (iValue > Max || iValue == 0)
				{
					ScrollValue = Max;
				}
				else
				{
					ScrollValue = iValue;
				}
			
		}
		return ScrollValue;
	}

	
	private PayLoad HandleDialogAction(String locateBy, String locateValue,
			String controlAction, String Value) 
	{	

	   JDialog dialogElement=FindDialogElement(locateBy, locateValue);
		
		if (dialogElement!= null)
		{
			GingerAgent.WriteLog("Dialog Found - " + locateValue);
			if (controlAction.equals("GetDialogText"))
			{
				String dialogText="";
				if (dialogElement.getContentPane().getComponent(0) instanceof JOptionPane)
				{
					dialogText=((JOptionPane) dialogElement.getContentPane().getComponent(0)).getMessage().toString();
					PayLoad Response = new PayLoad("DialogText");
					String val = dialogElement.getTitle();
					Response.AddValue(dialogText);		
					Response.ClosePackage();
					return Response;
				}
				else
				{
					return PayLoad.Error("Not able to retrieve text for Dialog");
				}
			}
			if(controlAction.equals("AcceptDialog"))
			{				
				JButton jb=FindDialogButton(dialogElement,"Accept");
				if(jb!=null)
				return ClickComponent(jb,Value,-1);
				else 
					return PayLoad.Error("Dialog Accept button not found - " + locateBy + " " + locateValue);
			}
			if(controlAction.equals("DismissDialog"))
			{			
				JButton jb=FindDialogButton(dialogElement,"Dismiss");
				if(jb!=null)
				return ClickComponent(jb,Value,-1);
				else 
					return PayLoad.Error("Dialog Dismiss button not found - " + locateBy + " " + locateValue);
			}
		
			else 
			{
				return PayLoad.Error("Invalid Control Action " + controlAction + " for dialog box " + locateValue);			
			}
			
		}
		else
		{
			return PayLoad.Error("Dialog not found - " + locateBy + " " + locateValue);
		}
	}
	
	private JDialog FindDialogElement(String LocateBy, String LocateValue)
    {
		Window[] allWindos = SwingHelper.GetAllWindowsByReflection(); 
  	  	JDialog dialog=null;
  	  	for(Window w:allWindos)
  	  	{
  	  		if((w instanceof JDialog))
  	  		{
  	  			dialog= (JDialog)w;
  	  			if(dialog.getTitle().equals(LocateValue) && dialog.isDisplayable() ==true)
  	  			return dialog;
  	  		}
  	  	} 
  	  	return null;
    }
	

	private JButton FindDialogButton(Container container,String value)
	{
		Component[] list = container.getComponents();
			
		JButton jb=null;
    	for (Component c : list)
    	{
    		GingerAgent.WriteLog(list[0].toString());    	
    		if(c instanceof JButton)
    		{
    			jb=(JButton)c;
    			if(value.equals("Accept"))
    			{
    			  if(jb.getText().toUpperCase().equals("OK")||jb.getText().toUpperCase().equals("YES"))  			  
    			  {
    				return jb;
    			  }
    			}    
    			else if(value.equals("Dismiss"))
    			{
      			  if(jb.getText().toUpperCase().equals("NO")||jb.getText().toUpperCase().equals("CANCEL"))  			  
      			  {
      				return jb;
      			  }
      			}    	
    		}

    		// recursive drill down
    		if (c instanceof Container)
    		{
    			JButton jBtn = FindDialogButton((Container)c,value);
    			if (jBtn != null)
    			{
    				return jBtn;
    			}    			
    		}    		
    	}    	
    	
    	return null;
	}	
	
private PayLoad GetComponentValue(Component c) 
	{	

		GingerAgent.WriteLog("Inside GetComponentValue");
		PayLoad Response = new PayLoad("ComponentValue");
		List<String> val = mSwingHelper.GetCompValue(c);
		
		if (c.getClass().getName() != null && c.getClass().getName().contains("com.amdocs.uif.widgets.DateTimeNative"))
		{
			String dateValue = mASCFHelper.getUIFDatePickerValue(c);
			if (dateValue != null || dateValue == "" )
			{
				if(val.size()==1 && val.get(0)=="")
					val.remove(0);				
				
				val.add(dateValue);
			}
			else
			{
				val.add("");
			}
		}
		
		
		
		GingerAgent.WriteLog("val: " +val);
		Response.AddValue(val);	
		Response.ClosePackage();		
		return Response;
	}
private PayLoad GetComponentState(Component c) 
{	
	PayLoad Response = new PayLoad("ComponentValue");
	String val = GetCompState(c);
	GingerAgent.WriteLog("val: " +val);
	Response.AddValue(val);	
	Response.ClosePackage();
	return Response;		
}
	

	private PayLoad GetAllValues(Component c) 
	{	
		GingerAgent.WriteLog("Inside GetComponentValue");
		PayLoad Response = new PayLoad("ComponentValue");
		List<String> val = GetComboBoxValues(c);
		GingerAgent.WriteLog("val: " +val);
		Response.AddValue(val);		
		Response.ClosePackage();
		return Response;
	}

	private PayLoad GetDialogComponentText(Component c)
	{
			JOptionPane jop=((JOptionPane)c);
		
			String val = jop.getMessage().toString();
			PayLoad Response = new PayLoad("DialogText");
			Response.AddValue(val);		
			Response.ClosePackage();
			return Response;		
	}
	
	
	private PayLoad getControlProperty(Component c, String propertyName) {
		PayLoad Response = new PayLoad("ComponentValue");
		String propValue= "";
		
		
		if (propertyName.equalsIgnoreCase("Value")) {
			
			if (c instanceof JCheckBox)
				propValue = Boolean.toString(((JCheckBox) c).isSelected());
			else if (c instanceof JRadioButton)
				propValue = Boolean.toString(((JRadioButton) c).isSelected());
				// add check for getName()=null
			else if (c.getClass().getName() != null && c.getClass().getName().contains("com.amdocs.uif.widgets.DateTimeNative"))
				propValue = mASCFHelper.getUIFDateMillisecondValue(c);			
			else
				return GetComponentValue(c);
		} 
		else if (propertyName.equalsIgnoreCase("Text")) {	
			propValue = mASCFHelper.getText(c);
		}
		else if (propertyName.equalsIgnoreCase("HTML")) {	
			propValue = mASCFHelper.getHTMLContent(c);
		}
		else if (propertyName.equalsIgnoreCase("Type")) {	
			if (c.getClass().getName() != null)
			{
				String DumpME = c.getClass().getName();
				DumpME = DumpME.substring(DumpME.lastIndexOf('.') + 1);
				DumpME = DumpME.substring(0, DumpME.length()-2);
				DumpME = DumpME.replace("Native", "");
				if (DumpME.contains("uif"))
				{
					DumpME = "Uif" + DumpME;
				} 
				propValue = DumpME;
			}
		}
		else if (propertyName.equalsIgnoreCase("Style")) {	
			// Not implemented in GTB
		}
		else if (propertyName.equalsIgnoreCase("List")) {	
			return GetComponentValue(c);
		}
		else if (propertyName.equalsIgnoreCase("DateTimeValue")) {	
			propValue = mASCFHelper.getDateTimeValue(c);
		}
		else if (propertyName.equalsIgnoreCase("ToolTip")) {		
			propValue = ((JComponent) c).getToolTipText();
		}
		else if (propertyName.equalsIgnoreCase("Color"))
		{
			Color clr= ((JComponent)c).getBackground();
			
			PayLoad pl= new PayLoad("GetControlPropertyResponse");
			
			pl.AddKeyValuePair("Hex Code", ColortoHexString(clr));
			pl.AddKeyValuePair("R", ""+clr.getRed());
			pl.AddKeyValuePair("G", ""+clr.getGreen());
			pl.AddKeyValuePair("B", ""+clr.getBlue());
			pl.ClosePackage();
			
			return pl;		
		}
		else if (propertyName.equalsIgnoreCase("Path")) {		
		}
		else if (propertyName.equalsIgnoreCase("GETCLASS") || propertyName.equalsIgnoreCase("CLASS")) {		
			propValue=c.getClass().getName();
		} else if (propertyName.equalsIgnoreCase("ISENABLED") || propertyName.equalsIgnoreCase("ENABLED")) {		
			propValue=Boolean.toString(c.isEnabled());
		}else if (propertyName.equalsIgnoreCase("ISVISIBLE") || propertyName.equalsIgnoreCase("VISIBLE")) {
			propValue = Boolean.toString(c.isVisible());			
		}else if (propertyName.equalsIgnoreCase("GETNAME") || propertyName.equalsIgnoreCase("NAME")) {		
			propValue = c.getName();
		}else if (propertyName.equalsIgnoreCase("ISMANDATORY") || propertyName.equalsIgnoreCase("MANDATORY")) {		
			GingerAgent.WriteLog("INSIDE ISMANDATORY");
			propValue = Boolean.toString(mASCFHelper.checkIsMandatory(c));	
		}  
		else {
			return PayLoad.Error("Unsupported property name");
		}
				
		Response.AddValue(propValue);		
		Response.ClosePackage();
		return Response;
	}

	
		public final static String ColortoHexString(Color colour)  {
		  String hexColour = Integer.toHexString(colour.getRGB() & 0xffffff);
		  if (hexColour.length() < 6) {
		    hexColour = "000000".substring(0, 6 - hexColour.length()) + hexColour;
		  }
		  return "#" + hexColour;
		}
	
	private PayLoad HighLightElement(Component c) {
		// Restore border for previous highlighted component
		if (CurrentHighlighedComponent != null)
		{
			((JComponent)CurrentHighlighedComponent).setBorder(CurrentHighlighedComponentOriginalBorder);
		}
		
		if (!(c instanceof JComponent))
		{			
			GingerAgent.WriteLog("Component is not JComponent - " + c.getName());
			return PayLoad.Error("Component is not JComponent - " + c.getName());
		}
		
		//Save Original Border for restore later
		CurrentHighlighedComponent = c;
		CurrentHighlighedComponentOriginalBorder = ((JComponent)CurrentHighlighedComponent).getBorder(); 
		
		Border border = BorderFactory.createLineBorder(Color.RED, 1);		
		((JComponent)c).setBorder(border);
		
		return PayLoad.OK("Done");
	}
		
	private PayLoad ClickComponent(final Component c,final String value,final int Timeout) {
		 final String[] response = new String[3];

		 GingerAgent.WriteLog("ClickComponent " + c.getClass() + " - " + value);
		 response[0]="false";// Set it to true before any doclick method inside
		 response[1]="false";// to ensure the click passed and used to come out in case no response from application
		 response[2]=""; // to keep error message
		//TODO: check control is enabled
		 if (!(c instanceof JButton) && !(c instanceof JRadioButton) && !(c instanceof JMenu) 
				 && !(c instanceof JMenuItem) && !(c instanceof JTree) && !((c instanceof JCheckBox)) 
				 && !(c instanceof JPanel) && !(c instanceof JScrollPane)
				 && !(c.getClass().toString().contains("uif.widgets.DropDownButtonNative")))
				return PayLoad.Error("Unknown Element for click action - Class=" + c.getClass().getName());
		 
		 if (c instanceof JTree)
		 {
			GingerAgent.WriteLog("c instanceof JTree");
			TreePath p = null;
			String[] nodes = value.split("/");					
			for (String node : nodes) {
				int row = (p == null ? 0 : ((JTree)c).getRowForPath(p));
				((JTree)c).expandRow(row);
				p = ((JTree)c).getNextMatch(node.trim(), row, Position.Bias.Forward);
			}
			if (p != null)
			{
				GingerAgent.WriteLog("TreePath != null");
				 Rectangle rect = ((JTree)c).getPathBounds(p);
			     ((JTree)c).scrollPathToVisible(p);				   				   
			     String value1 = rect.x + "," + rect.y;
				
				PayLoad plrc =  MousePressAndReleaseComponent(c,value1,mCommandTimeout,1);
				return plrc;
			}
			else
			{
				GingerAgent.WriteLog("ClickComponent - TreePath = null");
				return PayLoad.Error(" There is no tree path for " + value);
			}
		    
		 }
		 Runnable r = new Runnable() {
			
			public void run() 
			{	
				try 
				{
									
					  if (c instanceof JButton) {

						GingerAgent.WriteLog("inside JButton"
								+ c.getClass().toString());
						response[0] = "true";

						JButton b = (JButton) c;

						GingerAgent.WriteLog("Button Info: " + b.getName()
								+ " Actions lis="
								+ b.getActionListeners().length + " Focus lis="
								+ b.getFocusListeners().length + " mouse lis="
								+ b.getMouseListeners().length);

						GingerAgent.WriteLog("Grab focus and doClick only");

						b.grabFocus();
						b.doClick();

					}
					
					// Added to support Pop-up button next to Look Up button on CRM Screen
					 else if (c.getClass().toString().contains("uif.widgets.DropDownButtonNative"))   // also check if the control type is BUTTON
					 {					 
						GingerAgent.WriteLog("inside uif.widgets.DropDownButtonNative");
						boolean flag = mASCFHelper.performUIFDoClick(c);
						if (!flag)
						{							
							response[1] = "false";
							return;
						}
						else
							response[0] = "true";
					}
					
					//TODO: Fix me. Not working
//					if (c instanceof JLabel) {
//
//						GingerAgent.WriteLog("inside JLabel"
//								+ c.getClass().toString());
//						response[0] = "true";
//
//						JLabel b = (JLabel) c;
//
//						GingerAgent.WriteLog("JLabel Info: " + b.getName()
//								+ b.getFocusListeners().length + " mouse lis="
//								+ b.getMouseListeners().length);
//
//						GingerAgent.WriteLog("Grab focus and doClick only");
//
//						b.grabFocus();
//
//					}
					
					//TODO: Fix me. This is not working for checkbox inside jtable 
					
					if (c instanceof JCheckBox) {
						GingerAgent.WriteLog("inside JCheckBox"
								+ c.getClass().toString());
						response[0] = "true";

						JCheckBox cb = (JCheckBox) c;
						boolean b =cb.isSelected();
						
						cb.grabFocus();
						cb.doClick();
						
						if(cb.isSelected()==b)
							cb.setSelected(!b);
					}
					
					if (c instanceof JRadioButton) {
						GingerAgent.WriteLog("inside Radio Button Click"
								+ c.getClass().toString());
						response[0] = "true";

						JRadioButton b = (JRadioButton) c;
						
						GingerAgent.WriteLog("Radio Button Info: " + b.getName()
								+ " Actions lis="
								+ b.getActionListeners().length + " Focus lis="
								+ b.getFocusListeners().length + " mouse lis="
								+ b.getMouseListeners().length);
					
						GingerAgent.WriteLog("Grab focus and doClick only");

						b.grabFocus();
						b.doClick();
					}					
					if(c instanceof JMenuItem)
					{	
						JMenuItem jm=(JMenuItem)c;
						System.out.println( "Click for Menu Class is :" + c.getClass());					
						if(value.isEmpty()||value==null)
						{	
							response[0]="true";
							jm.doClick(10);						
						}
						else
						{
							if (c instanceof JMenu) {
								JMenu jm1=(JMenu)c;
								((JMenuItem)jm1).doClick(100);	
								GingerAgent.WriteLog(jm1.toString());
								Thread.sleep(500);							
								String valueCalc;
								if (value.indexOf("\\;") > 0)
									valueCalc = value.replace("\\;", "chArSplit");
								else
									valueCalc = value;
								String[] menuitem = valueCalc.split(";");					
															
								for (int j = 0 ;j <= menuitem.length -1;j++ )
								{											
									menuitem[j] = menuitem[j].replace("chArSplit", ";");								
									int i =0;							
									for(i=0;i<jm1.getItemCount();i++)
									{										
										JMenuItem jmi;
										try
										{
											jmi= jm1.getItem(i);											
											String seperatorTest= jmi.getText();//to make sure it is not a seperator (will jump to exception if it is)
										}
										catch(Exception e)
										{
											continue;//proabably seperator
										}											
										if(jmi.getText().equals(menuitem[j]) || jmi.getName().equals(menuitem[j]))
										{
											if(j== menuitem.length -1)
											{	
												GingerAgent.WriteLog(jm1.toString());
												response[0]="true";
												jmi.doClick(100);	
												Thread.sleep(500);
											}
											else
											{										
												jmi.doClick(100);	
												Thread.sleep(500);
												GingerAgent.WriteLog(jm1.toString());
												jm1=(JMenu)jmi;
											}											
											break;
										}								
									}
									if (i==jm1.getItemCount())
									{
										response[0]="true";
										response[2]="Menu Item : '" + menuitem[j] + "' not found";										
										break;
									}
								}		
							
							}	
							if (c instanceof JMenuItem) {
								JMenuItem jm2 = (JMenuItem) c;
								GingerAgent.WriteLog(" Start using Mouse events MOUSE_CLICKED: " + jm2.toString());
								MouseEvent event = new MouseEvent(
										jm2,
										MouseEvent.MOUSE_CLICKED, 0, 0, 0, 0, 0,
										true);
								jm2.dispatchEvent(event);
								GingerAgent.WriteLog(" Before sleep ");
								
								//TODO to add mechanism to reduce sleep time
								
								Thread.sleep(2500);
								event = new MouseEvent(jm2,
										MouseEvent.MOUSE_RELEASED, 0, 0, 0, 0, 0,
										false);
								jm2.dispatchEvent(event);
								GingerAgent.WriteLog(" Before sleep ");
								jm2.menuSelectionChanged(true);
								
								response[0] = "true";
								GingerAgent.WriteLog(jm2.toString());
							}
														
					  }				 
					}
					
					// Added below to handle TAB actions converted from ASCF. 
					if (c instanceof JPanel) {
						boolean status = HandleTabClickForJPanel(c);
						response[0]= String.valueOf(status);
					}
					if (c instanceof JScrollPane) {
					
						Component cPanel=FindPanelInsideScroll(c);						
						boolean status = HandleTabClickForJPanel(cPanel);
						response[0]= String.valueOf(status);
					}							
					
					response[1] = "true";
				}
				catch(Exception e){	
					response[1] = "true";
					response[2]=e.getMessage();
				}
			}				
		};
		
		
		if(!(Timeout==-1))
		{
			//If the timeout is not -1 i.e. if is not asycn click
			//Then We use Suntoolkit.executeOnEDTAndWait. 
			//This executes a chunk of code on the Java event handler thread for the given target.  
			// And Waits for the execution to occur before returning to the caller.
			if (SwingUtilities.isEventDispatchThread())
			{
				GingerAgent.WriteLog("\n***************\nClickComponent-already in EDT\n***************");
				r.run();
			}
			else
			{
				try {
					GingerAgent.WriteLog("\n***************\nClickComponent-run in EDT\n***************");
					SunToolkit.flushPendingEvents();
				
					
					SunToolkit.executeOnEDTAndWait(c, r);
					
					
					
				} catch (InvocationTargetException e) {
					GingerAgent.WriteLog("Inovation target exception while starting thread for click-"+e.getMessage());
					e.printStackTrace();
				} catch (InterruptedException e) {
					GingerAgent.WriteLog("InterruptedException while starting thread for click-"+e.getMessage());
					e.printStackTrace();
					
				}
				}			

		}
		
		else
		{
			//If it is async Click then we do not use SunToolkit
			Thread t1= new Thread(r);			
			t1.start();
		}
		
		
		try 
		{				
			if(Timeout == -1) // Async Click
				while (response[0] != "true")
					Thread.sleep(1);							
			else		
			while (response[1] == "false" && !mWaitForIdleHandler.isCommandTimedOut)
				{	
					Thread.sleep(1000);	
				}			
		} 		
		
		catch (Exception e) {
			return PayLoad.Error(" PayLoad ClickComponent Error: " + e.getMessage());
		}		
		
		if (response[0] == "false")
			return PayLoad.Error("Fail to perform click operation");
		
		if (Timeout != -1 && response[1] == "false")
			return PayLoad.OK("Click Activity Passed after Timeout");
		
		if (response[2] != "")
			return PayLoad.Error(response[2]);
		else
			return  PayLoad.OK("Click Activity Passed");
		
		// TODO: Add other type of controls + err if not known
		
	}
	
	private Component FindPanelInsideScroll(Component c)
	{
		JScrollPane scrollPane = (JScrollPane) c;
		JViewport view = scrollPane.getViewport();

		if(view!=null)
		{
			Component[] components = view.getComponents();
			for (int i = 0; i < components.length; i++) {
				if (components[i] instanceof JPanel) {
					return components[i];				
				}
			}
		}
		return null;
	}
	
	private boolean HandleTabClickForJPanel(Component c)
	{
		boolean status=false;
		if (c.getClass().toString().contains("uif")) {
			status = mASCFHelper.selectUIFTab(c);			
		} 
		else 
		{
			Component parentComp = c.getParent();

			boolean flag = false;
			while (parentComp != null && parentComp != mSwingHelper.getCurrentWindow()) {
				if (parentComp instanceof JTabbedPane) {
					flag = true;
					break;
				}
				parentComp = parentComp.getParent();
			}

			if (flag) {
				JTabbedPane jtp = (JTabbedPane) parentComp;
				for (int i = 0; i < jtp.getTabCount(); i++) {
					String tabName = jtp.getComponent(i).getName();
					GingerAgent.WriteLog("tabName = " + tabName);
					if (tabName.equals(c.getName())) {
						jtp.setSelectedIndex(i);
					}
				}
				status = true;
			} else
				status = false;
		}
		return status;
	}
	
	private PayLoad WinClickComponent(final Component c,final String value,final int Timeout) {
		 String response = "";
		
		 if (!( (c instanceof JButton) || (c instanceof JTree)))
				return PayLoad.Error("Unknown Element for Win click action - Class=" + c.getClass().getName());
		 									    
		 response=mSwingHelper.winClick(c,value);							    
		
		if (response != "")
			return PayLoad.Error(response);
		else
			return  PayLoad.OK("Win Click Activity Passed");
		
		// TODO: Add other type of controls + err if not known
		
	}

	private PayLoad winDoubleClickComponent(final Component c,final String value,final int Timeout) {
		 String response = "";
		
		 if (!( (c instanceof JButton) || (c instanceof JTree)))
				return PayLoad.Error("Unknown Element for win Double click action - Class=" + c.getClass().getName());
		 									    
		 response=mSwingHelper.winDoubleClick(c,value);							    
		
		if (response != "")
			return PayLoad.Error(response);
		else
			return  PayLoad.OK("win Double Click Activity Passed");
		
		// TODO: Add other type of controls + err if not known
		
	}	
	
	
	private PayLoad ToggleComponentValue(Component c)
	{	
		String msg="";
		if (c instanceof JCheckBox)
		{			
			JCheckBox cb=((JCheckBox)c);
			boolean b = cb.isSelected();
			try
			{	
				cb.setSelected(!b);
				
				if(cb.isSelected()==b)
					ClickComponent(cb, "", mCommandTimeout);
				
			}
			catch (Exception e) {
				// TODO Auto-generated catch block
				if(e.getMessage().indexOf("Event handler had an unexpected error") != -1)
					msg= " :: " + e.getMessage();
				else
					return PayLoad.Error(e.getMessage());
			}
			if(cb.isSelected() == b)
				return PayLoad.Error("Failed to set Toggle the Value ");
			
			return PayLoad.OK("Checbox Toggled" + msg);
		}
		else
		{
			return PayLoad.Error("Unknowon Element for Toggle action - Class=" + c.getClass().getName());
		}
	}
	
	private PayLoad SetComponentSelected(Component c,String value,int Timeout)
	{	
		String msg="";		
		final String[] response = new String[3];

		 GingerAgent.WriteLog("SetComponentSelected " + c.getClass() + " - " + value);
		 response[0]="false";// Set it to true before any doclick method inside
		 response[1]="false";// to ensure the click passed and used to come out in case no response from application
		 response[2]=""; // to keep error message
		 
		if (c instanceof JRadioButton)
		{					
			JRadioButton cb=(JRadioButton)c;
			
			
			ClickComponent(cb,value,Timeout);
			
			if (cb.isSelected() != true)
				cb.setSelected(true);
						
			if (cb.isSelected() != true)
				return PayLoad.Error("Not able to Select Radio Button");
			
			return PayLoad.OK("Radio Button Selected" + msg);
		}		
		else if (c instanceof JComboBox) {
			final JComboBox jcb = (JComboBox) c;
			
			final String ItemToSelect = value;
			Runnable r = new Runnable() {
				public void run() {
					try {
						response[0] = "true";
						jcb.requestFocus(true);
						jcb.setSelectedItem(ItemToSelect);
						response[1] = "true";
					} catch (Exception e) {
						response[1] = "true";
						GingerAgent
								.WriteLog("Exception in combo box SetSelected item "
										+ e.getMessage());
						response[2] = e.getMessage();
					}
				}
			};
			if(!(Timeout==-1))
			{
				//If the timeout is not -1 i.e. if is not asycn click
				//Then We use Suntoolkit.executeOnEDTAndWait. 
				//This executes a chunk of code on the Java event handler thread for the given target.  
				// And Waits for the execution to occur before returning to the caller.
				if (SwingUtilities.isEventDispatchThread())
				{
					GingerAgent.WriteLog("\n***************\nalready in EDT\n***************");
					r.run();
				}
				else
				{
					GingerAgent.WriteLog("\n***************\nrun un EDT\n***************");
					try {
						SunToolkit.flushPendingEvents();
						SunToolkit.executeOnEDTAndWait(jcb, r);
					} catch (InvocationTargetException e) {
						GingerAgent
								.WriteLog("Inovation target exception while starting thread for click-"
										+ e.getMessage());
						e.printStackTrace();
					} catch (InterruptedException e) {
						GingerAgent
								.WriteLog("InterruptedException while starting thread for click-"
										+ e.getMessage());
						e.printStackTrace();					
					}				
				}
				}			
			else
			{
				//If it is async Select then we do not use SunToolkit
				Thread t1= new Thread(r);			
				t1.start();
			}
			
			try {
				Thread.sleep(100);
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			try 
			{				
				if(Timeout == -1) // Async Select
					while (response[0] != "true")
						Thread.sleep(1);							
				else		
						while (response[1] == "false" && !mWaitForIdleHandler.isCommandTimedOut)
					{	
						Thread.sleep(1000);
					}			
			} 		
			
			catch (Exception e) {
				return PayLoad.Error(" PayLoad ClickComponent Error: " + e.getMessage());
			}		
			
			if (response[0] == "false")
				return PayLoad.Error("Fail to perform click operation");
			
			if (response[2] != "")
				return PayLoad.Error(response[2]);			
				
			if(!jcb.getSelectedItem().equals(value))
				return PayLoad.Error("Failed to select combo box item");
			
			if (Timeout != -1 && response[1] == "false")
				return PayLoad.OK("Select Activity Passed after Timeout");
			
			return PayLoad.OK("Combo Box value Selected");
		}
		else if (c instanceof JTabbedPane)
		{
			JTabbedPane jtp=(JTabbedPane)c;
			
			for(int i=0;i< jtp.getTabCount();i++)
			{
				String tabName=jtp.getTitleAt(i);	
				
				if(tabName.equals(value))
				{
				  jtp.setSelectedIndex(i);				  
				  return PayLoad.OK("Tab  Selected");	  
				}
			}
			return PayLoad.Error("No Matching Tab Found for Value=" + value);
		}
		
		else
		{
			return PayLoad.Error("Unknowon Element for Select action - Class=" + c.getClass().getName());
		}		
	}	
	
	private PayLoad SelectItemByIndex(Component c, int index)
	{
		GingerAgent.WriteLog("Inside SelectByIndex");
		if (c instanceof JComboBox)
		{
			JComboBox jcb = (JComboBox)c;		
			if(index >= 0 && index < jcb.getItemCount())
			{
				jcb.setSelectedIndex(index);
				return PayLoad.OK("ComboBox value selected");	  
			}
			return PayLoad.Error("No ComboBox item found at index = " + index);
		}
		else if (c instanceof JTabbedPane)
		{
			JTabbedPane jtp = (JTabbedPane)c;
			if(index >= 0 && index < jtp.getTabCount())
			{
				jtp.setSelectedIndex(index);
				return PayLoad.OK("Tab  selected");	  
			}				
			return PayLoad.Error("No matching tab found at index = " + index);
		}		
		else
		{
			return PayLoad.Error("Unknown element for SelectByIndex action - Class=" + c.getClass().getName());
		}	
	}
	
	
	private PayLoad GetValueByIndex(Component c,int index)
	{
		if (c instanceof JComboBox)
		{
			JComboBox jcb = (JComboBox)c;	
			if(index >= 0 && index < jcb.getItemCount())
			{
				String item = (String) jcb.getItemAt(index);	
				if (item != null)
				{
					PayLoad Response = new PayLoad("ComponentValue");
					Response.AddValue(item);
					Response.ClosePackage();
					return Response;
				}
				else
				{
					return PayLoad.Error("ComboBox item is null");
				}
			}
		    else
		    {
		    	return PayLoad.Error("No ComboBox item found at index = " + index);
		    }
		}
		else if (c instanceof JTabbedPane)
		{
			JTabbedPane jtp = (JTabbedPane)c;
			if(index >= 0 && index < jtp.getTabCount())
			{
				String item = (String) jtp.getTitleAt(index);
				if (item != null)
				{
					PayLoad Response = new PayLoad("ComponentValue");
					Response.AddValue(item);
					Response.ClosePackage();
					return Response;
				}
				else
				{
					return PayLoad.Error("Tab Value is null");
				}
			}
			else
			{
				return PayLoad.Error("No matching tab found at index = " + index);
			}
		}
		return PayLoad.Error("Unknown element for GetValueByIndex action - Class = " + c.getClass().getName());
	}
	
	
	private PayLoad IsCheckboxChecked(Component c)
	{
		Boolean val;
		if (c instanceof JCheckBox)
		{
			JCheckBox jcb=(JCheckBox)c;	
			val= jcb.isSelected();
		
			PayLoad Response = new PayLoad("ComponentValue");
			Response.AddValue(val.toString());
			Response.ClosePackage();
			return Response;
		}
		else
		{
			return PayLoad.Error("Unknowon Element for GetValueByIndex action - Class=" + c.getClass().getName());
		}	
	}
	
	private PayLoad GetItemCount(Component c)
	{
		if (c instanceof JComboBox)
		{
			JComboBox jcb=(JComboBox)c;	
			int itemCount= jcb.getItemCount();
			PayLoad Response = new PayLoad("ComponentValue");
			Response.AddValue(itemCount);
			Response.ClosePackage();
			return Response;
		}
		else
		{
			return PayLoad.Error("Unknowon Element for GetItemCount action - Class=" + c.getClass().getName());
		}			
	}
	
	private PayLoad SetComponentDate(Component c, String value) 
	{
		GingerAgent.WriteLog("c.tostring::"+c.toString());
		GingerAgent.WriteLog("c.class::"+c.getClass().getName());
					
		if (c.getClass().toString().contains("uif"))			
		{
			return mASCFHelper.setUIFDatePickerValue(c, value);	
		}
		else
		{
			return PayLoad.Error("SetComponentValue - Unknown Component type");
		}		

	}

private PayLoad SetComponentFocus(Component c)
	{
		 c.requestFocus();
		 return PayLoad.OK("Set Focus on "+c.getClass().getName()+" end successfully");
	}	
	
	private PayLoad SetComponentValue(Component c, String value) {
		// based on control type handle the way to set value
		
		//TODO: check control is enabled
		GingerAgent.WriteLog("c.tostring :: "+c.toString());		

		if (c instanceof JTextField)
		{		

			if(c.getClass().getName().equals("javax.swing.JTextField"))
			{
				JTextField f = (JTextField)c;				
				f.setText(value);
				return PayLoad.OK("Text Field Value Set to - " + value);
			}
			
			else if (c.getClass().toString().contains("uif"))
				{
					GingerAgent.WriteLog("UIF Control");
					return mASCFHelper.setUIFTextField(c, value);	
				}
			else 
			{				
				 mASCFHelper.setText(c,value);				 
				 return PayLoad.OK("TextBox value set using reflection "+ value);

			}		
		}
		else if (c instanceof JTextArea)
		{		
				if (c.getClass().toString().contains("uif"))
				{
					GingerAgent.WriteLog("UIF Control");
					return mASCFHelper.setUIFTextArea(c, value);	
				}		
				//TODO: need else?? check at start if contain uif and do the rest in Uif helper...
				JTextArea f = (JTextArea)c;
				f.setText(value);
				return PayLoad.OK("Text Area Value Set to - " + value);

		}
		else if (c instanceof JTextPane)
		{
			if (c.getClass().toString().contains("uif"))
				{
					GingerAgent.WriteLog("UIF Control");
					return mASCFHelper.setUIFTextPane(c, value);	
				}
			   	JTextPane f = (JTextPane)c;
			   	javax.swing.text.Document doc = f.getDocument();
			   	try {
			   		doc.remove(0, doc.getLength());
			   		doc.insertString(doc.getLength(), value, null);			   		

				} catch (BadLocationException e) {
					// TODO Auto-generated catch block
					GingerAgent.WriteLog("Exception on SetComponentValue JTextPane = "+e.getMessage());
					e.printStackTrace();
				}
			   	GingerAgent.WriteLog("JTextPane.getText() = "+f.getText());
				return PayLoad.OK("Text pane Value Set to - " + value);								
			
		}
		else if (c instanceof JTree)
		{						
			JTree f = (JTree)c;
			
			TreePath path = new TreePath(value);
			f.scrollPathToVisible(path);
			f.setSelectionPath(path);				
			return PayLoad.OK("Tree Value Set to - " + value);								
			
		}
		else if (c instanceof JCheckBox)
		{
			((JCheckBox)c).doClick(100);
			
			boolean b = true;
if (value.equalsIgnoreCase("true") || value.equalsIgnoreCase("on"))
				b=true;
			else
				b=false;
			
			((JCheckBox)c).setSelected(b);
			
			if(((JCheckBox)c).isSelected() != b)
				return PayLoad.Error("Failed to set JCheckBox Value Set to - " + value);
			return PayLoad.OK("JCheckBox Value Set to - " + value);	
		}
		else if (c instanceof JList) {
			JList jl = (JList) c;

			jl.clearSelection();

			List<String> items = Arrays.asList(value.split(","));
		
			
		
			if (jl.getSelectionMode() == ListSelectionModel.SINGLE_SELECTION && items.size()>1) {
				return PayLoad.Error("Failure : Trying to Select multiple values in Single Section List ");
			}

			List<Integer> selectedItems = new ArrayList<Integer>();

			for (String s : items) {

				ListModel model = jl.getModel();
				for (int i = 0; i < model.getSize(); i++) {

					GingerAgent.WriteLog(s);
					if (s.equals(String.valueOf(model.getElementAt(i)))) {
						selectedItems.add(i);
						break;
					}
				}

			}

			int[] selectItems = new int[selectedItems.size()];

			for (int i = 0; i < selectedItems.size(); i++) {

				selectItems[i] = selectedItems.get(i);
			}

			if(selectItems.length<items.size()){
				return PayLoad.Error("Failure : Element(s) doesn't exist in the List ");
				
			}
			jl.setSelectedIndices(selectItems);

			
			PayLoad PL = new PayLoad("Done");
			PL.ClosePackage();
			return PL;
		}
		else if (c instanceof JComboBox)
		{
			JComboBox jcb=(JComboBox)c;
			String respString=null;
		
			if(c.getClass() != null)
			{
				System.out.println("c.getClass()" + c.getClass());
				if(c.getClass().getName() != null)
				{
					System.out.println("c.getClass().getName() " + c.getClass().getName());
				}
			}
			
			if(c.getClass().getName().equals("javax.swing.JComboBox") || c.getClass().getName().contains("uif"))
			{
				jcb.requestFocus(true);		
						try {				
					Thread.sleep(100);
				} catch (InterruptedException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			
				jcb.setSelectedItem(value);
				respString="Combo Box value Selected";
			}
			else
			{
				GingerAgent.WriteLog("in Else");;
				ComboBoxModel cbm=jcb.getModel();				
				mASCFHelper.setSelectedItemModel(jcb,cbm,value);
				respString="Combo Box value Selected using Reflection";
			}


			if(!jcb.getSelectedItem().equals(value))
				return PayLoad.Error("Failed to select combo box item");
			
			return PayLoad.OK(respString);
		}
		else if (c.getClass().getName() != null && c.getClass().getName().toString().contains("uif.widgets.DateTimeNative"))
		{	
			if(!mASCFHelper.validateDateTimeValue(value))
			{
				// this is millisecond value
				Date date = new Date(Long.parseLong(value));
				DateFormat formatter = new SimpleDateFormat("MM/dd/yyyy hh:mm:ss a");
				String dateFormatted = formatter.format(date);
				if(dateFormatted!=null)
				{
					return SetComponentDate(c, dateFormatted);	
				}
			}
			else if(mASCFHelper.validateDateTimeValue(value))
			{
				// this is in MM/dd/yyyy hh:mm:ss a format
				return mASCFHelper.setUIFDatePickerValue(c, value);	
			}
			else
				return PayLoad.Error("SetComponentValue - Unknown Component type");
	
			return PayLoad.OK("DateTime Value Set to - " + value);	
		}	
		else
		{
			return PayLoad.Error("SetComponentValue - not supported for Component type"+c.getClass().getName());
		}
		// TODO: Add other type of controls + err if not known
		
	}
	
		
	private Object getTreeNodeFromPathAndSet(JTree tr,String locate) {	
		GingerAgent.WriteLog( "  getTreeNodeFromPathAndSet::locate  " +  locate);
		String path = locate;
		TreePath p = null;
		String[] nodes = path.split("/");
				
		TreePath p1=null;
		for (String node : nodes) {
			int row = (p == null ? 0 : tr.getRowForPath(p));
			tr.expandRow(row);
			p1 = tr.getNextMatch(node.trim(), row, Position.Bias.Forward);
			// TODO: Handle full Path for JTREE now we are supporting only Last node .
			//Add for Handling Same Prefix on the node, need to create new way to handle Jtree  
			if(p1==p)
			{
				p=tr.getNextMatch(node.trim(), row+1, Position.Bias.Forward);
				tr.expandRow(row);
			}
			else
				p=p1;
		}
		if (p==null)
			return null;
		else
			tr.setSelectionPath(p);
		return (p.getLastPathComponent());

	}
	
	List<PayLoad> GetComponentProperties(Component comp)	
	{
		List<PayLoad> FieldProperties = new ArrayList<PayLoad>();


	    PayLoad PL1 = new PayLoad("ComponentProperty");	    
    	PL1.AddValue("Name");
    	String Name=comp.getName();
    	if(Name!=null)
    		PL1.AddValue(Name);
    	else
    		PL1.AddValue("");
    	PL1.ClosePackage();
    	FieldProperties.add(PL1);

    	//Add Component Value
	    PayLoad PL2 = new PayLoad("ComponentProperty");	    
    	PL2.AddValue("Value");
    	PL2.AddValue(mSwingHelper.GetCompValue(comp));
    	PL2.ClosePackage();
    	FieldProperties.add(PL2);
    	
	    //Add Component Class
    	PayLoad PL3 = new PayLoad("ComponentProperty");	    
    	PL3.AddValue("Class");
    	PL3.AddValue(comp.getClass().getName());
    	PL3.ClosePackage();
    	FieldProperties.add(PL3);
    	
	    //Add Component Swing Class
    	PayLoad PL4 = new PayLoad("ComponentProperty");	    
    	PL4.AddValue("Swing Class");
    	PL4.AddValue(mSwingHelper.GetComponentSwingClass(comp));
    	PL4.ClosePackage();
    	FieldProperties.add(PL4);
    	    	    	
		// Add all generic fields
	    Field[] allFields = comp.getClass().getFields();	    
	    for (Field field : allFields) {
	    	PayLoad PL = new PayLoad("ComponentProperty");
	    	field.setAccessible(true);
	    	PL.AddValue(field.getName());
	    	Object value = null;
			try {				
				value = field.get(comp);
			} catch (IllegalArgumentException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			} catch (IllegalAccessException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			if (value == null)
			{
				PL.AddValue("");
			}
			else
			{
				PL.AddValue(value.toString());
			}
	    	PL.ClosePackage();
	    	FieldProperties.add(PL);
	    }
	    
	    
	    return FieldProperties;
	}
	private List<String> GetComboBoxValues(Component comp)
	{
		List<String> CompValue = new ArrayList<String>();
		try{
			GingerAgent.WriteLog("Inside GetCompValue" );
			if (comp instanceof JComboBox)
			{	
				Object count = ((JComboBox)comp).getItemCount();
				GingerAgent.WriteLog("Count: " + count.toString());
								
				if (count!=null){
					GingerAgent.WriteLog("Inside if loop");
					for (int i=0; i < ((Integer)count) ; i++)
					{						
						String value = ((JComboBox)comp).getItemAt(i).toString();
						GingerAgent.WriteLog("Value: " + value);
						CompValue.add(value);
					}
				}
				else 
					CompValue.add("");
			}
			return CompValue;
		}
		catch(Exception e)
		{
			CompValue.add(e.getMessage());
			return CompValue;
		}
	}	

			
	private String GetCompState(Component comp) {

		String CompValue = null;
		
		if (comp instanceof JRadioButton)
		{
			if (((JRadioButton)comp).isSelected())
				CompValue = "TRUE";
			else
				CompValue = "FALSE";
		}
		else
		{
			CompValue = "get state is not supported for componenent type"+comp.getClass().getName();	
		}
		return CompValue;
	}
	
	private PayLoad GetComponentFromCursor() {
		
		Window CurrentWindow= mSwingHelper.getCurrentWindow();
		if (CurrentWindow != null) {
			CurrentWindow.requestFocus();
			Point location = MouseInfo.getPointerInfo().getLocation();
			SwingUtilities.convertPointFromScreen(location, CurrentWindow);
			Component c = SwingUtilities.getDeepestComponentAt(CurrentWindow,
					location.x, location.y);

			if (c.getName()!=null && c.getName().contains("canvas") || 
					c.getClass().toString().contains("com.jniwrapper.win32.ie.aw")||
					(c.getName()!=null && c.getName().contains("LightWeightWidget"))|| //  added to support live spy in JxBrowserBrowserComponent
					c.getClass().toString().contains("LightWeightWidget"))
			{
				Component browserComponent=mSwingHelper.GetParentBrowser(c);
				String cXpath=mSwingHelper.GetComponentXPath(browserComponent);
				mBrowserHelper = GetInitializedBrowser(cXpath);
				if (mBrowserHelper != null)//exists and valid
				{
					PayLoad PL = new PayLoad("GetComponentFromCursor");
					GingerAgent.WriteLog("I am C.X :" + location.x);
					GingerAgent.WriteLog("I am C.Y :" + location.y);
					PL.ClosePackage();				
					PayLoad response = mBrowserHelper.ExceuteJavaScriptPayLoad(PL);
					GingerAgent.WriteLog("I am the Response: " +response.toString());
					return response;					
				}
				else
				{
					//New\Invalid -Require Initialize Browser
					return RequireInitializeBrowser(cXpath);
				}
			}
			else	
			{
				// change component to parent for internal frame.
				if(c.getParent() != null && c.getParent() instanceof JInternalFrame)
				{
					c = c.getParent();
				}
				return GetCompInfo(c);
			}

			//return c;
		} else {
			return PayLoad.Error("Live spy failed Current Window is null");
		}	
	}
	private PayLoad RequireInitializeBrowser(String xpath)
	{
		Component cBrowser=mSwingHelper.FindElement("ByXPath", xpath);
		PayLoad PLIB =  GetCompInfo(cBrowser);
	
		if (PLIB.IsErrorPayLoad())
		{
			return PLIB;
		}
		
		PayLoad PLRIB = new PayLoad("RequireInitializeBrowser"); 
		PLRIB.AddValue(PLIB.GetValueString());//elementTitle
		PLRIB.AddValue(PLIB.GetValueString());//elementType
		PLRIB.AddValue(PLIB.GetValueString());//Value
		PLRIB.AddValue(PLIB.GetValueString());//Path
		PLRIB.AddValue(PLIB.GetValueString());//XPath
		PLRIB.AddValue(PLIB.GetValueString());//IsExpandable
		PLRIB.ClosePackage();

		return PLRIB;
		
	}
	private BrowserHelper GetInitializedBrowser(String xpath)
	{
		for(int i =0 ; i < lstmBrowser.size(); i++)
		{
			if(lstmBrowser.get(i).getmBrowserXPath().equals(xpath))
			{
				if (lstmBrowser.get(i).isBrowserValid() && lstmBrowser.get(i).CheckIfScriptExist())
				{
					GingerAgent.WriteLog("Getting Exists And Valid Browser");
					return lstmBrowser.get(i);
				}
				else
				{
					GingerAgent.WriteLog("Browser Is Invalid");
					RemoveFromlstmBrowser(lstmBrowser.get(i).getmBrowserXPath());
					return null;
				}
			}
		}
		return null;
	}
    private void RemoveFromlstmBrowser(String xpath)
    {
    	GingerAgent.WriteLog("Remove Browser");
    	for (Iterator<BrowserHelper> iter = lstmBrowser.listIterator(); iter.hasNext();)
    	{
    		BrowserHelper itrBrowserHelper = iter.next();
    		if (itrBrowserHelper.getmBrowserXPath().equals(xpath))
    		{
    			iter.remove();
     		}
    	}
    }

	private PayLoad HandleGetContainerControls(String containerXPath) 
	{
		//TODO: make me work find the container by XPath.

		Container c=null;				
		if("/".equals(containerXPath))   // '/' = current Window
		{
			c=mSwingHelper.getCurrentWindow();
		}
		else
		{			
			// It is Xpath to container - /Class[Index]
			c=(Container)mSwingHelper.FindElement("ByXPath", containerXPath);	
			
		}
		
		List<PayLoad> Elements = new ArrayList<PayLoad>(); 	
		String PayLoadName="";
		if(c instanceof JEditorPane)
		{

			PayLoadName="HTML Element Children";
			Elements= getEditorComponents();
		}
		else
		{
			PayLoadName="ContainerComponents";
			Elements=getContainerComponents(c);				
		}
		PayLoad pl2 = new PayLoad(PayLoadName);
		pl2.AddListPayLoad(Elements);
		pl2.ClosePackage();
		return pl2;	
		
	}		
	
	private List<PayLoad> getEditorComponents()
	{
		List<PayLoad> Elements = new ArrayList<PayLoad>(); 	   
		Elements allElements = mEditorHelper.getAllElements();
		PayLoad plx = null;
		int i=0;
		for (org.jsoup.nodes.Element singleElement  : allElements)
		{
			if (singleElement.tagName().equals("td") || singleElement.tagName().equals("tr"))
			{
				continue;
			}
			
			plx = mEditorHelper.GetEditorComponentInfo(singleElement, i++);
			Elements.add(plx);
		}
		System.out.println("ELEMENTS = " + Elements);
		return Elements;

	}

	private boolean hasVisibleChild(Container c)
	{
		 Component[] comps = c.getComponents();//May be need to do getAllComponents
		    for (Component comp : comps) {
		    	
		    	if (comp.isVisible())
		    	{   		
		    		return true;				
				}		
		    }
		return false;
	}
	
	public  List<PayLoad> getContainerComponents(final Container c) {
	    Component[] comps = c.getComponents();
	   
	    List<PayLoad> Elements = new ArrayList<PayLoad>(); 	   
	       
	    for (Component comp : comps) {
	    	
			if (comp.isVisible())
			{	
	    		PayLoad PL=GetCompInfo(comp);    		
				
				Elements.add(PL);				
		
			}		

	    }
	    return Elements;
	}	
	
	private PayLoad GetCompInfo(Component comp) {
		//Return PayLoad for JavaElementInfo
		
		String elementTitle = comp.getName();
		if (elementTitle == null) 
		{
			elementTitle = "";
		}
				
		List<String> compValues=mSwingHelper.GetCompValue(comp);
		
		String Value="";		
		if(compValues.size()!=0)
		{
			Value = mSwingHelper.GetCompValue(comp).get(0);
		}
		
		String Path = elementTitle;
		
		//TODO: Add to Payload bool value
		// Boolean IsExpandable = comp instanceof Container;   // If it is container then can be expandable
		//Check if has at least one visible child then only set expandable else not needed
		String IsExpandable = "N";
		if ((comp instanceof Container && hasVisibleChild((Container)comp)) || comp instanceof JEditorPane) IsExpandable = "Y";		
		
		PayLoad PL=new PayLoad("Element");
					
		String elementType = mSwingHelper.GetComponentSwingClass(comp);		
		
		String XPath = mSwingHelper.GetComponentXPath(comp);		
		
				// Create Element PayLoad		
		PL.AddValue(elementTitle);		
		PL.AddValue(elementType);		
		PL.AddValue(Value);		
		PL.AddValue(Path);		
		PL.AddValue(XPath);		
		PL.AddValue(IsExpandable);
					
		PL.ClosePackage();
		
		return PL;
	}		


	// Function to check active window and if it is JDialog component
	public Window CheckActiveWindowJS(){

		Window[] windows = SwingHelper.GetAllWindowsByReflection(); 
		if( windows != null ) { 
			for( Window w : windows ) {
				if( w.isShowing() && w instanceof Dialog ) {
					return w;
				}					
			}			
		}		
		return null;
	}
	
	public void CheckJExplorerPopup(Window w){
		
		if (w != null)		
		{			
			List<Component> list = SwingHelper.getAllComponents(w);
			for(Component c : list)
			{
				String browserStr = c.getClass().toString();
				if (c.isVisible() && browserStr.contains("com.amdocs.uif.widgets.browser"))
				{	
					GingerAgent.WriteLog(" JExplorerBrowser Popup Found:= " + c.getClass());
					String LocateBy = "ByXPath";
					String LocateValue = mSwingHelper.GetComponentXPath(c);
					
					GingerAgent.WriteLog(" JExplorerBrowser Popup Found:= xPath " + LocateValue);
					
		            if (LocateValue.startsWith("/dialog")){
		            	LocateValue = LocateValue.substring(Recorder.ordinalIndexOf(LocateValue,"/",2), LocateValue.length());
		            }
		            
		            GingerAgent.WriteLog(" JExplorerBrowser Popup Found:= LocateValue: " + LocateValue);
		            GingerAgent.WriteLog(" JExplorerBrowser Popup Found:= Before FindElement");
					Component browser = mSwingHelper.FindElement(LocateBy, LocateValue);
					GingerAgent.WriteLog(" JExplorerBrowser Popup Found:= Browser");
					
					if (browser != null)
					{	
						GingerAgent.WriteLog("Pop-up browser::" + browser.toString());
						mBrowserHelper = new BrowserHelper(browser);
						GingerAgent.WriteLog("Pop-up Inside browser2::" + mBrowserHelper.toString());
					}					
				} 				
			}			
		}	
	}

	private PayLoad CheckJExplorerExists() 
	{
		Component c= mSwingHelper.getBrowserComponentFromCurrentWindow();
		if(c!=null)
		{
			return GetCompInfo(c);
		}
		else
		{
			return PayLoad.Error("Browser component not exist");
		}
				
		
	}

	private PayLoad HandleGetCurrentWindowVisibleControls() 
	{
		GingerAgent.WriteLog("HandleGetCurrentWindowVisibleControls()");
		Window CurrentWindow= mSwingHelper.getCurrentWindow();
		GingerAgent.WriteLog("Current Window Title = " + CurrentWindow.getName());
		
		if (CurrentWindow != null)		
		{			
			// We get all components - drill down 
			List<Component> list = SwingHelper.getAllComponents(CurrentWindow);
			
			GingerAgent.WriteLog("Total Elements Found: " + list.size());
			GingerAgent.WriteLog("CurrentWindow.getClass().getName() = " + CurrentWindow.getClass().getName());			
			
			List<PayLoad> Elements = new ArrayList<PayLoad>(); 
			for(Component c : list)
			{
				if (c.isVisible())
				{
				
				PayLoad PL = GetCompInfo(c);

				Elements.add(PL);
				}				
			}
			
			GingerAgent.WriteLog("Visible Elements Found: " + Elements.size());
			
			PayLoad pl2 = new PayLoad("WindowComponents");
			pl2.AddListPayLoad(Elements);
			pl2.ClosePackage();
			return pl2;
		}
		else
		{			
			return PayLoad.Error("No current Window");
		}	
	}


	private PayLoad HandleTableActions(String locateBy, String locateValue,
			List<String> cellLocator, String controlAction, String Value) 
	{
		GingerAgent.WriteLog("Inside Table Action");
		GingerAgent.WriteLog("controlAction = " + controlAction);
		GingerAgent.WriteLog("Cell Locator = " + cellLocator.toString());
		Component ct = FindElementWithImplicitSync(locateBy, locateValue);
		if (ct == null) 
		{
			return PayLoad.Error("Table Element Not Found by locate value:" + locateValue);
		}
		GingerAgent.WriteLog("Table Element Found");
		
		JTable CurrentTable = (JTable) ct;
		
		//Not required as we are running entire action on Swing invoker
		//if facing any exception for table action need to enable it back
		//mWaitForIdleHandler.WaitForTableToLoad(CurrentTable);
		
		TableModel TM = CurrentTable.getModel();		
		
		if (controlAction.equals("GetTableDetails")) 
		{
			List<String> ColomumnNames = new ArrayList<String>();
			
			for (int i = 0; i < CurrentTable.getColumnCount(); i++) 
			{
				
				String colName = "";
				if (!CurrentTable.getColumnModel().getColumn(i).getHeaderValue().toString().equalsIgnoreCase(""))
				{
					colName = CurrentTable.getColumnModel().getColumn(i).getHeaderValue().toString();
				}
					
				else
				{
					colName = CurrentTable.getModel().getColumnName(i);
				}
					
				ColomumnNames.add(colName);

			}

			PayLoad PL = new PayLoad("ControlProperties");
			List<PayLoad> list = GetComponentProperties(CurrentTable);
			PL.AddListPayLoad(list);
			PL.AddValue(ColomumnNames);
			PL.AddValue(TM.getRowCount());
			PL.ClosePackage();
			return PL;
		}

		String rowLocator = cellLocator.get(0);
		GingerAgent.WriteLog("ROW LOCATOR" + rowLocator);
		int nxtIndex = 1;
		int rowNum = -1;
		// wait for Table to load
		int iRC = 0;
		while (TM.getRowCount() < 0 && iRC < 10) 
		{
			try 
			{
				Thread.sleep(1000);
			} 
			catch (InterruptedException e) 
			{
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			iRC++;
		}

		if (controlAction.equals("GetRowCount")) 
		{
			PayLoad PL = new PayLoad("GetRowCount");
			PL.AddValue(TM.getRowCount());
			PL.ClosePackage();
			return PL;
		}
		GingerAgent.WriteLog("Get Row Number");
		if (rowLocator.equals("Where")) 
		{
			String whereColSel = cellLocator.get(1);
			String whereColTitle = cellLocator.get(2);
			String whereProp = cellLocator.get(3);
			String whereOper = cellLocator.get(4);
			String whereColVal = cellLocator.get(5);
			nxtIndex = 6;		
			GingerAgent.WriteLog("<<Where>>");
			try
			{
				//TOOD: Enhance the code for get row num . Very senstitive code , sometime results in null pointer exception.
				// Need to handle
				//Sometimes we are getting rownum -1 when table is still loading. And if we rerun the action it is working fine.
//				int i=0;
//				while(i<3 && rowNum==-1)
//				{
					GingerAgent.WriteLog("Getting Row numer iteration::");
					rowNum = getRowNum(CurrentTable, whereColSel, whereColTitle,whereProp, whereOper, whereColVal);	
//					i++;
//				}				
			}
			catch(NullPointerException e){
				GingerAgent.WriteLog("Exception in GetRowNum");
				e.printStackTrace();
			}
		} 
		else if (rowLocator.equals("Any Row")) 
		{
			Random rand = new Random();
			rowNum = rand.nextInt(TM.getRowCount());
		} 
		else if (rowLocator.equals("By Selected Row")) 
		{
			ListSelectionModel selectmodel = CurrentTable.getSelectionModel();
			selectmodel.setSelectionInterval(rowNum, rowNum);
			rowNum = CurrentTable.getSelectedRow();
		} 
		else 
		{
			rowNum = Integer.parseInt(cellLocator.get(1));
			if (TM.getRowCount() <= rowNum)
				rowNum = -1;
			nxtIndex = 2;
		}
		if (rowNum == -1)
			return PayLoad.Error("Row not found with given Condition");

		GingerAgent.WriteLog("getRowNum::" + rowNum);

		int colNum = -1;
		if (controlAction.equalsIgnoreCase("SetValue")
				|| controlAction.equalsIgnoreCase("Type")
				|| controlAction.equalsIgnoreCase("Click")
				|| controlAction.equalsIgnoreCase("WinClick")
				|| controlAction.equalsIgnoreCase("GetValue")
				|| controlAction.equalsIgnoreCase("Toggle")
				|| controlAction.equals("IsCellEnabled")
				|| controlAction.equals("GetSelectedRow")
				|| controlAction.equalsIgnoreCase("AsyncClick")
				|| controlAction.equalsIgnoreCase("DoubleClick")
				|| controlAction.equalsIgnoreCase("SetFocus")
				|| controlAction.equalsIgnoreCase("IsVisible")				
				|| controlAction.equalsIgnoreCase("MousePressAndRelease")
				|| controlAction.equalsIgnoreCase("SendKeys")
				|| controlAction.equalsIgnoreCase("IsChecked")
			) 
			
		{
			String colBy = cellLocator.get(nxtIndex);
			String colVal = cellLocator.get(nxtIndex + 1);
			colNum = getColumnNum(CurrentTable, colBy, colVal);
			if (colNum == -1)
				return PayLoad.Error("Coloumn not found with " + colBy + " :"
						+ colVal);
		}

		if (controlAction.equals("GetSelectedRow")) 
		{
			rowNum = -1;
			for (int i = 0; i < TM.getRowCount(); i++) 
			{
				Component CellComponent = CurrentTable.prepareRenderer(
						CurrentTable.getCellRenderer(i, colNum), i, colNum);
				if (CellComponent != null) 
				{
					if (CellComponent instanceof JRadioButton) 
					{
						if (((JRadioButton) CellComponent).isSelected()) 
						{
							rowNum = i;
							break;
						}
					}
				}
			}
			if (rowNum == -1) 
			{
				ListSelectionModel selectmodel = CurrentTable.getSelectionModel();
				selectmodel.setSelectionInterval(rowNum, rowNum);
				rowNum = CurrentTable.getSelectedRow();
			}
			PayLoad PL = new PayLoad("GetSelectedRow");
			PL.AddValue(rowNum);
			PL.ClosePackage();
			return PL;
		} 
		else if (controlAction.equals("Toggle")) 
		{			
			Component CellComponent = getTableCellComponent(CurrentTable,rowNum, colNum);
			if (CellComponent instanceof JCheckBox) 
			{
				return ToggleComponentValue(CellComponent);
			} 
			else 
			{
				return PayLoad.Error(" Toggle Operation is not valid for cell type-"+ CellComponent.getClass().getName());
			}
		} 
		else if (controlAction.equals("SelectDate"))
		{		
			GingerAgent.WriteLog("Current Table : " + CurrentTable +", Row Num : " + rowNum + ", Col Num : " +colNum +", Value : " +Value );
			Component CellComponent = getTableCellComponent(CurrentTable, rowNum, colNum);
			GingerAgent.WriteLog("CellComponent : " +CellComponent);
			GingerAgent.WriteLog("Inside Set Date");
			if(CellComponent!=null)
			{
				return SetComponentDate(CellComponent, Value);	
			}
			else
			{
				return PayLoad.Error("Cell componenent not found");
			}
		}
		else if (controlAction.equals("IsCellEnabled")) {
			Component CellComponent = getTableCellComponent(CurrentTable, rowNum, colNum);
			if (CellComponent != null) {
				PayLoad Response = new PayLoad("ComponentValue");
				Response.AddValue(CellComponent.isEnabled() + "");
				Response.ClosePackage();
				return Response;
				}   else {
				return PayLoad.Error("Cell component not found");
			}
		} else if (controlAction.equals("IsVisible")) {
			Component CellComponent = getTableCellComponent(CurrentTable, rowNum, colNum);
			if (CellComponent != null) {
				PayLoad Response = new PayLoad("ComponentValue");
				Response.AddValue(CellComponent.isVisible() + "");
				Response.ClosePackage();
				return Response;
			}   else {
				return PayLoad.Error("Cell component not found");
			}
			
		}
		else if (controlAction.equals("SetValue")) 
		{
			GingerAgent.WriteLog("Current Table : " +CurrentTable+", Row Num : " + rowNum + ", Col Num : " +colNum +", Value : " +Value );
			Component CellComponent = getTableCellComponent(CurrentTable,rowNum,colNum);
			GingerAgent.WriteLog("CellComponent : " +CellComponent);
			GingerAgent.WriteLog("Inside Set Value");
				if(CellComponent!=null)
				{
					return SetComponentValue(CellComponent, Value);	
				}
				else
				{
					return PayLoad.Error("Cell componenent not found");
				}

		}else if (controlAction.equals("SendKeys")){
			GingerAgent.WriteLog("Current Table : " +CurrentTable+", Row Num : " + rowNum + ", Col Num : " +colNum +", Value : " +Value );
			Component CellComponent = getTableCellComponent(CurrentTable,rowNum,colNum);
			GingerAgent.WriteLog("CellComponent : " +CellComponent);
			GingerAgent.WriteLog("Inside SendKeys");
				if(CellComponent!=null)
				{
					return SendKeys(CellComponent, Value);
				}
				else
				{
					return PayLoad.Error("Cell componenent not found");
				}
		
		} else if (controlAction.equals("SetFocus")) {
		
			setFocus(CurrentTable, rowNum, colNum);
			
			return PayLoad.OK("Set Focus Successful");

		} else if (controlAction.equals("Type")) {
			Component CellComponent = getTableCellComponent(CurrentTable,
					rowNum, colNum);

			CurrentTable.grabFocus();
			CurrentTable.scrollRectToVisible(CurrentTable.getBounds());
			Point pos = CurrentTable.getLocationOnScreen();
			Rectangle size = CurrentTable.getCellRect(rowNum, colNum, true);
			pos.x += size.x;
			pos.y += size.y;

			Robot bot;
			try {
				bot = new Robot();

				bot.mouseMove(pos.x, pos.y);
				bot.mousePress(InputEvent.BUTTON1_MASK);

				Thread.sleep(500);
				bot.mouseRelease(InputEvent.BUTTON1_MASK);
				Thread.sleep(500);
				String val1 = getCellValue(CellComponent, rowNum);
				for (int i = 0; i < val1.length(); i++) {
					bot.keyPress(KeyEvent.VK_DELETE);
					bot.keyRelease(KeyEvent.VK_DELETE);
				}
				type(Value);
	

			} catch (Exception e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
				return PayLoad.Error(e.getMessage());
			}

			return PayLoad.OK("Type Activity Passed");
		} else if (controlAction.equals("Select")) {
			Component CellComponent = getTableCellComponent(CurrentTable,
					rowNum, colNum);
			if (CellComponent instanceof JComboBox) {
				JComboBox cb = (JComboBox) CellComponent;

				cb.setSelectedItem(Value);

				if (cb.getSelectedItem() != Value)
					return PayLoad.Error("Failed to Select combo box item "
							+ Value);
			} else {
				return PayLoad.Error("Component is not of type combo box ");
			}

		} else if (controlAction.equals("WinClick")) {
			Component CellComponent = CurrentTable.prepareRenderer(
					CurrentTable.getCellRenderer(rowNum, colNum), rowNum,
					colNum);

			CurrentTable.grabFocus();
			CurrentTable.scrollRectToVisible(CurrentTable.getBounds());
			Point pos = CurrentTable.getLocationOnScreen();
			Rectangle size = CurrentTable.getCellRect(rowNum, colNum, true);

			if (!Value.isEmpty()) {
				String[] sValue = Value.split(",");
				List<Integer> intlist = ConvertListStringToInt(sValue);
				int x = intlist.get(0);
				int y = intlist.get(1);
				// We start from top corner of a control and add the coordinates
				// to it
				pos.x += size.x + x;
				pos.y += size.y + y;

			} else {
				pos.x += size.x + (size.width / 2);
				pos.y += size.y + (size.height / 2);
			}

			Robot bot;
			try {
				bot = new Robot();

				bot.mouseMove(pos.x, pos.y);
				bot.mousePress(InputEvent.BUTTON1_MASK);

				Thread.sleep(500);
				bot.mouseRelease(InputEvent.BUTTON1_MASK);
			} catch (Exception e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
				return PayLoad.Error(e.getMessage());
			}

			return PayLoad.OK("Win Click Activity Passed");
		}
		else if (controlAction.equals("DoubleClick")) {
			
			GingerAgent.WriteLog("In Double CLick");
			
			Component CellComponent = CurrentTable.prepareRenderer(CurrentTable.getCellRenderer(rowNum, colNum), rowNum,
					colNum);
			GingerAgent.WriteLog("CellComponent instanceof JLabel" + CellComponent.toString());
			CurrentTable.grabFocus();
			setFocus(CurrentTable,rowNum,colNum);
					
			Point pos = CurrentTable.getLocationOnScreen();	
			//if false, return the true cell bounds - computed by subtracting the intercell spacing from the height and widths of the column and row models
			Rectangle size = CurrentTable.getCellRect(rowNum, colNum, true);
			size.x += size.width/2;
			size.y += size.height/2;
			MousePressAndReleaseComponent(CurrentTable, size.x + "," + size.y,mCommandTimeout,2);
			
			return PayLoad.OK("Double Click Activity Passed");
		}
		else if (controlAction.equals("Click")) {

			Component CellComponent = CurrentTable.prepareRenderer(CurrentTable.getCellRenderer(rowNum, colNum), rowNum,
					colNum);
			Component Cell =CurrentTable.prepareRenderer(CurrentTable.getCellRenderer(0, 0), 0,0);
			
			// If we do below then suntoolkit.Execute will get stuck.It will not start the thread and the action will always be running
			
			if (Cell instanceof JTree)
			{								
				
				JTree jt=((JTree)Cell);
				mWaitForIdleHandler.WaitForJTreeToLoad(jt);
				TreePath p=jt.getPathForRow(0);
				GingerAgent.WriteLog("TreePath : " + p.toString());
				jt.setSelectionPath(p);					
			}
			
			GingerAgent.WriteLog("Type of a  cellcomnponent is: "+CellComponent.getClass().getName());
			if(CellComponent instanceof JCheckBox)
			{					
				
				try
				{
					//TODO: Add Null object chekcs
				
					Component cellEditorComponent = getTableCellComponent(CurrentTable,rowNum, colNum);
					
					if(cellEditorComponent!=null)
					{
						
						JCheckBox cb=(JCheckBox)cellEditorComponent;
						
						boolean isSelected=cb.isSelected();
						
						
						cb.setSelected(!isSelected);
						
						
						if(((JCheckBox)CellComponent).isSelected() == isSelected)
						return PayLoad.Error("Failed to click on JCheckBox");
					}
					else
					{
						return PayLoad.Error("Cell component not found");
					}
				
				}
				catch(NullPointerException e)
				{
					GingerAgent.WriteLog("Exception-"+ e.getMessage());
					e.printStackTrace();
				}
				

				GingerAgent.WriteLog("after check::" + ((JCheckBox)CellComponent).isSelected());
			}
			else if(CellComponent instanceof JButton)
			{					
				GingerAgent.WriteLog("Jtable cell of type Button");
							
				 ClickComponent(CellComponent, Value, mCommandTimeout);
				
			}						
			else if(CellComponent instanceof JLabel)
			{					
				GingerAgent.WriteLog("CellComponent instanceof JLabel");
				CurrentTable.grabFocus();
				CurrentTable.scrollRectToVisible(CurrentTable.getBounds());
				Point pos = CurrentTable.getLocationOnScreen();
				Rectangle size = CurrentTable.getCellRect(rowNum, colNum, true);				
				pos.x += size.x; 
				pos.y += size.y; 
				
				MousePressAndReleaseComponent(CurrentTable, size.x + "," + size.y,mCommandTimeout,1);
									
				GingerAgent.WriteLog("After MousePressReleaseComponent on JLabel");
			
			}
			else if (CellComponent instanceof JTree)
			{	
				
				Object treeNode=getTreeNodeFromPathAndSet((JTree) CellComponent, Value);
				if(treeNode == null)				
				{				
					return PayLoad.Error("Path " + Value + " not found");
				}
				((JTree)CellComponent).requestFocus();	
				//already in EDT - this call will cues exception
				try {
					Thread.sleep(500);
				} catch (InterruptedException e) {						
					e.printStackTrace();
				}	
				JTree treeComponent=((JTree)CellComponent) ;
				 TreePath p=treeComponent.getPathForRow(rowNum);
			     Rectangle rect = ((JTree)CellComponent).getPathBounds(p);
			     ((JTree)CellComponent).scrollPathToVisible(p);
			     Point pos = CurrentTable.getLocationOnScreen();
			     
			     Value = rect.x + "," + rect.y;
			     Rectangle size = CurrentTable.getCellRect(rowNum, colNum, true);	
				
			     PayLoad plrc =  MousePressAndReleaseComponent(CurrentTable,size.x + "," + size.y,mCommandTimeout,1);
				GingerAgent.WriteLog("After Mouse Click");				
				
				return plrc;
				
			}
			else if(CellComponent instanceof JRadioButton){
									

			 	ClickComponent(CellComponent, Value, mCommandTimeout);	
				
			GingerAgent.WriteLog("Radio Button After do click");			

			}		
			
			//TODO: Not sure why we have text field under Click action. Need to clean up
			else if(CellComponent instanceof JTextField)
				((JTextField)CellComponent).setText(((JTextField)CellComponent).getText());
			else if(CellComponent instanceof JTextPane)
				((JTextPane)CellComponent).setText(((JTextPane)CellComponent).getText());
			else if(CellComponent instanceof JScrollPane)
			{
				GingerAgent.WriteLog("JScrollPaneTest:");
				GingerAgent.WriteLog("Test:" + ((JScrollPane)CellComponent).getViewport().getView().toString());
			}					
			else
				{
				CurrentTable.setRowSelectionAllowed(true);
				CurrentTable.setColumnSelectionAllowed(true);				
				
				CurrentTable.changeSelection(rowNum, colNum, true, false);
				
				Component editorComp = CurrentTable.getEditorComponent();					
				  if (editorComp != null) 
				    editorComp.requestFocus();
				    
				}	
			PayLoad Response = new PayLoad("ComponentValue");
			List<String> val = new ArrayList<String>();
			val.add("Done");

			Response.AddValue(val);
			Response.ClosePackage();
			return Response;		    	
		}
		else if(controlAction.equals("MousePressAndRelease"))
		{
			Component CellComponent = CurrentTable.prepareRenderer(CurrentTable.getCellRenderer(rowNum, colNum), rowNum,
					colNum);
			Component Cell =CurrentTable.prepareRenderer(CurrentTable.getCellRenderer(0, 0), 0,0);

			GingerAgent.WriteLog("MousePressAndRelease - CellComponent instanceof JRadioButton");
			CurrentTable.grabFocus();
			CurrentTable.scrollRectToVisible(CurrentTable.getBounds());
			Rectangle rect = CurrentTable.getCellRect(rowNum, colNum, true);
			rect.x += rect.width/2;
			rect.y += rect.height/2;
			return MousePressAndReleaseComponent(CurrentTable, rect.x + "," + rect.y,mCommandTimeout,1);

		}
		else if(controlAction.equals("IsChecked"))
		{
			GingerAgent.WriteLog("controlAction.equals('IsChecked')");
			Component CellComponent = getTableCellComponent(CurrentTable,
					rowNum, colNum);
			return IsCheckboxChecked(CellComponent);
		}
		else if (controlAction.equals("AsyncClick")) {
			
			Component CellComponent = getTableCellComponent(CurrentTable,
					rowNum, colNum);

			if (CellComponent instanceof JLabel) {
				GingerAgent.WriteLog("CellComponent instanceof JLabel");
				setFocus(CurrentTable, rowNum, colNum);
				CurrentTable.grabFocus();
				CurrentTable.scrollRectToVisible(CurrentTable.getBounds());
				Point pos = CurrentTable.getLocationOnScreen();
				Rectangle size = CurrentTable.getCellRect(rowNum, colNum, true);
				pos.x += size.x;
				pos.y += size.y;

				return MousePressReleaseComponent(CurrentTable, size.x + ","
						+ size.y, -1,1);

			} else {
				return ClickComponent(CellComponent, Value, -1);
			}
		}	
		else if (controlAction.equals("GetValue")) {
			GingerAgent.WriteLog("Row Num : " + rowNum +" Col Num : " +colNum);
			// TODO: Change this to table cell editor
			Component CellComponent = CurrentTable.prepareRenderer(
					CurrentTable.getCellRenderer(rowNum, colNum), rowNum,
					colNum);
			PayLoad Response = new PayLoad("ComponentValue");
			List<String> val = new ArrayList<String>();
			String val1;

			if (CellComponent != null) {
				val1 = getCellValue(CellComponent, rowNum);
				GingerAgent.WriteLog("getCellValue::" + val1);
				val.add(val1);
			} else
				val.add("");

			Response.AddValue(val);
			Response.ClosePackage();
			return Response;
		} else if (controlAction.equals("ActivateRow")) {
			int sRow;
			int col;
			if(CurrentTable.getColumnCount()==1)
				col= 0;
			else
				col=1;
			try {				
				
				CurrentTable.grabFocus();
				setFocus(CurrentTable,rowNum,col);
						
				Point pos = CurrentTable.getLocationOnScreen();			
				Rectangle size = CurrentTable.getCellRect(rowNum, col, true);
				MousePressAndReleaseComponent(CurrentTable, size.x + "," + size.y,mCommandTimeout,2);
				return PayLoad.OK("ActivateRow Activity Passed");
				
			} catch (Exception e) {
				GingerAgent.WriteLog("In Exception");
				return PayLoad.Error(e.getMessage());
			}
		} else {
			return PayLoad.Error("Unsupported Table Operation");
		}

		return null;
	}
		
	private Component getTableCellComponent(final JTable table, final int rowNum,final int colNum)
	{
		
		
		 final Component [] cellEditorComponent=new Component[3];
		 cellEditorComponent[0]=null;
		 Runnable r = new Runnable() {
				
				public void run() 
				{
					
					GingerAgent.WriteLog("Checkpoint 1");
					table.requestFocus(true);
					GingerAgent.WriteLog("Checkpoint 2");
					TableModel tm=table.getModel();
					GingerAgent.WriteLog("Checkpoint 3");
					if(tm!=null)
					{
						GingerAgent.WriteLog("Checkpoint 4");
						Object cellDocObject=tm.getValueAt(rowNum, colNum);						
						GingerAgent.WriteLog("Checkpoint 5");
						
						TableCellEditor TCE= table.getCellEditor(rowNum, colNum);
						GingerAgent.WriteLog("Checkpoint 6");
						if(TCE!=null && cellDocObject!=null)
						{							
							cellEditorComponent[0]=TCE.getTableCellEditorComponent(table, cellDocObject, false, rowNum, colNum);
							
							GingerAgent.WriteLog("Checkpoint 7");
						
							GingerAgent.WriteLog("Checkpoint 8");
						}		
						GingerAgent.WriteLog("Checkpoint 9");
					}
					
					
				}
				
		 };
		
			if (SwingUtilities.isEventDispatchThread())
		{
			GingerAgent.WriteLog("\n***************\ngetTableCellComponent-already in EDT\n***************");
			r.run();
		}
		else
		{
			 try {	
				    GingerAgent.WriteLog("\n***********\ngetTableCellComponent-run in EDT\n***********");				   
					SunToolkit.flushPendingEvents();
				
					SunToolkit.executeOnEDTAndWait(table,r);
					
				} catch (InvocationTargetException e) {
					GingerAgent.WriteLog("Inovation target exception while starting thread for click-"+e.getMessage());
					e.printStackTrace();
				} catch (InterruptedException e) {
					GingerAgent.WriteLog("InterruptedException while starting thread for click-"+e.getMessage());
					e.printStackTrace();
		}
		}
		 
		 

		 
		 try
		 {
			 if(cellEditorComponent[0]!=null && !(cellEditorComponent[0] instanceof JPanel))
				 return cellEditorComponent[0];		 
			 else
			 {
				 GingerAgent.WriteLog("Trying to find Cell component using rendered");
				 if(table.getCellRenderer(rowNum, colNum)!=null)
				 {
					 cellEditorComponent[0]=table.prepareRenderer(table.getCellRenderer(rowNum, colNum), rowNum,colNum);
					 return cellEditorComponent[0];
				 }
				 else 
					 return null;
			 }
		 }
		 catch(Exception ex)
		 {
			 ex.printStackTrace();
			 GingerAgent.WriteLog("Exception in get table cell component"+ex.getMessage());
		 }

		 return cellEditorComponent[0];
		
	}
	
	private void setFocus(final JTable table, final int row, final int col)
	{
		//We do this on separate thread because if doing on same thread 
		//then SunToolkit will get stuck while doing Click 
        //boolean flag = false;
		final String[] flag= new String[1];
		flag[0]="false";
    	GingerAgent.WriteLog("Inside set focus - " + table.toString());
       
					table.changeSelection(row, col, false, false);	
	}
	
    private String isCellSelected(Component CellComponent,int rowNum){
		Boolean val1;
		
		if (CellComponent instanceof JRadioButton)			
		{	
			val1= ((JRadioButton)CellComponent).isSelected();
			return val1.toString();
		}
		if (CellComponent instanceof JCheckBox)			
		{	
			val1= ((JCheckBox)CellComponent).isSelected();
			return val1.toString();
		}
		return "";
	}
    private String getCellTreePathValue(Component CellComponent, int rowNum)
    {
	      String val1="";
	      GingerAgent.WriteLog("CellComponent.toString()::" + CellComponent.toString());
	      try {
	             Thread.sleep(10);
	      } catch (InterruptedException e) {
	             // TODO Auto-generated catch block
	             e.printStackTrace();
	      }
	      if (CellComponent instanceof JTree)
	      {
	             JTree jt=((JTree)CellComponent);
	             mWaitForIdleHandler.WaitForJTreeToLoad(jt);
	             TreePath p=jt.getPathForRow(rowNum);                                       
	             if(p!=null)
	             {
	                   jt.setSelectionPath(p);
	                   val1= jt.getPathForRow(rowNum).toString();             
	             }                                 
	      }
	
	      if(val1 == null)
	      {
	             val1="";
	      }
	      return val1;
    }

	String getCellValue(Component CellComponent,int rowNum)
	{
		String val1="";
		GingerAgent.WriteLog("CellComponent.toString()::" + CellComponent.toString());
		try {
			Thread.sleep(10);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		if (CellComponent instanceof JTree)
		{
			JTree jt=((JTree)CellComponent);
			
			mWaitForIdleHandler.WaitForJTreeToLoad(jt);
			
			TreePath p=jt.getPathForRow(rowNum);						
			if(p!=null)
			{
				Object t=p.getLastPathComponent();
				
				jt.setSelectionPath(p);
				
				if(t!=null)
				{
					val1= t.toString();		
				}					
			}
									
		}
		else if (CellComponent instanceof JTextField)
		{	
			GingerAgent.WriteLog("Text::");					
			val1=((JTextField)CellComponent).getText();						
		}
		else if (CellComponent instanceof JTextArea)
		{	
			GingerAgent.WriteLog("JTextArea Text::");					
			val1=((JTextArea)CellComponent).getText();						
		}
		else if (CellComponent instanceof TextArea)
		{	
			GingerAgent.WriteLog("Text Area::");					
			val1=((TextArea)CellComponent).getText();						
		}
		else if (CellComponent instanceof JComboBox)
		{	
			GingerAgent.WriteLog("JComboBox Text::");
			Object o= ((JComboBox)CellComponent).getSelectedItem();
			if(o!=null){
				val1=o.toString();
			}
			else 
			val1="";
		}
		else if (CellComponent instanceof JTextPane)
		{		
			GingerAgent.WriteLog("JTextPane Text::");		
			val1=((JTextPane)CellComponent).getText();
		}
		else if (CellComponent instanceof JLabel)
		{		
			GingerAgent.WriteLog("JLabel Label Text::");
			val1=((JLabel)CellComponent).getText();
		}
		else if (CellComponent instanceof JCheckBox)
		{		
			GingerAgent.WriteLog("JCheckBox Checkbox Text::");		
			val1=((JCheckBox)CellComponent).getText();
		}
		else if (CellComponent instanceof JScrollPane)
		{	
			JViewport jvp=((JScrollPane)CellComponent).getViewport();
			if(jvp!=null)
			{
				Component Cell=jvp.getView();
				if (Cell!=null && Cell instanceof JTextPane)							
					val1=((JTextPane)Cell).getText();
				else
					val1="";
			}
			else 
				val1="";
			
		}	
		else
		{
			val1 = CellComponent.toString();					
		}
		
		if(val1 == null)
		{
			val1="";
		}
					
	
			GingerAgent.WriteLog("val1::" + val1);
			if(val1.indexOf(",text=") > 0)
			{	
				GingerAgent.WriteLog("Inside Value text=::");
				GingerAgent.WriteLog("val1 before substring:"+val1);
				val1=val1.substring(val1.indexOf(",text=")+6);
				GingerAgent.WriteLog("val1 after substring:"+val1);
				if(val1.indexOf("=") > 0 && val1.indexOf(",") > 0)
				{
					val1=val1.substring(0,val1.indexOf("="));						
					val1=val1.substring(0,val1.lastIndexOf(","));						
				}
				else if(val1.indexOf("]") > 0)
				{				
					val1=val1.substring(0,val1.indexOf("]"));
				}
			}				
			val1=convertHtmlToPlainText(val1);
			
			//For few controls in CRM we are getting value with ASCI embedded for &, So we replace it. Ugly but will work for now
			if(val1.contains("&#38"))
			{
				val1=val1.replace("&#38;", "&");	
			}				
		return val1;
	}
	
	private int getColumnNum(JTable CurrentTable,String colBy,String colVal){
		int colNum=-1;		
		if(colBy.equalsIgnoreCase("ColTitle"))
		{ 	
			for (int i =0;i<CurrentTable.getColumnCount();i++)
			{
				String colName="";
				if(!CurrentTable.getColumnModel().getColumn(i).getHeaderValue().toString().equalsIgnoreCase(""))
					colName=CurrentTable.getColumnModel().getColumn(i).getHeaderValue().toString();
				else
					colName=CurrentTable.getModel().getColumnName(i);
				GingerAgent.WriteLog("Col" + i+ ":" +colName);
				if(colVal.equalsIgnoreCase(colName))
				{
					colNum =i;	
					break;
				}
			}
		}
		else if(colBy.equalsIgnoreCase("ColName"))
		{ 
			GingerAgent.WriteLog("Inside ColName");
			GingerAgent.WriteLog("colVal = " + colVal);
			for (int i =0;i<CurrentTable.getColumnCount();i++)
			{
				if(!CurrentTable.getColumnModel().getColumn(i).getHeaderValue().toString().equalsIgnoreCase(""))
				{
					if(!CurrentTable.getColumn(colVal).getHeaderValue().toString().equalsIgnoreCase(""))
					{
						colNum =i;					
						break;
					}
				}
			}
		}
		else
		{
			colNum = Integer.parseInt(colVal);
			GingerAgent.WriteLog("Colnum::" + colNum);
			if(colNum>=CurrentTable.getColumnCount())
			{
				colNum=-1;
			}			
		}
		return colNum;
	}
	
	private int getRowNum(JTable CurrentTable, String colSel, String colTitle, String prop, String oper, String colVal){
		int colNum=-1;
		String propVal="";
		Component CellComponent=null;	
		
		if (colVal.equals(""))
			colVal = "<empty>";
		
		TableModel TM = CurrentTable.getModel();		
		colNum = getColumnNum(CurrentTable,colSel,colTitle);
		if(colNum == -1)
			return -1;
		int iRow=-1;
		boolean bFound = false;
		for(iRow=0 ;iRow< TM.getRowCount();iRow++)
		{
			bFound = false;
			 CellComponent = getTableCellComponent(CurrentTable,iRow, colNum);
			 if(CellComponent ==null)
			 {
				 continue;
			 }
			 
			 if (prop.equalsIgnoreCase("Value"))
			 {				
				propVal=getCellValue(CellComponent,iRow);
				
			 }					
			 else if(prop.equalsIgnoreCase("isSelected"))
			 {
				propVal=isCellSelected(CellComponent,iRow);
			 }
			 else if(prop.equalsIgnoreCase("TreePath"))
			 {
                 propVal=getCellTreePathValue(CellComponent, iRow);
                 propVal = ((propVal.substring(propVal.indexOf(",")+2, propVal.length()-1)).replaceAll(", ", "/"));
			 }
			 else if(prop.equalsIgnoreCase("Type"))
			 {
				propVal = CellComponent.getClass().getName();
				propVal = propVal.substring(propVal.lastIndexOf('.') + 1);
				propVal = propVal.replace("Native", "");
				propVal = "Uif" + propVal;
			 }	
			 else if(prop.equalsIgnoreCase("Text"))
			 {
				propVal = "";
			 }	
			 else if(prop.equalsIgnoreCase("Enabled"))
			 {
				propVal = Boolean.toString(CellComponent.isEnabled());
			 }
			 else if(prop.equalsIgnoreCase("Visible"))
			 {
				propVal = Boolean.toString(CellComponent.isVisible());
			 }	
			 else if(prop.equalsIgnoreCase("HTML"))
			 {
				propVal="";
			 }	
			 else if(prop.equalsIgnoreCase("Path"))
			 {
				propVal="";
			 }	
			 else if(prop.equalsIgnoreCase("List"))
			 {
				propVal="";
			 }	
			 else if(prop.equalsIgnoreCase("Tooltip"))
			 {
				propVal="";
			 }	
			 else if(prop.equalsIgnoreCase("DateTimeVaue"))
			 {
				propVal="";
			 }	
			 else
			 {
				propVal="";	
			 }
						 
			 if (oper.equals("Equals"))
			 {
				bFound = colVal.equalsIgnoreCase(propVal);
			 }				
			 else if (oper.equals("NotEquals"))
			 {
				bFound = !colVal.equals(propVal);
			 }
				
			 else if (oper.equals("Contains"))
			 {
				bFound = (propVal != null && propVal.contains(colVal));
			 }	
				
			 else if (oper.equals("NotContains"))
			 {
				bFound = (propVal == null || !propVal.contains(colVal));// condition should be Propval!=null
			 }
				
			 else if (oper.equals("StartsWith"))
			 {				 
				bFound = (propVal != null && propVal.startsWith(colVal));				
			 }				
			 else if (oper.equals("NotStartsWith"))
			 {
				bFound = (propVal == null || !propVal.startsWith(colVal));
			 }				
			 else if (oper.equals("EndsWith"))
			 {
				bFound = (propVal != null && propVal.endsWith(colVal));
			 }				
			 else if (oper.equals("NotEndsWith"))
			 {
				bFound = (propVal == null || !propVal.endsWith(colVal));
			 }
			 			 
			if (bFound)
				return iRow;
		}		
		return -1;		
	}
	
	public static String convertHtmlToPlainText(String htmlStr)
	{
	     if (htmlStr == null)
		 {
	      return null;
	     }	     
	     StringBuilder plainText = new StringBuilder();
	     Pattern htmlPattern = null;
	     String match = "";
	     int strIdx = 0;
	     
	 
	 
	 
	     htmlPattern = Pattern.compile("<[^>]+>|(&[#a-z0-9]+;)");
	     Matcher matcher = htmlPattern.matcher(htmlStr);
	     
	     matcher.find();
	     
	     while (!matcher.hitEnd())
	     {


	       matcher.start();
	       int entityStart = matcher.start(1);
	       
	       match = htmlStr.substring(matcher.start(), matcher.end());
	       String subStr = htmlStr.substring(strIdx, matcher.start());
	       plainText.append(subStr);
	       
	 
	       if (matcher.start() == entityStart) 
	       {
	         String entity = matcher.group(1);
	         plainText.append(convertCharEntityToPlainText(entity));
	       }
	       
	       if (match.toUpperCase().equals("<BR>")) 
	       {
	         plainText.append(System.getProperty("line.separator"));
	       }
	       
	       strIdx = matcher.end();
	       matcher.find();
	     }
	     
	 
	     if (strIdx <= htmlStr.length() - 1) 
	     {
	       plainText.append(htmlStr.substring(strIdx, htmlStr.length()));
	     }
	     
	     return plainText.toString();
	 }
	
	private List<Integer> ConvertListStringToInt(String[] sList) {
		
		 List<Integer> intList = new ArrayList<Integer>();
		
		for(String s : sList) 
		{
			intList.add(Integer.parseInt(s));
		}
		return intList;
	}
	
	private static String convertCharEntityToPlainText(String charEntity)
	   {
	     String plainStr = "";
	     int entityUnicode = 0;
	     String entityCode = null;
	     
	     if (charEntity != null)
	     {
	 
	 
	       if (charEntity.charAt(1) == '#') {
	         entityCode = charEntity.substring(2, charEntity.length() - 1);
	         try
	         {
	           entityUnicode = Integer.parseInt(entityCode);
	           if (entityUnicode == 8203)
	           {
	 
	             return "";
	           }
	         } catch (Exception e) {
	           return charEntity;
	         }
	       }
	       
	       entityCode = charEntity.substring(1, charEntity.length() - 1);
	       
	 
	 
	       if (entityCode.equals("nbsp")) {
	         entityUnicode = 160;
	       }
	       else if (entityCode.equals("amp")) {
	         entityUnicode = 38;
	       }
	       else if (entityCode.equals("lt")) {
	         entityUnicode = 60;
	       }
	       else if (entityCode.equals("gt")) {
	         entityUnicode = 62;
	       }
	       else if (entityCode.equals("quot")) {
	         entityUnicode = 34;
	 
	       }
	       else
	       {
	 
	         plainStr = charEntity;
	         return plainStr;
	       }
	       
	 
	       if (Character.isDefined(entityUnicode)) {
	         plainStr = String.valueOf((char)entityUnicode);
	       } else {
	         plainStr = charEntity;
	       }
	     }
	     
	     return plainStr;
	   }
	
	private void DumpThreadInfo() {
		GingerAgent.WriteLog("Wait For Idle - Threads Info");
		Set<Thread> threadSet = Thread.getAllStackTraces().keySet();
		
		int i =1;
		for (Thread t : threadSet)
		{
			ThreadMXBean tmxb = ManagementFactory.getThreadMXBean();
	        long cpuTime = tmxb.getThreadCpuTime(t.getId());
			
			String st = t.getId() + ", " + t.getName() + ", " + t.getState().toString() + " CPU Time=" + cpuTime;			
			
			GingerAgent.WriteLog("Thread - " + i + ": " + st);
			i++;
		}		
		
	}
	
	public void type(CharSequence characters) {
        int length = characters.length();
        for (int i = 0; i < length; i++) {
            char character = characters.charAt(i);
            type(character);
        }
    }
	private void doType(int[] keyCodes, int offset, int length) {
        if (length == 0) {
            return;
        }
        Robot robot;
		try {
			robot = new Robot();		
	        robot.keyPress(keyCodes[offset]);
	        doType(keyCodes, offset + 1, length - 1);
	        robot.keyRelease(keyCodes[offset]);
		} catch (AWTException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
    }

	private void doType(int... keyCodes) {
        doType(keyCodes, 0, keyCodes.length);
    }

	//TODO: this is not multi lang
	public void type(char character) {
        switch (character) {
        case 'a': doType(KeyEvent.VK_A); break;
        case 'b': doType(KeyEvent.VK_B); break;
        case 'c': doType(KeyEvent.VK_C); break;
        case 'd': doType(KeyEvent.VK_D); break;
        case 'e': doType(KeyEvent.VK_E); break;
        case 'f': doType(KeyEvent.VK_F); break;
        case 'g': doType(KeyEvent.VK_G); break;
        case 'h': doType(KeyEvent.VK_H); break;
        case 'i': doType(KeyEvent.VK_I); break;
        case 'j': doType(KeyEvent.VK_J); break;
        case 'k': doType(KeyEvent.VK_K); break;
        case 'l': doType(KeyEvent.VK_L); break;
        case 'm': doType(KeyEvent.VK_M); break;
        case 'n': doType(KeyEvent.VK_N); break;
        case 'o': doType(KeyEvent.VK_O); break;
        case 'p': doType(KeyEvent.VK_P); break;
        case 'q': doType(KeyEvent.VK_Q); break;
        case 'r': doType(KeyEvent.VK_R); break;
        case 's': doType(KeyEvent.VK_S); break;
        case 't': doType(KeyEvent.VK_T); break;
        case 'u': doType(KeyEvent.VK_U); break;
        case 'v': doType(KeyEvent.VK_V); break;
        case 'w': doType(KeyEvent.VK_W); break;
        case 'x': doType(KeyEvent.VK_X); break;
        case 'y': doType(KeyEvent.VK_Y); break;
        case 'z': doType(KeyEvent.VK_Z); break;
        case 'A': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_A); break;
        case 'B': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_B); break;
        case 'C': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_C); break;
        case 'D': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_D); break;
        case 'E': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_E); break;
        case 'F': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_F); break;
        case 'G': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_G); break;
        case 'H': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_H); break;
        case 'I': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_I); break;
        case 'J': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_J); break;
        case 'K': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_K); break;
        case 'L': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_L); break;
        case 'M': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_M); break;
        case 'N': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_N); break;
        case 'O': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_O); break;
        case 'P': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_P); break;
        case 'Q': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_Q); break;
        case 'R': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_R); break;
        case 'S': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_S); break;
        case 'T': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_T); break;
        case 'U': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_U); break;
        case 'V': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_V); break;
        case 'W': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_W); break;
        case 'X': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_X); break;
        case 'Y': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_Y); break;
        case 'Z': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_Z); break;
        case '`': doType(KeyEvent.VK_BACK_QUOTE); break;
        case '0': doType(KeyEvent.VK_0); break;
        case '1': doType(KeyEvent.VK_1); break;
        case '2': doType(KeyEvent.VK_2); break;
        case '3': doType(KeyEvent.VK_3); break;
        case '4': doType(KeyEvent.VK_4); break;
        case '5': doType(KeyEvent.VK_5); break;
        case '6': doType(KeyEvent.VK_6); break;
        case '7': doType(KeyEvent.VK_7); break;
        case '8': doType(KeyEvent.VK_8); break;
        case '9': doType(KeyEvent.VK_9); break;
        case '-': doType(KeyEvent.VK_MINUS); break;
        case '=': doType(KeyEvent.VK_EQUALS); break;
        case '~': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_BACK_QUOTE); break;
        case '!': doType(KeyEvent.VK_EXCLAMATION_MARK); break;
        case '@': doType(KeyEvent.VK_AT); break;
        case '#': doType(KeyEvent.VK_NUMBER_SIGN); break;
        case '$': doType(KeyEvent.VK_DOLLAR); break;
        case '%': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_5); break;
        case '^': doType(KeyEvent.VK_CIRCUMFLEX); break;
        case '&': doType(KeyEvent.VK_AMPERSAND); break;
        case '*': doType(KeyEvent.VK_ASTERISK); break;
        case '(': doType(KeyEvent.VK_LEFT_PARENTHESIS); break;
        case ')': doType(KeyEvent.VK_RIGHT_PARENTHESIS); break;
        case '_': doType(KeyEvent.VK_UNDERSCORE); break;
        case '+': doType(KeyEvent.VK_PLUS); break;
        case '\t': doType(KeyEvent.VK_TAB); break;
        case '\n': doType(KeyEvent.VK_ENTER); break;
        case '[': doType(KeyEvent.VK_OPEN_BRACKET); break;
        case ']': doType(KeyEvent.VK_CLOSE_BRACKET); break;
        case '\\': doType(KeyEvent.VK_BACK_SLASH); break;
        case '{': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_OPEN_BRACKET); break;
        case '}': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_CLOSE_BRACKET); break;
        case '|': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_BACK_SLASH); break;
        case ';': doType(KeyEvent.VK_SEMICOLON); break;
        case ':': doType(KeyEvent.VK_COLON); break;
        case '\'': doType(KeyEvent.VK_QUOTE); break;
        case '"': doType(KeyEvent.VK_QUOTEDBL); break;
        case ',': doType(KeyEvent.VK_COMMA); break;
        case '<': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_COMMA); break;
        case '.': doType(KeyEvent.VK_PERIOD); break;
        case '>': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_PERIOD); break;
        case '/': doType(KeyEvent.VK_SLASH); break;
        case '?': doType(KeyEvent.VK_SHIFT, KeyEvent.VK_SLASH); break;
        case ' ': doType(KeyEvent.VK_SPACE); break;
        default:
            throw new IllegalArgumentException("Cannot type character " + character);
        }
    }
	
	private boolean IsBrowserBusyWithImplicitSync(int implicitWait)
	{
		if (mBrowserHelper != null)
		{
			boolean bBrowserBusy = true;
			long start = System.currentTimeMillis();
			long elapsed = System.currentTimeMillis() - start;
			
			while(bBrowserBusy && !mWaitForIdleHandler.isCommandTimedOut)
			{
				if(elapsed> implicitWait*1000)
				{
					GingerAgent.WriteLog("*********Sync for load page in Browser timeout after: "+elapsed);
					break;
				}
				if (!mBrowserHelper.isBrowserBusy())
				{
					bBrowserBusy = false;
				}
	     		else 
				{
					try {
						Thread.sleep(500);
					} catch (InterruptedException e) {					
						e.printStackTrace();
					}
				}			
				elapsed = System.currentTimeMillis() - start;	
				GingerAgent.WriteLog("Waiting for browser to be ready:"+elapsed);
			}
			return bBrowserBusy;
		}
		return true;
	}
}
