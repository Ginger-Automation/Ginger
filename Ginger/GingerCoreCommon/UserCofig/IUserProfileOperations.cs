using Amdocs.Ginger.Common;
//using Ginger.SolutionGeneral;

namespace Ginger
{
    public interface IUserProfileOperations
    {
        bool SourceControlIgnoreCertificate { get; set; }
        bool SourceControlUseShellClient { get; set; }
        string UserProfileFilePath { get; }

        void LoadDefaults();
        UserProfile LoadPasswords(UserProfile userProfile);
        void LoadRecentAppAgentMapping();
        void SavePasswords();
        void SaveRecentAppAgentsMapping();
        void SaveUserProfile();
    }
}
