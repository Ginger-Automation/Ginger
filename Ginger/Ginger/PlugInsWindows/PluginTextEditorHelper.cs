using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;

namespace Ginger.PlugInsWindows
{
    class PluginTextEditorHelper
    {

        internal static IEnumerable<ITextEditor> GetTextFileEditors(PluginPackage pluginPackage)
        {
            List<ITextEditor> textEditors = new List<ITextEditor>();

            if (string.IsNullOrEmpty(pluginPackage.PluginPackageInfo.UIDLL))
            {
                return textEditors;
            }

            string UIDLLFileName = Path.Combine(pluginPackage.Folder, "UI", pluginPackage.PluginPackageInfo.UIDLL);
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
