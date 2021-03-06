﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34011
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FrameOfReference.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("FrameOfReference.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The game is already running..
        /// </summary>
        internal static string AlreadyRunning {
            get {
                return ResourceManager.GetString("AlreadyRunning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are about to start an automatic benchmark. This will collect data about your hardware and the performance of the game, which is then sent to the developers.
        ///This process will take approximately 20 minutes. In order not to falsify the results you should not run any other applications during the process. The game will run the benchmark in full-screen mode, independently of your settings..
        /// </summary>
        internal static string BenchmarkInfo {
            get {
                return ResourceManager.GetString("BenchmarkInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &amp;Start.
        /// </summary>
        internal static string BenchmarkInfoContinue {
            get {
                return ResourceManager.GetString("BenchmarkInfoContinue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The benchmark data was collected successfully and can now be sent to the developers..
        /// </summary>
        internal static string BenchmarkReady {
            get {
                return ResourceManager.GetString("BenchmarkReady", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &amp;Don&apos;t send
        ///Throw away the data and exit.
        /// </summary>
        internal static string BenchmarkReadyCancel {
            get {
                return ResourceManager.GetString("BenchmarkReadyCancel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &amp;Send
        ///Send the data and then exit.
        /// </summary>
        internal static string BenchmarkReadyContinue {
            get {
                return ResourceManager.GetString("BenchmarkReadyContinue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to read game content archives..
        /// </summary>
        internal static string FailedReadArchives {
            get {
                return ResourceManager.GetString("FailedReadArchives", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Icon similar to (Icon).
        /// </summary>
        internal static System.Drawing.Icon Icon {
            get {
                object obj = ResourceManager.GetObject("Icon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Loading {
            get {
                object obj = ResourceManager.GetObject("Loading", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Loading graphics....
        /// </summary>
        internal static string LoadingGraphics {
            get {
                return ResourceManager.GetString("LoadingGraphics", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file Editor.exe could not be found..
        /// </summary>
        internal static string MissingEditor {
            get {
                return ResourceManager.GetString("MissingEditor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is currently no game session loaded..
        /// </summary>
        internal static string NoSessionLoaded {
            get {
                return ResourceManager.GetString("NoSessionLoaded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please try restarting or reinstalling the game..
        /// </summary>
        internal static string PleaseRestart {
            get {
                return ResourceManager.GetString("PleaseRestart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The updates are complete.
        ///The game is now ready to launch..
        /// </summary>
        internal static string ReadyToLaunch {
            get {
                return ResourceManager.GetString("ReadyToLaunch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &amp;Play.
        /// </summary>
        internal static string ReadyToLaunchContinue {
            get {
                return ResourceManager.GetString("ReadyToLaunchContinue", resourceCulture);
            }
        }
    }
}
