using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ALM.MappedToALMWizard
{
    public enum Type { Ginger, ALM }
    public class AlmMappingGridParam : RepositoryItemBase
    {
        public string mName;
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public string mGUID;
        public string GUID
        {
            get
            {
                return mGUID;
            }
            set
            {
                if (mGUID != value)
                {
                    mGUID = value;
                    OnPropertyChanged(nameof(GUID));
                }
            }
        }

        public bool isMapped { get; set; }
        public override string ItemName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
