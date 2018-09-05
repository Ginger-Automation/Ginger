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

        public void AddRowHeader(List<string> headerList)
        {
            logDataTable.Columns.Add("#");
            ColumnLength.Add(2);

            foreach (string header in headerList)
            {
                ColumnLength.Add(header.Length);
                logDataTable.Columns.Add(header);
            }
        }

        public void AddRowValues(List<string> rowList)
        {
            DataRow dataRow = logDataTable.NewRow();
            int colId = 0;

            dataRow[colId++] = logDataTable.Rows.Count + 1;

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
            const string hyp = "-";
            int totRowLen = 0;
            int colLen = 0;

            if (logDataTable != null && logDataTable.Columns.Count > 0)
            {
                totRowLen = GetTotalRowLength();

                // log line
                sbr.AppendLine(hyp.PadRight(totRowLen + logDataTable.Columns.Count + 1, '-'));

                // log table headers
                int col = 0;
                foreach (DataColumn dataCol in logDataTable.Columns)
                {
                    colLen = ColumnLength[col++];
                    sbr.Append("|");
                    sbr.Append(PadSpacesInString(dataCol.ColumnName, colLen));
                }
                sbr.AppendLine("|");

                // log line
                sbr.AppendLine(hyp.PadRight(totRowLen + logDataTable.Columns.Count + 1, '-'));

                // log table rows
                foreach (DataRow dataRow in logDataTable.Rows)
                {
                    col = 0;
                    foreach (DataColumn dataCol in logDataTable.Columns)
                    {
                        string dataColVal = string.Empty;
                        if (dataRow[dataCol] != null)
                        {
                            dataColVal = dataRow[dataCol].ToString();
                        }
                        sbr.Append("|");
                        sbr.Append(PadSpacesInString(dataColVal, ColumnLength[col++]));
                    }
                    sbr.AppendLine("|");
                }

                // log line
                sbr.AppendLine(hyp.PadRight(totRowLen + logDataTable.Columns.Count + 1, '-'));
            }

            return sbr.ToString();
        }

        private static string PadSpacesInString(string str, int maxLen)
        {
            if (str == null || str == string.Empty)
            {
                str = " ";
            }
            return str.PadRight(maxLen);
        }


    }


}
