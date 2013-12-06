/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using World;
using World.EntityComponents;
using World.Templates;

namespace AlphaEditor.World.Dialogs
{
    /// <summary>
    /// Allows the user to add a new <see cref="RenderControl"/> to an <see cref="EntityTemplate"/>.
    /// </summary>
    /// <remarks>This is a non-modal floating toolbox window. Communication is handled via events (<see cref="NewRenderControl"/>).</remarks>
    public sealed partial class AddRenderControlTool : System.Windows.Forms.Form
    {
        #region Events
        /// <summary>
        /// Occurs when a new render control is to be added
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when a new render control is to be added")]
        public event Action<RenderControl> NewRenderControl;

        private void OnNewRenderControl(RenderControl control)
        {
            if (NewRenderControl != null) NewRenderControl(control);
        }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public AddRenderControlTool()
        {
            InitializeComponent();

            typeBox.Items.AddRange(new object[]
            {
                "TestSphere", "StaticMesh", "AnimatedMesh", "CpuParticleSystem", "GpuParticleSystem", "LightSource"
            });
        }
        #endregion

        //--------------------//

        #region Buttons
        private void buttonOK_Click(object sender, EventArgs e)
        {
            switch (typeBox.Text)
            {
                case "TestSphere":
                    OnNewRenderControl(new TestSphere());
                    break;
                case "StaticMesh":
                    OnNewRenderControl(new StaticMesh());
                    break;
                case "AnimatedMesh":
                    OnNewRenderControl(new AnimatedMesh());
                    break;
                case "CpuParticleSystem":
                    OnNewRenderControl(new CpuParticleSystem());
                    break;
                case "GpuParticleSystem":
                    OnNewRenderControl(new GpuParticleSystem());
                    break;
                case "LightSource":
                    OnNewRenderControl(new LightSource());
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
