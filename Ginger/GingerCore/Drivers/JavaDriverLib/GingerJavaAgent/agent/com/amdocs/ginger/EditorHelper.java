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
import java.awt.Rectangle;
import java.awt.event.InputEvent;
import java.awt.event.MouseEvent;
import java.lang.reflect.InvocationTargetException;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;

import javax.swing.JCheckBox;
import javax.swing.JEditorPane;
import javax.swing.SwingUtilities;
import javax.swing.text.BadLocationException;
import javax.swing.text.html.HTML;
import javax.swing.text.html.HTMLDocument;

import org.jsoup.Jsoup;
import org.jsoup.helper.StringUtil;
import org.jsoup.nodes.Attribute;
import org.jsoup.nodes.Document;
import org.jsoup.nodes.Element;
import org.jsoup.select.Elements;

import sun.awt.SunToolkit;
import us.codecraft.xsoup.Xsoup;

public class EditorHelper {
	
	 private JEditorPane mEditor = null;
	 private Document htmlDoc=null;
	
	 public EditorHelper(Component editor) 
	 {
		 mEditor = (JEditorPane)editor;	

		 htmlDoc= Jsoup.parse(mEditor.getText());
	 }

	 public  Elements getAllElements()
	 {		
		 return htmlDoc.getAllElements();
	 }

	 public PayLoad GetElement(String x)
	 {
		 Element resultLinks = htmlDoc.select(x).get(0);
		 PayLoad PL=new PayLoad("Element");
		 PL.AddValue(resultLinks.toString());
		 PL.ClosePackage();			
		 return PL;		 
	 }
	 public PayLoad GetEditorComponentInfo(Element el,int index)
		{			
			String elementTitle=el.tagName();
			
			if(elementTitle==null)
			{
				elementTitle="";
			}
			
			List<String> compValues= new ArrayList() ;
			compValues.add("");
		
			String Value="";		
			if(compValues.size()!=0)
			{
				Value = el.val();

				if(Value==null ||Value=="")
					Value=el.text();
			}
			
			String Path =el.cssSelector();			
			String IsExpandable = "N";
			PayLoad PL=new PayLoad("Element");			
			String elementType = "JEditor."+elementTitle;			
			String XPath = "";		
			PL.AddValue(elementTitle);	
			PL.AddValue(Path);
			PL.AddValue(Value);
			PL.AddValue("NAME");
			PL.AddValue(elementType);							
			PL.AddValue(Path);		
			PL.AddValue(XPath);
			PL.AddValue("");
			PL.AddValue(IsExpandable);									
			PL.ClosePackage();			
			return PL;			
		}
	 
	 
	 	public Element FindEditorElement(String locateBy, String locateValue)
	 	{	 		
	 		if(locateBy.equalsIgnoreCase("ByOffset")|| locateBy.equalsIgnoreCase("ByCSSSelector") )
			{
	 			return htmlDoc.select(locateValue).first();
	 		}
	 		else if (locateBy.equalsIgnoreCase("ByID"))
	 		{
	 			return htmlDoc.getElementById(locateValue);
	 		}
	 		else if (locateBy.equalsIgnoreCase("ByXPath"))
	 		{	 		
	 			return Xsoup.compile(locateValue).evaluate(htmlDoc).getElements().first();
	 		}
	 		return null;
	 		//TODO: Add more
	 	}
	 
	 	
	 	private void AddGingerHighlightStyle()
	 	{
	 	    String css ="<style type=\"text/css\">.GingerHighlight{border: solid #ef0707;}</style>";//,	 	      
	 		Element head= htmlDoc.head();
	 		head.append(css);
	 	}
	 	
	 	public void HighlightElement(String locateValue)
	 	{ 		
	 		return;
	 	} 	
	 	
	 	public List<PayLoad> GetElementProperties(Element el)
	 	{
	 		List<PayLoad> properties= new ArrayList<PayLoad>();
	 		
	 	List<Attribute> elementAttributes= el.attributes().asList();	
	 	
	 	for(Attribute a: elementAttributes)
	 	{
	 		String key=a.getKey();
	 		String value= a.getValue();
	 		properties.add(GetPropertyPayLoad(key,value));
	 		
	 	}	 	
	 		return properties;
	 	}
	 	
	 	private PayLoad GetPropertyPayLoad(String Key, String Value)
	 	{
	 		 PayLoad PL2 = new PayLoad("ComponentProperty");	    
	     	PL2.AddValue(Key);
	     	PL2.AddValue(Value);
	     	PL2.ClosePackage();
	     	return PL2;
	 	}
	 	
	 	void parseWithJsoup() {
	 	    System.out.println("*** JSOUP ***");
	 	   try
	 	   {
	 		   
	 		   GingerAgent.WriteLog("1111111");
	 		   StringUtil.isBlank("aaaa");
	 		  GingerAgent.WriteLog("222222222222");
	 		  Document doc = Jsoup.parse(mEditor.getText());
	 		  GingerAgent.WriteLog("3");
		 	    System.out.println("Title: " + doc.getElementsByTag("title").text());
		 	    System.out.println("H1: " + doc.getElementsByTag("h1").text());
		 	    org.jsoup.nodes.Element table = doc.getElementsByTag("table").first();
		 	    Elements trs = table.getElementsByTag("tr");
		 	    for (org.jsoup.nodes.Element tr : trs) {
		 	        System.out.println("TR: " + tr.text());
		 	        for (org.jsoup.nodes.Element td : tr.getAllElements()) {
		 	            System.out.println("TD: " + td.text());
		 	        }
		 	    }
		 	    System.out.println();
	 	   }
	 	   catch(Exception e)
	 	   {
	 		   GingerAgent.WriteLog("@@@@@@@@@@@@@@@"+e.getMessage());
	 	   }
	 	}
	 
		public PayLoad HandleEditorTableActions(String locateBy, String locateValue,List<String> cellLocator, String controlAction, String Value) 
		{	
			Element CurrentTable= FindEditorElement(locateBy,locateValue);
			GingerAgent.WriteLog("Editor table element = + " + CurrentTable);
			if(CurrentTable==null)
			{
				GingerAgent.WriteLog("Editor table element not found");
				return PayLoad.Error("Editor table element not found");
				
			}
			List<String> ColomumnNames = new ArrayList<String>();
			if (controlAction.equals("GetEditorTableDetails")) 
			{
				ColomumnNames= GetColumnNames(CurrentTable);
				PayLoad PL = new PayLoad("ControlProperties");
				List<PayLoad> list = GetComponentProperties();
				PL.AddListPayLoad(list);
				PL.AddValue(ColomumnNames);
				PL.AddValue(GetRowCount(CurrentTable));
				PL.ClosePackage();
				return PL;
			}
			String rowLocator = cellLocator.get(0);
			GingerAgent.WriteLog("ROW LOCATOR" + rowLocator);
			int nxtIndex = 1;
			int rowNum = -1;
			if (controlAction.equals("GetRowCount")) 
			{
				PayLoad PL = new PayLoad("GetRowCount");
				PL.AddValue(GetRowCount(CurrentTable));
				PL.ClosePackage();
				return PL;
			}
			GingerAgent.WriteLog("Get Row Number");
			if (rowLocator.equals("Where")) 
			{
				String whereColSel = cellLocator.get(1);
				String whereColTitle = cellLocator.get(2);
				String whereProp = cellLocator.get(3);
				String whereOper = cellLocator.get(4);
				String whereColVal = cellLocator.get(5);
				nxtIndex = 6;		
				GingerAgent.WriteLog("<<Where>>");
				try
				{

						GingerAgent.WriteLog("Getting Row numer iteration::");
						rowNum = getRowNum(CurrentTable, whereColSel, whereColTitle,whereProp, whereOper, whereColVal);	
			
				}
				catch(NullPointerException e){
					GingerAgent.WriteLog("Exception in GetRowNum");
					e.printStackTrace();
				}
			} 
			else if (rowLocator.equals("Any Row")) 
			{
				Random rand = new Random();
				rowNum = rand.nextInt(GetRowCount(CurrentTable));
			} 
			else 
			{
				rowNum = Integer.parseInt(cellLocator.get(1));
				if (GetRowCount(CurrentTable) <= rowNum)
					rowNum = -1;
				nxtIndex = 2;
			}
			if (rowNum == -1)
				return PayLoad.Error("Row not found with given Condition");

			GingerAgent.WriteLog("getRowNum::" + rowNum);
			
			int colNum = -1;
			if (controlAction.equalsIgnoreCase("SetValue")
					|| controlAction.equalsIgnoreCase("Type")
					|| controlAction.equalsIgnoreCase("Click")
					|| controlAction.equalsIgnoreCase("WinClick")
					|| controlAction.equalsIgnoreCase("GetValue")
					|| controlAction.equalsIgnoreCase("Toggle")
					|| controlAction.equals("IsCellEnabled")
					|| controlAction.equals("GetSelectedRow")
					|| controlAction.equalsIgnoreCase("AsyncClick")
					|| controlAction.equalsIgnoreCase("DoubleClick")
					|| controlAction.equalsIgnoreCase("SetFocus")
					|| controlAction.equalsIgnoreCase("IsVisible")				
					|| controlAction.equalsIgnoreCase("MousePressAndRelease")
					|| controlAction.equalsIgnoreCase("SendKeys")
					|| controlAction.equalsIgnoreCase("IsChecked")
				) 
				
			{
				String colBy = cellLocator.get(nxtIndex);
				String colVal = cellLocator.get(nxtIndex + 1);
				colNum = getColumnNum(CurrentTable, colBy, colVal);
				if (colNum == -1)
					return PayLoad.Error("Coloumn not found with " + colBy + " :"
							+ colVal);
			}
			
			 if (controlAction.equals("GetValue")) {			 
				 
					Element CellComponent = getTableCellComponent(CurrentTable,rowNum, colNum);		
					PayLoad Response = new PayLoad("ComponentValue");
					List<String> val = new ArrayList<String>();
					String val1;

					if (CellComponent != null) {
						GingerAgent.WriteLog("CellComponent = " + CellComponent);
						
						val1 = CellComponent.val();      
						if(val1==null ||val1=="")
                        {
                               val1=CellComponent.text();
                               if(val1==null ||val1=="")
                               {
                                      Element el=CellComponent.select("input").first();
                                      if(el!=null)
                                             val1 = el.attr("value");
                               }
                                                    
                        }



						GingerAgent.WriteLog("getCellValue ::" + val1);
						val.add(val1);
					} else
						val.add("");

					Response.AddValue(val);
					Response.ClosePackage();
					return Response;
				 }
			 else if (controlAction.equalsIgnoreCase("SetValue"))
			 {
				 GingerAgent.WriteLog("Inside Set Value");
				 GingerAgent.WriteLog("Current Table : " +CurrentTable+", Row Num : " + rowNum + ", Col Num : " +colNum +", Value : " +Value );
				 Element CellComponent = getTableCellComponent(CurrentTable,rowNum,colNum);
				 GingerAgent.WriteLog("CellComponent : " + CellComponent);
				 if(CellComponent!=null)
				 {
					 Elements inputElements = CellComponent.getElementsByTag("input ");
					 if (inputElements != null)
					 {
						 GingerAgent.WriteLog("inputElements : " + inputElements.attr("value", Value));
						 inputElements.attr("value");						
						    GingerAgent.WriteLog("inputElements : " + inputElements);
							GingerAgent.WriteLog("CellComponent ::" + CellComponent);
							GingerAgent.WriteLog("htmldoc ::" + htmlDoc);
					 }					 
				 }
				 else
				 {
					 return PayLoad.Error("Cell component not found");
				 }				 
		}
		else if (controlAction.equals("Click") || controlAction.equals("AsyncClick")) {

			final boolean[] response = new boolean[3];
			response[0] = false;

			Element CellComponent = getTableCellComponent(CurrentTable, rowNum, colNum);		
			
			Element elementTag = CellComponent.getAllElements().first().child(0);
			
			if (elementTag.tagName().contains("input"))
			{
				Element inputElement = CellComponent.select("input").first();
				GingerAgent.WriteLog(CellComponent.getAllElements().first().child(0).toString());
				final HTMLDocument doc = (HTMLDocument) ((JEditorPane) mEditor).getDocument();
				final javax.swing.text.Element elmnt = doc.getElement(doc.getDefaultRootElement(), "collectionindex",
						inputElement.attr("collectionindex"));

				if (elmnt != null) {
					int position = 0;
					Rectangle r = new Rectangle();
					try {
						position = doc.createPosition(elmnt.getStartOffset()).getOffset();
						r = ((JEditorPane) mEditor).modelToView(position);
					} catch (BadLocationException e1) {
						// TODO Auto-generated catch block
						e1.printStackTrace();
					}
					
					final Component c = SwingUtilities.getDeepestComponentAt(mEditor, r.x, r.y);
					
					
					// TODO - Need to use the same Click function used in Java Driver
					
					Runnable rr = new Runnable() {
						public void run() {
							if (c instanceof JCheckBox) {
								response[0] = true;
								JCheckBox jck = (JCheckBox) c;
								boolean b = jck.isSelected();
								jck.grabFocus();
								jck.doClick();
								if (jck.isSelected() == b)
									jck.setSelected(!b);
							}
						}
					};
					if (controlAction.equals("AsyncClick")) {
						Thread t1 = new Thread(rr);
						t1.start();
						
						
						try {
							while (response[0] != true) {
								Thread.sleep(1);
							}
						} catch (Exception e) {
							return PayLoad.Error("PayLoad ClickComponent Error: " + e.getMessage());
						}
						
						
					} else {
						if (SwingUtilities.isEventDispatchThread()) {
							GingerAgent.WriteLog("\n***************\nClickComponent-already in EDT\n***************");
							rr.run();
						} else {
							try {
								GingerAgent.WriteLog("\n***************\nClickComponent-run in EDT\n***************");
								SunToolkit.flushPendingEvents();
								SunToolkit.executeOnEDTAndWait(c, rr);

							} catch (InvocationTargetException e) {
								GingerAgent.WriteLog(
										"Invocation target exception while starting thread for click-" + e.getMessage());
								e.printStackTrace();
							} catch (InterruptedException e) {
								GingerAgent
										.WriteLog("InterruptedException while starting thread for click-" + e.getMessage());
								e.printStackTrace();
							}
						}

					}					
				}
			}
			else if (elementTag.tagName().contains("a"))
			{
				Element link = CellComponent.select("a").first();
				String href = link.absUrl("href");
				HTMLDocument d = (HTMLDocument) ((JEditorPane) mEditor).getDocument();

				final javax.swing.text.Element el = d.getElement(d.getDefaultRootElement(), HTML.Attribute.HREF, link.absUrl("href") );
				if ( el != null)
				{
					final javax.swing.text.AttributeSet a = el.getAttributes();
					final javax.swing.text.AttributeSet anchor = (javax.swing.text.AttributeSet) a.getAttribute(HTML.Tag.A);
					GingerAgent.WriteLog("el = " + el.getName());
					if (anchor != null) {
						if (href != null) {
							try {
								int position = d.createPosition(el.getStartOffset()).getOffset();
								Rectangle r = ((JEditorPane) mEditor).modelToView(position);
								MouseEvent me = new MouseEvent(((JEditorPane) mEditor), MouseEvent.MOUSE_CLICKED,
										System.currentTimeMillis(), InputEvent.BUTTON1_MASK, r.x, r.y, 1, false);

								((JEditorPane) mEditor).dispatchEvent(me);
								response[0] = true;

							} catch (BadLocationException e) {
								e.printStackTrace();
							}
						}
					}
				}
			}
			if (response[0] == false)
				return PayLoad.Error("Fail to perform click operation");
			else
				return PayLoad.OK("Performed click operation");			
		}
		return PayLoad.Error("Unsupported operation");
	}
	
		private List<PayLoad> GetComponentProperties()
		{
			List<PayLoad> list= new ArrayList<PayLoad>();
			return list;			
		}
		
		private int GetRowCount(Element table)
		{			
			return table.getElementsByTag("tr").size()-1;
		}
		private int GetColumnCount(Element table)
		{
			List<String> ColomumnNames = new ArrayList<String>();
			Elements cells=table.getElementsByTag("td");


			for(Element e: cells)
			{
				if(e.className().equals("header"))
				{

					ColomumnNames.add(e.text());
				}
			}
			return ColomumnNames.size();
		}
		
		private List<String> GetColumnNames(Element table)
		{
			List<String> ColomumnNames= new ArrayList<String>();
			Elements cells=table.getElementsByTag("td");
			for(Element e: cells)
			{
				if(e.className().equals("header"))
				{

					ColomumnNames.add(e.text());
				}
			}
			return ColomumnNames;
		}
		
		
		private Element getTableCellComponent(Element table,int row,int col)
		{
			Elements rowElements=table.getElementsByTag("tr");
			
			Element targetRow= rowElements.get(row+1);
			
			Elements columns= targetRow.getElementsByTag("td");
			
			Element targetColumn= columns.get(col);
			
			return targetColumn;	
		}
		
		private int getRowNum(Element CurrentTable, String colSel, String colTitle, String prop, String oper, String colVal){
			int colNum=-1;
			String propVal="";
			Element CellComponent=null;	
			
			if (colVal.equals(""))
				colVal = "<empty>";
				
			colNum = getColumnNum(CurrentTable,colSel,colTitle);
			if(colNum == -1)
				return -1;
			int iRow=-1;
			boolean bFound = false;
			for(iRow=0 ;iRow< GetRowCount(CurrentTable);iRow++)
			{
				bFound = false;
					
				 CellComponent = getTableCellComponent(CurrentTable,iRow, colNum);
				 if(CellComponent ==null)
				 {
					 continue;
				 }
				 
				 if (prop.equalsIgnoreCase("Value"))
				 {				
					propVal=CellComponent.text();
					
				 }		
				 else if(prop.equalsIgnoreCase("Type"))
				 {
					propVal = CellComponent.getClass().getName();
					propVal = propVal.substring(propVal.lastIndexOf('.') + 1);
					propVal = propVal.replace("Native", "");
					propVal = "Uif" + propVal;
				 }	
				 else if(prop.equalsIgnoreCase("Text"))
				 {
					propVal = "";
				 }	
				 else if(prop.equalsIgnoreCase("HTML"))
				 {
					propVal="";
				 }	
				 else if(prop.equalsIgnoreCase("Path"))
				 {
					propVal="";
				 }	
				 else if(prop.equalsIgnoreCase("List"))
				 {
					propVal="";
				 }	
				 else if(prop.equalsIgnoreCase("Tooltip"))
				 {
					propVal="";
				 }	
				 else if(prop.equalsIgnoreCase("DateTimeVaue"))
				 {
					propVal="";
				 }	
				 else
				 {
					propVal="";	
				 }
							 
				 if (oper.equals("Equals"))
				 {
					bFound = colVal.equalsIgnoreCase(propVal);
				 }				
				 else if (oper.equals("NotEquals"))
				 {
					bFound = !colVal.equals(propVal);
				 }
					
				 else if (oper.equals("Contains"))
				 {
					bFound = (propVal != null && propVal.contains(colVal));
				 }	
					
				 else if (oper.equals("NotContains"))
				 {
					bFound = (propVal == null || !propVal.contains(colVal));// condition should be Propval!=null
				 }
					
				 else if (oper.equals("StartsWith"))
				 {				 
					bFound = (propVal != null && propVal.startsWith(colVal));				
				 }				
				 else if (oper.equals("NotStartsWith"))
				 {
					bFound = (propVal == null || !propVal.startsWith(colVal));
				 }				
				 else if (oper.equals("EndsWith"))
				 {
					bFound = (propVal != null && propVal.endsWith(colVal));
				 }				
				 else if (oper.equals("NotEndsWith"))
				 {
					bFound = (propVal == null || !propVal.endsWith(colVal));
				 }
				 			 
				if (bFound)
					return iRow;
			}		
			return -1;		
		}
	 
		private int getColumnNum(Element CurrentTable,String colBy,String colVal){
			int colNum=-1;		
			if(colBy.equalsIgnoreCase("ColTitle"))
			{ 	
				List<String> columnNames=GetColumnNames(CurrentTable);
				
				
				
				
				for (int i =0;i<columnNames.size();i++)
				{
					String colName="";
				
						colName=columnNames.get(i);
					GingerAgent.WriteLog("Col" + i+ ":" +colName);
					if(colVal.equalsIgnoreCase(colName))
					{
						colNum =i;	
						break;
					}
				}
			}
			else if(colBy.equalsIgnoreCase("ColName"))
			{ 
				
			}
			else
			{
				colNum = Integer.parseInt(colVal);
				GingerAgent.WriteLog("Colnum::" + colNum);
				if(colNum>=GetColumnCount(CurrentTable))
				{
					colNum=-1;
				}			
			}
			return colNum;
		}
}
