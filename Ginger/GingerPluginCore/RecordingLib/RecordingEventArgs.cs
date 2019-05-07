using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public class RecordingEventArgs
    {
        public object EventArgs { get; set; }

        public eRecordingEvent EventType { get; set; }
    }
}
