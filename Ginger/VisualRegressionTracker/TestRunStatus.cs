using System;

namespace VisualRegressionTracker
{
    public enum TestRunStatus
    {
        New,
        Ok,
        Unresolved,
        Failed,
        Approved,
        AutoApproved
    }
}