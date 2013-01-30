/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
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
    // This file contains methods for determining the rendering capabilities of the graphics hardware
    partial class Engine
    {
        #region Properties
        private Hardware _hardware;

        /// <summary>
        /// Information about the hardware of this computer.
        /// </summary>
        public Hardware Hardware { get { return _hardware; } }

        /// <summary>
        /// The capabilities of the <see cref="Direct3D"/> device.
        /// </summary>
        internal Capabilities Capabilities { get; private set; }
        #endregion

        //--------------------//

        #region Hardware information
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
                foreach (ManagementObject mob in mos.Get())
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
                foreach (ManagementObject mob in mos.Get())
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
            DisplayModes = Manager.Adapters[EngineConfig.Adapter].GetDisplayModes(Format.X8R8G8B8);
            var adapter = Manager.Adapters[EngineConfig.Adapter].Details;
            _hardware.Gpu.Manufacturer = adapter.VendorId;
            _hardware.Gpu.Name = adapter.Description;
            _hardware.Gpu.MaxAA = MaxAA;

            try
            {
                var mos = new ManagementObjectSearcher("SELECT AdapterRAM FROM Win32_VideoController");
                foreach (ManagementObject mob in mos.Get())
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
        #endregion

        #region Device capabilities
        /// <summary>
        /// Helper method for the constructor that fills <see cref="Capabilities"/> and <see cref="MaxShaderModel"/> with information and checks certain conditions are met.
        /// </summary>
        /// <param name="engineConfig"></param>
        private void DetermineDeviceCapabilities(EngineConfig engineConfig)
        {
            Capabilities = Manager.GetDeviceCaps(0, DeviceType.Hardware);

            #region Pixel shader
            if (engineConfig.ForceShaderModel == null)
            {
                // Detect pixel shader version (and find subversions of Pixel Shader 2.0)
                MaxShaderModel = Capabilities.PixelShaderVersion;
                if (MaxShaderModel == new Version(2, 0) && Capabilities.MaxPixelShader30InstructionSlots >= 96)
                {
                    if (Capabilities.PS20Caps.TempCount >= 22 &&
                        MathUtils.CheckFlag((int)Capabilities.PS20Caps.Caps, (int)(PixelShaderCaps.ArbitrarySwizzle | PixelShaderCaps.GradientInstructions | PixelShaderCaps.Predication | PixelShaderCaps.NoDependentReadLimit | PixelShaderCaps.NoTextureInstructionLimit)))
                    { // Pixel shader 2.0a
                        MaxShaderModel = new Version(2, 0, 1);
                    }
                    else if (Capabilities.PS20Caps.TempCount >= 32 &&
                        MathUtils.CheckFlag((int)Capabilities.PS20Caps.Caps, (int)PixelShaderCaps.NoTextureInstructionLimit))
                    { // Pixel shader 2.0b
                        MaxShaderModel = new Version(2, 0, 2);
                    }
                }
            }
            else MaxShaderModel = engineConfig.ForceShaderModel;
            #endregion

            // Log GPU capabilities
            Log.Info("GPU capabilities:\n" +
                "HWTransformAndLight: " + MathUtils.CheckFlag((int)Capabilities.DeviceCaps, (int)DeviceCaps.HWTransformAndLight) + "\n" +
                "PureDevice: " + MathUtils.CheckFlag((int)Capabilities.DeviceCaps, (int)DeviceCaps.PureDevice) + "\n" +
                "Anisotropic: " + SupportsAnisotropic + "\n" +
                "VertexShaderVersion: " + Capabilities.VertexShaderVersion + "\n" +
                "PixelShaderVersion: " + MaxShaderModel + "\n" +
                "SupportedAA: " + SupportedAA + "\n");

            // Ensure support for linear texture filtering
            if (!MathUtils.CheckFlag((int)Capabilities.TextureFilterCaps, (int)(FilterCaps.MinLinear | FilterCaps.MagLinear | FilterCaps.MipLinear)))
            {
                //throw new NotAvailableException(Properties.Resources.NoLinearTextureFiltering);
                Log.Warn("Missing support for linear texture filtering");
            }
        }
        #endregion

        //--------------------//

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
            {
                return manager.Adapters[adapter].GetDisplayModes(Format.X8R8G8B8).Any(mode => mode.Width == width && mode.Height == height);
            }
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
            return (Manager.CheckDeviceMultisampleType(EngineConfig.Adapter, DeviceType.Hardware,
                Manager.Adapters[EngineConfig.Adapter].CurrentDisplayMode.Format, true, (MultisampleType)sample));
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
