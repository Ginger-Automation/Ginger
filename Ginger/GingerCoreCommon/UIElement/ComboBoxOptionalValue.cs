using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.UIElement
{
    public class ComboBoxOptionalValue : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public string TagName { get; set; }

        [IsSerializedForLocalRepository]
        public string Value { get; set; }
        public override string ItemName { get { return TagName; } set { TagName = value; } }
    }
}
