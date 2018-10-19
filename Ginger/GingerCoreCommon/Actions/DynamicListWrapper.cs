using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.UserControlsLib.ActionInputValueUserControlLib
{
    public class DynamicListWrapper
    {        
        public ObservableList<dynamic> list { get; set; }

        public ObservableList<dynamic> GetList()
        {
            list = new ObservableList<dynamic>();

            // temp code - need to get from the plugin action json
            dynamic expando = new ExpandoObject();
            expando.Name = "Brian";
            expando.Country = "USA";

            // Use Addprop as it dynamic
            AddProperty(expando, "value", "123");

            dynamic expando2 = new ExpandoObject();
            expando2.Name = "Sam";
            expando2.Country = "ffff";
            AddProperty(expando2, "value", "44");

            list.Add(expando);
            list.Add(expando2);            
            return list;
        }
        

        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

    }
}
