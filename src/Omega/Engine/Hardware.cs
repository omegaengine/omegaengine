/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Globalization;
using System.Xml.Serialization;

namespace OmegaEngine
{
    /// <summary>
    /// Information about the hardware the <see cref="Engine"/> is running on
    /// </summary>
    /// <seealso cref="Engine.Hardware"/>
    [XmlRoot("hardware")]
    public struct Hardware
    {
        /// <summary>
        /// The current operating system.
        /// </summary>
        [XmlElement("os")]
        public HardwareOS OS;

        /// <summary>
        /// Details about the CPU.
        /// </summary>
        [XmlElement("cpu")]
        public HardwareCpu Cpu;

        /// <summary>
        /// Details about the RAM.
        /// </summary>
        [XmlElement("ram")]
        public HardwareRam Ram;

        /// <summary>
        /// Details about the graphics card.
        /// </summary>
        [XmlElement("gpu")]
        public HardwareGpu Gpu;

        public override string ToString()
        {
            return "Hardware:\n" +
                "OS: " + OS + "\n" +
                    "CPU: " + Cpu + "\n" +
                        "RAM: " + string.Format(CultureInfo.InvariantCulture, "{0} MB", Ram) + "\n" +
                            "GPU: " + Gpu;
        }
    }

    /// <seealso cref="Hardware.OS"/>
    public struct HardwareOS
    {
        /// <summary>
        /// The operating system platform (e.g. Windows NT).
        /// </summary>
        [XmlAttribute("platform")]
        public PlatformID Platform;

        /// <summary>
        /// True if the operating system is a 64-bit version of Windows.
        /// </summary>
        [XmlAttribute("is64bit")]
        public bool Is64Bit;

        /// <summary>
        /// The version of the operating system (e.g. 6.0 for Vista).
        /// </summary>
        [XmlAttribute("version")]
        public string Version;

        /// <summary>
        /// The service pack level (e.g. "Service Pack 1").
        /// </summary>
        [XmlAttribute("service-pack")]
        public string ServicePack;

        public override string ToString()
        {
            return Platform + " " + (Is64Bit ? "64-bit " : "") + Version + " " + ServicePack;
        }
    }

    /// <seealso cref="Hardware.Cpu"/>
    public struct HardwareCpu
    {
        /// <summary>
        /// The manufacturer of the CPU
        /// </summary>
        [XmlAttribute("manufacturer")]
        public string Manufacturer;

        /// <summary>
        /// The name of the CPU (including the manufacturer)
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The speed of the CPU in MHz
        /// </summary>
        [XmlAttribute("speed")]
        public int Speed;

        /// <summary>
        /// The number of physical cores in the CPU
        /// </summary>
        [XmlAttribute("cores")]
        public int Cores;

        /// <summary>
        /// The number of logical processors in the CPU
        /// </summary>
        [XmlAttribute("logical")]
        public int Logical;

        public override string ToString()
        {
            return Name + " (" + Cores + "x" + Speed + "MHz, " + Logical + " logical threads)";
        }
    }

    /// <seealso cref="Hardware.Ram"/>
    public struct HardwareRam
    {
        /// <summary>
        /// The size of the system RAM in MB.
        /// </summary>
        [XmlAttribute("size")]
        public int Size;

        public override string ToString()
        {
            return Size.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <seealso cref="Hardware.Gpu"/>
    public struct HardwareGpu
    {
        /// <summary>
        /// The ID number of the manufacturer of the graphics card.
        /// </summary>
        [XmlAttribute("manufacturer")]
        public int Manufacturer;

        /// <summary>
        /// The name of the graphics card (including the manufacturer).
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The size of the graphics RAM in MB.
        /// </summary>
        [XmlAttribute("ram")]
        public int Ram;

        /// <summary>
        /// The maximum level of anti-aliasing support by the graphics card.
        /// </summary>
        [XmlAttribute("max-aa")]
        public byte MaxAA;

        public override string ToString()
        {
            return Name + " (" + Ram + "MB RAM, " + MaxAA + "x AA)";
        }
    }
}
