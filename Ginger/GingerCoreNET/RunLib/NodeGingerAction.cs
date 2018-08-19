using Amdocs.Ginger.Plugin.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class NodeGingerAction : IGingerAction
    {
        public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ExInfo { get { return "xexexec"; } }  //!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        public string Errors { get { return "errrrrrrrr"; } } //!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        // List<ActionInput>

        public void AddError(string error)
        {
            throw new NotImplementedException();
        }

        public void AddExInfo(string info)
        {
            throw new NotImplementedException();
        }

        public void AddOutput(string param, object value, string path = null)
        {
            //throw new NotImplementedException();
            // Output.Ad
        }

        // public ActionOutput Output { get; set; }
    }
}
