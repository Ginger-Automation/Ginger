#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerSikuliStandard.sikuli_JSON
{
    public class json_WaitVanish
    {
        public json_Pattern jPattern { get; set; }
        public Double timeout { get; set; }
        public bool patternDisappeared { get; set; }
        public json_Result jResult { get; set; }

        public json_WaitVanish(json_Pattern ptrn, Double tmout)
        {
            jPattern = ptrn;
            timeout = tmout;
        }

        public static json_WaitVanish getJWaitVanish(String json)
        {
            json_WaitVanish jWaitVanish = JsonConvert.DeserializeObject<json_WaitVanish>(json);
            return jWaitVanish;
        }
    }
}
