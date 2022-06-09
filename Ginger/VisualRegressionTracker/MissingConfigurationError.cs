using System;

namespace VisualRegressionTracker
{
    public class MissingConfigurationError : VisualRegressionTrackerError
    {
        public MissingConfigurationError(string fieldName, string message) : base(message)
        {
            FieldName = fieldName;
        }

        public string FieldName { get; }
    }
}