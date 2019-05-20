using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.GeneralLib
{
    public class ComboGroupedEnumItem
    {
        public static class Fields
        {
            public static string text = "text";
            public static string Value = "text";
            public static string Category = "Value";
        }

        public object text { get; set; }
        public object Value { get; set; }
        public string Category { get; set; }
    }
}
