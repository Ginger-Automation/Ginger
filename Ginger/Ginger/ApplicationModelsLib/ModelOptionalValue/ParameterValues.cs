using System.Collections.Generic;
using System.Linq;

namespace Ginger.ApplicationModelsLib
{
    /// <summary>
    /// This class is used to hold the parameter details while import
    /// </summary>
    public class ParameterValues
    {
        /// <summary>
        /// Gets/Sets ParamName
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// Gets/Sets Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets/Sets ParameterValuesByNameDic
        /// </summary>
        List<string> mParameterValuesByNameDic = new List<string>();// EXCEL & DB
        public List<string> ParameterValuesByNameDic
        {
            get { return mParameterValuesByNameDic; }
            set { mParameterValuesByNameDic = value; }
        }        
    }
}
