using CommandLine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    class CLIOptionClassHelper
    {
        public const string FILENAME = "filename";

        public static string GetClassVerb<T>()
        {
            VerbAttribute verbAttr = typeof(T).GetCustomAttribute<VerbAttribute>();
            return verbAttr.Name;
        }

        public static string GetAttrShorName<T>(string name)
        {
            OptionAttribute attrOption = typeof(T).GetProperty(name).GetCustomAttribute<OptionAttribute>();
            return attrOption.ShortName;
        }

        public static string GetAttrLongName<T>(string name)
        {
            OptionAttribute attrOption = typeof(T).GetProperty(name).GetCustomAttribute<OptionAttribute>();
            return attrOption.LongName;
        }
    }
}
