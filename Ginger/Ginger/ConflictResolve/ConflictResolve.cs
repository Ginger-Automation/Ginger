using Amdocs.Ginger.Common;

namespace Ginger.ConflictResolve
{

    public class ConflictResolve
    {
        public enum eResolveOperations
        {
            [EnumValueDescription("Accept Server Changes")]
            AcceptServer,
            [EnumValueDescription("Keep Local Changes")]
            KeepLocal
        }
        public string ConflictPath { get; set; }

        public string RelativeConflictPath
        {
            get
            {
                return amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(ConflictPath);
            }
        }

        public string ItemName { get; set; }

        public eResolveOperations resolveOperations { get; set; }
    }
}
