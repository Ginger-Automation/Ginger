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
