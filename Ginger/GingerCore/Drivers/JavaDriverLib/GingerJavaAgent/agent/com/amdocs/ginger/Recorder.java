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
import java.awt.AWTEvent;
import java.awt.Component;
import java.awt.Container;
import java.awt.Window;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.FocusEvent;
import java.awt.event.FocusListener;
import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import java.awt.event.MouseEvent;
import java.awt.event.MouseListener;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

import javax.swing.AbstractButton;
import javax.swing.DefaultListSelectionModel;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JComboBox;
import javax.swing.JDialog;
import javax.swing.JList;
import javax.swing.JMenu;
import javax.swing.JMenuItem;
import javax.swing.JOptionPane;
import javax.swing.JRadioButton;
import javax.swing.JSplitPane;
import javax.swing.JTabbedPane;
import javax.swing.JTable;
import javax.swing.JTextArea;
import javax.swing.JTextField;
import javax.swing.JTextPane;
import javax.swing.JTree;
import javax.swing.event.ChangeEvent;
import javax.swing.event.ChangeListener;
import javax.swing.event.ListSelectionEvent;
import javax.swing.event.ListSelectionListener;
import javax.swing.event.MenuEvent;
import javax.swing.event.MenuListener;
import javax.swing.event.TableModelEvent;
import javax.swing.event.TableModelListener;
import javax.swing.event.TreeExpansionEvent;
import javax.swing.event.TreeExpansionListener;
import javax.swing.event.TreeSelectionEvent;
import javax.swing.event.TreeSelectionListener;
import javax.swing.table.TableModel;
import javax.swing.tree.TreePath;

public class Recorder implements ActionListener, FocusListener, TreeSelectionListener, ListSelectionListener, TreeExpansionListener, MenuListener, MouseListener, TableModelListener, ChangeListener, PropertyChangeListener, ItemListener
{
	SwingHelper mSwingHelper;
	JavaDriver mJavaDriver;
	List<PayLoad> mRecording = new ArrayList<PayLoad>(); 
	
	public Recorder(JavaDriver javaDriver,SwingHelper swingHelper) {
		mJavaDriver=javaDriver;
		mSwingHelper = swingHelper;
	}

	
	public void StartWindowTracker() {		
		Timer t = new Timer();
		
		t.schedule(new TimerTask() {
		    @Override
		    public void run() {
		    	mJavaDriver.CheckJExplorerPopup(mJavaDriver.CheckActiveWindowJS());
		    }
		}, 0, 1000);
		
		//TODO: This timer is running forever. Must cancel it during stop recording.
	}
	
	public void actionPerformed(ActionEvent e) {
		 Object source = e.getSource();
		 		 
	        if (source instanceof JButton) {
	        	// a button was clicked
	            JButton btn = (JButton)source;
	            String name = btn.getText();
	            GingerAgent.WriteLog("Action performed for JButton: " + name + ","
					+ e.getActionCommand() + "," + e.paramString());
	            	            
	            // Add button Click PayLoad
	            PayLoad pl = new PayLoad("JButton");
	            String Xpath = mSwingHelper.GetComponentXPath(btn);

	            // Following If to remove /dialog when it comes for JButton inside Pop-up Window
	            // e.g: /dialog4/javax.swing.JRootPane[0]/java.swing.JButton[0]
	            // by          /javax.swing.JRootPane[0]/java.swing.JButton[0]
	            // Reason: /dialog# will change every time a pop-up is opened and play back will
	            //         fail because next time /dialog# will change to /dialog#+1  
	            if (Xpath.startsWith("/dialog")){
	            	Xpath = Xpath.substring(ordinalIndexOf(Xpath,"/",2), Xpath.length());
	            }	            	 
	            
	            pl.AddValue(Xpath);
	            pl.AddValue(name);
	            pl.ClosePackage();	            
	            mRecording.add(pl);	            
	        }
	        
	        
	        if (source instanceof JCheckBox) {
	        	JCheckBox chk = (JCheckBox)source;
	            String name = chk.getName();
	            GingerAgent.WriteLog("Action performed for JCheckBox: " + name + ","
					+ e.getActionCommand() + "," + e.paramString());
	            PayLoad pl = new PayLoad("JCheckBox");
	            String Xpath = mSwingHelper.GetComponentXPath(chk);
	            pl.AddValue(Xpath);
	            
	            String selected;
	            if (chk.isSelected())
	            	{	selected = "true";  }
	            else 
	            	{ 	selected = "false"; }
	            
	            pl.AddValue(selected);
	            pl.AddValue(name);
	            
	            //TODO: Add if to check or uncheck
	            pl.ClosePackage();	            
	            mRecording.add(pl);	            
	        }
	        
	        if (source instanceof JComboBox) {
	        	// a JComboBox was selected	

				JComboBox cmbBox = (JComboBox)source;
	            String value = cmbBox.getSelectedItem().toString();
	            String name = (cmbBox.getName() == null)? "" : cmbBox.getName() ;
	            GingerAgent.WriteLog("Action performed for JComboBox: " + name + ","
						+ e.getActionCommand() + "," + e.paramString());
	            // Add button Click PayLoad
	            PayLoad pl = new PayLoad("JComboBox");
	            String Xpath = mSwingHelper.GetComponentXPath(cmbBox);
	            pl.AddValue(Xpath);
	            pl.AddValue(value);
	            pl.AddValue(name);
	            pl.ClosePackage();	            
	            mRecording.add(pl);
	            
	        }
	        //JRadioButton
	        if (source instanceof JRadioButton) {
	        	// a JRadioButton was selected	

	        	JRadioButton radio = (JRadioButton)source;
	            String value = radio.getText();
	            String name = radio.getName();
	            GingerAgent.WriteLog("Action performed for JRadioButton: " + name + ","
						+ e.getActionCommand() + "," + e.paramString());
	            // Add button Click PayLoad
	            PayLoad pl = new PayLoad("JRadioButton");
	            String Xpath = mSwingHelper.GetComponentXPath(radio);
	            pl.AddValue(Xpath);
	            pl.AddValue(value);
	            pl.AddValue(name);
	            pl.ClosePackage();	            
	            mRecording.add(pl);
	            
	        }	        

	        //JMenu
	        if (source instanceof JMenu) {
	        	// a JMenu was selected	

	        	JMenu menu = (JMenu)source;
	            String value = "";
	            String name  = menu.getName();
	            
	            GingerAgent.WriteLog("Action performed for JMenu: " + name + ","
						+ e.getActionCommand() + "," + e.paramString());
	            
	            // Add button Click PayLoad
	            PayLoad pl = new PayLoad("JMenu");
	            String Xpath = mSwingHelper.GetComponentXPath(menu);
	            pl.AddValue(Xpath);
	            pl.AddValue(value);
	            pl.ClosePackage();	            
	            mRecording.add(pl);	            
	        }	
	        
	        //JMenuItem
	        if (source instanceof JMenuItem) {
	        	// a JMenuItem was selected	

	        	JMenuItem menu = (JMenuItem)source;
	            String value = menu.getText();
	            String name  = menu.getName();
	            GingerAgent.WriteLog("Action performed for JMenuItem:: " + name + ","
						+ e.getActionCommand() + "," + e.paramString());
	            
	            // Add button Click PayLoad
	            PayLoad pl = new PayLoad("JMenuItem");
	            String Xpath = mSwingHelper.GetComponentXPath(menu);
	            pl.AddValue(Xpath);
	            pl.AddValue(value);
	            pl.AddValue(name);	            
	            pl.ClosePackage();	            
	            mRecording.add(pl);	  				          
	        }		             
	}

	//Returns position for the search character
	public static int ordinalIndexOf(String str, String substr, int n) {
	    int pos = str.indexOf(substr);
	    while (--n > 0 && pos != -1)
	        pos = str.indexOf(substr, pos + 1);
	    return pos;
	}

	 // To check current active Window	
	 Window getSelectedWindow() { 
	 	    Window result = null; 

		    Window[] windows = Window.getWindows();
		    if( windows != null ) { // don't rely on current implementation, which at least returns [0].
		        for( Window w : windows ) {
		            if( w.isShowing() && w instanceof JDialog && ((JDialog)w).isModal() ){
		            	GingerAgent.WriteLog("sActive > " + w.isActive());
		            	GingerAgent.WriteLog("sTitle  > " + ((JDialog)w).getTitle());
		            	
		    			PayLoad pl = new PayLoad("SwitchWindow");
		    	        String Title = ((JDialog) w).getTitle();		    	        
		    	        pl.AddValue(Title);
		    	        pl.ClosePackage();	            
		    	        mRecording.add(pl);		            	
		            	result = w;
		            }	
		        }
		    }		    
	 	    return result; 
	 	}
	 		
	 public void focusGained(FocusEvent e)
	 {
		 //TODO: we can keep the last elem focused and in lost focus compare with original value, if no change then no event		 
	 }
	    
    public void focusLost(FocusEvent e)
    {
    	 if (e.getSource() instanceof JTextField)
    	 {
    		 JTextField txt = (JTextField)e.getSource();
			 String s = txt.getName() + " " + txt.getText();
			 String Xpath = mSwingHelper.GetComponentXPath(txt);
	            
			 GingerAgent.WriteLog("JTextField::" + s);
			 PayLoad pl = new PayLoad("JTextField");
			 pl.AddValue(Xpath);
	         pl.AddValue(txt.getText());
	         pl.AddValue(txt.getName());
	         
	         pl.ClosePackage();	            
	         mRecording.add(pl);
    	 }
    	 
    	 if (e.getSource() instanceof JTextArea)
    	 {
    		 JTextArea txtArea = (JTextArea)e.getSource();
    		 String Xpath = mSwingHelper.GetComponentXPath(txtArea);
			 String s = txtArea.getName() + " xx " + txtArea.getText();
			 GingerAgent.WriteLog("JTextArea::" + s);
			 
			 PayLoad pl = new PayLoad("JTextArea");
			 pl.AddValue(Xpath);
	         pl.AddValue(txtArea.getText());
	         pl.AddValue(txtArea.getName());
	         
	         pl.ClosePackage();	            
	         mRecording.add(pl);
    	 }    

    	 if (e.getSource() instanceof JTextPane)
    	 {
    		 JTextPane txtPane = (JTextPane)e.getSource();
    		 String Xpath = mSwingHelper.GetComponentXPath(txtPane);
			 String s = txtPane.getName() + " :: " + txtPane.getText();
			 GingerAgent.WriteLog(s);
			 
			 PayLoad pl = new PayLoad("JTextPane");
			 pl.AddValue(Xpath);
	         pl.AddValue(txtPane.getText());
	         pl.AddValue(txtPane.getName());
	         
	         pl.ClosePackage();	            
	         mRecording.add(pl);
    	 }
    	 if (e.getSource() instanceof JTable)
    	 {	            
    		 	JTable table = (JTable)e.getSource(); 
    		 	int column 	= table.getSelectedColumn();
    		 	int row		= table.getSelectedRow();
    		 	String component = "";
    		 	
    		 	System.out.println("JTable row - column: " + " - row: " + row + " - column: " + column);
    		 	GingerAgent.WriteLog("JTable row - column: " + " - row: " + row + " - column: " + column);
    		 	
    		 	Component CellComponent = table.prepareRenderer(table.getCellRenderer(row, column), row,
						column);
    		 	
    		 	if (CellComponent instanceof JButton)
    		 		component = "JButton";
    		 	else if (CellComponent instanceof JTextArea)
    		 		component = "JTextArea";
    		 	else if (CellComponent instanceof JTextField)
    		 		component = "JTextField";
    		 	else if (CellComponent instanceof JComboBox)
    		 		component = "JComboBox";
    		 	else if (CellComponent instanceof JRadioButton)
    		 		component = "JRadioButton";
    		 	else if (CellComponent instanceof JCheckBox)
    		 		component = "JCheckBox";
    		 	else if (CellComponent instanceof JTextPane)
    		 		component = "JTextPane";
    		 	else if (CellComponent instanceof JList)
    		 		component = "JList";
    		 	else if (CellComponent instanceof JTree)
    		 		component = "JTree";
    		 	
    		 	System.out.println("JTable CellComponent Class: " + CellComponent.getClass());    		 	
    		 	String valueTmp = mJavaDriver.getCellValue(CellComponent,row);    		 	
    		 	String value = (valueTmp.toString() == null)? "": valueTmp.toString();    		 	
    		 	System.out.println("JTable Value: " + value.toString() + " - row: " + row + " - column: " + column );
    		 	
    		    // Add button Click PayLoad
    		    PayLoad pl = new PayLoad("JTable");
    		    String Xpath = mSwingHelper.GetComponentXPath(table);
    		    
    		    GingerAgent.WriteLog(">> Xpath Table   =" + Xpath);    		    
    		    pl.AddValue(Xpath);
    		    pl.AddValue(value.toString());
    		    pl.AddValue(table.getName());
    		    pl.AddValue(component);
    		    pl.AddValue(Integer.toString(row));
    		    pl.AddValue(Integer.toString(column));      	     
    		    pl.ClosePackage();	
    		    mRecording.add(pl);    		 	
    	 }    	     	 
    }

	public List<PayLoad> GetRecording() {		
		return mRecording;
	}
	
	public void ClearRecording()
	{
		mRecording.clear();
	}

	public void valueChanged(TreeSelectionEvent e) {
		
        if (e.getSource() instanceof JTree)
   	 	{        			
        	//new test
        	Object node=((JTree) e.getSource()).getLastSelectedPathComponent();			
        	//end test
        	       	   		
        	TreePath tp = e.getNewLeadSelectionPath();  
        	String tmp = "";
        	Object elements[];
        	if (tp != null){
        		elements = tp.getPath();
        	}else{
        		System.out.println("TreePath is null: ");
        		return;
        	}	
        	
            for (int i = 0, n = elements.length; i < n; i++) {
            	tmp = tmp + "/" + elements[i];
            }
        	
    		if (node == null)
    			// Nothing is selected.
    			return;
    		
        	JTree jTree = (JTree)e.getSource();
        	String name  = jTree.getName();
			String value = tmp;
			String Xpath = mSwingHelper.GetComponentXPath(jTree);
	        
			if(name.equals("OpenedWindowsTree"))
				return;
			
			GingerAgent.WriteLog( "  Action performed for JTree valueChanged > " + name + ", value: " + value );
			PayLoad pl = new PayLoad("JTree");
			pl.AddValue(Xpath);
	        pl.AddValue(value);
	        pl.AddValue(name);
	         
	        pl.ClosePackage();	            
	        mRecording.add(pl);        	
   	 	}       
	}
	
	public void treeExpanded(TreeExpansionEvent e) {
       
        if (e.getSource() instanceof JTree)
   	 	{        			
        	    		
    		Object node=((JTree) e.getSource()).getLastSelectedPathComponent();			
			if (node != null)
				System.out.println(" nodeTmp>> " + node.toString());
			else 
				System.out.println(" nodeTmp>> is null " );
    		     		    		
        	JTree jTree = (JTree)e.getSource();
			String value = node.toString();
			System.out.println(" node value treeExpanded: "+ value );
			
			String name  = jTree.getName();
			//Added to handle automatic recording for this element
			if(name.equals("OpenedWindowsTree"))
				return;
			
			String Xpath = mSwingHelper.GetComponentXPath(jTree);
	            
			GingerAgent.WriteLog( "  Action performed for JTree TreeExpansionEvent:treeExpanded > " + name + ", value: " + value );
			PayLoad pl = new PayLoad("JTree");
			pl.AddValue(Xpath);
	        pl.AddValue(value);
	        pl.AddValue(name);
	         
	        pl.ClosePackage();	            
	        mRecording.add(pl);        	
   	 	}		
	}

	public void treeCollapsed(TreeExpansionEvent e) {
		
        if (e.getSource() instanceof JTree)
   	 	{        			
        	
    		Object node=((JTree) e.getSource()).getLastSelectedPathComponent();			
			if (node != null)
				System.out.println(" nodeTmp>> " + node.toString());
			else 
				System.out.println(" nodeTmp>> is null " );
    		     		    		
        	JTree jTree = (JTree)e.getSource();
			String value = node.toString();
			System.out.println(" node value treeCollapsed: "+ value );
			String name  = jTree.getName();
			String Xpath = mSwingHelper.GetComponentXPath(jTree);
	            
			GingerAgent.WriteLog( " Action performed for JTree TreeExpansionEvent:treeCollapsed > " + name + ", value: " + value );
			PayLoad pl = new PayLoad("JTree");
			pl.AddValue(Xpath);
	        pl.AddValue(value);
	        pl.AddValue(name);
	         
	        pl.ClosePackage();	            
	        mRecording.add(pl);        	
   	 	}		
	}

	public void valueChanged(ListSelectionEvent e) {
		
		GingerAgent.WriteLog("Action performed ListSelectionEvent: " + e.getSource().getClass());
		
		if (e.getSource() instanceof JList)
   	 	{
	    	JList list = (JList)e.getSource();
	        String value = list.getSelectedValue().toString();
	        String name  = list.getName();
	        GingerAgent.WriteLog("Action performed for JList: " + name + "," + value);
	        
	        // Add button Click PayLoad
	        PayLoad pl = new PayLoad("JList");
	        String Xpath = mSwingHelper.GetComponentXPath(list);
	        pl.AddValue(Xpath);
	        pl.AddValue(value);
	        pl.ClosePackage();	            
	        mRecording.add(pl);
   	 	}
		
		if (e.getSource() instanceof DefaultListSelectionModel)
   	 	{
			DefaultListSelectionModel table = (DefaultListSelectionModel)e.getSource();
			int first = table.getMinSelectionIndex();
		    int last = table.getMaxSelectionIndex();
			
			String value = "";
	        String name  = "";
	        GingerAgent.WriteLog("Action performed for JTable <DefaultListSelectionModel>: " + first + "," + last);
	        
	        // Add button Click PayLoad
	        PayLoad pl = new PayLoad("JTable");
	        String Xpath = "test"; 
	        pl.AddValue(Xpath);
	        pl.AddValue(value);
	        pl.ClosePackage();	            
	        mRecording.add(pl);
   	 	}		
	}
	
	public void menuSelected(MenuEvent e) {
		
		if (e.getSource() instanceof JMenu)
   	 	{
			JMenu  menu = (JMenu)e.getSource();
            String value = "";
            String name  = menu.getName();
            
            GingerAgent.WriteLog("Action performed for JMenu <::::> " + name + "," + value );
            
            // Add button Click PayLoad
            PayLoad pl = new PayLoad("JMenu");
            String Xpath = mSwingHelper.GetComponentXPath(menu);                    	            
            pl.AddValue(Xpath);
            pl.AddValue(value);
            pl.AddValue(name);
            pl.ClosePackage();	            
            mRecording.add(pl);		
   	 	}	
	}

	public void menuDeselected(MenuEvent e) {
		// TODO Auto-generated method stub		
	}

	public void menuCanceled(MenuEvent e) {
		// TODO Auto-generated method stub		
	}

	public void mouseClicked(MouseEvent e) {
		// TODO Auto-generated method stub
		System.out.println(" mouseClicked ");
		if (e.getSource() instanceof JMenuItem)
   	 	{
			JMenuItem  menuItem = (JMenuItem)e.getSource();
            String value = "";//menu.getText();
            String name  = menuItem.getName();
            
            GingerAgent.WriteLog("Action performed for JMenuItem: " + name + "," + value );
            Component comp1 = menuItem.getComponent();
            
            if(comp1 instanceof JMenuItem){
                JMenuItem menuItem1 = (JMenuItem) comp1;
                System.out.println("MenuItem mouseClicked------>>>>>>>>>>>> " + menuItem1.getName().getClass());
            }
            
            // Add button Click PayLoad
            PayLoad pl = new PayLoad("JMenuItem");
            String Xpath = mSwingHelper.GetComponentXPath(menuItem);
            pl.AddValue(Xpath);
            pl.AddValue(value);
            pl.ClosePackage();	            
            mRecording.add(pl);		
   	 	}			
	}

	public void mousePressed(MouseEvent e) {
		// TODO Auto-generated method stub
		System.out.println(" mousePressed ");
		if (e.getSource() instanceof JMenuItem)
   	 	{
			JMenuItem  menuItem = (JMenuItem)e.getSource();
            String value = "";
            String name  = menuItem.getName();
            System.out.println( "Event source While::getComponentgetClass  " + menuItem.getComponent().getClass());
            System.out.println( "Event source While::getName  " + menuItem.getName());
            System.out.println( "Event source While::getName  " + menuItem.getParent());
            Component comp1 = menuItem.getComponent();
            
            if(comp1 instanceof JMenuItem){
                JMenuItem menuItem1 = (JMenuItem) comp1;
                System.out.println("MenuItem ------>>>>>>>>>>>> " + menuItem1.getText());
            }
            
            GingerAgent.WriteLog("Action performed for JMenuItem: " + name + "," + value );
            
            // Add button Click PayLoad
            PayLoad pl = new PayLoad("JMenuItem");
            String Xpath = mSwingHelper.GetComponentXPath(menuItem);
            pl.AddValue(Xpath);
            pl.AddValue(value);
            pl.ClosePackage();	            
            mRecording.add(pl);		
   	 	}	
	}

	public void mouseReleased(MouseEvent e) {
		// TODO Auto-generated method stub		
	}


	public void mouseEntered(MouseEvent e) {
		// TODO Auto-generated method stub		
	}


	public void mouseExited(MouseEvent e) {
		// TODO Auto-generated method stub		
	}

	/**
	 * 
	 * @return
	 */
	public Window getPopup() {
		// loop in reverse order since the most recent window seems to be at the
		// end of the array
		
		Window[] windows = Window.getWindows();
		for (int i = windows.length - 1; i >= 0; i--) {
			Window window = windows[i];
			if (!window.isVisible())
				continue;

			if (window instanceof JDialog) {
				JDialog dialog = (JDialog) window;

				if ("Problem".equals(dialog.getTitle())
						|| (dialog.getContentPane().getComponentCount() == 1 && dialog
								.getContentPane().getComponent(0) instanceof JOptionPane)) {
					return dialog;
				}
			}
		}
		return null;
	}
	
	/**
	 * If there is an open popup and it is not a "Problem" dialog, listeners are
	 * added to the popup's controls.
	 */
	public void addListenersToPopup() {
		final Window popup = this.getPopup();
		if (popup == null)
			return;

		if (popup instanceof JDialog) {
			JDialog dialog = (JDialog) popup;
			if ("Problem".equals(dialog.getTitle())) {
				return;
			} else if (dialog.getContentPane().getComponentCount() == 1
					&& dialog.getContentPane().getComponent(0) instanceof JOptionPane) {
				final JOptionPane optionPane = (JOptionPane) dialog
						.getContentPane().getComponent(0);
				final JButton[] buttons = new JButton[optionPane.getOptions().length];

				// button options
				for (int i = 0; i < optionPane.getOptions().length; i++) {
					String buttonText = (String) optionPane.getOptions()[i];
					System.out.println(" buttonText :: " + buttonText);
					buttons[i] = this.getPopupButton(dialog.getContentPane(),
							buttonText);
					System.out.println(" buttons[i].Name :: " + buttons[i].getName());
					buttons[i].addActionListener(this);
				}


			}
		} else {
			// FileChooser, PrintPreview, and other non-ASCF windows
		}
	}
	
	public JButton getPopupButton(Container container, String buttonText) {
		JButton btn = null;
		List<Container> children = new ArrayList<Container>(25);
		for (Component child : container.getComponents()) {
			if (child instanceof JButton) {
				JButton button = (JButton) child;
				System.out.println(" button.getText()");
				if (buttonText.equals(button.getText())) {
					btn = button;
					break;
				}
			} else if (child instanceof Container) {
				children.add((Container) child);
			}
		}
		if (btn == null) {
			for (Container cont : children) {
				JButton button = getPopupButton(cont, buttonText);
				if (button != null) {
					btn = button;
					break;
				}
			}
		}
		return btn;
	}

	public void tableChanged(TableModelEvent e) {
		
		System.out.println(" e.getClass() " +  e.getClass());
		
		int row = e.getFirstRow();
        int column = e.getColumn();
        TableModel model = (TableModel)e.getSource();
        Object data = model.getValueAt(row, column);
        
        System.out.println("getClass: " + e.getClass() );
        System.out.println("Data: " + data.toString() + " - row: " + row + " - column: " + column );
			
		String value = data.toString();
	    GingerAgent.WriteLog("Action performed for JTable: " + row + "," + column + " table name: " + ((JTable)model).getName());
	        
	    // Add button Click PayLoad
	    PayLoad pl = new PayLoad("JTable");
	    String Xpath = "test"; 
	    pl.AddValue(Xpath);
	    pl.AddValue(value);
	    pl.AddValue(Integer.toString(row));
	    pl.AddValue(Integer.toString(column));
	    pl.ClosePackage();	           
	}

	void AddActionListener(Container container, Recorder recorder) {
		
		Component[] list = container.getComponents();		
    	for (Component c : list)
    	{    		    		
			if (c instanceof JComboBox)
    		{    			
    			((JComboBox)c).addItemListener(this); 
    		}

			if (c instanceof JRadioButton)
    		{
				((JRadioButton)c).addActionListener(this);    			
    		}
			
			if (c instanceof JCheckBox)
    		{
    			((JCheckBox)c).addActionListener(this);    			
    		}			
			
			
			if (c instanceof JMenu)
    		{
				JMenu m = ((JMenu)c);
				MenuAddActionListenerRecursive(m);								
    		}
												
			if (c instanceof JList)
    		{
    			((JList)c).addListSelectionListener(this);    			
    		}	

			if (c instanceof JTree)
    		{
    			((JTree)c).addTreeSelectionListener(this);    			  			
    			((JTree)c).addTreeExpansionListener(this);
    			
    		}	

		
			if (c instanceof AbstractButton)
    		{
    			((AbstractButton)c).addActionListener(this);    			
    		}
    		
    		if (c instanceof JTextField)
    		{
				//TODO: check if there is a way to find the text change instead of focus
    			((JTextField)c).addFocusListener(this);
    		}
    		
			if (c instanceof JTextArea)
    		{
    			((JTextArea)c).addFocusListener(this);    			
    		}

			if (c instanceof JTextPane)
    		{
    			((JTextPane)c).addFocusListener(this);    			
    		}	

			if (c instanceof JTable)
    		{
				JTable table = ((JTable)c);
				//TODO Add proper modification for get cell data.
				table.addFocusListener(this);					
    		}	
			
    		//TODO: add other classes which are not covered, try to find the base class which impl addActionListener	
    		    		    	
			else if (c instanceof JTabbedPane)
			{
				JTabbedPane jtp=(JTabbedPane)c;
				jtp.addChangeListener(recorder);
				
			}
			if (c instanceof Container)
    		{
    			//drill down recursive 
    			AddActionListener((Container)c, recorder);
    		}    		
    	}		
	}

	// Add action listener to all controls, go recursive down the tree for Containers

	private void MenuAddActionListenerRecursive(JMenu m) {
 		 System.out.println( "Found JMenu - "  + m.getText() );
		 m.addMenuListener(this);    		
		 Component[] comps =  m.getMenuComponents();  // need to use getMenuComponents to get sub menus
		 for (Component c1 : comps)
		 {
			 if (c1 instanceof JMenuItem)
			 {
				 JMenuItem mi = (JMenuItem)c1;
	    			mi.addActionListener(this);
	    			    		    	
			 }
			 if (c1 instanceof JMenu)
    		{
				 JMenu m2 = (JMenu)c1;
				 System.out.println("Drill down - " + m2.getText());
				 MenuAddActionListenerRecursive(m2);
    		}
		 }		
	}

	public void MenuRemoveActionListenerRecursive(JMenu m) {
		 System.out.println( "Found JMenu - "  + m.getText() );
		 m.addMenuListener(this);    		
		 Component[] comps =  m.getMenuComponents();  // need to use getMenuComponents to get sub menus
		 for (Component c1 : comps)
		 {
			 if (c1 instanceof JMenuItem)
			 {
				 JMenuItem mi = (JMenuItem)c1;
	    			mi.removeActionListener(this);
	    			    		    	
			 }
			 if (c1 instanceof JMenu)
   		{
				 JMenu m2 = (JMenu)c1;
				 System.out.println("Drill down - " + m2.getText());
				 MenuRemoveActionListenerRecursive(m2);
   		}
		 }
		
	}
	public void stateChanged(ChangeEvent e) {
		// TODO Auto-generated method stub

		if (e.getSource() instanceof JTabbedPane)
   	 	{
			JTabbedPane  jTabbedPane = (JTabbedPane)e.getSource();
            String name  = jTabbedPane.getName();            
            int selectedIndex = jTabbedPane.getSelectedIndex();           
            String value =  jTabbedPane.getTitleAt(selectedIndex);
                        
            GingerAgent.WriteLog("Action performed for JTabbedPane <::::> " + name + "," + value );
            
            // Add button Click PayLoad
            PayLoad pl = new PayLoad("JTabbedPane");
            String Xpath = mSwingHelper.GetComponentXPath(jTabbedPane);
            pl.AddValue(Xpath);
            pl.AddValue(value);
            pl.AddValue(name);
            pl.ClosePackage();	            
            mRecording.add(pl);		
   	 	}
	}

	public  void propertyChange(PropertyChangeEvent evt)
	{	
		if (evt.getSource() instanceof JSplitPane){
			System.out.println(" JSplitPane ");
		}		
	}
	
	public void Start(Window CurrentWindow) {

		AddActionListener(CurrentWindow,this);
		
		//Adding listener for pop-up Windows
		registerListener(CurrentWindow, this);
	}
	
	public void registerListener(Component component, Recorder mRecorder) {
        
		GingerAgent.WriteLog(" AddAWTEventListener to popup Window ");
		component.getToolkit().addAWTEventListener(new WindowMonitor(mRecording,mRecorder),
                AWTEvent.WINDOW_EVENT_MASK); 
		
		if (WindowMonitor.activeWindow!=null){			
			mSwingHelper.setCurrentWindow(WindowMonitor.getActiveWindow());
		} 
    }

	public void itemStateChanged(ItemEvent e) {
		// TODO Auto-generated method stub

		if (e.getSource() instanceof JComboBox)
   	 	{
			GingerAgent.WriteLog("itemStateChanged :: ");
			if(e.getStateChange() == ItemEvent.SELECTED) {
				JComboBox cmbBox = (JComboBox)e.getSource();			
		        String value = cmbBox.getSelectedItem().toString();
		        String name = (cmbBox.getName() == null)? "" : cmbBox.getName() ;
		        GingerAgent.WriteLog("Action performed for JComboBox: " + name + ","
						+ "," + e.paramString());
		        // Add button Click PayLoad
		        PayLoad pl = new PayLoad("JComboBox");
		        String Xpath = mSwingHelper.GetComponentXPath(cmbBox);
		        pl.AddValue(Xpath);
		        pl.AddValue(value);
		        pl.AddValue(name);
		        pl.ClosePackage();	            
		        mRecording.add(pl);					
			}	
   	 	}    
   	 }
}