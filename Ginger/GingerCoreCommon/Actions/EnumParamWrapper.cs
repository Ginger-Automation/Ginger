using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.Actions
{
    public class EnumParamWrapper
    {
        List<string> mList = new List<string>();

        public EnumParamWrapper(string values)
        {
            //TODO: fixme!
            mList.Add("aa");
            mList.Add("bb");
        }

        public List<string> Values
        {
            get
            {
                return mList;
            }            
        }
    }
}
