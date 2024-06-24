using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly:InternalsVisibleTo("GingerCoreNETUnitTest")]
//required by Moq package to be able to mock internal types
[assembly:InternalsVisibleTo("DynamicProxyGenAssembly2")] 
namespace Amdocs.Ginger.CoreNET { }
