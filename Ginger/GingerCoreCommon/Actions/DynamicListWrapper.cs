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

        string mListType;

        public ObservableList<dynamic> Items { get; set; }

        List<string> mListProperties;

        public DynamicListWrapper(string listType, bool AddDummyItem = false)
        {
            mListType = listType;
            SetPropertiesList();
            Items = new ObservableList<dynamic>();

            if (AddDummyItem)
            {
                dynamic expando = new ExpandoObject();                
                foreach (string property in mListProperties)
                {                    
                    AddProperty(expando, property, null);
                }
                Items.Add(expando);
            }
        }

        void SetPropertiesList()
        {
            if (string.IsNullOrEmpty(mListType))
            {
                return;
            }
            mListProperties = new List<string>();
            string props = GetStringBetween(mListType, "{Properties=", "}");
            string[] arr = props.Split(',');
            foreach (string prop in arr)
            {                
                string[] pab = prop.Split(':');
                string name = pab[0];
                string type = pab[1];
                mListProperties.Add(name);
            }
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


        // TODO: dup code unite
        public string GetStringBetween(string STR, string FirstString, string LastString = null)
        {
            if (string.IsNullOrEmpty(STR))
            {
                return "";
            }
            string str = "";
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2;
            if (LastString != null)
            {
                Pos2 = STR.IndexOf(LastString, Pos1);
            }
            else
            {
                Pos2 = STR.Length;
            }

            if ((Pos2 - Pos1) > 0)
            {
                str = STR.Substring(Pos1, Pos2 - Pos1);
                return str;
            }
            else
            {
                return "";
            }
        }

        internal List<string> GetListItemProperties()
        {
            return mListProperties;
        }
    }
}
