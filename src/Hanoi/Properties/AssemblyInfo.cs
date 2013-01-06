using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Towers of Hanoi")]
[assembly: AssemblyDescription("Visualization of the classic puzzle \"The towers of hanoi\".")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("NanoByte")]
[assembly: AssemblyProduct("Towers of Hanoi")]
[assembly: AssemblyCopyright("Copyright 2006-2012 Bastian Eicher")]
[assembly: NeutralResourcesLanguage("en")]

// Version information
[assembly: AssemblyVersion("0.8.3")]
[assembly: AssemblyFileVersion("0.8.3")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
