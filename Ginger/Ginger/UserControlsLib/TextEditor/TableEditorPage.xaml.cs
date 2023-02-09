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

using Ginger.UserControlsLib.TextEditor.Common;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib.TextEditor
{
    /// <summary>
    /// Interaction logic for TableEditorPage.xaml
    /// </summary>
    public partial class TableEditorPage : Page
    {
        private ICSharpCode.AvalonEdit.TextEditor textEditor;
        private FoldingSection foldingSection;
        private string StartKeyWord;
        private string EndKeyWord;
        private string WorkingSection;
        private int FoldingOffset;
        private int GapBetweenFoldingOffsetAndTableLocaionIndication;
        public TableEditorPage(SelectedContentArgs SelectedContentArgs,string startKeyWord,string endKeyWord, string KeyWordForTableLocationIndication = null)
        {
            InitializeComponent();
            
            GapBetweenFoldingOffsetAndTableLocaionIndication = 0;
            this.textEditor = SelectedContentArgs.TextEditor;
            this.foldingSection = SelectedContentArgs.GetFoldingsAtCaretPosition()[0];
            int CaretLocation = SelectedContentArgs.GetCaretPosition();
            FoldingOffset = SelectedContentArgs.GetFoldingOffSet();
            if (KeyWordForTableLocationIndication != null)
            {
                int indexOfKeyWordForIndication = foldingSection.TextContent.IndexOf(KeyWordForTableLocationIndication);
                if (KeyWordForTableLocationIndication != null && indexOfKeyWordForIndication != -1 &&  CaretLocation - FoldingOffset > indexOfKeyWordForIndication)
                {
                    WorkingSection = foldingSection.TextContent.Substring(indexOfKeyWordForIndication);
                    GapBetweenFoldingOffsetAndTableLocaionIndication = foldingSection.TextContent.Length - WorkingSection.Length;
                }
                    
                else
                    WorkingSection = foldingSection.TextContent;
            }
            else
                WorkingSection = foldingSection.TextContent;

            StartKeyWord = startKeyWord;
            EndKeyWord = endKeyWord;
            InitTable();
        }

        private void InitTable()
        {
            string[] lines = WorkingSection.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            System.Data.DataTable DT = new System.Data.DataTable();
            int iStart = 0;
            int iEnd = 0 ;
            bool StartFound = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith(StartKeyWord) && !StartFound)
                {
                    iStart = i + 1;
                    StartFound = true;
                }
                if (lines[i].Trim().StartsWith(EndKeyWord) && StartFound)
                {
                    iEnd = i;
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
                    if (o.ToString().StartsWith("\"") && o.ToString().EndsWith("\""))
                        s.Append("|").Append(o.ToString() + "");
                    else
                        s.Append("|").Append("\"" + o.ToString() + "\"");
                }
                s.Append("|").Append(Environment.NewLine);
            }
            string st = formatTable(s.ToString());
            string[] rows = st.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string result = string.Empty;
            string tab1 = "\t\t\t";
            foreach (string row in rows)
            {
                result = result + tab1 + row + Environment.NewLine;
            }
            return result;
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
                        if (data[k].Length > ColsWidth[k - 1])  // We found data in column longer than the header mark for redo
                        {
                            ColsWidth[k - 1] = data[k].Length;
                            cols[k - 1] = Pad(cols[k - 1], data[k].Length);
                            bFound = true;
                        }

                        s1 += Pad(data[k], ColsWidth[k - 1]) + "|";
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
            while (s.Length < Len)
            {
                s += " ";
            }
            return s;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string txt = GetTableAsText();
            string[] lines = WorkingSection.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string tmp = "";
            bool bFound = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!bFound)
                {
                    if (lines[i].Trim().StartsWith("|") && lines[i-1].Trim().StartsWith(StartKeyWord))
                    {
                        bFound = true;
                    }
                }

                if (bFound && lines[i].Contains("|"))
                {
                    tmp += lines[i] + Environment.NewLine;
                    if (lines[i + 1].Trim().StartsWith(EndKeyWord))
                        break;
                }
            }
             textEditor.Select(FoldingOffset + GapBetweenFoldingOffsetAndTableLocaionIndication + WorkingSection.IndexOf(StartKeyWord) + StartKeyWord.Length + Environment.NewLine.Length, tmp.Length);
             textEditor.SelectedText = txt;
        }
    }
}
