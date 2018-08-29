using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.GeneralLib
{
    public class FormatTextTable
    {
        List<list2strings> twoColumnList = new List<list2strings>();
        List<list3strings> threeColumnList = new List<list3strings>();
        List<list4strings> fourColumnList = new List<list4strings>();

        int col1len = 0;
        int col2len = 0;
        int col3len = 0;
        int col4len = 0;
        
        public void clear()
        {
            if (twoColumnList.Count > 0)
            {
                twoColumnList.Clear();
            }
            if (threeColumnList.Count > 0)
            {
                threeColumnList.Clear();
            }
            col1len = col2len = col3len = col4len = 0;
        }

        public void AddRowHeader(string header1, string header2)
        {
            twoColumnList.Add( new list2strings(header1, header2));
            if (header1.Length > col1len) col1len = header1.Length;
            if (header2.Length > col2len) col2len = header2.Length;
        }

        public void AddRowHeader(string header1, string header2, string header3)
        {
            threeColumnList.Add(new list3strings(header1, header2, header3));
            if (header1.Length > col1len) col1len = header1.Length;
            if (header2.Length > col2len) col2len = header2.Length;
            if (header3.Length > col3len) col3len = header3.Length;
        }

        public void AddRowHeader(string header1, string header2, string header3, string header4)
        {
            fourColumnList.Add(new list4strings(header1, header2, header3, header4));
            if (header1.Length > col1len) col1len = header1.Length;
            if (header2.Length > col2len) col2len = header2.Length;
            if (header3.Length > col3len) col3len = header3.Length;
            if (header4.Length > col3len) col4len = header4.Length;
        }

        public void AddRowValues(string column1, string column2)
        {
            twoColumnList.Add(new list2strings(column1, column2));
            if (column1 != null && column1.Length > col1len) col1len = column1.Length;
            if (column2 != null && column2.Length > col2len) col2len = column2.Length;
        }

        public void AddRowValues(string column1, string column2, string column3)
        {
            threeColumnList.Add(new list3strings(column1, column2, column3));
            if (column1 != null && column1.Length > col1len) col1len = column1.Length;
            if (column2 != null && column2.Length > col2len) col2len = column2.Length;
            if (column3 != null && column3.Length > col3len) col3len = column3.Length;
        }

        public void AddRowValues(string column1, string column2, string column3, string column4)
        {
            fourColumnList.Add(new list4strings(column1, column2, column3, column4));
            if (column1 != null && column1.Length > col1len) col1len = column1.Length;
            if (column2 != null && column2.Length > col2len) col2len = column2.Length;
            if (column3 != null && column3.Length > col3len) col3len = column3.Length;
            if (column4 != null && column4.Length > col3len) col4len = column4.Length;
        }

        public string CreateTable()
        {
            StringBuilder sbr = new StringBuilder();
            int totalChars = 0;
            string hyp = "-";

            if (twoColumnList.Count > 0)
            {
                totalChars = col1len + col2len + 3;
                sbr.AppendLine(hyp.PadRight(totalChars, '-'));
                int rowNum = 1;

                foreach (list2strings twoColRow in twoColumnList)
                {
                    if (rowNum == 2)
                        sbr.AppendLine(hyp.PadRight(totalChars, '-'));
                    sbr.Append("|");
                    sbr.Append(PadSpacesInString(twoColRow.column1, col1len));
                    sbr.Append("|");
                    sbr.Append(PadSpacesInString(twoColRow.column2, col2len));
                    sbr.AppendLine("|");
                    rowNum++;
                }
                sbr.AppendLine(hyp.PadRight(totalChars, '-'));
            }
            else if (threeColumnList.Count> 0)
            {
                totalChars = col1len + col2len + col3len + 4;
                sbr.AppendLine(hyp.PadRight(totalChars, '-'));
                int rowNum = 1;

                foreach (list3strings threeColRow in threeColumnList)
                {
                    if (rowNum == 2)
                        sbr.AppendLine(hyp.PadRight(totalChars, '-'));
                    sbr.Append("|");
                    sbr.Append(PadSpacesInString(threeColRow.column1, col1len));
                    sbr.Append("|");
                    sbr.Append(PadSpacesInString(threeColRow.column2, col2len));
                    sbr.Append("|");
                    sbr.Append(PadSpacesInString(threeColRow.column3, col3len));
                    sbr.AppendLine("|");
                    rowNum++;
                }
                sbr.AppendLine(hyp.PadRight(totalChars, '-'));
            }
            else if (fourColumnList.Count > 0)
            {
                totalChars = col1len + col2len + col3len + col4len + 5;
                sbr.AppendLine(hyp.PadRight(totalChars, '-'));
                int rowNum = 1;

                foreach (list4strings fourColRow in fourColumnList)
                {
                    if (rowNum == 2)
                        sbr.AppendLine(hyp.PadRight(totalChars, '-'));
                    sbr.Append("|");
                    sbr.Append(PadSpacesInString(fourColRow.column1, col1len));
                    sbr.Append("|");
                    sbr.Append(PadSpacesInString(fourColRow.column2, col2len));
                    sbr.Append("|");
                    sbr.Append(PadSpacesInString(fourColRow.column3, col3len));
                    sbr.AppendLine("|");
                    rowNum++;
                }
                sbr.AppendLine(hyp.PadRight(totalChars, '-'));
            }

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


        private class list2strings
        {
            public string column1;
            public string column2;

            public list2strings(string col1, string col2)
            {
                column1 = col1; column2 = col2;
            }
        }

        private class list3strings
        {
            public string column1;
            public string column2;
            public string column3;

            public list3strings(string col1, string col2, string col3)
            {
                column1 = col1; column2 = col2; column3 = col3;
            }
        }

        private class list4strings
        {
            public string column1;
            public string column2;
            public string column3;
            public string column4;

            public list4strings(string col1, string col2, string col3, string col4)
            {
                column1 = col1; column2 = col2; column3 = col3; column4 = col4;
            }
        }

    }


}
