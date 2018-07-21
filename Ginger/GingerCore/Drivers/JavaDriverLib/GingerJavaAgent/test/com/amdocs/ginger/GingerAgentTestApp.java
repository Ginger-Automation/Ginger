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
import javax.swing.JFrame;

public class GingerAgentTestApp {
	static GingerJavaSocketServer srv;
	
	/**
	 * @param args
	 */
	public static void main(String[] args) {
 
		JEditorPaneExample frame= new JEditorPaneExample();
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		frame.setTitle("Java Swing Test App");
	    frame.pack();
	    frame.setVisible(true);
	    frame.setSize(600, 300);
	    //Start the Ginger Socket Server
	    GingerAgent.SHOW_AGENT = true;
	    GingerAgent GA = new GingerAgent();	    
	    GA.StartServer();	    
	}
}
