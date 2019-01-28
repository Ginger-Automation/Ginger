#region License
/*
Copyright © 2014-2018 European Support Limited

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
