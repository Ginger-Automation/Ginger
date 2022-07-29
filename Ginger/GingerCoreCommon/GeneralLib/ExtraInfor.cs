#region Assembly GingerExternal, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\saiprani@amdocs.com\source\repos\Ginger-Automation\Ginger\Ginger\GingerCoreCommon\DLLS\GingerExternal.dll
// Decompiled with ICSharpCode.Decompiler 6.1.0.5902
#endregion

using System.Collections.Generic;

namespace GingerExternal
{
    public class ExtraInfor
    {
        private static string PASS_PHRASE = "RDNeaGRmcjcld3M0S2I1Nj1RdA==";

        private static string SALT_VALUE = "RUJLQCVUVHNxdzNkdjU=";

        private static string INIT_VECTOR = "KjlPUHhjYzlyNEcyVzZIMg==";

        private static string HASH_ALGORITHM = "U0hBMQ==";

        private static string SCM = "";

        private static string ENCRYPTION_KEY = "D3^hdfr7%ws4Kb56=Qt";

        public static List<string> getInfo()
        {
            return new List<string>
            {
                PASS_PHRASE,
                SALT_VALUE,
                INIT_VECTOR,
                HASH_ALGORITHM,
                ENCRYPTION_KEY
            };
        }
    }
}
#if false // Decompilation log
'499' items in cache
------------------
Resolve: 'netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
WARN: Version mismatch. Expected: '2.0.0.0', Got: '2.1.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\netstandard.dll'
------------------
Resolve: 'System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Runtime.dll'
------------------
Resolve: 'System.IO.MemoryMappedFiles, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.IO.MemoryMappedFiles, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.IO.MemoryMappedFiles.dll'
------------------
Resolve: 'System.IO.Pipes, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.IO.Pipes, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.IO.Pipes.dll'
------------------
Resolve: 'System.Diagnostics.Process, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Diagnostics.Process, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Diagnostics.Process.dll'
------------------
Resolve: 'System.Security.Cryptography.X509Certificates, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Security.Cryptography.X509Certificates, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Security.Cryptography.X509Certificates.dll'
------------------
Resolve: 'System.Memory, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Memory, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Memory.dll'
------------------
Resolve: 'System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Collections.dll'
------------------
Resolve: 'System.Collections.NonGeneric, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.NonGeneric, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Collections.NonGeneric.dll'
------------------
Resolve: 'System.Collections.Concurrent, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.Concurrent, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Collections.Concurrent.dll'
------------------
Resolve: 'System.ObjectModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ObjectModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.ObjectModel.dll'
------------------
Resolve: 'System.Collections.Specialized, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.Specialized, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Collections.Specialized.dll'
------------------
Resolve: 'System.ComponentModel.TypeConverter, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.TypeConverter, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.ComponentModel.TypeConverter.dll'
------------------
Resolve: 'System.ComponentModel.EventBasedAsync, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.EventBasedAsync, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.ComponentModel.EventBasedAsync.dll'
------------------
Resolve: 'System.ComponentModel.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.ComponentModel.Primitives.dll'
------------------
Resolve: 'System.ComponentModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.ComponentModel.dll'
------------------
Resolve: 'Microsoft.Win32.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.Win32.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\Microsoft.Win32.Primitives.dll'
------------------
Resolve: 'System.Console, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Console, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Console.dll'
------------------
Resolve: 'System.Data.Common, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Data.Common, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Data.Common.dll'
------------------
Resolve: 'System.Runtime.InteropServices, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.InteropServices, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Runtime.InteropServices.dll'
------------------
Resolve: 'System.Diagnostics.TraceSource, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Diagnostics.TraceSource, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Diagnostics.TraceSource.dll'
------------------
Resolve: 'System.Diagnostics.Contracts, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Diagnostics.Contracts, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Diagnostics.Contracts.dll'
------------------
Resolve: 'System.Diagnostics.TextWriterTraceListener, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Diagnostics.TextWriterTraceListener, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Diagnostics.TextWriterTraceListener.dll'
------------------
Resolve: 'System.Diagnostics.FileVersionInfo, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Diagnostics.FileVersionInfo, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Diagnostics.FileVersionInfo.dll'
------------------
Resolve: 'System.Diagnostics.StackTrace, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Diagnostics.StackTrace, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Diagnostics.StackTrace.dll'
------------------
Resolve: 'System.Diagnostics.Tracing, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Diagnostics.Tracing, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Diagnostics.Tracing.dll'
------------------
Resolve: 'System.Drawing.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Drawing.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Drawing.Primitives.dll'
------------------
Resolve: 'System.Linq.Expressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq.Expressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Linq.Expressions.dll'
------------------
Resolve: 'System.IO.Compression.Brotli, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.IO.Compression.Brotli, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.IO.Compression.Brotli.dll'
------------------
Resolve: 'System.IO.Compression, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.IO.Compression, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.IO.Compression.dll'
------------------
Resolve: 'System.IO.Compression.ZipFile, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.IO.Compression.ZipFile, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.IO.Compression.ZipFile.dll'
------------------
Resolve: 'System.IO.FileSystem.DriveInfo, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.IO.FileSystem.DriveInfo, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.IO.FileSystem.DriveInfo.dll'
------------------
Resolve: 'System.IO.FileSystem.Watcher, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.IO.FileSystem.Watcher, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.IO.FileSystem.Watcher.dll'
------------------
Resolve: 'System.IO.IsolatedStorage, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.IO.IsolatedStorage, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.IO.IsolatedStorage.dll'
------------------
Resolve: 'System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Linq.dll'
------------------
Resolve: 'System.Linq.Queryable, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq.Queryable, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Linq.Queryable.dll'
------------------
Resolve: 'System.Linq.Parallel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq.Parallel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Linq.Parallel.dll'
------------------
Resolve: 'System.Threading.Thread, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading.Thread, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Threading.Thread.dll'
------------------
Resolve: 'System.Net.Requests, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Requests, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.Requests.dll'
------------------
Resolve: 'System.Net.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.Primitives.dll'
------------------
Resolve: 'System.Net.HttpListener, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Net.HttpListener, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.HttpListener.dll'
------------------
Resolve: 'System.Net.ServicePoint, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Net.ServicePoint, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.ServicePoint.dll'
------------------
Resolve: 'System.Net.NameResolution, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.NameResolution, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.NameResolution.dll'
------------------
Resolve: 'System.Net.WebClient, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Net.WebClient, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.WebClient.dll'
------------------
Resolve: 'System.Net.Http, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Http, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.Http.dll'
------------------
Resolve: 'System.Net.WebHeaderCollection, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.WebHeaderCollection, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.WebHeaderCollection.dll'
------------------
Resolve: 'System.Net.WebProxy, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Net.WebProxy, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.WebProxy.dll'
------------------
Resolve: 'System.Net.Mail, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Net.Mail, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.Mail.dll'
------------------
Resolve: 'System.Net.NetworkInformation, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.NetworkInformation, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.NetworkInformation.dll'
------------------
Resolve: 'System.Net.Ping, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Ping, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.Ping.dll'
------------------
Resolve: 'System.Net.Security, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Security, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.Security.dll'
------------------
Resolve: 'System.Net.Sockets, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Sockets, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.Sockets.dll'
------------------
Resolve: 'System.Net.WebSockets.Client, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.WebSockets.Client, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.WebSockets.Client.dll'
------------------
Resolve: 'System.Net.WebSockets, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.WebSockets, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Net.WebSockets.dll'
------------------
Resolve: 'System.Runtime.Numerics, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.Numerics, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Runtime.Numerics.dll'
------------------
Resolve: 'System.Numerics.Vectors, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Numerics.Vectors, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Numerics.Vectors.dll'
------------------
Resolve: 'System.Reflection.DispatchProxy, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Reflection.DispatchProxy, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Reflection.DispatchProxy.dll'
------------------
Resolve: 'System.Reflection.Emit, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Reflection.Emit, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Reflection.Emit.dll'
------------------
Resolve: 'System.Reflection.Emit.ILGeneration, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Reflection.Emit.ILGeneration, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Reflection.Emit.ILGeneration.dll'
------------------
Resolve: 'System.Reflection.Emit.Lightweight, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Reflection.Emit.Lightweight, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Reflection.Emit.Lightweight.dll'
------------------
Resolve: 'System.Reflection.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Reflection.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Reflection.Primitives.dll'
------------------
Resolve: 'System.Resources.Writer, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Resources.Writer, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Resources.Writer.dll'
------------------
Resolve: 'System.Runtime.CompilerServices.VisualC, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.CompilerServices.VisualC, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Runtime.CompilerServices.VisualC.dll'
------------------
Resolve: 'System.Runtime.InteropServices.RuntimeInformation, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.InteropServices.RuntimeInformation, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Runtime.InteropServices.RuntimeInformation.dll'
------------------
Resolve: 'System.Runtime.Serialization.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.Serialization.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Runtime.Serialization.Primitives.dll'
------------------
Resolve: 'System.Runtime.Serialization.Xml, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.Serialization.Xml, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Runtime.Serialization.Xml.dll'
------------------
Resolve: 'System.Runtime.Serialization.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.Serialization.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Runtime.Serialization.Json.dll'
------------------
Resolve: 'System.Runtime.Serialization.Formatters, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.Serialization.Formatters, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Runtime.Serialization.Formatters.dll'
------------------
Resolve: 'System.Security.Claims, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Security.Claims, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Security.Claims.dll'
------------------
Resolve: 'System.Security.Cryptography.Algorithms, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Security.Cryptography.Algorithms, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Security.Cryptography.Algorithms.dll'
------------------
Resolve: 'System.Security.Cryptography.Csp, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Security.Cryptography.Csp, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Security.Cryptography.Csp.dll'
------------------
Resolve: 'System.Security.Cryptography.Encoding, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Security.Cryptography.Encoding, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Security.Cryptography.Encoding.dll'
------------------
Resolve: 'System.Security.Cryptography.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Security.Cryptography.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Security.Cryptography.Primitives.dll'
------------------
Resolve: 'System.Text.Encoding.Extensions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Text.Encoding.Extensions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Text.Encoding.Extensions.dll'
------------------
Resolve: 'System.Text.RegularExpressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Text.RegularExpressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Text.RegularExpressions.dll'
------------------
Resolve: 'System.Threading, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Threading.dll'
------------------
Resolve: 'System.Threading.Overlapped, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading.Overlapped, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Threading.Overlapped.dll'
------------------
Resolve: 'System.Threading.ThreadPool, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading.ThreadPool, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Threading.ThreadPool.dll'
------------------
Resolve: 'System.Threading.Tasks.Parallel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading.Tasks.Parallel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Threading.Tasks.Parallel.dll'
------------------
Resolve: 'System.Transactions.Local, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Transactions.Local, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Transactions.Local.dll'
------------------
Resolve: 'System.Web.HttpUtility, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Web.HttpUtility, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Web.HttpUtility.dll'
------------------
Resolve: 'System.Xml.ReaderWriter, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Xml.ReaderWriter, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Xml.ReaderWriter.dll'
------------------
Resolve: 'System.Xml.XDocument, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Xml.XDocument, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Xml.XDocument.dll'
------------------
Resolve: 'System.Xml.XmlSerializer, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Xml.XmlSerializer, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Xml.XmlSerializer.dll'
------------------
Resolve: 'System.Xml.XPath.XDocument, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Xml.XPath.XDocument, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Xml.XPath.XDocument.dll'
------------------
Resolve: 'System.Xml.XPath, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Xml.XPath, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.4\ref\net6.0\System.Xml.XPath.dll'
#endif
