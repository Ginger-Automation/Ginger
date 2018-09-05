using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Amdocs.Ginger.CoreNET.GeneralLib
{
    public class FormatTextTable
    {
        DataTable logDataTable = new DataTable();
        List<string> HeadersList = new List<string>();
        List<int> ColumnLength = new List<int>();

        public void AddRowHeader(ArrayList headerList)
        {
            foreach (string header in headerList)
            {
                ColumnLength.Add(header.Length);
                logDataTable.Columns.Add(header);
            }
        }

        public void AddRowValues(ArrayList rowList)
        {
            DataRow dataRow = logDataTable.NewRow();
            int colId = 0; 
            foreach (string colValue in rowList)
            {
                if (colValue != null && colValue.Length > ColumnLength[colId])
                    ColumnLength[colId] = colValue.Length;
                dataRow[colId++] = colValue; 
            }
            logDataTable.Rows.Add(dataRow);
        }

        private int GetTotalRowLength()
        {
            int totRowLen = 0;
            if (ColumnLength.Count >0 )
            {
                foreach (int len in ColumnLength)
                {
                    totRowLen += len;
                }
            }
            return totRowLen;
        }


        public string FormatLogTable()
        {
            StringBuilder sbr = new StringBuilder();
            string hyp = "-";
            int rowNum = 0;
            int totRowLen = 0;
            int colLen = 0;

            totRowLen = GetTotalRowLength();

            sbr.AppendLine(hyp.PadRight(totRowLen + logDataTable.Columns.Count + 1, '-'));
            foreach (DataRow row in logDataTable.Rows)
            {
                rowNum++;
                if (rowNum == 1)
                {
                    int col = 0;
                    foreach (DataColumn dataCol in logDataTable.Columns)
                    {
                        colLen = ColumnLength[col++];
                        sbr.Append("|");
                        sbr.Append(PadSpacesInString(dataCol.ColumnName, colLen));
                    }
                    sbr.AppendLine("|");
                }
                else if (rowNum == 2)
                {
                    sbr.AppendLine(hyp.PadRight(totRowLen + logDataTable.Columns.Count + 1 , '-'));
                }
                else
                {
                    int col = 0;
                    foreach (DataColumn dataCol in logDataTable.Columns)
                    {
                        string dataColVal = row[dataCol].ToString();
                        sbr.Append("|");
                        sbr.Append(PadSpacesInString(dataColVal, ColumnLength[col++]));
                    }
                    sbr.AppendLine("|");
                }                
            }
            sbr.AppendLine(hyp.PadRight(totRowLen + logDataTable.Columns.Count + 1, '-'));

            return sbr.ToString();
        }

        private string PadSpacesInString(string str, int maxLen)
        {
            string formatStr = string.Empty; 
            if (str != null && str.Length > 0)
            {
                formatStr = str.PadRight(maxLen);
            } else
            {
                str = " ";
                formatStr = str.PadRight(maxLen);
            }
            return formatStr;
        }


    }


}
