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
using System.Text;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class TelemetryRecord
    {
        public TelemetryIndex index { get; set; }

        private object TelemetryObject;

        public TelemetryRecord(string index, object obj)
        {
            // ToLower() is required for elastic index
            this.index = new TelemetryIndex() {  _index = index.ToLower() , _id = Guid.NewGuid().ToString()};
            TelemetryObject = obj;
        }

        public object getTelemetry()
        {
            return TelemetryObject;
            
        }
    }
}
