#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using System.Reflection;

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
