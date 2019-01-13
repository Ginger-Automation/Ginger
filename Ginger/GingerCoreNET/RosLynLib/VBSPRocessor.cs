using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RosLynLib
{
    // Prep class for VBS calc in C#
    class VBSPRocessor
    {
        public string FormatDateTime(string date, string format)
        {
    //            0 = vbGeneralDate - Default.Returns date: mm / dd / yy and time if specified: hh: mm: ss PM/ AM.
    //1 = vbLongDate - Returns date: weekday, monthname, year
//2 = vbShortDate - Returns date: mm / dd / yy
//3 = vbLongTime - Returns time: hh: mm: ss PM/ AM
//4 = vbShortTime - Return time: hh: mm
             //TODO: switch case on enum int/string
             // trim in 
             DateTime dateTime = DateTime.Parse(date);
            return dateTime.ToString();
                
        }


    }
}
