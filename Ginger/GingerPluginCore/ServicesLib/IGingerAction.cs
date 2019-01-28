using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{

    public enum LogLevel
    {
        Info,
        Debug,
        Error
    }
    public interface IGingerAction
    {
        string Id { get; set; }    // is it needed ?
        void AddOutput(string param, object value, string path = null);
        void AddError(string error);
        void AddExInfo(string info);
        
        void Log(string text, LogLevel logType = LogLevel.Info);
    }
}
