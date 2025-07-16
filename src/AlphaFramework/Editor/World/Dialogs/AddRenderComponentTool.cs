/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using AlphaFramework.World.Components;
using AlphaFramework.World.Templates;

namespace AlphaFramework.Editor.World.Dialogs
{
    /// <summary>
    /// Allows the user to add a new <see cref="Render"/> component to an <see cref="EntityTemplateBase{TSelf}"/>.
    /// </summary>
    /// <remarks>This is a non-modal floating toolbox window. Communication is handled via events (<see cref="NewRenderComponent"/>).</remarks>
    public sealed partial class AddRenderComponentTool : System.Windows.Forms.Form
    {
        #region Events
        /// <summary>
        /// Occurs when a new render component is to be added
        /// </summary>
        [Description("Occurs when a new render component is to be added")]
        public event Action<Render> NewRenderComponent;

        private void OnNewRenderComponent(Render component)
        {
            NewRenderComponent?.Invoke(component);
        }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public AddRenderComponentTool()
        {
            InitializeComponent();

            typeBoxType.Items.AddRange([
                "TestSphere", "StaticMesh", "AnimatedMesh", "CpuParticleSystem", "GpuParticleSystem", "LightSource"
            ]);
        }
        #endregion

        //--------------------//

        #region Buttons
        private void buttonOK_Click(object sender, EventArgs e)
        {
            switch (typeBoxType.Text)
            {
                case "TestSphere":
                    OnNewRenderComponent(new TestSphere());
                    break;
                case "StaticMesh":
                    OnNewRenderComponent(new StaticMesh());
                    break;
                case "AnimatedMesh":
                    OnNewRenderComponent(new AnimatedMesh());
                    break;
                case "CpuParticleSystem":
                    OnNewRenderComponent(new CpuParticleSystem());
                    break;
                case "GpuParticleSystem":
                    OnNewRenderComponent(new GpuParticleSystem());
                    break;
                case "LightSource":
                    OnNewRenderComponent(new LightSource());
                    break;
            }

            Close();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}
