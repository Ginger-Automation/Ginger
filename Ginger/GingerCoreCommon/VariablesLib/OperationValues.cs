using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.VariablesLib
{
    public class OperationValues : RepositoryItemBase
    {
        string mValue;
        [IsSerializedForLocalRepository]
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                if (mValue != value)
                {
                    mValue = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public override string ItemName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
