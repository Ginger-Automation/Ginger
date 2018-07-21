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

/**
 * 
 */
package com.amdocs.ginger;

import java.awt.AWTEvent;
import java.awt.Window;
import java.awt.event.AWTEventListener;
import java.awt.event.WindowEvent;
import java.util.List;
import javax.swing.JDialog;
import javax.swing.JFrame;

/**
 * 
 *
 */
class WindowMonitor implements AWTEventListener {
    PayLoad pl = null;
	List<PayLoad> mRecordingWM = null;
	Recorder mRecorderWM = null;
	static Window activeWindow = null;
	
	public WindowMonitor (List<PayLoad> mRecording, Recorder mRecorder){
		this.mRecordingWM = mRecording;
		this.mRecorderWM  = mRecorder;
	}
	
	public void eventDispatched(AWTEvent event) {    	        	
        switch (event.getID()){
            case WindowEvent.WINDOW_OPENED:
            	GingerAgent.WriteLog("WindowEvent.WINDOW_OPENED " + event.getSource());
                if (event.getSource() instanceof JDialog){
                	GingerAgent.WriteLog("Popup Window JDialog");
                	JDialog cmp = (JDialog)event.getSource();
                	setActiveWindow(cmp); //TO DO: Add in JavaDriver code to update CurrentWindow object
                	String title = cmp.getTitle();
                	AddSwitchWindow(title);
                	mRecordingWM.add(getPl());
                	mRecorderWM.AddActionListener(cmp, mRecorderWM);                	                	
                }
                if (event.getSource() instanceof JFrame){
                	GingerAgent.WriteLog("Popup Window JFrame");
                	JFrame cmp = (JFrame)event.getSource();
                	setActiveWindow(cmp); //TO DO: Add in JavaDriver code to update CurrentWindow object
                	String title = cmp.getTitle();
                	AddSwitchWindow(title);
                }

                break;
            case WindowEvent.WINDOW_CLOSED:
            	setActiveWindow(Window.getWindows()[0]);
            	GingerAgent.WriteLog("WindowEvent.WINDOW_CLOSED:: " + Window.getWindows()[0].getName());
                break;
        }
    }
    
	public void AddSwitchWindow(String title){      
		PayLoad pl = new PayLoad("SwitchWindow");
        GingerAgent.WriteLog("AddSwitchWindow Title " + title);        
        pl.AddValue(title);
        pl.ClosePackage();
        setPl(pl);                       
    }
	
	public PayLoad getPl() {
		return pl;
	}

	public void setPl(PayLoad pl) {
		this.pl = pl;
	}
	
	public static Window getActiveWindow() {
		return activeWindow;
	}

	public void setActiveWindow(Window activeWindow) {
		WindowMonitor.activeWindow = activeWindow;
	}
}