using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace Amdocs.Ginger.CoreNET.ActionsLib
{
    class ActExcelNPOI
    {
        public static void Main()
        {
            ReadExcel();
        }
        private static void ReadExcel()
        {
            HSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(@"c:\test.xls", FileMode.Open, FileAccess.Read))
            {
                hssfwb = new HSSFWorkbook(file);
            }

            ISheet sheet = hssfwb.GetSheet("Arkusz1");
            for (int row = 0; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null) //null is when the row only contains empty cells 
                {
                    //MessageBox.Show(string.Format("Row {0} = {1}", row, sheet.GetRow(row).GetCell(0).StringCellValue));
                }
            }
        }
    }
}
