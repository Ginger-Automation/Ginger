using ALM_Common.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.ALM.Helper
{
    public class ALMCommonMapper
    {
        public static ResourceType GetGresourceType(AlmDataContractsStd.Enums.ResourceType resourceType)
        {
            return (ResourceType)resourceType;
        }
    }
}
