namespace Amdocs.Ginger.Common.Repository
{
    public class LazyLoadListConfig
    {
        public enum eLazyLoadType { StringData, NodePath}

        public string ListName { get; set; }

        public eLazyLoadType LazyLoadType { get; set; }
    }
}
