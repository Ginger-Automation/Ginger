#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
using System.Linq;
using Amdocs.Ginger.Common;
using Microsoft.Win32;

namespace GingerCore.GeneralFunctions
{
    public enum eRegistryRoot
    {
        HKEY_CLASSES_ROOT,HKEY_CURRENT_USER,HKEY_LOCAL_MACHINE,HKEY_USERS,HKEY_CURRENT_CONFIG
    }

    public class RegistryFunctions
    {
        public static RegistryKey GetRegistryKey(eRegistryRoot rootType, string keyPath, bool writeAcceessNeeded=false)
        {
            RegistryKey regKey=null;
            try
            {
                //return the registry key                
                switch (rootType)
                {
                    case eRegistryRoot.HKEY_CLASSES_ROOT:
                        regKey = Registry.ClassesRoot.OpenSubKey(keyPath, writeAcceessNeeded);
                        break;
                    case eRegistryRoot.HKEY_CURRENT_USER:
                        regKey = Registry.CurrentUser.OpenSubKey(keyPath, writeAcceessNeeded);
                        break;
                    case eRegistryRoot.HKEY_LOCAL_MACHINE:
                        regKey = Registry.LocalMachine.OpenSubKey(keyPath, writeAcceessNeeded);
                        break;
                    case eRegistryRoot.HKEY_USERS:
                        regKey = Registry.Users.OpenSubKey(keyPath, writeAcceessNeeded);
                        break;
                    case eRegistryRoot.HKEY_CURRENT_CONFIG:
                        regKey = Registry.CurrentConfig.OpenSubKey(keyPath, writeAcceessNeeded);
                        break;
                }
                return regKey;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to get the registry key for the RootType: '" + rootType +
                    "' and KeyPath: '" + keyPath + "'", ex);
                regKey = null;
                return regKey;
            }
        }

        public static bool CreateRegistryKey(eRegistryRoot rootType, string keyPath)
        {
            RegistryKey regKey = null;
            try
            {
                if (GetRegistryKey(rootType, keyPath) == null)
                {
                    switch (rootType)
                    {
                        case eRegistryRoot.HKEY_CLASSES_ROOT:
                            regKey = Registry.ClassesRoot.CreateSubKey(keyPath);
                            break;
                        case eRegistryRoot.HKEY_CURRENT_USER:
                            regKey = Registry.CurrentUser.CreateSubKey(keyPath);
                            break;
                        case eRegistryRoot.HKEY_LOCAL_MACHINE:
                            regKey = Registry.LocalMachine.CreateSubKey(keyPath);
                            break;
                        case eRegistryRoot.HKEY_USERS:
                            regKey = Registry.Users.CreateSubKey(keyPath);
                            break;
                        case eRegistryRoot.HKEY_CURRENT_CONFIG:
                            regKey = Registry.CurrentConfig.CreateSubKey(keyPath);
                            break;
                    }

                    if (regKey != null)
                        return true;
                    else
                        return false;
                }
                else
                {
                    //registry key already exist
                    return true;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to create the registry key for the RootType: '" + rootType +
                                                    "' and KeyPath: '" + keyPath + "'", ex);
                return false;
            }
        }

        public static object GetRegistryValue(eRegistryRoot rootType, string keyPath, string paramterName)
        {
            try
            {
                //Get the registry key
                RegistryKey regKey = GetRegistryKey(rootType, keyPath);
                if (regKey != null)
                {
                    //check if requied parameter exist
                    if ((regKey.GetValueNames()).Contains(paramterName))
                    {
                        //return parameter value
                        return regKey.GetValue(paramterName);
                    }
                    else
                    {
                        //requied parameter was not found
                        Reporter.ToLog(eAppReporterLogLevel.WARN, "Failed to get the registry key value for the RootType: '" + rootType +
                                      "', KeyPath: '" + keyPath + "' and ParameterName: '" + paramterName + "'- Parameter was not found");
                        return null;
                    }
                }
                else
                {
                    //Failed to get the registry key
                    return null;
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to get the registry key value for the RootType: '" + rootType +
                                      "', KeyPath: '" + keyPath + "' and ParameterName: '" + paramterName + "'", ex);
                return null;
            }
        }

        public static bool SetRegistryValue(eRegistryRoot rootType, string keyPath, string paramterName, 
                                                                        object value, RegistryValueKind valueType)
        {
            try
            {
                //Get the registry key
                RegistryKey regKey = GetRegistryKey(rootType, keyPath, true);
                if (regKey != null)
                {
                    //set the value
                    regKey.SetValue(paramterName, value, valueType);
                    return true;
                }
                else
                {
                    //Failed to get the registry key
                    return false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to set the registry key value: '" + value + 
                    "' for the RootType: '" + rootType + "', KeyPath: '" + keyPath + "' and ParameterName: '" + paramterName + "'", ex);
                return false;
            }
        }

        public static bool CheckRegistryValueExist(eRegistryRoot rootType, string keyPath, string paramterName, 
                                                        object expectedValue, Microsoft.Win32.RegistryValueKind expectedValueType, 
                                                                bool addValueIfMissing, bool silentMode)
        {
            bool addValue = false;
            try
            {
                if ((GetRegistryValue(rootType, keyPath, paramterName) != null) &&
                        GetRegistryValue(rootType, keyPath, paramterName).ToString() == expectedValue.ToString())
                {
                    return true;
                }
                else
                {                   
                    if (addValueIfMissing)
                    {
                        if (silentMode)
                        {
                            addValue = true;
                        }
                        else
                        {
                            //ask user
                            if ((Reporter.ToUser(eUserMsgKeys.AddRegistryValue, rootType + "\\" + keyPath)) == System.Windows.MessageBoxResult.Yes)
                                addValue = true;
                            else
                                return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    if (addValue)
                    {
                        //create the registry key if needed
                        if (CreateRegistryKey(rootType, keyPath))
                        {
                            //add the registry value
                            if (SetRegistryValue(rootType, keyPath, paramterName, expectedValue, expectedValueType))
                            {
                                //succeed to add the value
                                if (!silentMode)
                                {
                                    //show success to user
                                    Reporter.ToUser(eUserMsgKeys.AddRegistryValueSucceed, rootType + "\\" + keyPath);
                                }
                                return true;
                            }
                            else
                            {
                                //failed to add the value
                                if (!silentMode)
                                {
                                    //show failure to user
                                    Reporter.ToUser(eUserMsgKeys.AddRegistryValueFailed, rootType + "\\" + keyPath);
                                }
                                return false;
                            }
                        }
                        else
                        {
                            if (!silentMode)
                            {
                                //show failure to user
                                Reporter.ToUser(eUserMsgKeys.AddRegistryValueFailed, rootType + "\\" + keyPath);
                            }
                            return false;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to complete the registry value check for the key: " + 
                                                                                        rootType + "\\" + keyPath, ex);                
                if (!silentMode)
                {
                    Reporter.ToUser(eUserMsgKeys.RegistryValuesCheckFailed);
                }
                return false;
            }
        }
    }
}