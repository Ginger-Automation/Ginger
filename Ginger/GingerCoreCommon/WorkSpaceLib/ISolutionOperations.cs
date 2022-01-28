using GingerCoreNET.ALMLib;

namespace Ginger.SolutionGeneral
{
    public interface ISolutionOperations
    {
        bool AddValidationString();
        bool FetchEncryptionKey();
        bool SaveEncryptionKey();
        void SaveSolution(bool showWarning = true, Solution.eSolutionItemToSave solutionItemToSave = Solution.eSolutionItemToSave.GeneralDetails);
        void SetReportsConfigurations();
        bool ValidateKey(string encryptionKey = null);

        ALMUserConfig GetALMConfig();
    }
}
