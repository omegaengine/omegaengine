using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using OmegaGUI.Model;
using World.EntityComponents;

namespace AlphaEditor.World.Dialogs
{
    /// <summary>
    /// Allows the user to add a new <see cref="Control"/> to a <see cref="Dialog"/>.
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
