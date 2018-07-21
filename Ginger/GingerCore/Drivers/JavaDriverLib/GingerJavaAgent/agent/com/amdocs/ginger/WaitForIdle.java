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

import java.awt.AWTEvent;
import java.awt.EventQueue;
import java.awt.Toolkit;
import java.lang.reflect.InvocationTargetException;
import javax.swing.JTable;
import javax.swing.JTree;
import sun.awt.SunToolkit;

public class WaitForIdle {
	
	private EventQueue mEventQueue = null;
	private MyEventQueue mJDEventQueue = null;
	public volatile boolean isCommandTimedOut=false;
	
	 private static class MyEventQueue extends EventQueue {

	        private static int THRESHOLD = 500;   //ms
	        private long last = 0;
	        private AWTEvent lastEvent = null;	        
	        private Boolean isFreezeTracking = false;
	        
	        @Override
	        public void postEvent(AWTEvent e) {
	        	        	
	            super.postEvent(e);	                                  
	            if (!isFreezeTracking)
	            {	            
		            String st = e.toString();	          
		            // we ignore timer, since CRM run timer all the time...
		            if(!st.contains("runnable=javax.swing.Timer") 
		            		&& !st.contains("runnable=javax.swing.text.DefaultCaret"))
		            {
		            	lastEvent = e;
		            	last = System.currentTimeMillis(); 						
		            }		           
	            }
	        }
	        
	        public void setFreezeTracking(Boolean b)
	        {
	        	isFreezeTracking = b;
	        }

	        public void setTHRESHOLD(int ms)
	        {
	        	THRESHOLD = ms;
	        }
	        
	        public boolean isIdle() {	        	
	        	
	        	EventQueue eventQueue = Toolkit.getDefaultToolkit().getSystemEventQueue();
	        	if (eventQueue.peekEvent() == null )
		        {		        	
		        	lastEvent = null;
		        	return true;	        		
		        }		        	
	        	// if the last event is still in process then system is not idle
	        	if (eventQueue.peekEvent() == lastEvent)
	        	{
	        		System.out.println("Last Event is still in Process");
	        		last = System.currentTimeMillis();
	        		return false;
	        	}
	        	
	        	Boolean bIdle = System.currentTimeMillis() - last > THRESHOLD; 
	        	if (bIdle)
	        	{
	        		System.out.println("----------------------------------------------------------------------");
	        		System.out.println("----------------------------------------------------------------------");
	        		System.out.println("----------------------------------------------------------------------");
	        		System.out.println("--                         IDLE                                     --");
	        		System.out.println("*** Elapsed from last = " + (System.currentTimeMillis() - last));        		
	        		System.out.println("----------------------------------------------------------------------");
	        		System.out.println("----------------------------------------------------------------------");
	        		System.out.println("----------------------------------------------------------------------");
	        	}
	        	
	            return bIdle;
	        }
			}
	
	 public void pushEmptyEventAndWait() {
			try {			
				// will not happen but just in case
				if (EventQueue.isDispatchThread())
	        		throw new IllegalThreadStateException("Cannot call method from the event dispatcher thread !!!");
				
				EventQueue.invokeAndWait(new Runnable() {

				    public void run() {
				    	// empty impl event
				    }
				});
			} catch (InvocationTargetException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}			
		}	 	
	 
	 public void DoWaitWaitIdle(String IdleType) {   
		SunToolkit.getDefaultToolkit().sync();		
		if (mEventQueue == null)
        {  
        	mEventQueue = Toolkit.getDefaultToolkit().getSystemEventQueue();


        	if (EventQueue.isDispatchThread())
        		throw new IllegalThreadStateException("Cannot call method from the event dispatcher thread");
        	        	
        	mJDEventQueue = new MyEventQueue();
    		mEventQueue.push(mJDEventQueue);
        }
		
		mJDEventQueue.setFreezeTracking(true);
		GingerAgent.setStatus("Waiting for Idle - '" + IdleType + "'");                	
		mJDEventQueue.setFreezeTracking(false);
		int i=0;
		long start = System.currentTimeMillis();		
		int TimeOut = 120; //seconds	
		int SleepIntervalMS = 100;  

		if ("Short".equals(IdleType))
		{
			SleepIntervalMS = 100; //ms
			TimeOut = 30; // 30 seconds			
		}
		else if ("Medium".equals(IdleType))
		{
			SleepIntervalMS = 500; //ms
			TimeOut = 60; // 1 min
		}
		else if ("Long".equals(IdleType))
		{
			SleepIntervalMS = 1000; //ms
			TimeOut = 120; // 2 minutes 
		}
		else if ("VeryLong".equals(IdleType))
		{
			SleepIntervalMS = 5000; //ms
			TimeOut = 300; // 5 minutes
		}
		    
		mJDEventQueue.setTHRESHOLD(SleepIntervalMS - 10);

		Boolean bStopWait = false;				
		while (!bStopWait)							
		{				
			Sleep(SleepIntervalMS);  												
			i++;
			
			long elapsed = System.currentTimeMillis() - start;
			if (elapsed > TimeOut * 1000) 
			{
				System.out.println("Wait For Idle max time done, elapsed=" + elapsed);
				//If wait for idle is getting stuck then we clear the event queue to avoid it getting stuck on subsequent actions
				
				mJDEventQueue.lastEvent=null;
				mJDEventQueue.last=0;
				break;
			}

			// Updating GAF is UI event we freeze tracking before the change
			mJDEventQueue.setFreezeTracking(true);
			GingerAgent.setStatus("Waiting for Idle - '" + IdleType + "' - #" + i);
			mJDEventQueue.setFreezeTracking(false);										
			Boolean isIdle = mJDEventQueue.isIdle();			
			
			if (isIdle) 
			{				
				break;

			}

			if(isCommandTimedOut)
			{
				break;
			}

			// if modal is showing then avoid getting stuck...
			if (SwingHelper.isModalDialogShowing())
			{			
				break;
			}			
		}	
		long elapsed = System.currentTimeMillis() - start;
		System.out.println("Done Wait for Idle: Elapsed = " + elapsed);
	}	

	public void WaitForTableToLoad(JTable table)
	{
		long start = System.currentTimeMillis();
		int TimeOut = 30; //seconds		
		int SleepIntervalMS = 100;  //ms
		Sleep(500);
		while(table.isEditing()&& !isCommandTimedOut)
		{
			Sleep(SleepIntervalMS);		
			
			long elapsed = System.currentTimeMillis() - start;
			if (elapsed > TimeOut * 1000) 
			{
				System.out.println("Wait For Table Load max time done, elapsed=" + elapsed);
				break;
			}
		}
		long elapsed = System.currentTimeMillis() - start;
		System.out.println("Done Wait For Table Load: Elapsed = " + elapsed);
	}
	
	public void WaitForJTreeToLoad(JTree treeComponent)
	{
		long start = System.currentTimeMillis();
		int TimeOut = 30; //seconds		
		int SleepIntervalMS = 100;  //ms
		Sleep(500);
		while(treeComponent.isEditing() && !isCommandTimedOut)
		{
			Sleep(SleepIntervalMS);
			
			
			long elapsed = System.currentTimeMillis() - start;
			if (elapsed > TimeOut * 1000) 
			{
				System.out.println("Wait For Table Load max time done, elapsed=" + elapsed);
				break;
			}
		}
		long elapsed = System.currentTimeMillis() - start;
		System.out.println("Done Wait For Table Load: Elapsed = " + elapsed);
	}	
	
	private void Sleep(long ms)
    {
    	try {
			Thread.sleep(ms);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
    }
}