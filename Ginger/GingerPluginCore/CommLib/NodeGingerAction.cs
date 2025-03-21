#region License
/*
Copyright © 2014-2025 European Support Limited

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

            Log(error, LogLevel.Error);
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

        public void Log(string text, LogLevel logLevel = LogLevel.Info)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
            }

            Console.WriteLine(DateTime.Now + ": " + logLevel + " " + text);
        }

    }
}
