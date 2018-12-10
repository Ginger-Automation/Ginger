using GingerCore.Environments;

namespace Amdocs.Ginger.Common
{
    public interface IProjEnvironment
    {
        string Name { get; set; }
        object Guid { get; }

        EnvApplication GetApplication(string appName);
    }
}
