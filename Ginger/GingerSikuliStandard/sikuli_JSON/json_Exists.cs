using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerSikuliStandard.sikuli_JSON
{
    public class json_Exists
    {
        public json_Pattern jPattern { get; set; }
        public Double timeout { get; set; }
        public bool patternExists { get; set; }
        public json_Result jResult { get; set; }

        public json_Exists(json_Pattern ptrn, Double tmout)
        {
            jPattern = ptrn;
            timeout = tmout;
        }

        public static json_Exists getJExists(String json)
        {
            json_Exists jExists = JsonConvert.DeserializeObject<json_Exists>(json);
            return jExists;
        }
    }
}
