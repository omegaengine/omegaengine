/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine
{
    /// <summary>
    /// Helper class with static functions to signal the beginnings and endings of performance profiling events.
    /// In Debug builds events are also passed to PIX.
    /// </summary>
    public static class Profiler
    {
        #region Variables
        /// <summary>
        /// The current XML node we are writing into. This controls the nesting of events.
        /// </summary>
        internal static XmlNode CurrentXmlNode;
        #endregion

        #region Properties
        /// <summary>
        /// The current D3D device query to use for controlling the command buffer.
        /// </summary>
        public static Query DeviceQuery { get; set; }

        private static XmlDocument _currentLogXml;

        /// <summary>
        /// The XML DOM to store the log data in.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", Justification = "Direct access to an XmlDocument is the easiest way to allow for flexible storage")]
        internal static XmlDocument LogXml
        {
            get { return _currentLogXml; }
            set
            {
                _currentLogXml = value;
                if (value == null) CurrentXmlNode = null;
                else
                {
                    CurrentXmlNode = value.CreateElement("frame");
                    _currentLogXml.AppendChild(CurrentXmlNode);
                }
            }
        }
        #endregion

        //--------------------//

        #region Event control
        /// <summary>
        /// Adds a non-timed profiler event.
        /// </summary>
        /// <param name="name">Te name of the event.</param>
        [Conditional("DEBUG")]
        public static void AddEvent(string name)
        {
            SetMarker(name);

            if (_currentLogXml != null)
            {
                // Create a new event entry
                XmlNode node = _currentLogXml.CreateElement("marker");

                // Add the value as an attribute
                XmlAttribute valueAttr = _currentLogXml.CreateAttribute("value");
                valueAttr.Value = name;
                node.Attributes.Append(valueAttr);

                // Add to document
                CurrentXmlNode.AppendChild(node);
            }
        }
        #endregion

        #region PIX events
        /// <summary>
        /// Notifies PIX about the start of an event.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "This method is only used for debugging and may fail during normal operation without consequences")]
        [Conditional("DEBUG")]
        internal static void BeginEvent(string name)
        {
            Performance.BeginEvent(new Color4(), name);
        }

        /// <summary>
        /// Notifies PIX about the end of an event.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "This method is only used for debugging and may fail during normal operation without consequences")]
        [Conditional("DEBUG")]
        internal static void EndEvent()
        {
            Performance.EndEvent();
        }

        /// <summary>
        /// Notifies PIX about an untimed event.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "This method is only used for debugging and may fail during normal operation without consequences")]
        [Conditional("DEBUG")]
        internal static void SetMarker(string name)
        {
            Performance.SetMarker(new Color4(), name);
        }
        #endregion
    }

    /// <summary>
    /// Struct that allows you to profile timed execution blocks.
    /// </summary>
    /// <example>
    ///   <code>using(new LogEvent("Message")) {}</code>
    /// </example>
    /// <remarks>Do not use these over cross!</remarks>
    public struct ProfilerEvent : IDisposable
    {
        #region Variables
        private readonly Stopwatch _timer;
        private readonly XmlNode _node;
        #endregion

        #region Helpers
        /// <summary>
        /// Forces a render pipeline stall to finish all pending jobs.
        /// </summary>
        private static void PipelineStall()
        {
            Profiler.DeviceQuery.Issue(Issue.End);
            while (!Profiler.DeviceQuery.CheckStatus(true))
            {}
        }
        #endregion

        #region Event control
        /// <summary>
        /// Starts a new profiler event with a certain name.
        /// </summary>
        /// <param name="getName">A delegate for getting the name of the event.</param>
        public ProfilerEvent(Func<string> getName)
        {
            #region Sanity checks
            if (getName == null) throw new ArgumentNullException("getName");
            #endregion

            if (Profiler.DeviceQuery != null) PipelineStall();

            Profiler.BeginEvent(getName());

            if (Profiler.LogXml != null)
            {
                // Create a new event entry
                _node = Profiler.LogXml.CreateElement("event");

                // Add the value as an attribute
                XmlAttribute nameAttr = Profiler.LogXml.CreateAttribute("name");
                nameAttr.Value = getName();
                _node.Attributes.Append(nameAttr);

                // Add to document
                Profiler.CurrentXmlNode.AppendChild(_node);
                Profiler.CurrentXmlNode = _node;

                // Start the timer
                _timer = Stopwatch.StartNew();
            }
            else
            {
                _node = null;
                _timer = null;
            }
        }

        /// <summary>
        /// Starts a new profiler event with a certain name.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        public ProfilerEvent(string name) : this(() => name)
        {}

        /// <summary>
        /// Ends the event.
        /// </summary>
        public void Dispose()
        {
            if (Profiler.DeviceQuery != null) PipelineStall();

            if (Profiler.LogXml != null)
            {
                // Add the time as an attribute
                var time = (float)_timer.Elapsed.TotalMilliseconds;
                XmlAttribute timeAttr = Profiler.LogXml.CreateAttribute("time");
                timeAttr.Value = string.Format(CultureInfo.InvariantCulture, "{0}", time);
                _node.Attributes.Append(timeAttr);

                // Restore the last node
                Profiler.CurrentXmlNode = Profiler.CurrentXmlNode.ParentNode;
            }

            Profiler.EndEvent();
        }
        #endregion
    }
}
