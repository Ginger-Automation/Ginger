#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ginger.PlugInsWindows
{
    class PluginTextEditorHelper
    {

        internal static IEnumerable<ITextEditor> GetTextFileEditors(PluginPackage pluginPackage)
        {
            List<ITextEditor> textEditors = [];

            if (string.IsNullOrEmpty(((PluginPackageOperations)pluginPackage.PluginPackageOperations).PluginPackageInfo.UIDLL))
            {
                return textEditors;
            }

            string UIDLLFileName = Path.Combine(pluginPackage.Folder, "UI", ((PluginPackageOperations)pluginPackage.PluginPackageOperations).PluginPackageInfo.UIDLL);
            if (!File.Exists(UIDLLFileName))
            {
                throw new Exception("Plugin UI DLL not found: " + UIDLLFileName);
            }


            Assembly assembly = Assembly.LoadFrom(UIDLLFileName); // Assembly.UnsafeLoadFrom(UIDLLFileName);               
            var list = from type in assembly.GetTypes()
                       where typeof(ITextEditor).IsAssignableFrom(type) && type.IsAbstract == false
                       select type;

            foreach (Type t in list)
            {
                ITextEditor textEditor = (ITextEditor)assembly.CreateInstance(t.FullName); // Activator.CreateInstance(t);
                textEditors.Add(textEditor);
            }

            return textEditors;
        }
    }
}
