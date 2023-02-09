#region License
/*
Copyright © 2014-2023 European Support Limited

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

using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using System.Data;
using Ginger.UserControlsLib.TextEditor.Common;

namespace Ginger.UserControlsLib.TextEditor.Gherkin
{
    /// <summary>
    /// Interaction logic for GherkinTableEditorPage.xaml
    /// </summary>
    public partial class GherkinTableEditorPage : Page
    {
        private ICSharpCode.AvalonEdit.TextEditor textEditor;
        private FoldingSection foldingSection;
        
        public GherkinTableEditorPage(SelectedContentArgs SelectedContentArgs)
        {
            InitializeComponent();

            this.textEditor = SelectedContentArgs.TextEditor;
            this.foldingSection = SelectedContentArgs.GetFoldingsAtCaretPosition()[0];

            InitTable();
        }

        private void InitTable()
        {
            string[] lines = foldingSection.TextContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            System.Data.DataTable DT = new System.Data.DataTable();

            int iStart = 0;
            int iEnd = lines.Length;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith("Examples:"))
                {
                    iStart = i + 1;
                    break;
                }
            }

            string[] Cols = null;
            // First row is columns headers

            if (lines.Length > iStart)
                Cols = lines[iStart].Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            else
                return;

            foreach (string col in Cols)
            {
                if (!string.IsNullOrWhiteSpace(col))
                    DT.Columns.Add(col.Trim());
            }

            for (int i = iStart + 1; i < iEnd; i++)
            {
                System.Data.DataRow DR;
                object[] o = GetColsData(lines[i]);
                if (DT.Columns.Count == o.Length)
                    DR = DT.Rows.Add(o);
            }

            TableDataGrid.ItemsSource = DT.AsDataView();
        }

        private object[] GetColsData(string line)
        {
            string[] b = line.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            int leanth = 0;
            for (int i = 0; i < b.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(b[i]))
                    leanth++;
            }
            string[] a = new string[leanth];
            int counter = 0;
            for (int i = 0; i < b.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(b[i]))
                {
                    a[counter] = b[i];
                    counter++;
                }
            }

            object[] ar = new object[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                ar[i] = a[i].Trim();
            }
            return ar;
        }

        private string GetTableAsText()
        {
            StringBuilder s = new StringBuilder("");
            foreach (DataGridColumn DGC in TableDataGrid.Columns)
            {
                s.Append("|").Append(DGC.Header.ToString());
            }
            s.Append("|").Append(Environment.NewLine);

            foreach (DataRowView DRV in TableDataGrid.ItemsSource)
            {
                foreach (object o in DRV.Row.ItemArray)
                {
                    s.Append("|").Append(o.ToString() + "");
                }
                s.Append("|").Append(Environment.NewLine);
            }
            string st = formatTable(s.ToString());
            
            return st;
        }

        private string formatTable(string s)
        {
            //probably not the most fast and efficient algorithm but will work for now ;)
            bool bFound = false;
            string txt = s;
            StringBuilder ft;

            string[] rows = txt.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string[] cols = rows[0].Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            do
            {
                bFound = false;                
                ft = new StringBuilder("");
                rows = txt.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                //First create cols width based on col header length, if later we find data is bigger we will recalc and pad
                int[] ColsWidth = new int[cols.Count()];
                int i = 0;
                foreach (string c in cols)
                {
                    ColsWidth[i] = c.Length;
                    i++;
                }

                foreach (string r in rows)
                {
                    string s1 = "|";
                    string[] data = r.Split(new string[] { "|" }, StringSplitOptions.None);
                                        
                    for (int k = 1; k < data.Length - 1; k++)
                    {
                        if (data[k].Length > ColsWidth[k-1])  // We found data in column longer than the header mark for redo
                        {
                            ColsWidth[k-1] = data[k].Length;
                            cols[k-1] = Pad(cols[k-1], data[k].Length);
                            bFound = true;
                        }

                        s1 += Pad(data[k], ColsWidth[k-1]) + "|";                        
                    }
                    s1 += Environment.NewLine;
                    ft.Append(s1);
                }

                txt = ft.ToString();
            } while (bFound);

            return txt;
        }

        private string Pad(string data, int Len)
        {
            string s = data;
            while (s.Length<Len)
            {
                s += " ";
            }
            return s;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string txt = GetTableAsText();
            string[] lines = foldingSection.TextContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            System.Data.DataTable DT = new System.Data.DataTable();
            string tmp = "";
            bool bFound = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!bFound)
                {
                    if (lines[i].Trim().StartsWith("|"))
                    {
                        bFound = true;
                    }
                }

                if (bFound && lines[i].Contains("|"))
                {
                    tmp += lines[i] + Environment.NewLine;
                }                                
            }

            textEditor.Text = textEditor.Text.Replace(tmp.Substring(0,tmp.LastIndexOf("|")+1), txt);
        }
    }
}
