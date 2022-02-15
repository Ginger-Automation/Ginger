using Ginger.Reports;

namespace Ginger.Run.RunSetActions
{
    public interface IRunSetActionBaseOperations
    {
        void ExecuteWithRunPageBFES();
        void RunAction(IReportInfo RI);
    }
}
