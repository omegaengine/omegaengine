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
using NanoByte.Common;
using NanoByte.Common.Storage.SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// Helper class for dynamically generating <see cref="Shader"/> code
    /// </summary>
    /// <remarks>Uses partial .fx files with XML control comments as input</remarks>
    public static class DynamicShader
    {
        #region Counters
        private abstract class Counter
        {
            public string ID { get; }

            protected Counter(string id)
            {
                ID = id;
            }

            public abstract string GetValue(int run);
        }

        private sealed class IntCounter : Counter
        {
            private readonly int _min, _max;
            private readonly float _step = 1;

            public IntCounter(string id, int min, int max) : base(id)
            {
                _min = min;
                _max = max;
            }

            public IntCounter(string id, int min, int max, float step) : base(id)
            {
                _min = min;
                _max = max;
                _step = step;
            }

            public override string GetValue(int run)
            {
                var num = (int)Math.Ceiling(run * _step);
                return num.Clamp(_min, _max).ToString(CultureInfo.InvariantCulture);
            }
        }

        private sealed class CharCounter : Counter
        {
            private readonly char[] _chars;

            public CharCounter(string id, ICollection<char> chars) : base(id)
            {
                _chars = new char[chars.Count];
                chars.CopyTo(_chars, 0);
            }

            public override string GetValue(int run)
            {
                run--;
                while (run >= _chars.Length)
                    run -= _chars.Length;
                return _chars[run].ToString(CultureInfo.InvariantCulture);
            }
        }
        #endregion

        //--------------------//

        #region Code helpers
        private static string HandleCounters(string source, IEnumerable<Counter> counters, int run)
        {
            return counters.Aggregate(source, (current, counter) => current.Replace("{" + counter.ID + "}", counter.GetValue(run)));
        }
        #endregion

        #region Parse
        /// <summary>
        /// Parses and compiles a dynamic shader string
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to compile the effect in</param>
        /// <param name="source">The source code to be parsed and compiled</param>
        /// <param name="lighting">Optimize the shader for lighting or no lighting</param>
        /// <param name="controllers">A set of int arrays that control the counters; <c>null</c> if there is no sync-code in the shader</param>
        /// <returns>The compiled effect</returns>
        public static Effect Parse(Engine engine, string source, bool lighting, IDictionary<string, IEnumerable<int>> controllers)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));
            if (controllers == null) throw new ArgumentNullException(nameof(controllers));
            #endregion

            source += "\n";
            string[] lines = source.SplitMultilineText();
            var xmlBuffer = new StringBuilder(); // Accumulates XML data until it is complete and ready to be parsed
            var fxBuffer = new StringBuilder(); // Accumulates the actual HLSL code until it is ready to be compiled
            bool include = true;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("///", StringComparison.Ordinal))
                {
                    // Store XML code for later parsing
                    xmlBuffer.AppendLine((trimmedLine.Substring(3, 1) == " ") ?
                        trimmedLine.Substring(4) : trimmedLine.Substring(3));
                }
                else if (!trimmedLine.StartsWith("//", StringComparison.Ordinal))
                { // Parse XML code once it stops coming
                    string xmlData = xmlBuffer.ToString();
                    if (!string.IsNullOrEmpty(xmlData))
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml("<Data>" + xmlData + "</Data>");

                        var counters = new LinkedList<Counter>();
                        XmlElement element = xmlDoc["Data"];
                        if (element != null)
                        {
                            foreach (XmlNode node in element.ChildNodes)
                            {
                                switch (node.Name)
                                {
                                        #region Counter
                                    case "Counter":
                                        if (!include) break;
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
                                        break;
                                        #endregion

                                        #region Code
                                    case "Code":
                                        if (!include) break;
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
                                        break;
                                        #endregion

                                        #region Filter
                                    case "BeginnFilter":
                                        XmlAttribute targetValue = node.Attributes["Target"];
                                        if (targetValue != null)
                                        {
                                            switch (targetValue.Value)
                                            {
                                                case "PS14":
                                                    include = (engine.Capabilities.MaxShaderModel == new Version(1, 4));
                                                    break;
                                                case "PS20":
                                                    include = (engine.Capabilities.MaxShaderModel == new Version(2, 0));
                                                    break;
                                                case "PS2x":
                                                    include = (engine.Capabilities.MaxShaderModel >= new Version(2, 0));
                                                    break;
                                                case "PS2ab":
                                                    include = (engine.Capabilities.MaxShaderModel > new Version(2, 0));
                                                    break;
                                                case "PS2a":
                                                    include = (engine.Capabilities.MaxShaderModel == new Version(2, 0, 1));
                                                    break;
                                                case "PS2b":
                                                    include = (engine.Capabilities.MaxShaderModel >= new Version(2, 0, 2));
                                                    break;
                                            }
                                        }

                                        XmlAttribute lightingFlag = node.Attributes["Lighting"];
                                        if (include && lightingFlag != null)
                                        {
                                            if (!lighting && lightingFlag.Value == "true")
                                                include = false;
                                            if (lighting && lightingFlag.Value == "false")
                                                include = false;
                                        }
                                        break;

                                    case "EndFilter":
                                        include = true;
                                        break;
                                        #endregion
                                }
                            }
                        }

                        xmlBuffer = new StringBuilder();
                    }

                    // Store normal code for later compilation
                    if (include && !string.IsNullOrEmpty(line))
                        fxBuffer.AppendLine(line);
                }
            }

            // Compile code
            string fxCode = fxBuffer.ToString();
            try
            {
                return Effect.FromString(engine.Device, fxCode, null, null, null, ShaderFlags.None);
            }
            catch (Direct3D9Exception ex)
            {
                throw new ShaderCompileException(Resources.DynamicShaderCompileFail, ex, fxCode);
            }
        }
        #endregion

        #region From Content
        /// <summary>
        /// Loads a dynamic shader file from a game asset source via the <see cref="ContentManager"/>. 
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to compile the effect in</param>
        /// <param name="id">The ID of the shader to be loaded</param>
        /// <param name="lighting">Optimize the shader for lighting or no lighting</param>
        /// <param name="controllers">A set of int arrays that control the counters; <c>null</c> if there is no sync-code in the shader</param>
        /// <returns>The compiled effect</returns>
        public static Effect FromContent(Engine engine, string id, bool lighting, IDictionary<string, IEnumerable<int>> controllers)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            using (var stream = ContentManager.GetFileStream("Graphics/Shaders", id))
            {
                var reader = new StreamReader(stream);
                string source = reader.ReadToEnd();
                return Parse(engine, source, lighting, controllers);
            }
        }
        #endregion
    }
}
