using System;

namespace GingerCore.GeneralLib
{
    public class ComboItem
    {
        public static class Fields
        {
            public static string text = "text";
            public static string Value = "Value";
        }

        public override String ToString()
        {
            return text;
        }

        public string text { get; set; }
        public object Value { get; set; }
    }
}
