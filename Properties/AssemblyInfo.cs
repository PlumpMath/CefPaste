using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Chromium")]
[assembly: AssemblyProduct( "CefPaste" )]
[assembly: AssemblyCopyright("Copyright © 2013 Alex Forster")]
[assembly: ComVisible(false)]

[assembly: AssemblyVersion("1.0.38.13115")]

#if DEBUG
[assembly: AssemblyConfiguration( "Debug" )]
#else
[assembly: AssemblyConfiguration( "Release" )]
#endif
