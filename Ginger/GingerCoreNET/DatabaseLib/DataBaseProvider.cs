using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.DataBaseLib;
using Amdocs.Ginger.Plugin.Core.Database;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.CoreNET.DatabaseLib
{
    class DataBaseProvider : IDBProvider
    {
        public IDatabase GetDBImpl(string serviceId)
        {
            PluginPackage pluginPackage = WorkSpace.Instance.PlugInsManager.GetDatabasePluginPackage(serviceId);

            // TODO: if null

            string fileName = Path.Combine(pluginPackage.Folder, pluginPackage.StartupDLL);
            Assembly assembly = Assembly.LoadFrom(fileName);

            // TODO: find the correct interface class impl
            IDatabase databaseImpl = (IDatabase)assembly.CreateInstance("MSAccessDB.MSAccessDBCon");   // Temp !!!!!!!!!!!!!!!!!!

            return databaseImpl;
        }
    }
}
