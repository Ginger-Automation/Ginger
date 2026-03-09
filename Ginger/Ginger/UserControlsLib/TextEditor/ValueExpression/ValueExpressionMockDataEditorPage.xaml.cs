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
using Bogus;
using Ginger.UserControlsLib.TextEditor.Common;
using GingerCore;
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
        List<string> Localelst;
        List<string> objClasses;
        public ValueExpressionMockDataEditorPage(Context context, SelectedContentArgs SelectedContentArgs, string obj, string function, string Locale, string MockExpression)
        {
            try
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

                Localelst = Localelst ?? GetLocales();
                Type objType;
                List<string> methodList;

                Assembly assembly = Assembly.Load("Bogus"); // or load your target assembly

                string namespaceName = "Bogus.DataSets";

                objClasses = objClasses ?? GetObjectClasses(assembly, namespaceName);

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
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load ValueExpressionMockDataEditorPage", ex);
            }
        }


        private static List<string> GetLocales()
        {
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
            List<string> methodList = [];

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
                "Bogus.Randomizer" => ["EnumValues", "WeightedRandom", "ArrayElement", "ArrayElements", "ListItem", "ListItems", "ReplaceSymbols"],
                "Bogus.DataSets.Date" => ["BetweenTimeOnly"],
                _ => []
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

            return parameterType switch
            {
                Type type when type == typeof(DateTime) && parameter.Name.Equals("start") => "Past(1)",
                Type type when type == typeof(DateTime) && parameter.Name.Equals("end") => "Future(1)",

                Type type when type == typeof(DateOnly) && parameter.Name.Equals("start") => "PastDateOnly(1)",
                Type type when type == typeof(DateOnly) && parameter.Name.Equals("end") => "FutureDateOnly(1)",

                Type type when type == typeof(DateTimeOffset) && parameter.Name.Equals("start") => "PastOffset(1)",
                Type type when type == typeof(DateTimeOffset) && parameter.Name.Equals("end") => "FutureOffset(1)",

                Type type when type == typeof(Array) => "[1,2,3,4]",

                Type type when type == typeof(List<int>) => "[1,2,3,4,5]",

                Type type when type == typeof(string) => parameter.Name switch
                {
                    "format" => "\"12#34\"",
                    "symbol" => "'#'",
                    _ => "\"ABCDEFGHIJKLMNOPQRSTUVWXYZ\""
                },

                _ => GetDefaultForType(parameterType)
            };
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
            return defaultValue switch
            {
                null => "null",
                string s => $"\"{s}\"",
                char c => $"'{c}'",
                bool b => b.ToString().ToLower(),
                _ => defaultValue.ToString(),
            };
        }
        public void UpdateContent()
        {
            try
            {
                string selectedItem = (string)FunctionsList.SelectedItem;
                string initialText = mSelectedContentArgs.TextEditor.Text[..mSelectedContentArgs.StartPos];

                // Parsing the text editor content
                string objStr, functions, Locale;
                Mockdata expParams = GingerCore.ValueExpression.GetMockDataDatasetsFunction(mSelectedContentArgs.TextEditor.Text);

                string editorText = mSelectedContentArgs.TextEditor.Text;

                int caretPosition = mSelectedContentArgs.GetCaretPosition();
                string resultText = BuildResultText(selectedItem, expParams.MockDataDatasets, expParams.Locale, expParams.Function, caretPosition, expParams.DatasetStartIndex, expParams.DatasetEndIndex, expParams.LocaleStartIndex, expParams.LocaleEndIndex);

                // Append the remaining text and update the text editor content
                resultText += editorText[(mSelectedContentArgs.EndPos + 1)..];
                mSelectedContentArgs.TextEditor.Text = resultText;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to update content in ValueExpressionMockDataEditorPage", ex);
            }
        }

        static string ExtractSubstring(string text, int startIndex, int length)
        {
            return text.Substring(startIndex, length);
        }

        static string BuildResultText(string selectedItem, string objStr, string locale, string functions, int caretPosition, int datasetStartIndex, int datasetEndIndex, int localeStartIndex, int localeEndIndex)
        {
            string resultText = string.Empty;
            bool isWithinDataset = datasetStartIndex < caretPosition && caretPosition < datasetEndIndex;
            bool isWithinLocale = localeStartIndex < caretPosition && caretPosition < localeEndIndex;

            if (objStr.Equals("Randomizer") || objStr.Equals("Finance"))
            {
                resultText += isWithinDataset
                    ? GenerateResultText(selectedItem, "Bogus", isWithinDataset)
                    : "{MockDataExp Fun=" + objStr + "()." + selectedItem + ";}";
            }
            else
            {

                if (isWithinDataset)
                {
                    resultText += GenerateResultText(selectedItem, "Bogus", isWithinDataset, locale);
                }
                else if (isWithinLocale)
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

        private static string GenerateResultText(string selectedItem, string baseNamespace, bool isWithinDataset, string locale = null)
        {
            Assembly assembly = Assembly.Load("Bogus");
            string objTypeName = selectedItem.Equals("Randomizer") ? $"{baseNamespace}.{selectedItem}" : $"{baseNamespace}.DataSets.{selectedItem}";
            Type objType = assembly.GetType(objTypeName);

            List<string> methodList = GetMethodsOfType(objType);
            string methodCall = methodList.FirstOrDefault();

            if (string.IsNullOrEmpty(locale))
            {
                locale = "en";
            }
            if (selectedItem is "Randomizer" or "Finance")
            {
                return "{MockDataExp Fun=" + selectedItem + "()." + methodCall + ";}";
            }
            else
            {
                return "{MockDataExp Fun=" + selectedItem + "(@\"" + locale + "\")." + methodCall + ";}";
            }
        }
    }
}
