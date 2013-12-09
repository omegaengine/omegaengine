using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("OmegaEngine")]
[assembly: AssemblyDescription("The core of the OmegaEngine, providing graphics and sound features.")]
[assembly: AssemblyConfiguration("")]
[assembly: NeutralResourcesLanguage("en")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
