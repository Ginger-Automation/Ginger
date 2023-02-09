#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System.IO;

namespace GingerCore.GeneralLib
{
    public class CommonLib
    {
        /* System Environment Variable 
         * Routines to pull the environment variables
         */
        public static string GetSystemEnvironmentVariableValue(string environmentVariable)
        {
            string envVarValue = System.Environment.GetEnvironmentVariable(environmentVariable);

            if (string.IsNullOrEmpty(envVarValue))
            {
                throw new Exception("Error: " + environmentVariable + " not defined, please setup " + environmentVariable + " in Environment Variables");
            }
            else
            {
                return envVarValue;
            }
        }
        
        /*
         * Getting Python Executable from the PYTHON_EXEC environment variable
         */
        public static string GetPythonExecutable()
        {
            string PythonExec = GetSystemEnvironmentVariableValue("PYTHON_EXEC");

            if (!File.Exists(PythonExec))
            {
                throw new Exception("Error: python.exe not found at: " + PythonExec);
            }
            else
            {
                return PythonExec;
            }
        }

        /*
         * Getting JAVA_HOME location from the JAVA_HOME environment variable
         */
        public static String GetJavaHome()
        {
            string javaPath = Environment.GetEnvironmentVariable("JAVA_HOME");

            if (!string.IsNullOrEmpty(javaPath))
            {
                return javaPath;
            }
            else
            {
                //TODO: find from registry or somewhere else
                return string.Empty;
            }

            // TODO:  put in GingerUtils
            // TODO: if no env var then try to do shell: where java check if good for windows and Linux
            // + search C:\Program Files (x86)\Common Files\Oracle\Java\javapath
        }
    }
}
