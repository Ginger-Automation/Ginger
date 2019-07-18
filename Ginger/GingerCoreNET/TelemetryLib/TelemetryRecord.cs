using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class TelemetryRecord
    {
        public TelemetryIndex index { get; set; }

        public TelemetryRecord(string index, string type, string id)
        {
            this.index = new TelemetryIndex() {  _index = index, _type = type, _id = id};
        }
    }
}
