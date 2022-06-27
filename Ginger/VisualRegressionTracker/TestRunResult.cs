using System;

namespace VisualRegressionTracker
{
    public class TestRunResult {
        public TestRunStatus Status { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string DiffUrl { get; set; }
        public string BaselineUrl { get; set; }
    }
}