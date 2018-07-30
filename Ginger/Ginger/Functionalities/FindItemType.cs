using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.Functionalities
{
    public class FindItemType
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool HasSubType { get; set; }
        public GetItemsToSearchInFunction GetItemsToSearchIn;
        public List<FindItemType> mSubItemsTypeList { get; set; }
        public GetSubItemsFunction GetSubItems { get; set; }
    }
}
