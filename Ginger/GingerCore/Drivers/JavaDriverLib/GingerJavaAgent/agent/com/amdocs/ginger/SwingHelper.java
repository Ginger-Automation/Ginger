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
import java.awt.Dialog;
import java.awt.Dimension;
import java.awt.Frame;
import java.awt.Point;
import java.awt.Rectangle;
import java.awt.Robot;
import java.awt.Window;
import java.awt.event.InputEvent;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.List;

import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JComboBox;
import javax.swing.JDialog;
import javax.swing.JFrame;
import javax.swing.JInternalFrame;
import javax.swing.JLabel;
import javax.swing.JList;
import javax.swing.JMenu;
import javax.swing.JMenuItem;
import javax.swing.JRadioButton;
import javax.swing.JTabbedPane;
import javax.swing.JTable;
import javax.swing.JTextArea;
import javax.swing.JTextField;
import javax.swing.JTextPane;
import javax.swing.JTree;
import javax.swing.ListModel;
import javax.swing.text.Position;
import javax.swing.tree.TreePath;

import sun.awt.util.IdentityArrayList;

public class SwingHelper implements IXPath {

	private Window  CurrentWindow = null;
	
	private XPathHelper mXPathHelper= new XPathHelper(this);
	
	public Window getCurrentWindow() {
		return CurrentWindow;
	}

	public void setCurrentWindow(Window currentWindow) {
		
		GingerAgent.WriteLog("setCurrentWindow:: " + currentWindow.getClass());
		CurrentWindow = currentWindow;
	}
	
	
	public Component getBrowserComponentFromCurrentWindow()
	{
		GingerAgent.WriteLog("CheckJExplorerExists()");
		GingerAgent.WriteLog("Current Window Title = " + CurrentWindow.getName());
		
		if (CurrentWindow != null)		
		{			
			List<Component> list = getAllComponents(CurrentWindow);
			for(Component c : list)
			{
				String browser = c.getClass().toString();
				if (c.isVisible() && browser.contains("com.amdocs.uif.widgets.browser"))
				{	
					GingerAgent.WriteLog(" JExplorerBrowser Found:= " + c.getClass());
					//TODO: We check if we are able to locate browser. It is needed For few env. 
					//Ugly but will work for now
					String XPath = mXPathHelper.GetComponentXPath(CurrentWindow,c);		
					Component c1=FindElement("ByXPath", XPath);
					if(c1!=null)
					 return c1;
				}			
			}			
		}
		return null;
	}

	public Component FindElement(String LocateBy, String LocateValue) 
	{
		Component comp = null;
		if(LocateBy.equals("ByName")||LocateBy.equals("ByValue"))
		{	
			if (CurrentWindow == null) {
				//TODO: Not sure why we are doing this. User is supposed do Switch window before running any other action. 
				//So if current window is null we should give the message switch window is needed
				Frame[] Frames = Frame.getFrames();
				for (Frame f : Frames) {
					comp = FindElementRecursive(f, LocateBy, LocateValue);
					if (comp != null) {
						CurrentWindow = f;
						return comp;
					}
				}
			}
			if (CurrentWindow == null) {
				return null;
			}
			comp = FindElementRecursive(CurrentWindow, LocateBy, LocateValue);									
		}
		else if(LocateBy.equals("ByContainerName"))
		{
			Component tblComp = null;
			if (CurrentWindow == null) {
				Frame[] Frames = Frame.getFrames();
				for (Frame f : Frames) {
					comp = FindElementRecursive(f, "ByName", LocateValue);
					if (comp != null) { 
						CurrentWindow = f;
						tblComp = comp;
						return tblComp;
					}
				}
			}			
			else
			{
				tblComp = FindElementRecursive(CurrentWindow, "ByName", LocateValue);	
			}		
			tblComp = FindElementRecursive((Container)tblComp, "ByClassName", "SearchJTable");
			return tblComp;
		}
		else if(LocateBy.equalsIgnoreCase("ByXPath"))
		{
			comp=mXPathHelper.GetComponentByXPath(CurrentWindow,LocateValue);
		}	
		return comp;
	}
	
	private Component FindElementRecursive(Container Container, String LocateBy, String LocateValue)	
    {
		Component[] list = Container.getComponents();
    	for (Component c : list)
    	{
    		
			// TODO: By XPath and other locators
    		// By Name
    		if(LocateBy.equals("ByName"))
    		{
    			if (LocateValue.equals(c.getName()))
    			{ 
    				return c;
    			}
    		}
    		
    		else if(LocateBy.equals("ByValue"))
    		{
    			List<String> values=GetCompValue(c);
    			
    			if (LocateValue.equals(values.get(0)))
    			{
        			return c;
    			}
    		}
    		else if(LocateBy.equals("ByClassName"))
    		{
    			if (c.getClass().getName() != null && c.getClass().getName().contains(LocateValue))
    			{
    				return c;
    			}
    		}
    		// recursive drill down
    		if (c instanceof Container)
    		{
    			Component comp = FindElementRecursive((Container)c, LocateBy, LocateValue);
    			if (comp != null)
    			{
    				return comp;
    			}
    			
    		}
    		if (c instanceof JMenu)
    		{
    			JMenu jm=(JMenu)c;
    			    			
    			Component[] comps =  jm.getMenuComponents();
    			for (Component c1 : comps)
    			{    	    				    			
    				if (c1 instanceof JMenuItem)
    				 {
    					
    					JMenuItem mi = (JMenuItem)c1;    				
	    				if(LocateBy.equals("ByName"))
			    		{
	    					if (LocateValue.equals(mi.getName())){				 
	    					 return c1;
	    					}
			    		}    				 
	    				else if(LocateBy.equals("ByValue"))
	    		    	{
	    		    		List<String> values=GetCompValue(c);	    		    		
	    		    		if (LocateValue.equals(values.get(0)))
	    		    		{
	    		        		return c;
	    		    		}
	    		    	}
    				 }
    				if (c1 instanceof JMenu)
    	    		{
    					 Component cm = FindElementRecursive(((JMenu) c1).getPopupMenu(), LocateBy, LocateValue);
    					 if (cm != null){
    						 return cm;
    					 }    					 
    					 
    	    		}	 
    			}	     			
        		
    		}  
    	}
    	   	    	    	
    	return null;
    }

	
	
	
	
	
	
	public String GetPropertyValue(Component c,String propertyName)
	{
		String propertyValue="";
		   if("Name".equals(propertyName))
		   {
			   propertyValue=c.getName();
		   }
		   else if("ClassName".equals(propertyName))
		   {
			   propertyValue=c.getClass().getName();
		   }
		   else if("Value".equals(propertyName))
		   {
			   propertyValue=GetCompValue(c).get(0);
		   }
		  //TODO: add more properties here to support as part of x path
		   
		 return propertyValue;
	}
	

		
		public List<String> GetCompValue(Component comp) {
			
			//TODO: Why returning a list?  change it to return a simple string value
			
			List<String> CompValue = new ArrayList<String>();
			try
			{
				GingerAgent.WriteLog("Inside GetCompValue" );
			
				if (comp instanceof JTextField)
				{			
					CompValue.add(((JTextField)comp).getText()+"");
				}
				else if (comp instanceof JTextArea)
				{			
					CompValue.add(((JTextArea)comp).getText()+"");
				}
				else if (comp instanceof JTextPane)
				{	
					CompValue.add(((JTextPane)comp).getText()+"");
				}
				else if (comp instanceof JLabel)
				{
					String txt = ((JLabel)comp).getText();
					if (txt==null) txt ="";
					CompValue.add(txt);
				}
				
				else if (comp instanceof JButton)
				{
					CompValue.add(((JButton)comp).getText() + "");
					
				}

				else if (comp instanceof JCheckBox)
				{
					CompValue.add( ((JCheckBox)comp).getText() + "");
				}
				else if (comp instanceof JRadioButton)
				{
					CompValue.add(((JRadioButton)comp).getText() + "");
				}
		
				else if (comp instanceof JComboBox)
				{			
					Object selectedValue= ((JComboBox)comp).getSelectedItem();
				
					
					if(selectedValue!=null){
						CompValue.add(selectedValue.toString());
					}			
					else 
						CompValue.add("");
				}			
				else if (comp instanceof JTable)
				{
					if(((JTable)comp).getName() !=null )
						CompValue.add(((JTable)comp).getName().toString());
					else
						CompValue.add("");
				}
				else if (comp instanceof JList)
				{					
					JList JL= (JList)comp;
					ListModel JLm=JL.getModel();
					int[] JSel=JL.getSelectedIndices();	
				
					for(int i =0;i<JSel.length;i++)
					{
						CompValue.add(JLm.getElementAt(JSel[i]).toString());
					}
				}
				
				else if (comp instanceof JTabbedPane)
				{
					JTabbedPane jtp=((JTabbedPane)comp);
					int i=jtp.getSelectedIndex();
					CompValue.add(jtp.getTitleAt(i));
				}
				
				else if (comp instanceof JTree)
				{
					Object node=((JTree) comp).getLastSelectedPathComponent();			
					if (node != null)
						CompValue.add(node.toString());
					else 
						CompValue.add("");
					return CompValue;
				}

				else if (comp instanceof JInternalFrame)
				{
					CompValue.add(((JInternalFrame)comp).getTitle()+"");
				}	
				else 
					CompValue.add("");
				//TODO: continue for all other comp types
					
				return CompValue;
			}
			
			catch(Exception e)
			{
				CompValue.add(e.getMessage());
				return CompValue;
			}
			
		}
	
		
		public String GetComponentSwingClass(Component c) 
		{	
			//TODO: find a generic way to return base swing class for all component types
			
			if (c instanceof JTextField) return "javax.swing.JTextField";
			if (c instanceof JTextArea) return "javax.swing.JTextArea";
			if (c instanceof JTextPane) return "javax.swing.JTextPane";
			if (c instanceof JLabel) return "javax.swing.JLabel";
			if (c instanceof JButton) return "javax.swing.JButton";
			if (c instanceof JTree) return "javax.swing.JTree";
			
			//TODO: add all the rest
			
			return c.getClass().getName();		
		}
		
		
	
	
	
    public List<String> GetAllWindows()    
    {    	
    	List<String> Titlelist = new ArrayList<String>();   	
    	Titlelist = GetWindowTitle();
    	return Titlelist;
    }   

    
	
	public static Window[] GetAllWindowsByReflection()//catch also blocking window(Security, Update...)
    {
    	List<Window> list = new ArrayList<Window>();   
    	Class<Window> cls = Window.class;
		IdentityArrayList<Window>  AllWindows = new IdentityArrayList<Window>();
		Method mGetAllWindows;
		try {			 
			mGetAllWindows = cls.getDeclaredMethod("getAllWindows",null);
			mGetAllWindows.setAccessible(true);
			AllWindows = (IdentityArrayList<Window>) mGetAllWindows.invoke(null);
			for (Object wnd : AllWindows.toArray()) {
				if (wnd instanceof Window)
				{
					list.add((Window)wnd);
				}
			}
		} catch (Exception err) {
			// TODO Auto-generated catch block
			err.printStackTrace();
		}
		return list.toArray(new Window[list.size()]);
    }
	public List<String> GetWindowTitle() {
    	GingerAgent.WriteLog("GetWindowTitle");
    	List<String> list = new ArrayList<String>();    
    	Window[] windows1 = GetAllWindowsByReflection();

		//TODO: Add handle for applet which do not have Title = "" and ignored below

    	for (Window a : windows1 )
		{
			GingerAgent.WriteLog("Window Name=" + a.getName());		    	
	    	if (a.isShowing())
    		{    
	    		String winTitleToAdd="";
	    		if (a instanceof JFrame)	
	    		{
	    			winTitleToAdd= ((JFrame)a).getTitle();
	    		}    		
	    		if (a instanceof JDialog)
	    		{
	    			winTitleToAdd= ((JDialog)a).getTitle();
	    		}	    			 
	    		
	    		//TODO: FIXNME For applet there is no title
				// if no title found show name
				// ((JWindow)a).
	    		//if (a instanceof JWindow)
	    		//{
	    		//	winTitleToAdd= ((JWindow)a).getName();
	    		//}
	    			    		
	    		if (winTitleToAdd!="" && winTitleToAdd!=null)
	    		{
	    			// Ignore our own Ginger Console
	    			if (winTitleToAdd.equals(GingerAgentFrame.GINGER_AGENT_CONSOLE) )
	    			{
	    				continue;
	    			}
	    			GingerAgent.WriteLog("Window Title:" + winTitleToAdd);
	    			if (list.contains(winTitleToAdd))
	    			{
	    				//find out how many same windows
	    				int num= GetNumberOfDuplications(list, winTitleToAdd);
	    				list.add(winTitleToAdd + "[Index:"+(num+1)+"]");
	    			}
	    			else
	    				list.add(winTitleToAdd);
	    		}	    		
	    		else
	    		{
	    			// for applet there is no name
	    			winTitleToAdd = a.getName();
					if (a instanceof JDialog)
		    		{
		    			winTitleToAdd= ((JDialog)a).getTitle();
		    			// START- Added to identify windows with no title - 17/04/2017
		    			if (winTitleToAdd == null || winTitleToAdd == "")
		    			{
		    				int num= GetNumberOfDuplications(list, winTitleToAdd);
		    				winTitleToAdd = "NoTitleDialog[Index:"+ (num+1) +"]";
		    			}
		    			GingerAgent.WriteLog("Title name = " +winTitleToAdd );
		    			// END- Added to identify windows with no title - 17/04/2017
		    		}	
	    			if (a instanceof JFrame)
		    		{
		    			winTitleToAdd= ((JFrame)a).getTitle();
		    			// START- Added to identify windows with no title - 17/04/2017
		    			if (winTitleToAdd == null || winTitleToAdd == "")
		    			{
		    				int num= GetNumberOfDuplications(list, winTitleToAdd);
		    				winTitleToAdd = "NoTitleFrame[Index:"+ (num+1) +"]";
		    			}
		    			GingerAgent.WriteLog("Title name = " +winTitleToAdd );
		    			// END- Added to identify windows with no title - 17/04/2017
		    		}	
	    			list.add(winTitleToAdd);
	    		}
	    	}    
      }
    	return list;
    }   
	
	public Component GetParentBrowser(Component c)
	{
		Component parent = c.getParent();
		while(parent!=null || parent!=CurrentWindow)
		{
			String elementClass = parent.getClass().getName();
			if(elementClass !=null && elementClass.contains("uif.widgets.browser"))
			{
				return parent;
			}
			parent=parent.getParent();
		}
		return null;
	}
    
    public int GetNumberOfDuplications(List<String> list,String WinTilte){
    	int num = 0;
		
    	for(Object item: list){
    		
    		String str = item.toString();
    		if (str.contains("[Index:"))
    		{
    		  int i = str.indexOf("[Index:");
    		  str = str.substring(0, i);
    		}
    		if (str.equals(WinTilte))
    		{
    			num++;	
    		}
    	}
    	return num;
    }  
    
	public static List<Component> getAllComponents(final Container c) {
	    Component[] comps = c.getComponents();
	    List<Component> compList = new ArrayList<Component>();
	    for (Component comp : comps) {
	        compList.add(comp);
	        if (comp instanceof Container)
	            compList.addAll(getAllComponents((Container) comp));
	    }	   
	    return compList;
	}

	public String GetComponentXPath(Component comp) {
		
		return mXPathHelper.GetComponentXPath(CurrentWindow,comp);
	}

	public Component GetComponentByXPath(String componentXPath) {
		// TODO Auto-generated method stub
		return mXPathHelper.GetComponentByXPath(CurrentWindow, componentXPath);
	}
    
	public static boolean isModalDialogShowing()
	{
	    Window[] windows = GetAllWindowsByReflection(); 
	    if( windows != null ) { 
	        for( Window w : windows ) {
	            if( w.isShowing() && w instanceof Dialog && ((Dialog)w).isModal() )
	                return true;	            
	        }
	    }
	    return false;
	}
	
	public Boolean isWindowExist(String LocateBy,String LocateValue)
	{
		Window[] sWindow = SwingHelper.GetAllWindowsByReflection(); 
		String title = null;
		int i = 0;
		
		for (Window a : sWindow )
		{	
			GingerAgent.WriteLog("window :" +a.getName());
			if (a instanceof JFrame)
			{
				title = ((JFrame)a).getTitle();
				// START- Added to identify windows with no title - 17/04/2017
				if (title == "" || title == null)
	    		{
	    			title = "NoTitleFrame [Index:"+ (i+1) +"]";	    			
	    		}
				// END- Added to identify windows with no title - 17/04/2017
				GingerAgent.WriteLog("JFrame title :" +title);
				if(title.contains(LocateValue) && a.isVisible() && a.isEnabled())
				{
					return true;
				}
			}
			if (a instanceof JDialog)
			{
				title = ((JDialog)a).getTitle();
				// START- Added to identify windows with no title - 17/04/2017
	    		if (title == "" || title == null)
	    		{
	    			title = "NoTitleDialog [Index:"+ (i+1) +"]";	    			
	    		}
	    		// END- Added to identify windows with no title - 17/04/2017
				GingerAgent.WriteLog("JDialog title :" +title);
				if(title.contains(LocateValue) && a.isVisible() && a.isEnabled())
				{
					return true;
				}
			}
		}
		return false;
	}
	
	public boolean SwitchWindow(String Title) 
    {  	    	    
    	Window[] windows2 =SwingHelper.GetAllWindowsByReflection();   

    	String actualTitle = "";
		int actualIndex=0;
    	int numOfFoundWins=0;  
    	
    	//Get actual window title and index
    	if (Title.contains("[Index:"))
    	{

    		  int i = Title.indexOf("[Index:");
    		  actualTitle = Title.substring(0, i);
    		  int j = Title.indexOf("]",i);
    		  actualIndex=Integer.parseInt(Title.substring(i+7, j));    		
    	}
    	else
    	{
    		actualTitle= Title;
    		actualIndex=1;
    	}    	
   
    	for(Window w:windows2)
    	{    		
    		String Wtitle="";
    		if (w instanceof JDialog && w.isShowing() == true )
    		{
    			Wtitle = ((JDialog) w).getTitle(); 
    			// START- Added to identify windows with no title - 17/04/2017
    			if (Wtitle == "" || Wtitle == null)
    			{
    				Wtitle = "NoTitleDialog";
    			}
    			
    		}
			else if (w instanceof JFrame)
    		{
				Wtitle = ((JFrame) w).getTitle();
				if (Wtitle == "" || Wtitle == null)
    			{
    				Wtitle = "NoTitleFrame";
    			}
				// END- Added to identify windows with no title - 17/04/2017
    		}		
    		
    		//checking if it the same
    		if(Wtitle.equals(actualTitle))
    		{
    			numOfFoundWins++;
    			if(actualIndex == numOfFoundWins)
    			{
    				CurrentWindow = w;
    				return true;
    			}
    		}    			
    	}
    	//if  user specify partial window text the find window using contains
    	for(Window w:windows2)
    	{
    		String Wtitle="";
    		if (w instanceof JDialog && w.isShowing() == true )
    		{
    			Wtitle = ((JDialog) w).getTitle(); 
    			if (Wtitle == "" || Wtitle == null)
    			{
    				Wtitle = "NoTitleDialog";
    			}
    		}
			else if (w instanceof JFrame)
    		{
				Wtitle = ((JFrame) w).getTitle();
				if (Wtitle == "" || Wtitle == null)
    			{
    				Wtitle = "NoTitleFrame";
    			}
    		}			
    		
    		//checking if it the same
    		if(Wtitle.contains(actualTitle))
    		{  
    			CurrentWindow = w;
    			return true;    			
    		}    
    	}
    	
		//temp solution to be able to see it in Win Explorer
    	// for applets we might have only name
    	for(Window w:windows2)
    	{
    		if (Title.equals(w.getName()))
			{
    			CurrentWindow = w;
    			return true;    	
			}
    	
    	}    	
    	return false;
    }
	
	public String winClick(Component c,String value)
	{
		if(CurrentWindow instanceof JFrame)				
		{
			((JFrame)CurrentWindow).setExtendedState(Frame.MAXIMIZED_BOTH);
			((JFrame)CurrentWindow).requestFocus();
		}
		else if(CurrentWindow instanceof JDialog)
		{					
			((JDialog)CurrentWindow).requestFocus();
		}
		
		if (c instanceof JButton)						
		{
			try {	
			
			
						
				((JButton)c).requestFocus();
				((JButton)c).scrollRectToVisible(((JButton)c).getBounds());
				Thread.sleep(500);
			     Point pos = ((JButton)c).getLocationOnScreen();
			     Dimension size = ((JButton)c).getSize();
			     pos.x += (size.width / 2);
			     pos.y += (size.height / 2);		
			     
		         Robot bot = new Robot();
		         bot.mouseMove(pos.x, pos.y);
		         bot.mousePress(InputEvent.BUTTON1_MASK);
		         Thread.sleep(500);
		         bot.mouseRelease(InputEvent.BUTTON1_MASK);
		     } catch (Exception ex) {
		    	 ex.printStackTrace();
		     	 return ex.getMessage(); 
		     }
		}
		if (c instanceof JTree)						
		{
			try {			
				
				
				
				((JTree)c).requestFocus();				
				Thread.sleep(500);
				
				TreePath p = null;
				String[] nodes = value.split("/");
				Point pos = ((JTree)c).getLocationOnScreen();
				
				for (String node : nodes) {
					int row = (p == null ? 0 : ((JTree)c).getRowForPath(p));
					((JTree)c).expandRow(row);
					p = ((JTree)c).getNextMatch(node.trim(), row, Position.Bias.Forward);
				}				
			     Rectangle rect = ((JTree)c).getPathBounds(p);
			     ((JTree)c).scrollPathToVisible(p);
			   
			     int x=rect.x+(rect.width / 2);
			     int y=rect.y+(rect.height / 2);	
			     
			     pos.x += x;
			     pos.y += y;
			     
			     GingerAgent.WriteLog("Win X:" + pos.x);
			     GingerAgent.WriteLog("Win Y:" + pos.y);
		         Robot bot = new Robot();
		         bot.mouseMove(pos.x, pos.y);
		         bot.mousePress(InputEvent.BUTTON1_MASK);
		         Thread.sleep(500);
		         bot.mouseRelease(InputEvent.BUTTON1_MASK);
		     } catch (Exception ex) {
		    	 ex.printStackTrace();
		     	 return ex.getMessage(); 
		     }
		}
		
	     return "";
	}
	public String winDoubleClick (Component c,String value)
	{
		if(CurrentWindow instanceof JFrame)				
		{
			((JFrame)CurrentWindow).setExtendedState(Frame.MAXIMIZED_BOTH);
			((JFrame)CurrentWindow).requestFocus();
		}
		if (c instanceof JTree)						
		{
			try {					
				((JTree)c).requestFocus();				
				Thread.sleep(500);
				
				TreePath p = null;
				String[] nodes = value.split("/");
				Point pos = ((JTree)c).getLocationOnScreen();
				
				for (String node : nodes) {
					int row = (p == null ? 0 : ((JTree)c).getRowForPath(p));
					((JTree)c).expandRow(row);
					p = ((JTree)c).getNextMatch(node.trim(), row, Position.Bias.Forward);
				}				
			     Rectangle rect = ((JTree)c).getPathBounds(p);
			     ((JTree)c).scrollPathToVisible(p);
			   
			     int x=rect.x+(rect.width / 2);
			     int y=rect.y+(rect.height / 2);	
			     
			     pos.x += x;
			     pos.y += y;
			     
			     GingerAgent.WriteLog("Win X:" + pos.x);
			     GingerAgent.WriteLog("Win Y:" + pos.y);
		         Robot bot = new Robot();
		         bot.mouseMove(pos.x, pos.y);
		         bot.mousePress(InputEvent.BUTTON1_MASK);
		         Thread.sleep(500);
		         bot.mouseRelease(InputEvent.BUTTON1_MASK);
		         
		         bot.mousePress(InputEvent.BUTTON1_MASK);
		         Thread.sleep(500);
		         bot.mouseRelease(InputEvent.BUTTON1_MASK);
		         
		     } catch (Exception ex) {
		    	 ex.printStackTrace();
		     	 return ex.getMessage(); 
		     }
		}
		
	     return "";
	}	
	

	
	
}
