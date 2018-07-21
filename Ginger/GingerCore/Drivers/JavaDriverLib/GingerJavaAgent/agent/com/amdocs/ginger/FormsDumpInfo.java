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
import java.awt.Component;
import java.awt.Container;
import java.awt.Frame;


public class FormsDumpInfo {

	public static String DumpFormsInfo() 
	{
		String CRLF = "\n";
		StringBuilder s = new StringBuilder();
		s.append("============================== Dump Start =================================================").append(CRLF);
		
		 // Dump JSwing Info
		 
		 s.append("JFRame Info").append(CRLF);
		 for (Frame jf : Frame.getFrames())
		 {
			 // show all Frames excpet the Gigner Agent window
			 if (!"Ginger Agent Console".equals(jf.getTitle()))
			 {
				 s.append("============================== Dump Frame =================================================").append(CRLF);				 
				 s.append("Name=").append(jf.getName()).append(CRLF);
				 s.append("Title=").append(jf.getTitle()).append(CRLF);				 
				 s.append("Frame Components=").append(DumpFrameComponents(jf)).append(CRLF);				 
				 s.append("============================== End Dump Frame =================================================").append(CRLF);
			 }
		 }
		 			 		
		 s.append("============================== Dump End =================================================").append(CRLF);
		 
		 //TODO: write to file
		 return s.toString();
		//return "OK|Dump Done";
	}

 private static String DumpFrameComponents(Container jf) 
 {
	 StringBuilder s=new StringBuilder();
	 for (Component c : jf.getComponents())
	 {		
		 try
		 {
			 s.append("Name=").append(c.getName()).append(", ");
			 s.append("Class Name=").append(c.getClass().getName()).append(", ");
			 s.append("\n");
			 
			 if (c instanceof Container)
			 {			 
				 s.append("Control is Container - ");
				 s.append(DumpFrameComponents((Container)c));
			 }
		 }
		 catch (Exception ex)
		 {
			 GingerAgent.WriteLog(ex.getMessage());
		 }
	 }
	 return s.toString();
 }

private static String getComponentXPath(Component c) 
{
		String s="";
		Component c1 = c;
		while (c1 != null)
		{			
			String CName = "(" + c1.getClass().getName() + ")" + c1.getName();							
			s = CName + "/" + s;
			c1 = c1.getParent();
		}
	return s;
}
	
}
