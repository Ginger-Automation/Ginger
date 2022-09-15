using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.External.Configurations
{
    public class VariableConfiguration : RepositoryItemBase
    {
        private string mVarName;
        [IsSerializedForLocalRepository]
        public string VarName
        {
            get
            {
                return mVarName;
            }
            set
            {
                mVarName = value;
                OnPropertyChanged(nameof(VarName));
            }
        }

        public override string ItemName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
