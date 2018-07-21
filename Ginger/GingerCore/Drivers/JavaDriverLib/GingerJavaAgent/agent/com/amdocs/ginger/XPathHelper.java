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
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.Map.Entry;

public class XPathHelper 
{
	SwingHelper mSwingHelper;
	public XPathHelper(SwingHelper swingHelper) {
		
		mSwingHelper = swingHelper;
	}
	
	public Component GetComponentByXPath(Container currentWindow,String componentXPath) 
	{
		Container CurrentWindow=currentWindow;
		Container container =CurrentWindow;
		String [] pathNodes=componentXPath.split("/");
		GingerAgent.WriteLog(pathNodes.toString());
		for(String node: pathNodes)
		{	
			if(node.isEmpty()) continue;
			else if(node.equals(CurrentWindow.getName())) continue;
			else if(node.startsWith("[["))
			{
				container=(Container)GetNodeComponent(container,"ByMultiProperty",node);
			}
			else if(node.contains("Name:"))
			{
				container=(Container)GetNodeComponent(container,"ByNameIndex",node);
			}
			else if(node.contains("["))
			{
				container=(Container)GetNodeComponent(container,"ByClassIndex",node);
			}
			else
			{
				container=(Container)GetNodeComponent(container,"ByName",node);
			}	
			
			if(container==null)			
				return null;			
		}
		GingerAgent.WriteLog(container.getName());
		return container;		
		
	}
	
	private Component GetNodeComponent(Container c,String Locator, String LocateValue)
	{		
		Component [] childComponents=c.getComponents();	
		if(Locator.equals("ByName"))
		{
			for(Component child:childComponents)
			{
				if(child.isVisible() && LocateValue.equals(child.getName()))
				{
					return child;
				}
			}
		}
		else if(Locator.equals("ByMultiProperty"))
		{
		
			HashMap<String, String> propertyValueList= GetPropertyValueList(LocateValue);
			GingerAgent.WriteLog("propertyValueList::" + propertyValueList.toString());			
			for(Component childComp:childComponents)
			{				
		    	if (childComp.isVisible()&& childComp.getName()!=null && MatchAllProperties(propertyValueList,childComp) )
		    	{
		    		return childComp;		
				}	
			}						
			
		}
		else if(Locator.equals("ByClassIndex"))
		{			
			int i=LocateValue.indexOf('[');
			
			String className=LocateValue.substring(0, i);
			int index= Integer.parseInt(LocateValue.substring(i+1, LocateValue.indexOf(']')));
			int currentIndex=0;
			for(Component childComp:childComponents)
			{	
		    	if (childComp.isVisible()&& childComp.getName()==null && childComp.getClass().getName().equals(className))
				{	
		    		if(currentIndex==index) return childComp;
		    		
		    		else currentIndex++;		
				}	
			}			
		}	
		else if(Locator.equals("ByNameIndex"))
		{
			int i=LocateValue.indexOf('[');
			int j = LocateValue.indexOf(':');
			String Name=LocateValue.substring(j+1, i);
			int index= Integer.parseInt(LocateValue.substring(i+1, LocateValue.indexOf(']')));
			int currentIndex=0;
			for(Component childComp:childComponents)
			{				
		    	if (childComp.isVisible()&& childComp.getName()!=null && childComp.getName().equals(Name))
				{	
		    		if(currentIndex==index)
					{
						return childComp;
					}
		    		else currentIndex++;		
				}	
			}
		}
		List<Component> childComponentsAll=SwingHelper.getAllComponents(c);
				
		if(Locator.equals("ByName"))
		{
			for(Component child:childComponentsAll)
			{				
				if(child.isVisible() && LocateValue.equals(child.getName()))
				{
					return child;
				}
			}
		}
		else if(Locator.equals("ByMultiProperty"))
		{
		
			HashMap<String, String> propertyValueList= GetPropertyValueList(LocateValue);
			GingerAgent.WriteLog("propertyValueList::" + propertyValueList.toString());			
			for(Component childComp:childComponentsAll)
			{				
				GingerAgent.WriteLog("isVisible::" + childComp.isVisible());
				GingerAgent.WriteLog("getName::" + childComp.getName());
				GingerAgent.WriteLog("class::" + childComp.getClass().getName());
		    	if (childComp.isVisible()&& childComp.getName()!=null && MatchAllProperties(propertyValueList,childComp) )
		    	{
		    		return childComp;		
				}	
			}						
			
		}
		else if(Locator.equals("ByClassIndex"))
		{			
			int i=LocateValue.indexOf('[');
			
			String className=LocateValue.substring(0, i);
			int index= Character.getNumericValue(LocateValue.charAt(i+1));
			int currentIndex=0;
			for(Component childComp:childComponentsAll)
			{				
		    	if (childComp.isVisible()&& childComp.getName()==null && childComp.getClass().getName().equals(className))
				{	
		    		if(currentIndex==index) return childComp;
		    		
		    		else currentIndex++;			
				}	
			}			
		}	
		
		GingerAgent.WriteLog("Component Node  Not Found: "+ Locator + " - " + LocateValue);
		return null;
	}
	
	private HashMap<String, String> GetPropertyValueList(String LocateValue)
	{
		//If Xpath Node contains multiple properties like [[Name:A][ClassName:B]] 
		//then this function will return property and value list
		
		if(LocateValue.contains("[["))
		{
			LocateValue=LocateValue.replace("[[", "");
		}
		if(LocateValue.contains("["))
		{
			LocateValue=LocateValue.replace("[", "");
		}
		if(LocateValue.contains("]]"))
		{
			LocateValue=LocateValue.replace("]]", "");
		}
	
		String[] subNodes= LocateValue.split("]");	

		
		HashMap<String, String> propertyValueList=new HashMap<String, String>();
			
		for(String subNode: subNodes)
		{
			int i= subNode.indexOf(":");
			
			String propertyName=(String) subNode.subSequence(0, i);
			String propertyValue=(String) subNode.subSequence(i+1, subNode.length());		
			propertyValueList.put(propertyName, propertyValue);
		}
		return propertyValueList;
	}
	
	private boolean MatchAllProperties(HashMap<String, String> propertyValueList, Component c)
	{
		//This will return if the component matches all the properties on the propertylist
		boolean matchAllFlag=true;
		
		String actualValue=null;
		String expectedValue=null;
		
		Set<Map.Entry<String, String>> entrySet1 = propertyValueList.entrySet();
		Iterator<Entry<String, String>> entrySetIterator = entrySet1.iterator();

		while (entrySetIterator.hasNext()) 
		{	
			   Entry entry = entrySetIterator.next();
			   
			   actualValue=mSwingHelper.GetPropertyValue(c,entry.getKey().toString());			   
			   expectedValue= entry.getValue().toString();
			   
			   if(!(expectedValue.equals(actualValue)))
			   {
				   matchAllFlag=false;
				   break;
			   }
			}		
		return matchAllFlag;
	}
	
	
	
	
	
	// Staring with simple XPath - need to handle array and comp without names
	public String GetComponentXPath(Component CurrentWindow, Component comp) 
	{		
		String XPath = "";
		int index=0;
		
		if(comp.getName()==null)
		{
			index=GetClassComponentIndex(comp);
			String CompName = comp.getClass().getName() +  "["+index+"]";  
			XPath =  "/" + CompName +  XPath;
		}
		else if (ComponentWithDuplicateName(comp))
		{
				String value= mSwingHelper.GetCompValue(comp).get(0); 
        		if ( value!= null &&
        				value != ""	)
				{
        			XPath="/[[Name:"+ comp.getName()+"]"+"[ClassName:"+comp.getClass().getName()+"]"+"[Value:"+value+"]"+ "]";
            	}
        		else
        		{
        			index=GetNameComponentIndex(comp);
        			String CompName = "Name:"+comp.getName() +  "["+index+"]";  
        			XPath =  "/" + CompName +  XPath;
        		}
		}
		else
		{
			XPath="/[[Name:"+ comp.getName()+"]"+"[ClassName:"+comp.getClass().getName()+"]]";	
		}

		GingerAgent.WriteLog("GetComponentXPath XPath = "+XPath);
	
		Component parent = comp.getParent(); //Start with component

		while (parent != null )
		{
			if(parent.isVisible())
			{
				String Name = parent.getName();	
				if(CurrentWindow.getName()==parent.getName())
				{
					break;
				}
				if (Name == null) 
				{
	
					index=GetClassComponentIndex(parent);
					Name = parent.getClass().getName() +  "["+index+"]"; 
				}				
				XPath =  "/" + Name +  XPath;
			}				
			parent = parent.getParent();
		}	
		return XPath;	
	  }
	
	private boolean ComponentWithDuplicateName(Component comp)
	{	
		if (comp.getName() != null && comp.getName() != "")
		{
			String currentName = comp.getName();
			
			Container parentComponent=comp.getParent();			
				Component[] childComponents = parentComponent.getComponents();		
				
				 for (Component childComp : childComponents) {
					 
					 if (!childComp.equals(comp))
					 {	
							 if (childComp.getName() != null && childComp.getName() != "")
							 {
								if (currentName == childComp.getName())
								{
									 return true;
								}
							 }
					 }
				 }
			}	
		return false;
	}
	
	private int GetClassComponentIndex(Component comp)
	{		
		int index=0;
		String className=comp.getClass().getName(); 
		
		Container parentComponent=comp.getParent();	
		
		Component[] childComponents = parentComponent.getComponents();		
				
		 for (Component childComp : childComponents) {

		    	if (childComp.isVisible()&& childComp.getName()==null && childComp.getClass().getName().equals(className))
				{	
		    		if(comp.equals(childComp)) return index;
		    		
		    		else index++;			
				}		

		    }
		
		
		return index;
	}

	private int GetNameComponentIndex(Component comp)
	{		
		int index=0;
		String Name=comp.getName(); 
		Container parentComponent=comp.getParent();	
		Component[] childComponents = parentComponent.getComponents();		
				
		 for (Component childComp : childComponents) {
		    	if (childComp.isVisible()&& 
		    		childComp.getName()!=null &&
		    		childComp.getName().equals(Name))
				{	
		    		if(comp.equals(childComp)) return index;
		    		else index++;			
				}	
	    }

    	return index;
	}
	
	
}
