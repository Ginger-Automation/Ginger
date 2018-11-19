using GingerCore.Variables;

namespace Amdocs.Ginger.Common
{
    public interface IActivity
    {
        ObservableList<VariableBase> GetVariables();

    }
}
