using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ApplicationModelsLib.APIModels
{
    public class NodeToDelete
    {
        public string ParentOuterXml;
        public Tuple<int, int> stringNodeRange;

        public NodeToDelete(string parentOuterXml)
        {
            ParentOuterXml = parentOuterXml;
        }
    }
}
