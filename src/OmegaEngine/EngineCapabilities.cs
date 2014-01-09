/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using Common;
using Common.Info;
using Common.Utils;
using SlimDX.Direct3D9;

namespace OmegaEngine
{
    /// <summary>
    /// Methods for determining the rendering capabilities of the graphics hardware.
    /// </summary>
    public sealed class EngineCapabilities
    {
        #region Dependencies
        private readonly Direct3D _direct3D;
        private readonly Capabilities _capabilities;
        private readonly EngineConfig _engineConfig;

        /// <summary>
        /// Creates a new engine capabilities object.
        /// </summary>
        /// <param name="direct3D">Provides access to the Direct3D subsystem.</param>
        /// <param name="config">The settings used to initialize the <see cref="Engine"/>.</param>
        internal EngineCapabilities(Direct3D direct3D, EngineConfig config)
        {
            #region Sanity checks
            if (direct3D == null) throw new ArgumentNullException("direct3D");
            #endregion

            _direct3D = direct3D;
            _capabilities = _direct3D.GetDeviceCaps(0, DeviceType.Hardware);
            _engineConfig = config;

            DetermineHardwareInformation();
            DetermineDeviceCapabilities();
        }
        #endregion

        #region Determine information
        /// <summary>
        /// Helper method for the constructor that fills <see cref="Hardware"/> with information.
        /// </summary>
        private void DetermineHardwareInformation()
        {
            _hardware.OS = OSInfo.Current;

            #region CPU
            try
            {
                var mos = new ManagementObjectSearcher("SELECT Manufacturer, Name, MaxClockSpeed, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor");
                foreach (ManagementBaseObject mob in mos.Get())
                {
                    _hardware.Cpu.Manufacturer = (string)mob.Properties["Manufacturer"].Value;
                    _hardware.Cpu.Name = (string)mob.Properties["Name"].Value;
                    unchecked
                    {
                        _hardware.Cpu.Speed = (int)(uint)mob.Properties["MaxClockSpeed"].Value;
                        _hardware.Cpu.Cores = (int)(uint)mob.Properties["NumberOfCores"].Value;
                        _hardware.Cpu.Logical = (int)(uint)mob.Properties["NumberOfLogicalProcessors"].Value;
                    }

                    // Only handle first CPU
                    break;
                }
            }
            catch (ManagementException)
            {
                Log.Warn("Could not get Hardware-info for CPU");
            }
            #endregion

            #region RAM
            try
            {
                var mos = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (ManagementBaseObject mob in mos.Get())
                {
                    unchecked
                    {
                        _hardware.Ram.Size = (int)(Convert.ToUInt64(mob.Properties["TotalPhysicalMemory"].Value, CultureInfo.InvariantCulture) / 1024 / 1024);
                    }

                    // There is only one RAM
                    break;
                }
            }
            catch (ManagementException)
            {
                Log.Warn("Could not get Hardware-info for RAM");
            }
            #endregion

            #region GPU
            DisplayModes = _direct3D.Adapters[_engineConfig.Adapter].GetDisplayModes(Format.X8R8G8B8);
            var adapter = _direct3D.Adapters[_engineConfig.Adapter].Details;
            _hardware.Gpu.Manufacturer = adapter.VendorId;
            _hardware.Gpu.Name = adapter.Description;
            _hardware.Gpu.MaxAA = MaxAA;

            try
            {
                var mos = new ManagementObjectSearcher("SELECT AdapterRAM FROM Win32_VideoController");
                foreach (ManagementBaseObject mob in mos.Get())
                {
                    _hardware.Gpu.Ram = (int)(Convert.ToInt64(mob.Properties["AdapterRAM"].Value, CultureInfo.InvariantCulture) / 1024 / 1024);

                    // Keep first graphics adapter that has RAM
                    if (_hardware.Gpu.Ram > 0) break;
                }
            }
            catch (ManagementException)
            {
                Log.Warn("Could not get Hardware-info for GPU RAM");
            }

            // Write hardware in log-file
            Log.Info(_hardware.ToString());
            #endregion
        }

        /// <summary>
        /// Helper method for the constructor that fills <see cref="_capabilities"/> and <see cref="MaxShaderModel"/> with information and checks certain conditions are met.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PixelShaderVersion"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SupportedAA"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "VertexShaderVersion"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PureDevice"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "HWTransformAndLight")]
        private void DetermineDeviceCapabilities()
        {
            #region Pixel shader
            if (_engineConfig.ForceShaderModel == null)
            {
                // Detect pixel shader version (and find subversions of Pixel Shader 2.0)
                MaxShaderModel = _capabilities.PixelShaderVersion;
                if (MaxShaderModel == new Version(2, 0) && _capabilities.MaxPixelShader30InstructionSlots >= 96)
                {
                    if (_capabilities.PS20Caps.TempCount >= 22 &&
                        ((int)_capabilities.PS20Caps.Caps).CheckFlag((int)(PixelShaderCaps.ArbitrarySwizzle | PixelShaderCaps.GradientInstructions | PixelShaderCaps.Predication | PixelShaderCaps.NoDependentReadLimit | PixelShaderCaps.NoTextureInstructionLimit)))
                    { // Pixel shader 2.0a
                        MaxShaderModel = new Version(2, 0, 1);
                    }
                    else if (_capabilities.PS20Caps.TempCount >= 32 &&
                             ((int)_capabilities.PS20Caps.Caps).CheckFlag((int)PixelShaderCaps.NoTextureInstructionLimit))
                    { // Pixel shader 2.0b
                        MaxShaderModel = new Version(2, 0, 2);
                    }
                }
            }
            else MaxShaderModel = _engineConfig.ForceShaderModel;
            #endregion

            // Log GPU capabilities
            Log.Info("GPU capabilities:\n" +
                     "HWTransformAndLight: " + ((int)_capabilities.DeviceCaps).CheckFlag((int)DeviceCaps.HWTransformAndLight) + "\n" +
                     "PureDevice: " + ((int)_capabilities.DeviceCaps).CheckFlag((int)DeviceCaps.PureDevice) + "\n" +
                     "Anisotropic: " + Anisotropic + "\n" +
                     "VertexShaderVersion: " + _capabilities.VertexShaderVersion + "\n" +
                     "PixelShaderVersion: " + MaxShaderModel + "\n" +
                     "SupportedAA: " + SupportedAA + "\n");

            // Ensure support for linear texture filtering
            if (!((int)_capabilities.TextureFilterCaps).CheckFlag((int)(FilterCaps.MinLinear | FilterCaps.MagLinear | FilterCaps.MipLinear)))
            {
                //throw new NotAvailableException(Properties.Resources.NoLinearTextureFiltering);
                Log.Warn("Missing support for linear texture filtering");
            }
        }
        #endregion

        //--------------------//

        #region Properties
        private Hardware _hardware;

        /// <summary>
        /// Information about the hardware of this computer.
        /// </summary>
        public Hardware Hardware { get { return _hardware; } }

        /// <summary>
        /// A list of all supported monitor resolutions.
        /// </summary>
        public DisplayModeCollection DisplayModes { get; private set; }

        /// <summary>
        /// Does the graphics support rasterization, transform, lighting, and shading in hardware?
        /// </summary>
        public bool PureDevice { get { return ((int)_capabilities.DeviceCaps).CheckFlag((int)(DeviceCaps.PureDevice)); } }

        /// <summary>
        /// Does the graphics support hardware transformation and lighting?
        /// </summary>
        public bool HardwareVertexProcessing { get { return ((int)_capabilities.DeviceCaps).CheckFlag((int)DeviceCaps.HWTransformAndLight); } }

        /// <summary>
        /// Does the hardware the engine is running on support anisotropic texture filtering?
        /// </summary>
        /// <seealso cref="Engine.Anisotropic"/>
        public bool Anisotropic { get { return ((int)_direct3D.GetDeviceCaps(0, DeviceType.Hardware).TextureFilterCaps).CheckFlag((int)(FilterCaps.MinAnisotropic | FilterCaps.MagAnisotropic)); } }

        /// <summary>
        /// The maximum shader model version to be used (2.a is replaced by 2.0.1, 2.b is replaced by 2.0.2)
        /// </summary>
        public Version MaxShaderModel { get; private set; }

        /// <summary>
        /// Does the hardware the engine is running on support per-pixel effects?
        /// </summary>
        /// <seealso cref="EngineEffects.PerPixelLighting"/>
        /// <seealso cref="EngineEffects.NormalMapping"/>
        /// <seealso cref="EngineEffects.PostScreenEffects"/>
        /// <seealso cref="EngineEffects.Shadows"/>
        public bool PerPixelEffects { get { return MaxShaderModel >= new Version(2, 0); } }

        /// <summary>
        /// Does the hardware the engine is running on support terrain texture double sampling?
        /// </summary>
        /// <seealso cref="EngineEffects.DoubleSampling"/>
        public bool DoubleSampling { get { return MaxShaderModel >= new Version(2, 0, 1); } }
        #endregion

        #region Resolution check
        /// <summary>
        /// Checks whether the graphics card supports a certain monitor resolution
        /// </summary>
        /// <param name="adapter">The adapter to check</param>
        /// <param name="width">The resolution width</param>
        /// <param name="height">The resolution height</param>
        /// <returns><see langword="true"/> if the level is supported</returns>
        public static bool CheckResolution(int adapter, int width, int height)
        {
            using (var manager = new Direct3D())
                return manager.Adapters[adapter].GetDisplayModes(Format.X8R8G8B8).Any(mode => mode.Width == width && mode.Height == height);
        }

        /// <summary>
        /// Checks whether the graphics card supports a certain monitor resolution
        /// </summary>
        /// <param name="width">The resolution width</param>
        /// <param name="height">The resolution height</param>
        /// <returns><see langword="true"/> if the level is supported</returns>
        public bool CheckResolution(int width, int height)
        {
            return DisplayModes.Any(mode => mode.Width == width && mode.Height == height);
        }

        /// <summary>
        /// Checks whether a certain depth stencil format is supported by the <see cref="SlimDX.Direct3D9.Device"/>
        /// </summary>
        internal static bool TestDepthStencil(int adapter, Format depthFormat)
        {
            using (var manager = new Direct3D())
            {
                return manager.CheckDeviceFormat(adapter, DeviceType.Hardware, Format.X8R8G8B8,
                    Usage.DepthStencil, ResourceType.Surface, depthFormat);
            }
        }
        #endregion

        #region Anti aliasing check
        /// <summary>
        /// Checks whether the graphics card supports a certain level of anti aliasing
        /// </summary>
        /// <param name="adapter">The adapter to check</param>
        /// <param name="sample">The sample level to check</param>
        /// <returns><see langword="true"/> if the level is supported</returns>
        public static bool CheckAA(int adapter, int sample)
        {
            using (var manager = new Direct3D())
            {
                return (manager.CheckDeviceMultisampleType(adapter, DeviceType.Hardware,
                    manager.Adapters[adapter].CurrentDisplayMode.Format, true, (MultisampleType)sample));
            }
        }

        /// <summary>
        /// Checks whether the graphics card supports a certain level of anti aliasing
        /// </summary>
        /// <param name="sample">The sample level to check</param>
        /// <returns><see langword="true"/> if the level is supported</returns>
        public bool CheckAA(int sample)
        {
            return (_direct3D.CheckDeviceMultisampleType(_engineConfig.Adapter, DeviceType.Hardware,
                _direct3D.Adapters[_engineConfig.Adapter].CurrentDisplayMode.Format, true, (MultisampleType)sample));
        }

        /// <summary>
        /// The highest supported anti aliasing level
        /// </summary>
        public byte MaxAA
        {
            get
            {
                for (byte i = 16; i >= 2; i -= 2) // From 16 down to 2 in steps of 2
                {
                    // Stop as soon as a supported level is found
                    if (CheckAA(i)) return i;
                }
                return 0;
            }
        }

        /// <summary>
        /// A comma-separated list of all supported anti aliasing levels
        /// </summary>
        public string SupportedAA
        {
            get
            {
                var result = new StringBuilder();
                for (byte i = 2; i <= 16; i += 2) // From 2 up to 16 in steps of 2
                {
                    if (CheckAA(i)) result.Append(i + ","); // Append supported levels to result string
                    else break; // Stop as soon as a non-supported level is found
                }

                // Trim away last comma
                return result.ToString().TrimEnd(',');
            }
        }
        #endregion
    }
}
