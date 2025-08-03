/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Xml.Serialization;
using AlphaFramework.World;
using AlphaFramework.World.Positionables;
using NanoByte.Common;
using OmegaEngine.Values.Design;
using SlimDX;

namespace FrameOfReference.World.Positionables;

/// <summary>
/// Executes Lua scripts on proximity or timers.
/// </summary>
public sealed class Trigger : Positionable<Vector2>
{
    private float _range = 200f;

    /// <summary>
    /// The maximum distance at which an <see cref="Entity"/> activates this trigger.
    /// </summary>
    [XmlAttribute, DefaultValue(200f), Description("The maximum distance at which an Entity activates this trigger.")]
    public float Range { get => _range; set => value.To(ref _range, OnChangedRebuild); }

    /// <summary>
    /// The <see cref="UniverseBase{T}.GameTime"/> by which this trigger should have been activated.
    /// </summary>
    [XmlAttribute, DefaultValue(0.0), Description("The GameTime by which this trigger should have been activated.")]
    public double DueTime { get; set; }

    /// <summary>
    /// The name of the <see cref="Entity"/> whose proximity causes this trigger to activate.
    /// </summary>
    [XmlAttribute, Description("The name of the Entity whose proximity causes this trigger to activate.")]
    public string TargetEntity { get; set; }

    /// <summary>
    /// A Lua script to execute when <see cref="TargetEntity"/> gets within <see cref="Range"/>.
    /// </summary>
    [DefaultValue(""), Description("A Lua script to execute when TargetEntity gets within range."), FileType("Lua")]
    [Editor(typeof(CodeEditor), typeof(UITypeEditor))]
    public string OnActivation { get; set; }

    /// <summary>
    /// A Lua script to execute if the trigger was not activated by <see cref="DueTime"/>.
    /// </summary>
    [DefaultValue(""), Description("A Lua script to execute if the trigger was not activated by DueTime."), FileType("Lua")]
    [Editor(typeof(CodeEditor), typeof(UITypeEditor))]
    public string OnTimeout { get; set; }

    /// <summary>
    /// Indicates whether this trigger has already been triggered by <see cref="TargetEntity"/> or has timed out.
    /// </summary>
    [XmlAttribute, DefaultValue(false), Description("Indicates whether this trigger has already been triggered by TargetEntity or has timed out.")]
    public bool WasTriggered { get; set; }

    /// <summary>
    /// The name of another <see cref="Trigger"/> that has to be triggered before this one can be.
    /// </summary>
    [Description("The name of another Trigger that has to be triggered before this one can be.")]
    public string DependsOn { get; set; }

    /// <summary>
    /// Determines whether an entity is within range of this trigger.
    /// </summary>
    public bool IsInRange(Positionable<Vector2> entity)
    {
        #region Sanity checks
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        #endregion

        return (entity.Position - Position).Length() <= Range;
    }
}
