/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World.Paths;
using AlphaFramework.World.Templates;

namespace AlphaFramework.World.Positionables;

/// <summary>
/// A common base class for <see cref="Positionable{TCoordinates}"/> whose behaviour and graphical representation is controlled by components.
/// </summary>
/// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
/// <typeparam name="TTemplate">The specific type of <see cref="EntityTemplateBase{TSelf}"/> to use as a component container.</typeparam>
public abstract class EntityBase<TCoordinates, TTemplate> : Positionable<TCoordinates>, IUpdateable, ITemplated
    where TCoordinates : struct
    where TTemplate : EntityTemplateBase<TTemplate>
{
    #region Template
    private string _templateName;

    /// <summary>
    /// The name of the <typeparamref name="TTemplate"/>.
    /// </summary>
    /// <remarks>
    /// Setting this will overwrite <see cref="TemplateData"/> with a new clone of the appropriate <typeparamref name="TTemplate"/>.
    /// This is serialized/stored in map files. It is also serialized/stored in savegames, but the value is ignored there (due to the attribute order)!
    /// </remarks>
    [XmlAttribute("Template"), Description("The name of the entity template")]
    public string TemplateName
    {
        get { return _templateName; }
        set
        {
            #region Sanity check
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            #endregion

            // Create copy of the class so run-time modifications for individual entities are possible
            try
            {
                TemplateData = Template<TTemplate>.All[value].Clone();
            }
            #region Error handling
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"Entity '{Name}' references the non-existent '{value}' template.");
            }
            #endregion

            // Only set the new name once the according class was successfully located
            _templateName = value;
        }
    }

    private TTemplate _template;

    /// <summary>
    /// The <typeparamref name="TTemplate"/> controlling the behavior and look for this <see cref="EntityBase{TCoordinates,TTemplate}"/>.
    /// </summary>
    /// <remarks>
    /// This is always a clone of the original <typeparamref name="TTemplate"/>.
    /// This is serialized/stored in savegames but not in map files!
    /// </remarks>
    [Browsable(false)]
    public TTemplate TemplateData
    {
        get { return _templateDataMasked ? null : _template; }
        set
        {
            // Backup the original value (might be needed for restore on exception) and then set the new value
            var oldValue = _template;
            _template = value;

            try
            {
                OnChangedRebuild();
            }
            #region Error handling
            catch (Exception)
            {
                // Restore the original value, trigger a render update for that and then pass on the exception for handling
                _template = oldValue;
                OnChangedRebuild();
                throw;
            }
            #endregion
        }
    }

    // ReSharper disable once StaticFieldInGenericType
    private static bool _templateDataMasked;

    private struct TemplateDataMasking : IDisposable
    {
        public void Dispose()
        {
            _templateDataMasked = false;
        }
    }

    /// <summary>
    /// Makes all <see cref="TemplateData"/> values return <c>null</c> until <see cref="IDisposable.Dispose"/> is called on the returned object. This is not thread-safe!
    /// </summary>
    public static IDisposable MaskTemplateData()
    {
        _templateDataMasked = true;
        return new TemplateDataMasking();
    }
    #endregion

    /// <summary>
    /// Updates the position and other attributes of this entity.
    /// </summary>
    /// <param name="elapsedTime">How much game time in seconds has elapsed since this method was last called.</param>
    public abstract void Update(double elapsedTime);

    /// <summary>
    /// The path this entity is currently walking along.
    /// </summary>
    [XmlIgnore, Browsable(false)]
    public StoredPath<TCoordinates> CurrentPath { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string value = base.ToString();
        if (!string.IsNullOrEmpty(_templateName))
            value += " (" + _templateName + ")";
        return value;
    }

    #region Clone
    /// <summary>
    /// Creates a deep copy of this <see cref="EntityBase{TCoordinates,TTemplate}"/>.
    /// </summary>
    /// <returns>The cloned <see cref="EntityBase{TCoordinates,TTemplate}"/>.</returns>
    public override Positionable<TCoordinates> Clone()
    {
        var clonedEntity = (EntityBase<TCoordinates, TTemplate>)base.Clone();
        if (CurrentPath != null) clonedEntity.CurrentPath = CurrentPath.Clone();
        if (_template != null) clonedEntity._template = _template.Clone();
        return clonedEntity;
    }
    #endregion
}
