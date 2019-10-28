using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.DataBaseLib;
using Amdocs.Ginger.Plugin.Core.Database;
using Amdocs.Ginger.Repository;
using GingerCore.Environments;
using System.IO;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.DatabaseLib
{
    class DataBaseProvider : IDBProvider
    {
        public IDatabase GetDBImpl(Database database)
        {
            PluginPackage pluginPackage;


            // TODO: 
            // Start Plugin
            // Save GingerNodeProxy on DatabaseImpl
            // Provide Idatabase which goes via socket to the DB plugin to run DB actions
            

            //TODO: FIXME: Temp until we switch to db run using sockets !!!

            IDatabase databaseImpl = null;

            switch (database.DBType)
            {
                case Database.eDBTypes.MSAccess:
                    pluginPackage = WorkSpace.Instance.PlugInsManager.GetDatabasePluginPackage("MSAccessService");
                    // TODO: if null
                    string fileName = Path.Combine(pluginPackage.Folder, pluginPackage.StartupDLL);
                    Assembly assembly = Assembly.LoadFrom(fileName);
                    // TODO: find the correct interface class impl
                    databaseImpl = (IDatabase)assembly.CreateInstance("MSAccessDB.MSAccessDBCon");   

                    break;
                case Database.eDBTypes.MySQL:
                    pluginPackage = WorkSpace.Instance.PlugInsManager.GetDatabasePluginPackage("MySQLService");
                    // TODO: if null
                    string fileName2 = Path.Combine(pluginPackage.Folder, pluginPackage.StartupDLL);
                    Assembly assembly2 = Assembly.LoadFrom(fileName2);
                    // TODO: find the correct interface class impl
                    databaseImpl = (IDatabase)assembly2.CreateInstance("MySQLDatabase.MYSQLDBConnection");                    

                    break;


                    // TODO: all the rest
            }

            

            return databaseImpl;
        }
    }
}
