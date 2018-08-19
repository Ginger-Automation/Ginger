using Amdocs.Ginger.Plugin.Core.ActionsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class ActionInputParams
    {

        // TODO: add default value and more meta data
        //TODO: check if dictinary is faster but must keep the order
        public List<ActionParam> Values = new List<ActionParam>();

        // Easier way to get/set var    i.e: GA.ActionInputParams["URL"] = "http://aaa"

        public void Add(ActionParam Param)
        {
            Values.Add(Param);
        }

        public ActionParam GetOrCreateParam(string Name)
        {
            ActionParam AP = (from x in Values where x.Name == Name select x).FirstOrDefault();

            if (AP == null)
            {
                AP = new ActionParam() { Name = Name };
                Values.Add(AP);
            }

            return AP;
        }

        public void SetParamFromString(string Name, string value)
        {
            ActionParam AP = GetOrCreateParam(Name);
            AP.SetValueFromString(value);
        }

        public ActionParam this[string key]
        {
            get
            {
                return GetOrCreateParam(key);
            }
            set
            {
                GetOrCreateParam(key).Value = value;
            }
        }
    }
}

