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

import java.io.PrintStream;
import java.text.SimpleDateFormat;
import java.util.Date;
import javax.swing.JTextArea;
import javax.swing.SwingUtilities;
import javax.swing.text.BadLocationException;

public class Utils {
	static JTextArea mTextAreaControl;
	final static int MAX_LINES = 5000;
	static String CLEANING_MESSAGE = "**********Displays only the last 5000 lines**********";
	static int DelLineNum = 0;
	
	public static void redirectSystemStreams(final JTextArea TextAreaControl) {
			
		mTextAreaControl = TextAreaControl;		
		System.setOut(new PrintStream(System.out) {
			
			//TODO: we can capture System.out.println to show in ExInfo ! create Action: GetLastActionSysOut()
			
	    	  public void println(final String s) {
	    		  if (SwingUtilities.isEventDispatchThread())
	    		  {
	    			  updateTextArea("SysOut - " +  s);
	    		  }
	    		  else
	    		  {
		    		  SwingUtilities.invokeLater(new Runnable() {
		    			    public void run() {
		    			    	updateTextArea("SysOut - " +  s);	 
		    		   }
		    		  });	  
	    		  }  	    
	    	    super.println(s);
	    	  }
	    	});
		
		//TODO: hook syserr		
	  }
	
	private static void updateTextArea(final String text) {
		
		try
		{
			if (mTextAreaControl.getLineCount() > MAX_LINES)
			{
				try {
					for (int i = 0 ; i < mTextAreaControl.getLineCount() - MAX_LINES; i++)
					{
						int start = mTextAreaControl.getLineStartOffset(DelLineNum);
						int end = mTextAreaControl.getLineEndOffset(DelLineNum);
						mTextAreaControl.replaceRange("", start, end);
					}
					if (DelLineNum == 0)
					{
						int end = mTextAreaControl.getLineEndOffset(0);
						mTextAreaControl.replaceRange(getCurrentTimeStamp() + " - " + CLEANING_MESSAGE + "\n", 0, end);
						DelLineNum = 1;
					}
				} catch (BadLocationException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}
			mTextAreaControl.append(getCurrentTimeStamp() + " - " + text + "\n");
			mTextAreaControl.setCaretPosition(mTextAreaControl.getDocument().getLength());
		}
		catch (OutOfMemoryError e) {
			try {
				mTextAreaControl.getDocument().remove(0, mTextAreaControl.getDocument().getLength());
				mTextAreaControl.append(getCurrentTimeStamp() + " - " + "**********updateTextArea OutOfMemoryError**********" + "\n");
			} catch (BadLocationException e1) {
				// TODO Auto-generated catch block
				e1.printStackTrace();
			}
		}
		catch (IllegalArgumentException e)
		{
			try {
				mTextAreaControl.getDocument().remove(0, mTextAreaControl.getDocument().getLength());
				mTextAreaControl.append(getCurrentTimeStamp() + " - " + "**********updateTextArea IllegalArgumentException**********" + "\n");
				mTextAreaControl.append(getCurrentTimeStamp() + " - " + "Exception message = "+e.getMessage() + "\n");
			} catch (BadLocationException e1) {
				// TODO Auto-generated catch block
				e1.printStackTrace();
			}
		}	
	}
	
	public static String getCurrentTimeStamp() {
	    return new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS").format(new Date());
	}	
}