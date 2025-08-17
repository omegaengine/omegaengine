/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Globalization;
using System.Xml.Serialization;
using NanoByte.Common.Info;

namespace OmegaEngine;

/// <summary>
/// Information about the hardware the <see cref="Engine"/> is running on.
/// </summary>
/// <seealso cref="EngineCapabilities.Hardware"/>
[XmlRoot("hardware")]
public struct Hardware
{
    /// <summary>
    /// The current operating system.
    /// </summary>
    [XmlElement("os")]
    public OSInfo OS;

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

    public override string ToString() => $"Hardware:\nOS: {OS}\nCPU: {Cpu}\nRAM: {string.Format(CultureInfo.InvariantCulture, "{0} MB", Ram)}\nGPU: {Gpu}";
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

    public override string ToString() => $"{Name} ({Cores}x{Speed}MHz, {Logical} logical threads)";
}

/// <seealso cref="Hardware.Ram"/>
public struct HardwareRam
{
    /// <summary>
    /// The size of the system RAM in MB.
    /// </summary>
    [XmlAttribute("size")]
    public int Size;

    public override string ToString() => Size.ToString(CultureInfo.InvariantCulture);
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

    public override string ToString() => $"{Name} ({Ram}MB RAM, {MaxAA}x AA)";
}
