using GingerCore.Environments;

namespace Amdocs.Ginger.Common
{
    public interface IProjEnvironment
    {
        EnvApplication GetApplication(string appName);
    }
}
