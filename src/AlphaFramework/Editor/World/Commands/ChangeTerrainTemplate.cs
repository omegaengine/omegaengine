/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using AlphaFramework.World.Templates;
using AlphaFramework.World.Terrains;
using NanoByte.Common.Undo;

namespace AlphaFramework.Editor.World.Commands;

/// <summary>
/// Changes a terrain <see cref="Template{TSelf}"/> on a <see cref="Terrain{TTemplate}"/>s.
/// </summary>
/// <param name="terrain">The <see cref="Terrain{TTemplate}"/> to modify.</param>
/// <param name="templateIndex">The index in <see cref="Terrain{TTemplate}.Templates"/> to set.</param>
/// <param name="templateName">The name of the new <typeparamref name="TTemplate"/> to set.</param>
/// <param name="refreshHandler">Called when the <see cref="Terrain{TTemplate}"/> needs to be reset.</param>
public class ChangeTerrainTemplate<TTemplate>(Terrain<TTemplate> terrain, int templateIndex, string templateName, Action refreshHandler) : SimpleCommand
    where TTemplate : Template<TTemplate>
{
    private string _oldTemplateName;

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        _oldTemplateName = (terrain.Templates[templateIndex] == null) ? null : terrain.Templates[templateIndex].Name;
        terrain.Templates[templateIndex] = string.IsNullOrEmpty(templateName) ? null : Template<TTemplate>.All[templateName];
        refreshHandler();
    }

    /// <inheritdoc/>
    protected override void OnUndo()
    {
        terrain.Templates[templateIndex] = string.IsNullOrEmpty(_oldTemplateName) ? null : Template<TTemplate>.All[_oldTemplateName];
        refreshHandler();
    }
}
