using GingerCore.Environments;

namespace Amdocs.Ginger.Common
{
    public interface IProjEnvironment
    {
        string Name { get; set; }

        EnvApplication GetApplication(string appName);
    }
}
