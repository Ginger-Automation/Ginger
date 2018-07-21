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
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.io.File;
import javax.swing.DefaultListModel;
import javax.swing.DefaultListSelectionModel;
import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JList;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;

import com.sun.tools.attach.VirtualMachine;
import com.sun.tools.attach.VirtualMachineDescriptor;

//TODO: rename to GingerAgentConsole
public class GingerAgentStarterFrame extends JFrame {

	private JPanel jContentPane = null;
	private JButton attachButton = null;
	private JButton refreshButton = null;
	private JButton clearButton = null;		
	private JTextArea GingerAgentJarPathTextField = null;	
	private JTextArea GingerAgentArgsTextField = null;	
	private JList ProcsList = null;
	DefaultListModel listModel = new DefaultListModel();
	private String GingerJarPath = null;	
	private JTextArea ConsoleTextArea = null;
	

	/**
	 * This is the default constructor
	 */
	public GingerAgentStarterFrame() {
		super();
		initialize();
		Utils.redirectSystemStreams(ConsoleTextArea);
		System.out.println("Window Loaded, Loading java processes");
		ShowProcsList();
		System.out.println("Done Loading java processes");
		DumpJavaInfo();
	}		
	
	private void DumpJavaInfo() {
		try
		{
			System.out.println("java.io.tmpdir= " +  System.getProperty("java.io.tmpdir"));		
			System.out.println("user.home= " +  System.getProperty("user.home"));
			System.out.println("user.name= " +  System.getProperty("user.name"));
			
			String hsperfdata = System.getProperty("java.io.tmpdir") + "hsperfdata_" + System.getProperty("user.name"); 
			System.out.println("hsperfdata_= " +  hsperfdata);
			
			File file = new File(hsperfdata);
			if (file.isDirectory())
			{
				System.out.println("hsperfdata folder found!");
				int count = new File(hsperfdata).listFiles().length;
				System.out.println("Files count in hsperfdata= " + count);
			}
			else
			{
				System.out.println("hsperfdata folder Not found!");
			}
		}
		catch (Exception ex)
		{
			System.out.println(ex.getMessage());
		}		
	}

	/**
	 * This method initializes this
	 * 
	 * @return void
	 */
	private void initialize() {
		this.setSize(532, 406);
		this.setLocation(300, 500);
		this.setContentPane(getJContentPane());		
	}

	/**
	 * This method initializes jContentPane
	 * 
	 * @return javax.swing.JPanel
	 */
	private JPanel getJContentPane() {
		if (jContentPane == null) {
			jContentPane = new JPanel();
									
			jContentPane.setLayout(new GridBagLayout());

			// Add procs list
			GridBagConstraints c = new GridBagConstraints();			
			c.fill = GridBagConstraints.BOTH ;
			c.gridwidth = 3;
			c.weightx = 1.0;			
			c.weighty = 50.0;		
			c.gridx = 0;
			c.gridy = 0;					
			jContentPane.add(new JScrollPane(getProcsJList()), c);
			
			// add Console 
			c.weightx = 1.0;
			c.weighty = 50.0;
			c.gridy ++;			
			jContentPane.add(new JScrollPane(getConsolePane()), c);			
			
			c.gridwidth = 1;
			c.weightx = 0.5;
			c.weighty = 1.0;			
			
			// Add GingerJarPath Label					
			c.gridx = 0;
			c.gridy ++;	
			c.weightx = 1;
			jContentPane.add(new javax.swing.JLabel("GingerAgent.jar Path"), c);
			
			// Add GingerJarPath Text
			c.gridx = 0;
			c.gridy ++;
			c.weightx = 1;
			jContentPane.add(GetGingerAgentJarPathTextField(), c);
			
			// Add GingerJarArgs Label						
			c.gridx = 0;
			c.gridy ++;	
			c.weightx = 1;
			jContentPane.add(new javax.swing.JLabel("Ginger Agent Args"), c);
			
			// Add GingerArgs Text
			c.gridx = 0;
			c.gridy ++;	
			c.weightx = 1;			
			jContentPane.add(GetGingerAgentArgsTextField(), c);	
			c.fill = GridBagConstraints.NONE;			
			c.weightx = 0.5;
			
			// Add Attach Button Button						
			c.gridx = 0;
			c.gridy ++;						
			jContentPane.add(getAttachButton(), c );			
						
			//Add Refresh Button
			c.gridx = 1;				
			jContentPane.add(getRefreshButton(), c );
			
			//Add Copy Button
			c.gridx = 2;						
			jContentPane.add(getCopyButton(), c );
			
		}
		return jContentPane;
	}
	
	private Component getConsolePane() {
		if (ConsoleTextArea == null)
		{
			ConsoleTextArea = new JTextArea();
		}
		return ConsoleTextArea;
	}

	private Component GetGingerAgentArgsTextField() {

		if (GingerAgentArgsTextField == null)
		{
			GingerAgentArgsTextField = new JTextArea();
			GingerAgentArgsTextField.setText("Port=8888");
		}
		return GingerAgentArgsTextField;
	}

	private Component GetGingerAgentJarPathTextField() {
		System.out.println("Getting Ginger Jar Path");
		GingerJarPath = GingerAgent.GetJarPath();
		System.out.println("GingerJarPath = " + GingerJarPath);
		
		if (GingerAgentJarPathTextField == null)
		{
			GingerAgentJarPathTextField = new JTextArea();
			GingerAgentJarPathTextField.setText(GingerJarPath);			
		}
		return GingerAgentJarPathTextField;
	}

	/**
	 * This method initializes AttachButton	
	 * 	
	 * @return javax.swing.JButton	
	 */
	private JButton getAttachButton() {
		if (attachButton == null) {
			attachButton = new JButton();
			attachButton.setText("Attach Ginger Agent");			
			attachButton.addActionListener(new java.awt.event.ActionListener() {				
				public void actionPerformed(java.awt.event.ActionEvent e) {
										
					System.out.println("Attach clicked");
					
					String s = (String)ProcsList.getSelectedValue();
					
					if (s == null || s.length() == 0)
					{
						ShowMessage("Please select process from the list");						
						return;
					}
					
					String[] a = s.split(" - ");
					String PID = a[0];
					System.out.println("PID = " + PID);
					GingerJarPath = GingerAgentJarPathTextField.getText();									
					System.out.println("Checking GingerAgent.jar Exist at - " + GingerJarPath);
					File f = new File(GingerJarPath);			
					if(f.exists()) 
					{
						System.out.println("GingerAgent.jar Found");
						String Args = GingerAgentArgsTextField.getText();
						//adding PID to see it also on GingerAgent console						
						if (Args.contains("PID")==false)
						{
							if (Args.length() > 0)
								Args+=",";
							Args+="PID="+PID;
						} 
						
						System.out.println("Before Attach");
						GingerAgent.Attach(GingerJarPath, Args, PID);
						System.out.println("After Attach");
					}
					else
					{
						System.out.println("Error: GingerAgent.jar Not Found");
						ShowMessage("GingerAgent.jar not found at - " + GingerJarPath);
					}
				}

				
			});
		}
		return attachButton;
	}
	
	private void ShowMessage(String msg) {
		JOptionPane.showMessageDialog(null,msg);
		
	}
	
	private JButton getRefreshButton() {
		if (refreshButton == null) {
			refreshButton = new JButton();
			refreshButton.setText("Refresh");			
			refreshButton.addActionListener(new java.awt.event.ActionListener() {

				public void actionPerformed(java.awt.event.ActionEvent e) {
					System.out.println("Getting Java process list");
					ShowProcsList();
				}
			});
		}
		return refreshButton;
	}
	
	private JButton getCopyButton() {
		if (clearButton == null) {
			clearButton = new JButton();
			clearButton.setText("Clear");			
			clearButton.addActionListener(new java.awt.event.ActionListener() {

				public void actionPerformed(java.awt.event.ActionEvent e) {
					ConsoleTextArea.setText("");
				}
			});
		}
		return clearButton;
	}
	
private void ShowProcsList()
{
	System.out.println("Refreshing Procs list");
	listModel.clear();
	
	System.out.println("Getting VirtualMachine.list()");
	try
	{
		// if attach.dll is not in the path the following error will apear
		// java.util.ServiceConfigurationError: com.sun.tools.attach.spi.AttachProvider: Provider sun.tools.attach.WindowsAttachProvider could not be instantiated
		
		java.util.List<VirtualMachineDescriptor> VMs =  VirtualMachine.list();
		if (VMs.size() == 0)
		{
			System.out.println("Error: VirtualMachine.list() size=0, please make sure you have JDK with attach.dll and it is in the path");
		}
		else
		{
			System.out.println("Found " + VMs.size() + " VMs running");
			for (VirtualMachineDescriptor VMD : VMs)
			{
				listModel.addElement(VMD.id() + " - " + VMD.displayName());
			}
		}
	}
	catch (Exception e)
	{
		System.out.println("Error: " + e.getMessage());		
	}
}
	
	private JList getProcsJList() {
		if (ProcsList == null) {								
			ProcsList = new JList(listModel);
			ProcsList.setSelectionMode(DefaultListSelectionModel.SINGLE_SELECTION);							
		}
		return ProcsList;
	}
} 