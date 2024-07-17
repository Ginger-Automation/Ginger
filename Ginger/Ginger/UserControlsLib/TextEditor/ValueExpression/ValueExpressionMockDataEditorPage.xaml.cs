#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Bogus;
using Ginger.UserControlsLib.TextEditor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace Ginger.UserControlsLib.TextEditor.ValueExpression
{
    /// <summary>
    /// Interaction logic for ValueExpressionMockDataEditorPage.xaml
    /// </summary>
    public partial class ValueExpressionMockDataEditorPage : Page
    {
        SelectedContentArgs mSelectedContentArgs;
        Context mContext;
        string mObj;
        public ValueExpressionMockDataEditorPage(Context context, SelectedContentArgs SelectedContentArgs, string obj, string function, string Locale, string MockExpression)
        {
            InitializeComponent();
            mContext = context;
            mSelectedContentArgs = SelectedContentArgs;
            mObj = obj;
            int datasetStartindex = MockExpression.IndexOf('=');
            int datasetendindex = MockExpression.IndexOf('(');
            int localestartindex = MockExpression.IndexOf('"');
            int localeendindex = MockExpression.IndexOf(')');
            int functionstartindex = MockExpression.IndexOf('.');

            List<string> Localelst = GetLocales();
            Type objType;
            List<string> objClasses = new List<string>();
            List<string> methodList = new List<string>();

            Assembly assembly = Assembly.Load("Bogus"); // or load your target assembly

            string namespaceName = "Bogus.DataSets";

            objClasses = GetObjectClasses(assembly, namespaceName);
            

            // Example of fetching locales (replace with actual implementation)
            Localelst = GetLocales();

            string objTypeName = mObj.Equals("Randomizer") ? $"Bogus.{mObj}" : $"Bogus.DataSets.{mObj}";
            objType = assembly.GetType(objTypeName);

            methodList = GetMethodsOfType(objType);
            int CaretPossition = SelectedContentArgs.GetCaretPosition();
            if ((datasetStartindex < CaretPossition) && (CaretPossition < datasetendindex))
            {
                FunctionsList.ItemsSource = objClasses.OrderBy(x => x).ToList();
            }
            else if ((localestartindex < CaretPossition) && (CaretPossition < localeendindex) && (!mObj.Equals("Randomizer") || !mObj.Equals("Finance")))
            {
                FunctionsList.ItemsSource = Localelst.OrderBy(x => x).ToList();
            }
            else
            {
                FunctionsList.ItemsSource = methodList.OrderBy(x => x).ToList();
            }
        }
        
            
        private static List<string> GetLocales()
        {
            // Replace with actual implementation to fetch locales
            return Bogus.Database.GetAllLocales().ToList();
        }

        private static List<string> GetObjectClasses(Assembly assembly, string namespaceName)
        {
            Type[] types = assembly.GetTypes();
            List<string> objclass = types.Where(t => t.Namespace == namespaceName && typeof(DataSet).IsAssignableFrom(t) && t != typeof(DataSet) && t.FullName.Contains(namespaceName))
                        .Select(x => x.Name)
                        .ToList();
            objclass.Add("Randomizer");
            return objclass;
        }

        private static List<string> GetMethodsOfType(Type objType)
        {
            List<string> methodList = new List<string>();

            if (objType != null)
            {
                MethodInfo[] methods = objType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                if (methods != null)
                {
                    var excludedMethods = GetExcludedMethods(objType.ToString());
                    var groupedMethods = methods.GroupBy(m => m.Name);
                    foreach (var methodGroup in groupedMethods)
                    {
                        foreach (MethodInfo method in methodGroup)
                        {
                            if (excludedMethods.Contains(method.Name))
                            {
                                continue;
                            }
                            string parameters = string.Join(", ", method.GetParameters().Select(p => FormatParameter(p)));
                            
                            string functionString = $"{method.Name}({parameters})";
                            methodList.Add(functionString);
                        }
                    }
                }
            }

            return methodList;
        }

        private static HashSet<string> GetExcludedMethods(string objType)
        {
            return objType switch
            {
                "Bogus.Randomizer" => new HashSet<string> { "EnumValues", "WeightedRandom", "ArrayElement", "ArrayElements", "ListItem", "ListItems", "ReplaceSymbols" },
                "Bogus.DataSets.Date" => new HashSet<string> { "BetweenTimeOnly" },
                _ => new HashSet<string>()
            };
        }

        private static string FormatParameter(ParameterInfo parameter)
        {
            string paramType = parameter.ParameterType.Name;
            string paramName = parameter.Name;
            bool hasDefaultValue = parameter.HasDefaultValue;
            object defaultValue = hasDefaultValue ? $"{FormatDefaultValue(parameter.DefaultValue)}" : SetDefaultValue(parameter);
            return $"{defaultValue}";
            
        }

        private static object SetDefaultValue(ParameterInfo parameter)
        {
            Type parameterType = parameter.ParameterType;
            TimeOnly defaultSartTime = TimeOnly.MaxValue;
            if (parameterType == typeof(DateTime) && parameter.Name.Equals("start"))
            {
                return "Past(1)";
            }
            else if (parameterType == typeof(DateTime) && parameter.Name.Equals("end"))
            {
                return "Future(1)";
            }

            if (parameterType == typeof(DateOnly) && parameter.Name.Equals("start"))
            {
                return "PastDateOnly(1)";
            }
            else if (parameterType == typeof(DateOnly) && parameter.Name.Equals("end"))
            {
                return "FutureDateOnly(1)";
            }

            if (parameterType == typeof(DateTimeOffset) && parameter.Name.Equals("start"))
            {
                return "PastOffset(1)";
            }
            else if (parameterType == typeof(DateTimeOffset) && parameter.Name.Equals("end"))
            {
                return "FutureOffset(1)";
            }

            if(parameterType == typeof(Array))
            {
                return "[1,2,3,4]";
            }

            if (parameterType == typeof(List<int>))
            {
                return "[1,2,3,4,5]";
            }

            if (parameterType == typeof(string))
            {
                if(parameter.Name.Equals("format"))
                {
                    return "'12#34'";
                }
                else if (parameter.Name.Equals("symbol"))
                {
                    return "'#'";
                }
                else
                {
                    return "'ABCDEFGHIJKLMNOPQRSTUVWXYZ'";
                }
                
            }
            //DateTimeOffset
            return GetDefaultForType(parameterType);
        }

        public static T[] CreateArrayWithDefaultValues<T>(int length, T defaultValue)
        {
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = defaultValue;
            }
            return array;
        }
         
        private static object GetDefaultForType(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static string FormatDefaultValue(object defaultValue)
        {
            if (defaultValue == null)
                return "null";

            if (defaultValue is string)
                return $"\"{defaultValue}\"";

            if (defaultValue is char)
                return $"'{defaultValue}'";

            if (defaultValue is bool)
                return defaultValue.ToString().ToLower();

            return defaultValue.ToString();
        }
        public void UpdateContent()
        {

            string selectedItem = (string)FunctionsList.SelectedItem;
            string initialText = mSelectedContentArgs.TextEditor.Text.Substring(0, mSelectedContentArgs.StartPos);

            // Parsing the text editor content
            string editorText = mSelectedContentArgs.TextEditor.Text;
            int datasetStartIndex = editorText.IndexOf('=');
            int datasetEndIndex = editorText.IndexOf('(');
            int localeStartIndex = editorText.IndexOf('"');
            int localeEndIndex = editorText.IndexOf(')');
            int functionStartIndex = editorText.IndexOf('.');

            string functionSubstring = editorText.Substring(functionStartIndex + 1);
            int functionEndIndex = functionSubstring.IndexOf('(');

            string datasetObject = ExtractSubstring(editorText, datasetStartIndex + 1, datasetEndIndex - datasetStartIndex - 1);
            string locale = ExtractSubstring(editorText, localeStartIndex + 1, localeEndIndex - localeStartIndex - 2);
            string objStr = ExtractSubstring(editorText, datasetStartIndex + 1, datasetEndIndex - datasetStartIndex - 1);
            string functions = functionSubstring.Substring(0, functionEndIndex).Replace("\"", "").Trim();

            int caretPosition = mSelectedContentArgs.GetCaretPosition();
            string resultText = BuildResultText(selectedItem, objStr, locale, functions, caretPosition, datasetStartIndex, datasetEndIndex, localeStartIndex, localeEndIndex);

            // Append the remaining text and update the text editor content
            resultText += editorText.Substring(mSelectedContentArgs.EndPos + 1);
            mSelectedContentArgs.TextEditor.Text = resultText;
        }

        static string ExtractSubstring(string text, int startIndex, int length)
        {
            return text.Substring(startIndex, length);
        }

        static string BuildResultText(string selectedItem, string objStr, string locale, string functions, int caretPosition, int datasetStartIndex, int datasetEndIndex, int localeStartIndex, int localeEndIndex)
        {
            string resultText = string.Empty;
            
            if(objStr.Equals("Randomizer") || objStr.Equals("Finance"))
            {
                if (datasetStartIndex < caretPosition && caretPosition < datasetEndIndex)
                {
                    Assembly assembly = Assembly.Load("Bogus");
                    string objTypeName = selectedItem.Equals("Randomizer") ? $"Bogus.{selectedItem}" : $"Bogus.DataSets.{selectedItem}";
                    Type objType = assembly.GetType(objTypeName);

                    List<string> methodList = GetMethodsOfType(objType);
                    resultText += "{MockDataExp Fun=" + selectedItem + "()." + methodList.FirstOrDefault() + ";}";
                }
                else
                {
                    resultText += "{MockDataExp Fun=" + objStr + "()." + selectedItem + ";}";
                }
            }
            else
            {
                if (datasetStartIndex < caretPosition && caretPosition < datasetEndIndex)
                {
                    Assembly assembly = Assembly.Load("Bogus");
                    string objTypeName = selectedItem.Equals("Randomizer") ? $"Bogus.{selectedItem}" : $"Bogus.DataSets.{selectedItem}";
                    Type objType = assembly.GetType(objTypeName);

                    List<string> methodList = GetMethodsOfType(objType);
                    resultText += "{MockDataExp Fun=" + selectedItem + "(@\"" + locale + "\")." + methodList.FirstOrDefault() + ";}";
                }
                else if (localeStartIndex < caretPosition && caretPosition < localeEndIndex)
                {
                    resultText += "{MockDataExp Fun=" + objStr + "(@\"" + selectedItem + "\")." + functions + "();}";
                }
                else
                {
                    resultText += "{MockDataExp Fun=" + objStr + "(@\"" + locale + "\")." + selectedItem + ";}";
                }
            }
            

            return resultText;
        }
    }
}
