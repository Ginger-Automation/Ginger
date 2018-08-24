using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    
    public class NodeActionOutput
    {
        public List<NodeActionOutputValue> Values = new List<NodeActionOutputValue>();

        public void Add(string key, string value, string path = null)
        {
            Values.Add(new NodeActionOutputValue() { Param = key, ValueString = value, Path = path });
        }

        public void Add(string key, byte[] value, string path = null)
        {
            Values.Add(new NodeActionOutputValue() { Param = key, ValueByteArray = value, Path = path });
        }

        public string this[string key]
        {
            get
            {
                return (from x in Values where x.Param == key select x.ValueString).FirstOrDefault();
            }
        }

        public byte[] getBytes(string key)
        {
            return (from x in Values where x.Param == key select x.ValueByteArray).FirstOrDefault();
        }
    }

}
