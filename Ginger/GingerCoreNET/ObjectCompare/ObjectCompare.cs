using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Actions.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.ObjectCompare
{
    public static class ObjectCompare
    {
        public static List<string> CompareObjects(object obj1, object obj2)
        {
            List<string> differences = new List<string>();
            try
            {
                if (obj1.GetType() != obj2.GetType())
                {
                    differences.Add("Objects are of different types");
                    return differences;
                }

                PropertyInfo[] properties = obj1.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo property in properties)
                {
                    if (property.Name.Equals("ItemName") || property.Name.Equals("ItemNameField") || property.Name.Equals("FileName"))
                    {
                        continue;
                    }
                    object value1 = property.GetValue(obj1, null);
                    object value2 = property.GetValue(obj2, null);

                    if (value1 is IList list1 && value2 is IList list2)
                    {
                        if (list1.Count != list2.Count)
                        {
                            differences.Add($"{property.Name}: List counts are different ({list1.Count} vs {list2.Count})");
                            continue;
                        }

                        for (int i = 0; i < list1.Count; i++)
                        {
                            List<string> listDifferences = CompareObjects(list1[i], list2[i]);

                            if (listDifferences.Any())
                            {
                                differences.AddRange(listDifferences.Select(difference => $"{property.Name}[{i}].{difference}"));
                            }
                        }
                    }
                    else if (value1 is IDictionary dict1 && value2 is IDictionary dict2)
                    {
                        if (dict1.Count != dict2.Count)
                        {
                            differences.Add($"{property.Name}: Dictionary counts are different ({dict1.Count} vs {dict2.Count})");
                            continue;
                        }

                        foreach (object key in dict1.Keys)
                        {
                            if (!dict2.Contains(key) || !Equals(dict1[key], dict2[key]))
                            {
                                differences.Add($"{property.Name}[{key}]: {dict1[key]} != {dict2[key]}");
                            }
                        }
                    }
                    else if (value1 is RepositoryItemKey && value2 is RepositoryItemKey)
                    {
                        if (!Equals(((RepositoryItemKey)value1).ItemName, ((RepositoryItemKey)value2).ItemName))
                        {
                            string difference = $"{property.Name}: {((RepositoryItemKey)value1).ItemName} != {((RepositoryItemKey)value2).ItemName}";
                            differences.Add(difference);
                        }
                    }
                    else if (value1 is ActionDetails && value2 is ActionDetails)
                    {
                        var lstParams1 = (value1 as ActionDetails).Params;
                        var istParams2 = (value2 as ActionDetails).Params;
                        if (lstParams1.Count != istParams2.Count)
                        {
                            differences.Add($"{property.Name}: List counts are different ({lstParams1.Count} vs {istParams2.Count})");
                            continue;
                        }

                        for (int i = 0; i < lstParams1.Count; i++)
                        {
                            List<string> listDifferences = CompareObjects(lstParams1[i], istParams2[i]);

                            if (listDifferences.Any())
                            {
                                differences.AddRange(listDifferences.Select(difference => $"{property.Name}[{i}].{difference}"));
                            }
                        }
                    }
                    else if (value1 is RepositoryItemHeader && value2 is RepositoryItemHeader)
                    {
                        continue;
                    }
                    else if (!Equals(value1, value2))
                    {
                        string difference = $"{property.Name}: {value1} != {value2}";
                        differences.Add(difference);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return differences;
        }
    }
}
