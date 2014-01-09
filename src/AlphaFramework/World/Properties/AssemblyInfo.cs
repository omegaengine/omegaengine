using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("AlphaFramework Game World")]
[assembly: AssemblyDescription("Provides a generics- and template-based framework for storing game worlds.")]
[assembly: AssemblyConfiguration("")]
[assembly: NeutralResourcesLanguage("en")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
