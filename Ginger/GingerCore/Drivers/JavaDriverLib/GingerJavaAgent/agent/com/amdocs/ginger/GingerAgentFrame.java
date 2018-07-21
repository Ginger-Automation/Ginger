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
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Toolkit;
import java.awt.datatransfer.Clipboard;
import java.awt.datatransfer.StringSelection;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Calendar;

import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import javax.swing.SwingUtilities;

//TODO: rename to GingerAgentConsole
public class GingerAgentFrame extends JFrame {

	public final static String GINGER_AGENT_CONSOLE = "Ginger Agent Console " + GingerAgent.GINGER_JAVA_AGENT_VERSION;
	
	private static final long serialVersionUID = 1L;
	private JPanel jContentPane = null;
	private JButton DumpWindowsInfoButton = null;
	private JButton ClearButton = null;
	private JButton CopyButton = null;
	private JLabel StatusLabel = null;
	
	private JTextArea InfoTextArea = null;

	/**
	 * This is the default constructor
	 */
	public GingerAgentFrame() {
		super();
		initialize();		
	}	

	/**
	 * This method initializes this
	 * 
	 * @return void
	 */
	private void initialize() {
		this.setSize(532, 406);
		this.setContentPane(getJContentPane());
		this.setTitle(GINGER_AGENT_CONSOLE);	
		Utils.redirectSystemStreams(InfoTextArea);
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

			GridBagConstraints c = new GridBagConstraints();			
			c.fill = GridBagConstraints.BOTH ;
			c.gridwidth = 3;
			c.weightx = 1.0;			
			c.weighty = 100.0;		
			c.anchor = GridBagConstraints.PAGE_START;
			c.gridy = 0;
			c.gridx = 0;
			
			jContentPane.add(new JScrollPane(getInfoTextArea()), c);
									
			
			c.weighty = 1.0;					
			
			//Add Status Label
			c.gridy++;			
			c.gridx = 0;						
			jContentPane.add(getStatusLabel(), c );
			
			// Add Dump Forms Button Button
			c.gridwidth = 1;
			c.weightx = 0.8;
			c.fill = GridBagConstraints.NONE;
			c.anchor = GridBagConstraints.PAGE_END;
			c.gridy++;
			c.gridx = 0;								
			jContentPane.add(getDumpFormsButton(), c );			
						
			//Add Clear Button
			c.gridx = 1;		
			jContentPane.add(getClearButton(), c );
			
			//Add Copy Button
			c.gridx = 2;					
			jContentPane.add(getCopyButton(), c );		
		}
		return jContentPane;
	}

	private JLabel getStatusLabel() {
		if (StatusLabel == null) {
			StatusLabel = new JLabel();
			StatusLabel.setText("Ready");						
		}
		return StatusLabel;
	}
	
	public void setStatus(String txt) {
		StatusLabel.setText(txt);		
	}	
	
	/**
	 * This method initializes GoButton	
	 * 	
	 * @return javax.swing.JButton	
	 */
	private JButton getDumpFormsButton() {
		if (DumpWindowsInfoButton == null) {
			DumpWindowsInfoButton = new JButton();
			DumpWindowsInfoButton.setText("Dump Windows Info");			
			DumpWindowsInfoButton.addActionListener(new java.awt.event.ActionListener() {
				public void actionPerformed(java.awt.event.ActionEvent e) {
					String s = FormsDumpInfo.DumpFormsInfo();
					WriteLog(s);
				}
			});
		}
		return DumpWindowsInfoButton;
	}
	
	private JButton getClearButton() {
		if (ClearButton == null) {
			ClearButton = new JButton();
			ClearButton.setText("Clear");			
			ClearButton.addActionListener(new java.awt.event.ActionListener() {

				public void actionPerformed(java.awt.event.ActionEvent e) {
					InfoTextArea.setText("");
				}
			});
		}
		return ClearButton;
	}
	
	private JButton getCopyButton() {
		if (CopyButton == null) {
			CopyButton = new JButton();
			CopyButton.setText("Copy");			
			CopyButton.addActionListener(new java.awt.event.ActionListener() {

				public void actionPerformed(java.awt.event.ActionEvent e) {
					
					StringSelection txt = new StringSelection(InfoTextArea.getText());
					Clipboard clpbrd = Toolkit.getDefaultToolkit().getSystemClipboard();
					clpbrd.setContents(txt, null);
				}
			});
		}
		return CopyButton;
	}

	public void WriteLog(final String txt)
	{		
		 SwingUtilities.invokeLater(new Runnable() {
		      public void run() {
		    	  	DateFormat df = new SimpleDateFormat("dd/MM/yy HH:mm:ss.SSS");
					Calendar calobj = Calendar.getInstance();		
					String s = df.format(calobj.getTime());		 
					s+= " - " + txt + "\n";					
					InfoTextArea.append(s);					

					//Scroll to end
					InfoTextArea.setCaretPosition(InfoTextArea.getDocument().getLength());
					
					//Refresh so the user see the log now 
					//TODO: Needs to be done on separate thread
					// to heavy so commented out
		      }
		    });
	}
	

/**
 * This method initializes InfoTextArea	
 * 	
 * @return javax.swing.JTextArea	
 */
private JTextArea getInfoTextArea() {
	if (InfoTextArea == null) {
		InfoTextArea = new JTextArea();		
	}
	return InfoTextArea;
}

} 