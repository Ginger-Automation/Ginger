#region License
/*
Copyright © 2014-2026 European Support Limited

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
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            Regex CsExppattern = new Regex("{CS Exp({.*}|[^{}]*)*}", RegexOptions.Compiled);


            Pattern = new Regex(pattern);
            Regex Clean = new Regex("{CS(\\s)*Exp(\\s)*=");
            MatchCollection PatternMatchlist = CsExppattern.Matches(Expression);
            if (PatternMatchlist == null || PatternMatchlist.Count == 0)
            {
                Reporter.ToLog(eLogLevel.DEBUG, Expression + System.Environment.NewLine + " not a valid c# expression to evaluate");
                return string.Empty;
            }

            foreach (Match M in PatternMatchlist)
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
                if (result == null)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"{expression} evaluation returned null value");
                }
                else if (result.GetType() == typeof(Boolean))
                {
                    evalresult = result.ToString().ToLower();
                }
                else if (result.GetType() == typeof(string))
                {
                    evalresult = result.ToString();
                }
                else if (result.GetType() == typeof(string[]))
                {
                    evalresult = string.Join(",", (string[])result);
                    evalresult = $"[{evalresult}]";
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
            {
                return scriptState.ReturnValue;
            }

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
        /// <summary>
        /// function is designed to process a given Expression string and replace occurrences of a specific pattern ({MockDataExp(...)}) 
        /// with evaluated results obtained from another function (GetBogusExpressionEvaluateResult)
        /// </summary>
        /// <param name="Expression"></param>
        /// <returns></returns>
        public static string GetBogusDataGenerateresult(string Expression)
        {
            try
            {
                if (!Expression.Contains(@"{MockDataExp"))
                {
                    return Expression;
                }
                Pattern = new Regex("{MockDataExp({.*}|[^{}]*)*}", RegexOptions.Compiled, new TimeSpan(0, 0, 5));
                Regex Clean = new Regex("{MockDataExp(\\s)*Fun(\\s)*=", RegexOptions.Compiled);

                MatchCollection PatternMatchlist = Pattern.Matches(Expression);
                if (PatternMatchlist == null || PatternMatchlist.Count == 0)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, Expression + System.Environment.NewLine + " not a valid c# expression to evaluate");
                    return string.Empty;
                }

                foreach (Match M in PatternMatchlist)
                {
                    string Error = "";
                    string match = M.Value;
                    string exp = match;
                    exp = exp.Replace(Clean.Match(exp).Value, "");
                    exp = exp.Remove(exp.Length - 1);
                    string evalresult = GetBogusExpressionEvaluteResult(exp, out Error);
                    Expression = Expression.Replace(match, evalresult);
                }
                return Expression;
            }
            catch (RegexMatchTimeoutException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Timeout Exception", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// The GetBogusExpressionEvaluteResult function is responsible for evaluating and generating data using Bogus library expressions based on the provided expression. 
        /// It handles different types of expressions related to data generation and returns the evaluated result as a string.
        /// If the expression does not start with new Bogus.DataSets.
        /// it checks for special cases like Randomizer and constructs appropriate var Result = ...; return Result; expressions.
        /// Handles scenarios with special characters(@) and constructs expressions accordingly like for Between.
        /// If the expression starts with new Bogus.DataSets., it directly constructs the expression for evaluation.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string GetBogusExpressionEvaluteResult(string expression, out string evalresult)
        {
            evalresult = string.Empty;
            try
            {
                Assembly BogusAssembly = Assembly.Load("Bogus");
                /// If the expression does not start with new Bogus.DataSets.
                if (!expression.Contains("new Bogus.DataSets."))
                {
                    /// it checks for special cases like Randomizer and constructs appropriate var Result = ...; return Result; expressions.
                    if (expression.Contains("Randomizer"))
                    {
                        expression = $"var Result = new Bogus.{expression} return Result;";
                    }
                    else
                    {
                        /// Handles scenarios with special characters(@) and constructs expressions accordingly.
                        if (expression.Contains('@'))
                        {
                            string[] expressionlist = expression.Split('.');
                            /// Handles scenarios with special case like Between and constructs expressions accordingly
                            if (expressionlist[1].Contains("Between"))
                            {
                                string expressionsubstring = expressionlist[1].Substring(expressionlist[1].IndexOf('(') + 1, expressionlist[1].IndexOf("))") - expressionlist[1].IndexOf("(") + 1);
                                string[] Parameter = expressionsubstring.Split(',');
                                /// Handles scenarios with special case like Between function have inbuilt function as parameter and constructs expressions accordingly
                                if (expressionlist[1].Contains("Past") && expressionlist[1].Contains("Future"))
                                {
                                    if (expressionlist[1].Contains("BetweenTimeOnly"))
                                    {
                                        expressionlist[1] = expressionlist[1].Replace(Parameter[0], $"TimeOnly.FromDateTime(Result.{Parameter[0]})").Replace(Parameter[1], $"TimeOnly.FromDateTime(Result.{Parameter[1]})");
                                    }
                                    else
                                    {
                                        expressionlist[1] = expressionlist[1].Replace(Parameter[0], $"Result.{Parameter[0]}").Replace(Parameter[1], $"Result.{Parameter[1]}");
                                    }
                                }
                                expression = $"var Result = new Bogus.DataSets.{expressionlist[0]}; return Result.{expressionlist[1]}";
                            }
                            else
                            {
                                expression = $"var Result = new Bogus.DataSets.{expressionlist[0]}; return Result.{expressionlist[1]}";
                            }

                        }
                        else
                        {
                            expression = $"var Result = new Bogus.DataSets.{expression} return Result;";
                        }
                    }
                }/// If the expression starts with new Bogus.DataSets., it directly constructs the expression for evaluation.
                else
                {
                    expression = $"var Result = {expression} return Result;";
                }

                object result = CSharpScript.EvaluateAsync(expression, ScriptOptions.Default.WithReferences(BogusAssembly)).Result;
                //c# generate True/False for bool.tostring which fails in subsequent expressions
                if (result == null)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"{expression} evaluation returned null value");
                }
                else if (result.GetType() == typeof(int[]))
                {
                    evalresult = string.Join(",", (int[])result);
                }
                else if (result.GetType() == typeof(string[]))
                {
                    evalresult = string.Join(",", (string[])result);
                }
                else
                {
                    evalresult = result.ToString();
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, expression + System.Environment.NewLine + " not a valid Bogus data generate expression to evaluate", e);
            }
            return evalresult;
        }
    }
}
