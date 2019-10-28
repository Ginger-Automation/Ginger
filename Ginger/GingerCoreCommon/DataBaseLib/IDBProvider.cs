using Amdocs.Ginger.Plugin.Core.Database;
using GingerCore.Environments;

namespace Amdocs.Ginger.Common.DataBaseLib
{
    public interface IDBProvider
    {
        IDatabase GetDBImpl(Database database);
    }
}
