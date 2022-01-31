using GingerSikuliStandard.sikuli_REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerSikuliStandard.sikuli_JSON
{
    public class json_Type
    {
        public json_Pattern jPattern { get; set; }
        public String jKeyModifier { get; set; }
        public String text { get; set; }

        public json_Type(json_Pattern ptrn, String txt, KeyModifier kmod = KeyModifier.NONE)
        {
            jPattern = ptrn;
            jKeyModifier = kmod.ToString();
            text = txt;
        }
    }
}
