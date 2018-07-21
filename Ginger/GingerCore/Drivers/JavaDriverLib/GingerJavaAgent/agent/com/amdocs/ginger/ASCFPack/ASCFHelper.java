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

package com.amdocs.ginger.ASCFPack;

import java.awt.Component;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;
import javax.swing.ComboBoxModel;
import javax.swing.JComboBox;
import javax.swing.JTextArea;
import javax.swing.JTextField;
import javax.swing.JTextPane;
import com.amdocs.ginger.GingerAgent;
import com.amdocs.ginger.PayLoad;

public class ASCFHelper {
	
	public PayLoad setUIFTextField(Component c, String value) 
	{				
		JTextField tf = (JTextField)c;
		tf.setText(value);
		
		// Special for UIF we need to mark it modified, otherwise the field value will not go to the server
		setModfied(c);
		startEditComplete(c); 
		return PayLoad.OK("UifTextBox value set to "+ value);
	}
	
	public PayLoad setUIFTextArea(Component c, String value) 
	{				
		JTextArea ta = (JTextArea)c;
		ta.setText(value);
		
		// Special for UIF we need to mark it modified, otherwise the field value will not go to the server
		setModfied(c);
		startEditComplete(c); 
		return PayLoad.OK("UifTextBox value set to "+ value);
	}
	
	public PayLoad setUIFTextPane(Component c, String value) 
	{				
		JTextPane tp = (JTextPane)c;
		tp.setText(value);
		
		// Special for UIF we need to mark it modified, otherwise the field value will not go to the server
		setModfied(c);
		startEditComplete(c); 
		return PayLoad.OK("UifTextBox value set to "+ value);
	}
	public Boolean validateDateTimeValue(String value)
	{
	    SimpleDateFormat dateFormat = new SimpleDateFormat("MM/dd/yyyy hh:mm:ss a");
	    dateFormat.setLenient(false);
	    try {
	      dateFormat.parse(value.trim());
	    } catch (ParseException pe) {
	    	GingerAgent.WriteLog("Exception Details = "+ pe.getMessage());
			pe.printStackTrace();
			return false;
	    }
	    return true;	  
	}
	public String getHTMLContent(Component c)
	{
		try 
		{
			GingerAgent.WriteLog("Inside getText");
			Class DumpME = c.getClass().getEnclosingClass();
			GingerAgent.WriteLog("Dump Class Method: " + DumpME.getName());
			Field field = c.getClass().getDeclaredField("this$0");
		    field.setAccessible(true);
		    
		    Object o = field.get(c);
			
			Method m = DumpME.getMethod("getContent");
			m.setAccessible(true);	
			return (String) m.invoke(o);
		}		
		catch (Exception e) 
		{
			GingerAgent.WriteLog("Exception Details = "+ e.getMessage());
			e.printStackTrace();
			return null;
		}	
	}
	public String getText(Component c)
	{
		try 
		{
			GingerAgent.WriteLog("Inside getText");
			Class DumpME = c.getClass().getEnclosingClass();
			GingerAgent.WriteLog("Dump Class Method: " + DumpME.getName());
			Field field = c.getClass().getDeclaredField("this$0");
		    field.setAccessible(true);
		    
		    Object o = field.get(c);
			
			Method m = DumpME.getMethod("getDisplayText");
			m.setAccessible(true);	
			return (String) m.invoke(o);
		}		
		catch (Exception e) 
		{
			GingerAgent.WriteLog("Exception Details = "+ e.getMessage());
			e.printStackTrace();
			return null;
		}	
	}
	public String getDateTimeValue(Component comp) 
	{
		GingerAgent.WriteLog("getUIFDatePickerValue : ");
		Object obj = getSelectedDate(comp);
		GingerAgent.WriteLog("obj : " + obj);
		SimpleDateFormat dateFormat = new SimpleDateFormat("MM/dd/yyyy HH:mm:ss a");
		String dateValue = dateFormat.format(obj);
		GingerAgent.WriteLog("dateValue : " + dateValue);
		return dateValue;
	}
	public String getUIFDatePickerValue(Component comp) 
	{
		GingerAgent.WriteLog("getUIFDatePickerValue : ");
		Object obj = getSelectedDate(comp);
		GingerAgent.WriteLog("obj : " + obj);
		SimpleDateFormat dateFormat = new SimpleDateFormat("MM/dd/yyyy");
		String dateValue = dateFormat.format(obj);
		GingerAgent.WriteLog("dateValue : " + dateValue);
		return dateValue;
	}
	public String getUIFDateMillisecondValue(Component comp) 
	{
		GingerAgent.WriteLog("getUIFDatePickerValue : " + comp);
		String dateValue = getUIFDatePickerValue(comp);
		GingerAgent.WriteLog("I am the Value to Set: " + dateValue);
		SimpleDateFormat sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
		Date date = new Date();
		try {
			date = sdf.parse(dateValue);
		} catch (ParseException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		String milliseconds = Long.toString(date.getTime());
		GingerAgent.WriteLog("I am the Value to Set: " + milliseconds);
		return milliseconds;
	}
	public PayLoad setUIFDatePickerValue(Component c, String value) 
	{				
		GingerAgent.WriteLog("I am the Value to Set: " + value);
		
		SimpleDateFormat sdf = new SimpleDateFormat("MM/dd/yyyy hh:mm:ss a");
		Date date = new Date();
		try {
			date = sdf.parse(value);
		} catch (Exception pe) {
			GingerAgent.WriteLog("exception while parsing date"+pe.getMessage());
			return PayLoad.Error("Invalid date format. Expected format is MM/dd/yyyy hh:mm:ss a  e.g. 01/15/2017 01:20:05 AM");		   
		}
		GingerAgent.WriteLog("date"+date);
		setSelectedDate(c, date);
		
		Object o = getSelectedDate(c);
			
		if(o == null)
			return PayLoad.Error("Not able to set the value");
		
		String CurrentSelectedDate=o.toString().substring(0, 11) + o.toString().substring(20);
		String ExpectedDate=date.toString().substring(0, 11) + o.toString().substring(20);
		
		if(!CurrentSelectedDate.equalsIgnoreCase(ExpectedDate))
			return PayLoad.Error("Current Selected Value::" + CurrentSelectedDate + " - Expected Value::" + ExpectedDate);
		
		// Special for UIF we need to mark it modified, otherwise the field value will not go to the server
		setModfied(c);
		startEditComplete(c);
		return PayLoad.OK("UIFDatePicker value set to..." + value);
	}
	
	public void startEditComplete(Component c) 
	{				
			try {
				//Get the EnclosingClass obj
				Class DumpME = c.getClass().getEnclosingClass();
				
				Field field = c.getClass().getDeclaredField("this$0");
			    field.setAccessible(true);

			    Object o = field.get(c);
			    System.out.println("Obj=" + o.getClass().getName() );
				
				// Get the setModified method using reflection
				Method method = DumpME.getMethod("startEditComplete");
				method.setAccessible(true);
				
				// Invoke setModified with true
				// This is instead of writing something like: (UifTextField)c.setModified(true);  - which will need ref to Uif				 
				
			    method.invoke(o);		
			}		
			catch (Exception e) 
			{
				// TODO Auto-generated catch block
				e.printStackTrace();
			}		    
	}
	public boolean checkIsMandatory(Component c)
	{
		try
		{
//			Field field = c.getClass().getDeclaredField("this$0");
//		    field.setAccessible(true);
//		    Object o = field.get(c);
//		    Field fs = o.getClass().getSuperclass().getSuperclass().getSuperclass().getSuperclass().getSuperclass().getDeclaredField("required");
//		    fs.setAccessible(true);//		    
//		    return ((Boolean) fs.get(o)).booleanValue();		
			
			Field field = c.getClass().getDeclaredField("this$0");
		    field.setAccessible(true);
		    Object o = field.get(c);
			Class cls = o.getClass();
			
			while (cls.getSuperclass() != null)
			{	
				for (Field m : cls.getDeclaredFields())
				{
					if (m.getName().equalsIgnoreCase("required"))
					{
						Field requiredField = cls.getDeclaredField("required");
						requiredField.setAccessible(true);
						return ((Boolean) requiredField.get(o)).booleanValue();
					}
					
				}
				cls = cls.getSuperclass();
			}		
			return false;
					
		}
		catch (Exception e)
		{
			GingerAgent.WriteLog("Exception Details = "+ e.getMessage());
			e.printStackTrace();
		}
		return false;
	}
	public boolean performUIFDoClick(Component c) 
	{	
			try 
			{
				    GingerAgent.WriteLog("Inside performUIFDoClick");
					Class DumpME = c.getClass().getEnclosingClass();
					GingerAgent.WriteLog("Dump Class Method: " + DumpME.getName() );					
					Method[] method = DumpME.getDeclaredMethods();	
					//Get the EnclosingClass object
					Field field = c.getClass().getDeclaredField("this$0");
				    field.setAccessible(true);
				    
				    Object o = field.get(c);
				    System.out.println("Obj = " + o.getClass().getName());
					
					Method m = DumpME.getMethod("doClick");
					m.setAccessible(true);						
					m.invoke(o);
					return true;
			}		
			catch (Exception e) 
			{
				GingerAgent.WriteLog("Exception Details = "+ e.getMessage());
				e.printStackTrace();
			}
			return false;	
	}
	
	public void setSelectedValue(Component c, String value) 
	{				
			try 
			{
				// Using Reflection we can call method on the component with adding import/reference to the library
				// This enable us to compile without UIF and to be less version sensitive
				
				GingerAgent.WriteLog("setSelectedValue " + c.getName());		
				Class DumpME = c.getClass().getEnclosingClass();
				System.out.println("Dump Class Method: " + DumpME.getName() );
				Method[] methods = DumpME.getMethods(); 
				for (Method m : methods)
				{
					GingerAgent.WriteLog("Method: " +  m.getName());
				}				

				//Get the EnclosingClass obj
				Field field = c.getClass().getDeclaredField("this$0");
			    field.setAccessible(true);
			    Object o = field.get(c);
			    System.out.println("Obj=" + o.getClass().getName() );
				
				// Get the setModified method using reflection
				Method method = DumpME.getMethod("setSelectedValue", String.class);
				method.setAccessible(true);
				
				// Invoke setModified with true
				// This is instead of writing something like: (UifTextField)c.setModified(true);  - which will need ref to Uif	
			    method.invoke(o, value);		   
			}		
			catch (Exception e) 
			{
				// TODO Auto-generated catch block
				e.printStackTrace();
			}		
	}
	
	public void setText(Component c,String value) 
	{				
			try 
			{
				// Using Reflection we can call method on the component with adding import/reference to the library
				// This enable us to compile without UIF and to be less version sensitive
				
				GingerAgent.WriteLog("setText " + c.getName());
				Class DumpME = c.getClass();
				System.out.println("Dump Class Method: " + DumpME.getName() );
				Method[] methods = DumpME.getMethods(); 
				for (Method m : methods)
				{
					GingerAgent.WriteLog("Method: " +  m.getName());
				}							
				Method method = DumpME.getMethod("setText", String.class);			
				method.setAccessible(true);				
			    method.invoke(c, value);
			}		
			catch (Exception e) 
			{
				// TODO Auto-generated catch block
				GingerAgent.WriteLog("Exception in set text for edge"+e.getMessage());
				e.printStackTrace();
			}		    
	}
	
	public void setSelectedItemModel(JComboBox cb, ComboBoxModel c, Object value)
	{
		try 
		{
			Class DumpME = c.getClass();		
			GingerAgent.WriteLog("Enclosing class"+DumpME.getName());			
			Method method = DumpME.getMethod("setSelectedItem", Object.class);			
			method.setAccessible(true);			 	
		    method.invoke(c, value);
		    GingerAgent.WriteLog("After invoke Method setSelectedItem***************");	    
		}		
		catch (Exception e) 
		{
			// TODO Auto-generated catch block
			GingerAgent.WriteLog("Exception in setSelectedItem for combo"+e.getMessage()+""+e.getStackTrace().toString());			
			e.printStackTrace();
		}	    
	}	

	public void setSelectedDate(Component c, Date value) 
	{				
			try 
			{
				// Using Reflection we can call method on the component with adding import/reference to the library
				// This enable us to compile without UIF and to be less version sensitive			
				GingerAgent.WriteLog("setDate " + c.getName());
				GingerAgent.WriteLog("value:: " + value);
				Class DumpME = c.getClass().getSuperclass().getSuperclass().getSuperclass();
				GingerAgent.WriteLog("Dump Class Method: " + DumpME.getName() );
			 	
				// Get the setModified method using reflection
				Method method = DumpME.getMethod("setSelectedDate",Date.class); 				
				method.setAccessible(true);					
				method.invoke(c,value);				
			}		
			catch (Exception e) 
			{
				// TODO Auto-generated catch block
				e.printStackTrace();
			}			    		
	}
	
	public void activateTreeNode(Object c) 
	{				
			try 
			{
				// Using Reflection we can call method on the component with adding import/reference to the library
				Class DumpME = c.getClass();
				GingerAgent.WriteLog("Dump Class Method: " + DumpME.getSuperclass().getName() );
				
				Class[] sad = DumpME.getInterfaces();
				for(Class cls : sad)
				{
					if(cls.getName().contains("UifTreeNode"))
					{
						DumpME=cls;
						break;
					}
					GingerAgent.WriteLog("cls Class getPath: " + cls.getName() );
				}
				GingerAgent.WriteLog("cls Class DumpME: " + DumpME.getName() );
				Method[] methods = DumpME.getMethods(); 
				for (Method m : methods)
				{
					GingerAgent.WriteLog("Method: " +  m.getName());
				}
				
				Method method=c.getClass().getMethod("getTemplate");
				method.setAccessible(true);	
				c=method.invoke(c);
				GingerAgent.WriteLog("Dump Class getTemplate: " + c.getClass().getName() );
				method=c.getClass().getMethod("getNodeControl");
				method.setAccessible(true);	
				c=method.invoke(c);
				GingerAgent.WriteLog("Dump Class getNodeControl: " + c.getClass().getName() );								
				method=c.getClass().getMethod("getActivationAction");
				method.setAccessible(true);	
				c=method.invoke(c);				
				method = c.getClass().getMethod("execute");
				method.setAccessible(true);	
				method.invoke(c);
			}		
			catch (Exception e) 
			{
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			    		
	}
	public void activateTableRow(Object table,int row,int col) 
	{		
			try 
			{
				GingerAgent.WriteLog("Dump Class Method: " + table.getClass().getName() );
				Method method=table.getClass().getMethod("getModel");
				method.setAccessible(true);	
				
				Object model=method.invoke(table);
				GingerAgent.WriteLog("model Class Method: " + model.getClass().getName() );						
				
				method=model.getClass().getMethod("getDMValueAt",int.class);
				
				method.setAccessible(true);	
				Object activeRowDM =method.invoke(model,row);
				
				GingerAgent.WriteLog("activeRowDM Class Method: " + activeRowDM.getClass().getName() );
				Method[] methods = table.getClass().getMethods(); 
				
				method=activeRowDM.getClass().getMethod("setEnabled",boolean.class);
				method.setAccessible(true);	
				method.invoke(activeRowDM,false);
				
				method=table.getClass().getMethod("editCellAt",int.class,int.class);
				method.setAccessible(true);	
				
				method.invoke(table,row,col);
				
				method=activeRowDM.getClass().getMethod("setEnabled",boolean.class);
				method.setAccessible(true);	
				method.invoke(activeRowDM,true);
				
				
				method=table.getClass().getMethod("getGrid");
				method.setAccessible(true);	
				Object grid=method.invoke(table);
												
				GingerAgent.WriteLog("Dump Class Method: " + grid.getClass().getName() );
				
				methods = grid.getClass().getMethods(); 
				for (Method m : methods)
				{
					if(m.getName() == "performActivationAction")
					{
						method=m;
						break;
					}	
				}
				method.setAccessible(true);	
				GingerAgent.WriteLog("activeRowDM:" +  activeRowDM.toString());	
				GingerAgent.WriteLog("method: " + method.getName());
				method.invoke(grid,activeRowDM,null,null,row);
			}		
			catch (Exception e) 
			{
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			    		
	}
	public Object getSelectedDate(Component c) 
	{				
			try 
			{
				// Using Reflection we can call method on the component with adding import/reference to the library
				// This enable us to compile without UIF and to be less version sensitive			
				GingerAgent.WriteLog("setDate " + c.getName());				
				Class DumpME = c.getClass().getSuperclass().getSuperclass().getSuperclass();
								
				// Get the setModified method using reflection
				Method method = DumpME.getMethod("getSelectedDate"); 
				method.setAccessible(true);					
				return method.invoke(c);				
			}		
			catch (Exception e) 
			{
				// TODO Auto-generated catch block
				e.printStackTrace();
				return null;
			}					    		
	}
	public void setModfied(Component c) 
	{				
			try 
			{
				// Using Reflection we can call method on the component with adding import/reference to the library
				// This enable us to compile without UIF and to be less version sensitive
				
				Class DumpME = c.getClass().getEnclosingClass();
				//Get the EnclosingClass obj
				Field field = c.getClass().getDeclaredField("this$0");
			    field.setAccessible(true);

			    Object o = field.get(c);
			    System.out.println("Obj=" + o.getClass().getName());
				
				// Get the setModified method using reflection
				Method method = DumpME.getMethod("setModified", boolean.class);
				method.setAccessible(true);
				
				// Invoke setModified with true
				// This is instead of writing something like: (UifTextField)c.setModified(true);  - which will need ref to Uif			 
			    method.invoke(o, true);		   
			}		
			catch (Exception e) 
			{
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
	}
	
	public boolean selectUIFTab(final Component c) {
		boolean status = false;	
		try {
			Field nameField = c.getClass().getDeclaredField("this$0");
			nameField.setAccessible(true);
			Object o = nameField.get(c);
			Field parentField = o.getClass().getDeclaredField("parentTab");
			parentField.setAccessible(true);
			Object obj = parentField.get(o);
			Method method = obj.getClass().getMethod("setSelectedTabPage",
					String.class);
			method.setAccessible(true);
			method.invoke(obj, c.getName());
			status = true;
		} catch (Exception e) {
			GingerAgent.WriteLog("Exception Details  = " + e.getMessage());
			e.printStackTrace();
		}
		return status;		
	}	
}
