using Amdocs.Ginger.Plugin.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class NodeActionOutputValue :IGingerActionOutputValue
    {
        private object mValue;
        private OutputValueType mOutputValueType;

        public enum OutputValueType
        {
            String,
            JSON,
            ByteArray,
            //TODO: ???
        }

        public string Param { get; set; }

        public string Path { get; set; }

        public string ValueString
        {
            get
            {
                return (string)mValue;
            }
            set
            {
                mOutputValueType = OutputValueType.String;
                mValue = value;
            }
        }


        public byte[] ValueByteArray
        { get { return (byte[])mValue; } set { mOutputValueType = OutputValueType.ByteArray; mValue = value; } }

        public object Value { get { return mValue; }  set { mValue = value; } }

        public OutputValueType GetParamType()
        {
            return mOutputValueType;
        }
    }

}
