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
import java.awt.GridLayout;
import java.awt.Insets;
import javax.swing.DefaultListModel;
import javax.swing.JList;
import java.awt.Component;
import javax.swing.JDialog;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JTable;
import javax.swing.ListSelectionModel;

/**
 * Example of components laid out in a grid
 */
public class BasicSwingComponents extends javax.swing.JFrame {
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	private javax.swing.JButton ivjJButton1 = null;
	private javax.swing.JCheckBox ivjJCheckBox1 = null;
	private javax.swing.JComboBox ivjJComboBox1 = null;
	private javax.swing.JPanel ivjJFrameContentPane = null;
	private javax.swing.JLabel ivjJLabel1 = null;
	private javax.swing.JPasswordField ivjJPasswordField1 = null;
	private javax.swing.JProgressBar ivjJProgressBar1 = null;
	private javax.swing.JRadioButton ivjJRadioButton1 = null;
	private javax.swing.JScrollBar ivjJScrollBar1 = null;
	private javax.swing.JSlider ivjJSlider1 = null;
	private javax.swing.JTextArea ivjJTextArea1 = null;
	private javax.swing.JTextField ivjJTextField1 = null;
	private javax.swing.JToggleButton ivjJToggleButton1 = null;
	private javax.swing.JList ivJList1 = null;
	private javax.swing.JList ivJList2 = null;
	private javax.swing.JTable ivJTable = null;
	private javax.swing.JMenuBar ivjJMenuBar1 = null;
	private javax.swing.JTabbedPane ivjJTabbedPane1 = null;

	public BasicSwingComponents() {
		super();
		initialize();
	}

	private javax.swing.JPanel getJFrameContentPane() {
		if (ivjJFrameContentPane == null) {
			ivjJFrameContentPane = new JPanel(new GridBagLayout());

			GridBagConstraints constraints = new GridBagConstraints();
			constraints.anchor = GridBagConstraints.WEST;
			constraints.insets = new Insets(10, 10, 10, 10);
			constraints.gridx = 0;
			constraints.gridy = 0;
			ivjJFrameContentPane.setName("JFrameContentPane");
			constraints.gridx++;
			ivjJFrameContentPane.add(getJButton1(), constraints);
			constraints.gridx++;

			ivjJFrameContentPane.add(getJRadioButton1(), constraints);
			constraints.gridx++;
			ivjJFrameContentPane.add(getJToggleButton1(), constraints);
			constraints.gridx++;
			ivjJFrameContentPane.add(getJLabel1(), constraints);
			constraints.gridx++;
			ivjJFrameContentPane.add(getJPasswordField1(), constraints);
			constraints.gridx++;
			ivjJFrameContentPane.add(getJTextField1(), constraints);
			constraints.gridx++;
			ivjJFrameContentPane.add(getJTextArea1(), constraints);
			constraints.gridx++;

			ivjJFrameContentPane.add(getJSlider1(), constraints);
			constraints.gridx++;
			ivjJFrameContentPane.add(getJScrollBar1(), constraints);
			constraints.gridx++;
			ivjJFrameContentPane.add(getJProgressBar1(), constraints);
			constraints.gridx++;
			ivjJFrameContentPane.add(getJListSingleSelection(), constraints);
			constraints.gridx++;
			ivjJFrameContentPane.add(getJListMultipleSelection(), constraints);
			constraints.gridx = 0;
			constraints.gridy++;
			ivjJFrameContentPane.add(getJtable(), constraints);

			constraints.gridx++;
			ivjJFrameContentPane.add(getTabbedPane1(),constraints);
			constraints.gridx++;
			ivjJFrameContentPane.add(getJComboBox1(),constraints);
			
			constraints.gridx++;
			ivjJFrameContentPane.add(getJCheckBox1(),constraints);
			
			this.setJMenuBar(getMenuBar1());
		}
		return ivjJFrameContentPane;
	}

	/**
	 * Return the JButton1 property value.
	 * 
	 * @return javax.swing.JButton
	 */
	private javax.swing.JButton getJButton1() {
		if (ivjJButton1 == null) {
			ivjJButton1 = new javax.swing.JButton();
			ivjJButton1.setName("JButton1");
			ivjJButton1.setText("JButton");
			ivjJButton1.addActionListener(new java.awt.event.ActionListener() {
				public void actionPerformed(java.awt.event.ActionEvent e) {
					ButtonClick1();
				}
			});
		}
		return ivjJButton1;
	}

	private javax.swing.JTable getJtable() {
		if (ivJTable == null) {

			Object rowData[][] = { { "Row1-Column1", "Row1-Column2", "Row1-Column3" },
					{ "Row2-Column1", "Row2-Column2", "Row2-Column3" } };
			Object columnNames[] = { "Column One", "Column Two", "Column Three" };
			ivJTable = new JTable(rowData, columnNames);
			ivJTable.setName("JTable");
		}		
		return ivJTable;
	}

	private javax.swing.JList getJListSingleSelection() {
		if (ivJList1 == null) {

			DefaultListModel fruitsName = new DefaultListModel();

			fruitsName.addElement("Apple");
			fruitsName.addElement("Grapes");
			fruitsName.addElement("Mango");
			fruitsName.addElement("Peer");

			ivJList1 = new JList(fruitsName);
			ivJList1.setName("SingleSelectionjList");
			ivJList1.setSelectionMode(ListSelectionModel.SINGLE_SELECTION);
			ivJList1.setSelectedIndex(0);
			ivJList1.setVisibleRowCount(3);

		}
		return ivJList1;
	}

	private javax.swing.JList getJListMultipleSelection() {
		if (ivJList2 == null) {

			DefaultListModel fruitsName = new DefaultListModel();

			fruitsName.addElement("Apple");
			fruitsName.addElement("Grapes");
			fruitsName.addElement("Mango");
			fruitsName.addElement("Peer");

			ivJList2 = new JList(fruitsName);
			ivJList2.setName("MultiSelectionJList");
			ivJList2.setSelectionMode(ListSelectionModel.MULTIPLE_INTERVAL_SELECTION);
			ivJList2.setSelectedIndex(0);
			ivJList2.setVisibleRowCount(3);

		}
		return ivJList2;
	}

	void ButtonClick1() {
		JOptionPane.showMessageDialog(null, "Hello World !");
	}
	
	/**
	 * Return the JCheckBox1 property value.
	 * @return javax.swing.JCheckBox
	 */
	private javax.swing.JCheckBox getJCheckBox1() {
		if (ivjJCheckBox1 == null) {
			ivjJCheckBox1 = new javax.swing.JCheckBox();
			ivjJCheckBox1.setName("JCheckBox1");
			ivjJCheckBox1.setText("JCheckBox");
		}
		return ivjJCheckBox1;
	}

	/**
	 * Return the JComboBox1 property value. 
	 * @return javax.swing.JComboBox
	 */
	private javax.swing.JComboBox getJComboBox1() {
		if (ivjJComboBox1 == null) {
			ivjJComboBox1 = new javax.swing.JComboBox();
			ivjJComboBox1.setName("JComboBox1");
			ivjJComboBox1.addItem("Item 1");
			ivjJComboBox1.addItem("Item 2");
			ivjJComboBox1.addItem("Item 3");
		}
		return ivjJComboBox1;
	}

	/**
	 * Return the JLabel1 property value.
	 * @return javax.swing.JLabel
	 */
	private javax.swing.JLabel getJLabel1() {
		if (ivjJLabel1 == null) {
			ivjJLabel1 = new javax.swing.JLabel();
			ivjJLabel1.setName("JLabel1");
			ivjJLabel1.setText("JLabel");
		}
		return ivjJLabel1;
	}

	/**
	 * Return the JPasswordField1 property value.
	 * @return javax.swing.JPasswordField
	 */
	private javax.swing.JPasswordField getJPasswordField1() {
		if (ivjJPasswordField1 == null) {
			ivjJPasswordField1 = new javax.swing.JPasswordField();
			ivjJPasswordField1.setName("JPasswordField1");
		}
		return ivjJPasswordField1;
	}

	/**
	 * Return the JProgressBar1 property value.
	 * @return javax.swing.JProgressBar
	 */
	private javax.swing.JProgressBar getJProgressBar1() {
		if (ivjJProgressBar1 == null) {
			ivjJProgressBar1 = new javax.swing.JProgressBar();
			ivjJProgressBar1.setName("JProgressBar1");
			ivjJProgressBar1.setValue(50);
		}
		return ivjJProgressBar1;
	}

	/**
	 * Return the JRadioButton1 property value.
	 * @return javax.swing.JRadioButton
	 */
	private javax.swing.JRadioButton getJRadioButton1() {
		if (ivjJRadioButton1 == null) {
			ivjJRadioButton1 = new javax.swing.JRadioButton();
			ivjJRadioButton1.setName("JRadioButton1");
			ivjJRadioButton1.setText("JRadioButton");
		}
		return ivjJRadioButton1;
	}

	/**
	 * Return the JScrollBar1 property value.
	 * @return javax.swing.JScrollBar
	 */
	private javax.swing.JScrollBar getJScrollBar1() {
		if (ivjJScrollBar1 == null) {
			ivjJScrollBar1 = new javax.swing.JScrollBar();
			ivjJScrollBar1.setName("JScrollBar1");
		}
		return ivjJScrollBar1;
	}

	/**
	 * Return the JSlider1 property value.
	 * @return javax.swing.JSlider
	 */
	private javax.swing.JSlider getJSlider1() {
		if (ivjJSlider1 == null) {
			ivjJSlider1 = new javax.swing.JSlider();
			ivjJSlider1.setName("JSlider1");
		}
		return ivjJSlider1;
	}

	/**
	 * Return the JTextArea1 property value.
	 * @return javax.swing.JTextArea
	 */
	private javax.swing.JTextArea getJTextArea1() {
		if (ivjJTextArea1 == null) {
			ivjJTextArea1 = new javax.swing.JTextArea();
			ivjJTextArea1.setName("JTextArea1");
			ivjJTextArea1.setRows(3);
			ivjJTextArea1.setColumns(7);
		}
		return ivjJTextArea1;
	}

	/**
	 * Return the JTextField1 property value.
	 * @return javax.swing.JTextField
	 */
	private javax.swing.JTextField getJTextField1() {
		if (ivjJTextField1 == null) {
			ivjJTextField1 = new javax.swing.JTextField();
			ivjJTextField1.setName("JTextField1");
			ivjJTextField1.setText("JTextField");
		}
		return ivjJTextField1;
	}

	/**
	 * Return the JToggleButton1 property value.
	 * @return javax.swing.JToggleButton
	 */
	private javax.swing.JToggleButton getJToggleButton1() {
		if (ivjJToggleButton1 == null) {
			ivjJToggleButton1 = new javax.swing.JToggleButton();
			ivjJToggleButton1.setName("JToggleButton1");
			ivjJToggleButton1.setText("JToggleButton");
		}
		return ivjJToggleButton1;
	}

	private javax.swing.JMenuBar getMenuBar1() {
		if (ivjJMenuBar1 == null) {
			ivjJMenuBar1 = new javax.swing.JMenuBar();
			ivjJMenuBar1.setName("Application");
			
			javax.swing.JMenu jmFile=new javax.swing.JMenu("File");
			jmFile.setName("File");
			
			javax.swing.JMenuItem jmiNew=new javax.swing.JMenuItem("New");
			javax.swing.JMenuItem jmiOpen=new javax.swing.JMenuItem("Open");
			javax.swing.JMenuItem jmiClose=new javax.swing.JMenuItem("Closed");
			
			jmFile.add(jmiNew);	
			jmFile.add(jmiOpen);
			jmFile.add(jmiClose);			
			ivjJMenuBar1.add(jmFile);			
			
			jmiNew.addActionListener(new java.awt.event.ActionListener() {
				public void actionPerformed(java.awt.event.ActionEvent e) {					
					JOptionPane.showConfirmDialog(getJFrameContentPane(), "New Menu Clicked !","Menu Click Info",JOptionPane.YES_NO_CANCEL_OPTION);					
				}
			});
			
			jmiOpen.addActionListener(new java.awt.event.ActionListener() {
				public void actionPerformed(java.awt.event.ActionEvent e) {
					JOptionPane.showMessageDialog(getJFrameContentPane(), "Open Menu Clicked !","Menu Click Info",JOptionPane.INFORMATION_MESSAGE);					
				}
			});
			
			jmiClose.addActionListener(new java.awt.event.ActionListener() {
				public void actionPerformed(java.awt.event.ActionEvent e) {
					JOptionPane.showMessageDialog(getJFrameContentPane(), "Close Menu Clicked !","Menu Click Info",JOptionPane.INFORMATION_MESSAGE);
				}
			});		
		}
		return ivjJMenuBar1;
	}
	
	private javax.swing.JTabbedPane getTabbedPane1() {
		if (ivjJTabbedPane1 == null) {
			ivjJTabbedPane1 = new javax.swing.JTabbedPane();
			ivjJTabbedPane1.setName("JTabbedPane");
			Component c = new javax.swing.JLabel("This is First Tab");
			ivjJTabbedPane1.addTab("Tab 1",c );	
			ivjJTabbedPane1.addTab("Tab 2", new javax.swing.JLabel("This is Second Tab"));			
		}
		return ivjJTabbedPane1;
	}	

	/**
	 * Initialize the class.
	 */
	private void initialize() {
		this.setName("JFrame1");
		this.setDefaultCloseOperation(javax.swing.WindowConstants.DISPOSE_ON_CLOSE);
		this.setBounds(45, 25, 317, 273);
		this.setTitle("Basic Swing Components");
		this.setContentPane(getJFrameContentPane());
	}
}
