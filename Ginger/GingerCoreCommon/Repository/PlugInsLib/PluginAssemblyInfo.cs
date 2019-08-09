using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.Repository
{
    public class PluginAssemblyInfo
    {
        public string Name { get; set; }
        public string FilePath { get; set; }

        public bool IsLoaded { get; set; }

        private Assembly mAssembly = null;
        public Assembly Assembly
        {
            get
            {
                // Load assembly on demand and cache
                if (mAssembly == null)
                {                    
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                    mAssembly = Assembly.LoadFrom(FilePath);
                }
                return mAssembly;
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("Loading Assembly: " + args.Name);
            return null; // Should exist in the folder - let the domain handle it
        }



        public void CheckRef()
        {
            Console.WriteLine("CheckRef for Assembly:" + mAssembly.FullName);
            AssemblyName[] list = this.Assembly.GetReferencedAssemblies();
            foreach (AssemblyName an in list)
            {
                // Assembly.LoadFrom(an.Name);
                Console.WriteLine("CheckRef:" + an.Name);

            }


        }

    }
}
