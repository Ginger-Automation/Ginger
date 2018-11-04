using Amdocs.Ginger.Plugin.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class NodeGingerAction : IGingerAction
    {
        private string mErrors;
        private string mExInfo;

        public NodeActionOutput Output { get; } = new NodeActionOutput();
        public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ExInfo { get { return mExInfo; } }  

        public string Errors { get { return mErrors; } } 
                

        public void AddError(string error)
        {            
            if (mErrors != null)
            {
                mErrors += Environment.NewLine;
            }

            mErrors += error; 
        }

        public void AddExInfo(string info)
        {
            if (mExInfo != null)
            {
                mExInfo += Environment.NewLine;
            }

            mExInfo += info;
        }

        public void AddOutput(string param, object value, string path = null)
        {            
            if (value == null)
            {
                Output.Add(param, "", path);
            }
            else
            {
                // temp to string!!! - FIXME: enable all type output
                Output.Add(param, value.ToString(), path);
            }            
        }
        
    }
}
