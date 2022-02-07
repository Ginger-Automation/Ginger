using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerSikuliStandard.sikuli_JSON
{
    public class json_Wait
    {
        public json_Pattern jPattern { get; set; }
        public Double timeout { get; set; }

        public json_Wait(json_Pattern ptrn, Double tmout)
        {
            jPattern = ptrn;
            timeout = tmout;
        }
    }
}
