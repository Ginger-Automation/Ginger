using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib
{
    /// <summary>
    /// This class is used in the Export Process
    /// </summary>
    public class AppParameters
    {
        public string ItemName { get; set; }

        public ObservableList<OptionalValue> OptionalValuesList { get; set; }

        public string OptionalValuesString { get; set; }

        public string Description { get; set; }
    }
}
