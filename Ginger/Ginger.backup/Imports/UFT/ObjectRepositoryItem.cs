
using System.Collections.Generic;

namespace Ginger.Imports.UFT
{
    public class ObjectRepositoryItem
    {
        public string Name { get; set; }
        public string Class { get; set; }
        public List<ObjectRepositortyItemProperty> Properties = new List<ObjectRepositortyItemProperty>();
    }
}
