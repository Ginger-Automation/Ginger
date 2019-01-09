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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RosLynLib;
using Amdocs.Ginger.Repository;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace GingerCoreNET.RosLynLib
{
    public class CodeProcessor
    {
        public string EvalExpression(string expression)
        {
            Stopwatch st = Stopwatch.StartNew();
            Task<string> task = EvalExpressionTask(expression);
            task.Wait();
            st.Stop();
            Reporter.ToLog(eLogLevel.DEBUG, "Executed CodeProcessor - Elapsed: " + st.ElapsedMilliseconds + " ,Expression: " + expression + " ,Result: " + task.Result);
            return task.Result;
        }

        public async Task<string> EvalExpressionTask(string expression)
        {
            var rc = await CSharpScript.EvaluateAsync(expression);
            return rc.ToString();
        }

        private static ScriptState<object> scriptState = null;
        public static object Execute(string code)
        {
            Console.WriteLine("Executing script code: " + code);

            // Add ref to DLLs needed
            ScriptOptions options = ScriptOptions.Default.AddReferences(Assembly.GetAssembly(typeof(PluginPackage)));

            //Globals to pass in vars
            GingerConsoleScriptGlobals globals = new GingerConsoleScriptGlobals();

            scriptState = scriptState == null ? CSharpScript.RunAsync(code, options, globals).Result : scriptState.ContinueWithAsync(code).Result;
            if (scriptState.ReturnValue != null && !string.IsNullOrEmpty(scriptState.ReturnValue.ToString()))
                return scriptState.ReturnValue;

            Console.WriteLine("Executing script code complete");

            globals.WaitforallNodesShutDown();

            return null;
        }

        public void runcode()
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(@"var x = new DateTime(2016,12,1);");
            Console.WriteLine(tree.ToString()); // new DateTime(2016,12,1)
            var result = Task.Run<object>(async () =>
            {
                Console.WriteLine("Enter expression:");
                string exp = Console.ReadLine();
                var rc = await CSharpScript.RunAsync(exp);
                Console.WriteLine(exp + "=" + rc.ReturnValue);

                var s = await CSharpScript.RunAsync(@"using System;");
                // continuing with previous evaluation state
                s = await s.ContinueWithAsync(@"var x = ""my/"" + string.Join(""_"", ""a"", ""b"", ""c"") + "".ss"";");
                s = await s.ContinueWithAsync(@"var y = ""my/"" + @x;");
                s = await s.ContinueWithAsync(@"y // this just returns y, note there is NOT trailing semicolon");
                // inspecting defined variables
                Console.WriteLine("inspecting defined variables:");
                foreach (var variable in s.Variables)
                {
                    Console.WriteLine("name: {0}, type: {1}, value: {2}", variable.Name, variable.Type.Name, variable.Value);
                }
                return s.ReturnValue;

            }).Result;

            Console.WriteLine("Result is: {0}", result);
        }
    }
}
