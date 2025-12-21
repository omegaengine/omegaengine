/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Input;

namespace AlphaFramework.Editor.Graphics;

/// <summary>
/// Abstract base class for editing particle system presets
/// </summary>
public partial class ParticleSystemEditor : UndoCommandTab
{
    #region Variables
    /// <summary>
    /// The camera used by the presenter
    /// </summary>
    protected readonly ArcballCamera Camera = new() {MinRadius = 50, MaxRadius = 2000, Radius = 400};
    #endregion

    #region Constructor
    /// <inheritdoc/>
    protected ParticleSystemEditor()
    {
        InitializeComponent();
    }
    #endregion

    //--------------------//

    #region Render control
    private void timerRender_Tick(object sender, EventArgs e)
    {
        timerRender.Enabled = false; // Prevent multiple ticks from accumulating
        if (Visible) renderPanel.Engine?.Render();
        timerRender.Enabled = true;
    }
    #endregion

    #region View control
    protected override void OnInitialize()
    {
        renderPanel.AddInputReceiver(Camera);
        renderPanel.AddInputReceiver(new UpdateReceiver(() => renderPanel.Engine?.Render()));
    }
    #endregion
}
