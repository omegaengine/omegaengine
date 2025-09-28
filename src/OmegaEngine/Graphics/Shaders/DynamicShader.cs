/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NanoByte.Common.Streams;
using OmegaEngine.Foundation.Storage;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// Helper class for dynamically generating <see cref="Shader"/> code
/// </summary>
/// <remarks>Uses partial .fx files with XML control comments as input</remarks>
public static partial class DynamicShader
{
    /// <summary>
    /// Loads a dynamic shader file via the <see cref="ContentManager"/> and compiles it.
    /// </summary>
    /// <param name="id">The ID of the shader to be loaded</param>
    /// <param name="controllers">A set of int arrays that control the counters; <c>null</c> if there is no sync-code in the shader</param>
    /// <param name="lighting">Optimize the shader for lighting or no lighting</param>
    /// <param name="capabilities">The rendering capabilities available to the shader</param>
    /// <returns>The compiled shader</returns>
    public static DataStream FromContent(string id, Dictionary<string, IEnumerable<int>> controllers, bool lighting, EngineCapabilities capabilities)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        if (controllers == null) throw new ArgumentNullException(nameof(controllers));
        if (capabilities == null) throw new ArgumentNullException(nameof(capabilities));
        #endregion

        string[] lines = File.ReadAllLines(ContentManager.GetFilePath("Graphics/Shaders", id));

        var xmlBuffer = new StringBuilder(); // Accumulates XML data until it is complete and ready to be parsed
        var fxBuffer = new StringBuilder(); // Accumulates the actual HLSL code until it is ready to be compiled
        bool filtered = false;

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("///", StringComparison.Ordinal))
            {
                // Store XML code for later parsing
                xmlBuffer.AppendLine(trimmedLine.Substring(3, 1) == " " ?
                    trimmedLine[4..] : trimmedLine[3..]);
            }
            else if (!trimmedLine.StartsWith("//", StringComparison.Ordinal))
            { // Parse XML code once it stops coming
                string xmlData = xmlBuffer.ToString();
                if (!string.IsNullOrEmpty(xmlData))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml($"<Data>{xmlData}</Data>");

                    var counters = new LinkedList<Counter>();
                    XmlElement element = xmlDoc["Data"];
                    if (element != null)
                    {
                        foreach (XmlNode node in element.ChildNodes)
                        {
                            switch (node.Name)
                            {
                                case "Counter" when !filtered:
                                    ProcessCounterNode(node, counters);
                                    break;

                                case "Code" when !filtered:
                                    ProcessCodeNode(node, controllers, counters, fxBuffer);
                                    break;

                                case "BeginFilter":
                                    filtered = ProcessBeginFilterNode(node, lighting, capabilities);
                                    break;

                                case "EndFilter":
                                    filtered = false;
                                    break;
                            }
                        }
                    }

                    xmlBuffer = new();
                }

                // Store normal code for later compilation
                if (!filtered && !string.IsNullOrEmpty(line))
                    fxBuffer.AppendLine(line);
            }
        }

        return Compile(fxBuffer.ToString());
    }

    private static void ProcessCounterNode(XmlNode node, LinkedList<Counter> counters)
    {
        switch (node.Attributes["Type"].Value)
        {
            case "int":
                counters.AddLast(new IntCounter(node.Attributes["ID"].Value,
                    int.Parse(node.Attributes["Min"].Value, CultureInfo.InvariantCulture),
                    int.Parse(node.Attributes["Max"].Value, CultureInfo.InvariantCulture)));
                break;

            case "int-step":
                counters.AddLast(new IntCounter(node.Attributes["ID"].Value,
                    int.Parse(node.Attributes["Min"].Value, CultureInfo.InvariantCulture),
                    int.Parse(node.Attributes["Max"].Value, CultureInfo.InvariantCulture),
                    float.Parse(node.Attributes["Step"].Value, CultureInfo.InvariantCulture)));
                break;

            case "char":
                var chars = new LinkedList<char>();
                foreach (XmlNode subNode in node.ChildNodes)
                    if (subNode.Name == "Char") chars.AddLast(subNode.InnerText[0]);
                counters.AddLast(new CharCounter(node.Attributes["ID"].Value, chars));
                break;
        }
    }

    private static void ProcessCodeNode(XmlNode node, IDictionary<string, IEnumerable<int>> controllers, LinkedList<Counter> counters, StringBuilder fxBuffer)
    {
        switch (node.Attributes["Type"].Value)
        {
            case "Repeat":
                int count = int.Parse(node.Attributes["Count"].Value, CultureInfo.InvariantCulture);
                for (int i = 1; i <= count; i++)
                    fxBuffer.AppendLine(HandleCounters(node.InnerText, counters, i));
                break;

            case "Sync":
                int max = int.Parse(node.Attributes["Max"].Value, CultureInfo.InvariantCulture);
                foreach (int i in controllers[node.Attributes["Controller"].Value])
                {
                    if (i <= max)
                        fxBuffer.AppendLine(HandleCounters(node.InnerText, counters, i));
                }
                break;
        }
    }

    private static string HandleCounters(string source, IEnumerable<Counter> counters, int run)
        => counters.Aggregate(source, (current, counter) => current.Replace($"{{{counter.ID}}}", counter.GetValue(run)));

    private static bool ProcessBeginFilterNode(XmlNode node, bool lighting, EngineCapabilities capabilities)
    {
        if (node.Attributes["Target"] is {} targetValue)
        {
            if (targetValue.Value switch
                {
                    "PS14" => capabilities.MaxShaderModel != new Version(1, 4),
                    "PS20" => capabilities.MaxShaderModel != new Version(2, 0),
                    "PS2x" => capabilities.MaxShaderModel < new Version(2, 0),
                    "PS2ab" => capabilities.MaxShaderModel <= new Version(2, 0),
                    "PS2a" => capabilities.MaxShaderModel != new Version(2, 0, 1),
                    "PS2b" => capabilities.MaxShaderModel < new Version(2, 0, 2),
                    _ => false
                }) return true;
        }

        if (node.Attributes["Lighting"] is {} lightingFlag)
        {
            if (!lighting && lightingFlag.Value == "true")
                return true;
            if (lighting && lightingFlag.Value == "false")
                return true;
        }

        return false;
    }

    private static DataStream Compile(string fxCode)
    {
        try
        {
            using var compiler = EffectCompiler.FromStream(fxCode.ToStream(), ShaderFlags.None);
            return compiler.CompileEffect(ShaderFlags.EnableBackwardsCompatibility);
        }
        catch (Direct3D9Exception ex)
        {
            throw new ShaderCompileException(Resources.DynamicShaderCompileFail, ex, fxCode);
        }
    }
}
