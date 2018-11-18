using GingerCore.Variables;

namespace Amdocs.Ginger.Common
{
    public interface IBusinessFlow
    {
        ObservableList<VariableBase> GetVariables();

        VariableBase GetHierarchyVariableByName(string varName, bool considerLinkedVar = true);
    }
}
