using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IExcelAction
    {
        void ReadData();
        void WriteData();
        void ReadCellData();
    }
}
