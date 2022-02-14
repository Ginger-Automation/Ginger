using GingerSikuliStandard.sikuli_REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerSikuliStandard.sikuli_JSON
{
    public class json_DragDrop
    {
        public json_Pattern jClickPattern { get; set; }
        public json_Pattern jDropPattern { get; set; }
        public String jKeyModifier { get; set; }

        public json_DragDrop(json_Pattern clickPattern, json_Pattern dropPattern, KeyModifier kmod = KeyModifier.NONE)
        {
            jClickPattern = clickPattern;
            jDropPattern = dropPattern;
            jKeyModifier = kmod.ToString();
        }
    }
}
