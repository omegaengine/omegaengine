using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Frame of Reference World")]
[assembly: AssemblyDescription("The backend layer of the Frame of Reference containing the core game logic.")]
[assembly: AssemblyConfiguration("")]
[assembly: NeutralResourcesLanguage("en")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
