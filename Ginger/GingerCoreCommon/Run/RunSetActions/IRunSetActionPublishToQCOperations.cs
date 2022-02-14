using Amdocs.Ginger.Common;
using Ginger.Reports;

namespace Ginger.Run.RunSetActions
{
    public interface IRunSetActionPublishToQCOperations
    {
        void Execute(IReportInfo RI);
        void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers);
    }
}