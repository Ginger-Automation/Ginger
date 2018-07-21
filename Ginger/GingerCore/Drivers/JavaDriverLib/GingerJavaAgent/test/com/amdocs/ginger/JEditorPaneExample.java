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

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.InputStream;
import java.io.InputStreamReader;
import javax.swing.JEditorPane;
import javax.swing.text.html.HTMLEditorKit;

public class JEditorPaneExample extends javax.swing.JFrame{
	
	 public JEditorPaneExample() {
		 
		 this.setName("JFrame1");
			this.setDefaultCloseOperation(javax.swing.WindowConstants.EXIT_ON_CLOSE);
			this.setBounds(45, 25, 317, 273);
		 
	      JEditorPane jp= new JEditorPane();
	      jp.setContentType("text/html");
	      jp.setEditorKit(new HTMLEditorKit());
	      
	      try
	      {	    	 
	    	  String htmlStr="Hello from Editor Pane";
	      jp.setText(htmlStr);
	      }
	      catch(Exception e)
	      {	    	  
	      }     
	      this.add(jp);	        
	    }	
}
