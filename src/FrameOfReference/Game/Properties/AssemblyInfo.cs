using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Frame of Reference")]
[assembly: AssemblyDescription("An OmegaEngine game.")]
[assembly: AssemblyConfiguration("")]
[assembly: NeutralResourcesLanguage("en")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: ComVisible(false)]
