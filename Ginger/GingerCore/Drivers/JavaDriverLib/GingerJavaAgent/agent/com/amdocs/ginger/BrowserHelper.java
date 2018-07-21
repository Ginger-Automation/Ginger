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
import java.lang.reflect.Method;
import java.util.List;

public class BrowserHelper
{
	 private Component mBrowser = null;
	 private String mBrowserType = null;
	 private String mBrowserXPath = null;	 

	 public BrowserHelper(Component browser) 
	 {
		 mBrowser = browser;
		 mBrowserType = browser.getClass().getName();
		 
	 }
	 public BrowserHelper(Component browser, String xpath) 
	 {
		 mBrowser = browser;
		 mBrowserType = browser.getClass().getName();
		 mBrowserXPath = xpath;
	 }
	 
	 public String getmBrowserXPath()
	 {
		 return mBrowserXPath;
	 }

	 public boolean isBrowserValid()
	 {
		 if(mBrowser!=null && mBrowser.isValid() && mBrowser.isVisible())
		 {
			 return true;			 		
		 }

		 return false;						
		
	 }
	 
	 
	 public boolean CheckIfScriptExist()
	 {
		 GingerAgent.WriteLog("Checking if script exist");
		 String CheckExistScript = "(function(){ if (typeof GingerLib !== 'undefined'){return true;} else{return false;} })();";
		 Object obj =this.ExecuteScript(CheckExistScript);
		 if (obj != null && obj.toString().equals("true"))
		 {
			 GingerAgent.WriteLog("Script exist");
			 return true;
		 }
		 GingerAgent.WriteLog("Script doest not exist");
		 return false;
	 }
		
	public PayLoad HandleBrowserElementAction(String Action, PayLoad Request) 
	{		
		// All Actions will run on the current Browser, Use Set Active Browser to set the current browser	
				
		GingerAgent.WriteLog("HandleBrowserElementAction - " + Action + " - " + Request.Name);
		
		if ("GetVisibleElements".equals(Action))
		{	
			PayLoad Response = new PayLoad("VisibleElements");
			Response.AddValue("txt");
			Response.AddValue("ZZZZ");
			return Response;
		}
		return PayLoad.Error("Unknown Browser Element Action - " + Action);		
	}

	
	public Object InvokeScript(String JavaScript)
	{
		try
		{			
			Class DumpME = mBrowser.getClass();
			GingerAgent.WriteLog("Dump Class Method: " + DumpME.getName() );
			Method[] methods = DumpME.getDeclaredMethods();			
			for (Method m : methods)
			{
				GingerAgent.WriteLog("Method: " +  m.getName());
			}
			
		     Object o = mBrowser;	     
		     GingerAgent.WriteLog("Obj=" + o.getClass().getName() );
		     
		     Method method = DumpME.getMethod("invokeScript", String.class);
		     
		     GingerAgent.WriteLog("String.class" + JavaScript);
		     method.setAccessible(true);	
		     GingerAgent.WriteLog("String.class");
		     Object oRC = method.invoke(o, JavaScript);	
		     GingerAgent.WriteLog("String.class3" + oRC.toString());
		     return oRC;
		}
		catch (Exception ex)
		{
				GingerAgent.WriteLog(ex.getMessage());
				return "ERROR - " + ex.getMessage();
		}
		
	}
	public Object ExecuteScript(String JavaScript)
	{
     	try
		{	
		     if (mBrowser.isValid() == false)
					return "ERROR - Browser not Valid";
			  Class DumpME = mBrowser.getClass();
			  Object o = mBrowser; 				  
			  GingerAgent.WriteLog("mBrowserType : " + mBrowserType);			  
			  Object objRC = null;
			  
			  if (mBrowserType.contains("JxBrowser"))
			  {
				  GingerAgent.WriteLog("Inside JxBrowserBrowserComponent");
				  
			      Method getBrowser = DumpME.getSuperclass().getMethod("getBrowser", null);
				
			      Object getBrowserObj = getBrowser.invoke(o);		      			     
			  
				  Class getBrowserClass = getBrowserObj.getClass();	
				
				  Method executeJS = getBrowserClass.getMethod("executeJavaScriptAndReturnValue", String.class);
				 
				  executeJS.setAccessible(true);	
				 
				  Object getObj = executeJS.invoke(getBrowserObj, JavaScript);	
				  Class returnValueClass= getObj.getClass();
				  Method isNullMethod = returnValueClass.getMethod("isNull");
				  isNullMethod.setAccessible(true);
				  Object objNull = isNullMethod.invoke(getObj);
				  if ((Boolean)objNull == false)
				  {
						   Method stringObj = returnValueClass.getMethod("getValue");
						   stringObj.setAccessible(true);
						   objRC = stringObj.invoke(getObj);
				  }
			  }
			  else if (mBrowserType.contains("JExplorer"))
			  {
				  GingerAgent.WriteLog("Inside JExplorerBrowserComponent");
				  Method method = DumpME.getMethod("executeScript", String.class);
				  
				  method.setAccessible(true);	
				  objRC = method.invoke(o, JavaScript);			
				 
			  }			  
			  return objRC;	 
		}		
		catch (Exception ex)
		{				
				GingerAgent.WriteLog("Exception in execute script:"+ex.getMessage());
				return "ERROR - Failed to Execute Script. Exception message = " + ex.getMessage();
		}
		
	}	
	public PayLoad getScreenShot(){
		
		if(CheckIfScriptExist())
		{
			PayLoad PL21=new PayLoad("Screenshot");			
			PayLoad resp=ExceuteJavaScriptPayLoad(PL21);		
			return resp;
		}
		else
			return PayLoad.Error("Failed to get screenshot. Ginger script lost");
		
	
	}
	public boolean isBrowserBusy()
	{
		String JavaScript ="document.readyState";
		Object obj = ExecuteScript(JavaScript);
		if (obj != null && obj.toString().equals("complete"))
			return false;
		return true;
	}

	public void InjectInitializationScripts(List<String> scripts)
	{					
			String Response="";			
			ExecuteScript(scripts.get(0));			
			GingerAgent.WriteLog("html2canvas script injected");
			
			ExecuteScript(scripts.get(1));
			GingerAgent.WriteLog("ArrayBuffer injected");	
			
			Response+=ExecuteScript(scripts.get(2));
			GingerAgent.WriteLog("PayLoad injected");
			
			Response+=ExecuteScript(scripts.get(3));
			GingerAgent.WriteLog("GingerHTMLHelper injected");
			
			Response+=ExecuteScript("define_GingerLib();");
			
			GingerAgent.WriteLog("GingerLib() defined");		
						
			PayLoad PL1=new PayLoad("AddScript");
			PL1.AddValue(scripts.get(4));
			PayLoad respPL=ExceuteJavaScriptPayLoad(PL1);
			
			if(!respPL.IsErrorPayLoad())
			{
				GingerAgent.WriteLog("GingerLibXPath Injected");				
			}		
			
			PayLoad PL2=new PayLoad("AddScript");
			PL2.AddValue(scripts.get(5));
			respPL=ExceuteJavaScriptPayLoad(PL2);
			
			if(!respPL.IsErrorPayLoad())
			{
				GingerAgent.WriteLog("wgxpath_install Injected");				
			}
								
			PayLoad PL21=new PayLoad("JscriptInjected");			
			PayLoad resp=ExceuteJavaScriptPayLoad(PL21);
			String jExist= resp.GetValueString();
			GingerAgent.WriteLog("jExist::" + jExist);
			
			if(jExist.equalsIgnoreCase("JQuery Not Found")){
				PayLoad PL=new PayLoad("AddScript");
				GingerAgent.WriteLog(scripts.get(6));
				PL.AddValue(scripts.get(6));
				ExceuteJavaScriptPayLoad(PL);
				GingerAgent.WriteLog("JQuery injected");
			}
			///Execute the below function for Live Spy(we run it here and not in GetComponentFromCursor so it will be execute only once)		
			Response+=ExecuteScript("GingerLib.StartEventListner()");			
	}
	
	//Recorder HTML
	public void InjectRecordingScript(String script)
	{
		Object Response="";
		Response=ExecuteScript(script);
		
		if(Response!=null)
		{
			GingerAgent.WriteLog("Recording script injected");
		}		
		
		Response=ExecuteScript("define_GingerRecorderLib();");
		
		if(Response!=null)
		{
			GingerAgent.WriteLog("Ginger RecorderLib defined");
		}			
		
		Response=ExecuteScript("GingerLib.StartEventListner()");
		
		if(Response!=null)
		{
			GingerAgent.WriteLog("Widget Event Listener Started");
		}			
	}
	    
    public PayLoad ExceuteJavaScriptPayLoad(PayLoad RequestPL)
    {
        String PLString= RequestPL.GetHexString();
        
        GingerAgent.WriteLog("PLString::" + PLString);
        
        String script = "GingerLib.ProcessHexInputString('"+PLString+"')";
        	          
        	 Object rc = ExecuteScript(script);
             
             if (rc==null)
             {
             	GingerAgent.WriteLog("rc is null");
             	return PayLoad.Error("Failed to execute- response null");
             }               
             else if (rc.toString().startsWith("ERROR"))
             {
             	return PayLoad.Error(rc.toString());       
             }        
             else
             {
             	  PayLoad ResponsePL=PayLoad.GetPayLoadFromHexString((String)rc);    
                  GingerAgent.WriteLog("ResponsePL.Name" + ResponsePL.Name);
                  return ResponsePL; 
             } 
    }
    
    public PayLoad ExecutesetJavascriptErrorsSuppressed(boolean bSupress)
    {    
        Object rc = setJavascriptErrorsSuppressed(bSupress);
        
        if (rc!=null)
        	return PayLoad.OK("Error Supress Passed");
        else
        	return PayLoad.Error("Error Supress Failed");        
    }

    public Object setJavascriptErrorsSuppressed(boolean bSupress)
	{
		try
		{			
			Class DumpME = mBrowser.getClass(); 
			GingerAgent.WriteLog("Dump Class Method: " + DumpME.getName() );
			Method[] methods = DumpME.getDeclaredMethods();			
			for (Method m : methods)
			{
				GingerAgent.WriteLog("Method: " +  m.getName());
			}
			
		     Object o = mBrowser;	     
		     GingerAgent.WriteLog("Obj=" + o.getClass().getName() );		     
		     Method method = DumpME.getMethod("setJavascriptErrorsSuppressed", boolean.class);
		     method.setAccessible(true);	
		     GingerAgent.WriteLog("String.class");
		     Object oRC = method.invoke(o, bSupress);	
		     GingerAgent.WriteLog("String.class3" + oRC.toString());
		     return oRC;
		}
		catch (Exception ex)
		{
				GingerAgent.WriteLog(ex.getMessage());
				return "ERROR - " + ex.getMessage();
		}		
	}	
}
