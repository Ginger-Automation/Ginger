using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Amdocs.Ginger.Plugin.Core.Database;



namespace Amdocs.Ginger.CoreNET.DatabaseLib
{
    public class DatabaseManager
    {
        List<IDatabase> mDatatBaseList = new List<IDatabase>();

        public void Init()
        {
            // Assembly.GetExecutingAssembly()
            // temp FIXME to be dynamic or from Ginger Plugins/DBs folder
            AddDatabase(@"C:\Users\Yaron\source\repos\Ginger\Ginger\MSAccessDB\bin\Debug", "GingerMSAccessDB.dll", "MSAccessDB.MSAccessDBCon");
        }


        public void AddDatabase(string folder, string DLLFileName, string className)
        {
            Assembly assembly =  Assembly.LoadFrom(Path.Combine(folder, DLLFileName));
            object obj = assembly.CreateInstance(className);
            IDatabase db = (IDatabase) obj;
            mDatatBaseList.Add((db));
        }
    }
}
