using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCoreNET.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GingerCoreNET.Drivers.CommunicationProtocol
{
    public class ObjectReflectionHelper
    {
        private object mObj;

        public Object obj { get { return mObj; } set { mObj = value; } }

        public NewPayLoad GetObjectAsPayLoad(string PLName, Attribute attr)
        {
            NewPayLoad PL = new NewPayLoad(PLName);
            PL.AddValue(mObj.GetType().FullName);
            List<PropertyInfo> list = GetProperties(attr);

            foreach (PropertyInfo PI in list)
            {
                dynamic v = PI.GetValue(mObj);

                // Enum might be unknown = not set - so no need to write to xml, like null for object                        
                if (PI.PropertyType.IsEnum)
                {
                    string vs = v.ToString();
                    // No need to write enum unknown = null
                    if (vs != "Unknown")
                    {
                        PL.AddEnumValue(v);
                    }
                }
                else if (v is IObservableList)
                {
                    List<NewPayLoad> lst = new List<NewPayLoad>();                    
                    foreach (object o in v)
                    {
                        ObjectReflectionHelper ORHItem = new ObjectReflectionHelper();
                        ORHItem.obj = o;
                        NewPayLoad PL1 = ORHItem.GetObjectAsPayLoad("Item", attr);
                        lst.Add(PL1);                        
                    }
                    PL.AddListPayLoad(lst);

                }
                else if (PI.PropertyType.Name == "String" )
                {
                    PL.AddValue((string)v);
                }
                else
                {
                    throw new Exception("Unknown type to handle - " + v);
                }
            }

            PL.ClosePackage();
            return PL;
        }

        // Create an Object based on the Payload and set the obj value for properoties which have requested attr 
        public void CreateObjectFromPayLoad(NewPayLoad PL, Attribute attr)
        {
            string cls = PL.GetValueString();
            
            //TODO:  test it - where is it used? !!!
            //FIXME!! need for remoteobj
            //mObj = NewRepositorySerializer.GingerCoreNETAssembly.CreateInstance(cls);
            
            UpdateObjectFromPayLoad(PL, attr);
        }

        internal void UpdateObjectFromPayLoad(NewPayLoad PL, Attribute attr)
        {
            List<PropertyInfo> list = GetProperties(attr);

            //TODO: unpack the Payload and update the obj 
            foreach (PropertyInfo PI in list)
            {
                object v = PI.GetValue(mObj);

                if (PI.PropertyType.IsEnum)
                {
                    string s = PL.GetValueEnum();
                    object o = Enum.Parse(PI.PropertyType, s);
                    PI.SetValue(mObj, o);
                }
                else if (v is IObservableList)
                {
                    List<NewPayLoad> lst = PL.GetListPayLoad();
                    foreach (NewPayLoad PL1 in lst)
                    {
                        ObjectReflectionHelper ORH1 = new ObjectReflectionHelper();
                        ORH1.CreateObjectFromPayLoad(PL1, attr);
                        ((IObservableList)v).Add(ORH1.obj);
                    }
                }
                else if (PI.PropertyType.Name == "String")
                {
                    string s = PL.GetValueString();
                    PI.SetValue(mObj, s);
                }
                else
                {
                    throw new Exception("Unknown type to handle - " + PI.PropertyType);
                }
            }
        }

        // return all properites of object which contain specific attribute/annotation - can be [IsSerialLized, [IsSentToRemoteAgen] etc...
        public List<PropertyInfo> GetProperties(Attribute attr)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();

            var properties = mObj.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property).OrderBy(x => x.Name);
            foreach (MemberInfo mi in properties)
            {

                Attribute token = Attribute.GetCustomAttribute(mi, attr.GetType(), false);
                if (token == null) continue;                
                PropertyInfo PI = mObj.GetType().GetProperty(mi.Name);
                list.Add(PI);
            }
            return list;
        }

        public object GetPropertyValue(PropertyInfo PI)
        {
            return PI.GetValue(mObj);
        }
    }
}