using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.Repository
{
    public interface IPluginPackageOperations
    {
        string StartupDLL { get; }
        void CreateServicesInfo();
        void LoadInfoFromJSON();
        void LoadInfoFromDLL();
        void LoadPluginPackage(string folder);
        void LoadServicesFromJSON();
    }
}
