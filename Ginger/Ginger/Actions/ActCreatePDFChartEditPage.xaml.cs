#region License
/*
Copyright © 2014-2019 European Support Limited

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
#endregion

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for XLSReadDataToVariablesPage.xaml
    /// </summary>
    public partial class ActCreatePDFChartEditPage 
    {       
        public ActionEditPage actp;
        private ActCreatePDFChart mAct;        

        public ActCreatePDFChartEditPage(ActCreatePDFChart act)
        {     
                InitializeComponent();
                mAct = act;
                Bind();
                mAct.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();                   
        }
                
        public void Bind()
        {
            DataFileNameTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActCreatePDFChart.Fields.DataFileName), nameof (ActInputValue.Value));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ParamsComboBox.ComboBox, ComboBox.SelectedValueProperty, mAct, ActCreatePDFChart.Fields.ParamName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ParamsComboBox.ComboBox, ComboBox.ItemsSourceProperty, mAct, ActCreatePDFChart.Fields.ParamList);
            DataFileNameTextBox.ValueTextBox.TextChanged += ValueTextBox_TextChanged;
            mAct.ParamList = SetParamsCombo();
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            mAct.ParamList= SetParamsCombo();            
        }

        private void BrowseDataButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg=  new System.Windows.Forms.OpenFileDialog();

            dlg.DefaultExt = "*.csv";
            dlg.Filter = "csv Files (*.csv)|*.csv";
            string SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper(); 

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // replace Absolute file name with relative to solution
                string FileName = dlg.FileName.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                DataFileNameTextBox.ValueTextBox.Text = FileName;                
            }           
        }

        private List<string> SetParamsCombo()
        {
            List<string> tmp=new List<string>();
            object tempFileName = mAct.GetDataFileName();            
            string dataFileName = Convert.ToString(tempFileName);
            
                if (File.Exists(dataFileName))
                {
                    StreamReader sr = new StreamReader(dataFileName);
                    var lines = new List<string[]>();

                    while (!sr.EndOfStream && lines.Count<=0)
                    {
                        string[] Line = sr.ReadLine().Split(',').Select(a=>a.Trim()).ToArray();
                        lines.Add(Line);
                    };
                    tmp = lines[0].ToList();
                    for (int i = 0; i < tmp.Count; i++)
                    {
                        if (tmp[i].Contains("\""))
                            tmp[i]=tmp[i].Replace("\"", "");
                    }
                }
                
            return tmp;
            
        }     
    }
}
