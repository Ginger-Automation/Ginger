using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.Repository
{
    public interface IPluginPackageOperations
    {
        PluginPackageInfo PluginPackageInfo { get; }
        ObservableList<PluginServiceInfo> Services { get; }
        string StartupDLL { get; }

        void CreateServicesInfo();
        PluginServiceInfo GetService(string serviceId);
        void LoadInfoFromJSON();
        void LoadInfoFromDLL();
        void LoadPluginPackage(string folder);
        void LoadServicesFromJSON();
    }
}
