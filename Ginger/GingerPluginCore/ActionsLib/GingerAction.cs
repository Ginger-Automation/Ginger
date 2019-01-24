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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.Plugin.Core
{
    // IGingerAction impl for unit test class, when running from Ginger or using GingerCoreNET we use NodeGingerAction
    public class GingerAction : IGingerAction
    {
        public GingerActionOutput Output;

        public GingerAction()
        {
            Output = new GingerActionOutput();
            Output.OutputValues = new List<IGingerActionOutputValue>();
        }

        private string mExInfo;
        public void AddExInfo(string info)
        {
            if (!string.IsNullOrEmpty(mExInfo))
            {
                mExInfo += Environment.NewLine;
            }
            mExInfo += info;
        }

        // Keep it private so code must use AddError, and errors are added formatted
        private string mErrors { get; set; }        
        public void AddError(string err)
        {            
            if (!string.IsNullOrEmpty(mErrors))
            {
                mErrors += Environment.NewLine;
            }
            mErrors += err;

            Log(err, LogLevel.Error);
        }

        /// <summary>
        /// Return errors all errors collected as string
        /// </summary>
        public string Errors
        {
            get
            {
                return mErrors;
            }
        }


        /// <summary>
        /// Return errors all errors collected as string
        /// </summary>
        public string ExInfo
        {
            get
            {
                return mExInfo;
            }
        }

        public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AddOutput(string param, object value, string path = null)
        {            
            GingerActionOutputValue gingerActionOutputValue = new GingerActionOutputValue();
            gingerActionOutputValue.Param = param;
            gingerActionOutputValue.Value = value;
            gingerActionOutputValue.Path = path;
            Output.OutputValues.Add(gingerActionOutputValue);
        }

        public void Log(string text, LogLevel logLevel = LogLevel.Info)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            
            Console.WriteLine(DateTime.Now + ": " + logLevel + " " + text);

            //Back to white
            Console.ForegroundColor = ConsoleColor.White;
        }
        
    }
}
