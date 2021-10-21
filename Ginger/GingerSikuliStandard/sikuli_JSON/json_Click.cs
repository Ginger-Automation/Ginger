using GingerSikuliStandard.sikuli_REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerSikuliStandard.sikuli_JSON
{
    public class json_Click
    {
        public json_Pattern jPattern { get; set; }
        public String jKeyModifier { get; set; }

        public json_Click(json_Pattern ptrn, KeyModifier kmod = KeyModifier.NONE)
        {
            jPattern = ptrn;
            jKeyModifier = kmod.ToString();
        }
    }
}
