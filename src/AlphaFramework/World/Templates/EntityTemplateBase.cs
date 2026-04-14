/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using AlphaFramework.World.Components;
using AlphaFramework.World.Positionables;

namespace AlphaFramework.World.Templates;

/// <summary>
/// A common base for entity templates (collection of components used as a prototype for constructing new entities). Defines the behavior and look for a certain class of <see cref="EntityBase{TCoordinates,TTemplate}"/>.
/// </summary>
public abstract class EntityTemplateBase<TSelf> : Template<TSelf> where TSelf : EntityTemplateBase<TSelf>
{
    /// <summary>
    /// Controls how this class of entities shall be rendered.
    /// </summary>
    [Browsable(false)]
    [XmlElement(typeof(TestSphere)), XmlElement(typeof(StaticMesh)), XmlElement(typeof(AnimatedMesh)), XmlElement(typeof(CpuParticleSystem)), XmlElement(typeof(LightSource))]
    public List<Render> Render { get; private set; } = [];

    /// <summary>
    /// Controls the basic movement parameters.
    /// </summary>
    [Browsable(false)]
    public Movement? Movement { get; set; }

    //--------------------//

    #region Clone
    /// <summary>
    /// Creates a deep copy of this <see cref="EntityTemplateBase{TSelf}"/>.
    /// </summary>
    /// <returns>The cloned <see cref="EntityTemplateBase{TSelf}"/>.</returns>
    public override TSelf Clone()
    {
        var newClass = base.Clone();
        newClass.Render = Render.Select(x => x.Clone()).ToList();
        return newClass;
    }
    #endregion
}
