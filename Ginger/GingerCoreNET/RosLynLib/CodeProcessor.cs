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
using System.Text.RegularExpressions;
using System.Linq;

namespace GingerCoreNET.RosLynLib
{
    public class CodeProcessor
    {
        static Regex Pattern = new Regex("{CS(\\s)*Exp(\\s)*=(\\s)*([a-zA-Z]|\\d)*\\((\")*([^\\)}\\({])*(\")*\\)}", RegexOptions.Compiled);
        public object EvalExpression(string expression)
        {

            string code = expression.Replace("{CS Eval(", "").Trim().Replace(")}", "");
            Stopwatch st = Stopwatch.StartNew();
            Task<object> task = EvalExpressionTask(code);
            task.Wait();
            st.Stop();
            Reporter.ToLog(eLogLevel.DEBUG, "Executed CodeProcessor - Elapsed: " + st.ElapsedMilliseconds + " ,Expression: " + expression + " ,Result: " + task.Result);
            return task.Result;
        }

        public static string GetResult(string Expression)
        {
            if (!Expression.Contains(@"{CS"))
            {
                return Expression;

            }
            ScriptOptions SO = ScriptOptions.Default.WithReferences(new Assembly[] { Assembly.GetAssembly(typeof(string)), Assembly.GetAssembly(typeof(System.Net.Dns)) });

            SO.WithReferences(Assembly.GetAssembly(typeof(string)));


            string pattern = "^[^{}]*" +
                       "(" +
                       "((?'Open'{)[^{}]*)+" +
                       "((?'Close-Open'})[^{}]*)+" +
                       ")*" +
                       "(?(Open)(?!))";
            pattern = "{CS({.*}|[^{}]*)*}";


            Pattern = new Regex(pattern);
            Regex Clean = new Regex("{CS(\\s)*Exp(\\s)*=");

            foreach (Match M in Pattern.Matches(Expression))
            {
                string csharpError = "";
                string match = M.Value;
                string exp = match;
                exp = exp.Replace(Clean.Match(exp).Value, "");
                //not doing string replacement to
                exp = exp.Remove(exp.Length - 1);
                string evalresult = GetEvaluteResult(exp, out csharpError);
                Expression = Expression.Replace(match, evalresult);
            }

            return Expression;

        }

        public static string GetEvaluteResult(string expression, out string error)
        {
            string evalresult = "";
            error = "";
            try
            {
                System.Collections.Generic.List<String> Refrences = typeof(System.DateTime).Assembly.GetExportedTypes().Where(y => !String.IsNullOrEmpty(y.Namespace)).Select(x => x.Namespace).Distinct().ToList<string>();
                Refrences.AddRange(typeof(string).Assembly.GetExportedTypes().Where(y => !String.IsNullOrEmpty(y.Namespace)).Select(x => x.Namespace).Distinct().ToList<string>());
                object result = CSharpScript.EvaluateAsync(expression, ScriptOptions.Default.WithImports(Refrences)).Result;
                //c# generate True/False for bool.tostring which fails in subsequent expressions 
                if (result.GetType() == typeof(Boolean))
                {
                    evalresult = result.ToString().ToLower();
                }
                else
                {
                    evalresult = result.ToString();
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, expression + System.Environment.NewLine + " not a valid c# expression to evaluate", e);
                error = e.Message;
            }
            return evalresult;
        }
        public static bool EvalCondition(string condition)
        {
            try
            {
                bool Conditionparse;
                if (bool.TryParse(condition, out Conditionparse))
                {
                    return Conditionparse;
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("Error EvalCondition: " + condition + Ex.Message);
                // !!!!!!!!!!!!! throw; check next stmt !!!
            }
            //TODO: fix me !!!!!  bad double try to recover from exceptopn!? or we can use if else


            bool result = false;
            try
            {
                result = (bool)CSharpScript.EvaluateAsync(condition).Result;
            }
            catch (Exception EvalExcep)
            {
                Reporter.ToLog(eLogLevel.DEBUG, condition + System.Environment.NewLine + " not a valid c# expression to evaluate", EvalExcep);


                result = false;
            }
            return result;
        }

        // condition can be: 1=2 or complex like 1+3=5
        private async Task<bool> EvalConditionAsync(string condition)
        {
            // bool b;
            // if (1 == 1) b = true; else b = false;    

            var script = CSharpScript.
                Create<bool>("bool b;").
                ContinueWith("if (" + condition + ") b=true; else b=false;").   // check the condition
                ContinueWith("b");    // return the value of b

            return ((bool)(await script.RunAsync()).ReturnValue);
        }



        public async Task<object> EvalExpressionTask(string expression)
        {
            var rc = await CSharpScript.EvaluateAsync(expression);
            return rc;
        }



        //!!!!!   Cleanup

        private static ScriptState<object> scriptState = null;
        public static object Execute(string code)
        {
            Console.WriteLine("Executing script code: " + code);

            // Add ref to DLLs needed
            ScriptOptions options = ScriptOptions.Default.AddReferences(Assembly.GetAssembly(typeof(PluginPackage)));

            //Globals to pass in vars
            GingerScriptGlobals globals = new GingerScriptGlobals();

            scriptState = scriptState == null ? CSharpScript.RunAsync(code, options, globals).Result : scriptState.ContinueWithAsync(code).Result;
            if (scriptState.ReturnValue != null && !string.IsNullOrEmpty(scriptState.ReturnValue.ToString()))
                return scriptState.ReturnValue;

            Console.WriteLine("Executing script code complete");


            return null;
        }

        public object RunCode2(string code)
        {
            //SyntaxTree tree = CSharpSyntaxTree.ParseText(@"object result;");
            var result = Task.Run<object>(async () =>
            {
                var s = await CSharpScript.RunAsync(@"using System;");
                s = await s.ContinueWithAsync(@"object result;");
                // continuing with previous evaluation state
                s = await s.ContinueWithAsync(code);
                s = await s.ContinueWithAsync("result");   // output result
                // inspecting defined variables
                Console.WriteLine("inspecting defined variables:");
                foreach (var variable in s.Variables)
                {
                    string varInfo = string.Format("name: {0}, type: {1}, value: {2}", variable.Name, variable.Type.Name, variable.Value);
                    Reporter.ToLog(eLogLevel.DEBUG, varInfo);
                }
                return s.ReturnValue;

            }).Result;

            return result;
        }

        public static object ExecuteNew(string code)
        {
            Console.WriteLine("Executing script code: " + Environment.NewLine);
            Console.WriteLine("====================================================================================================================" + Environment.NewLine);
            Console.WriteLine(code + Environment.NewLine);
            Console.WriteLine("====================================================================================================================" + Environment.NewLine);

            // Add ref to DLLs needed
            ScriptOptions options = ScriptOptions.Default.AddReferences(Assembly.GetAssembly(typeof(PluginPackage)));

            //Globals to pass in vars
            GingerScriptGlobals globals = new GingerScriptGlobals();

            scriptState = scriptState == null ? CSharpScript.RunAsync(code, options, globals).Result : scriptState.ContinueWithAsync(code).Result;
            if (scriptState.ReturnValue != null && !string.IsNullOrEmpty(scriptState.ReturnValue.ToString()))
            {
                return scriptState.ReturnValue;
            }

            Console.WriteLine("Executing script code complete");

            return null;
        }

    }
}
