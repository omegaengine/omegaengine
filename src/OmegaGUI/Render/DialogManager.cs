/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2014 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using NanoByte.Common;
using NanoByte.Common.Storage.SlimDX;
using OmegaEngine;
using SlimDX.Direct3D9;
using Font = SlimDX.Direct3D9.Font;

namespace OmegaGUI.Render
{
    /// <summary>
    /// Manages shared resources of DirectX-based dialogs
    /// </summary>
    [CLSCompliant(false)]
    public sealed class DialogManager : IDisposable
    {
        #region Variables
        private readonly Engine _engine;

        // Lists of textures/fonts
        private readonly List<TextureNode> _textureCache = new List<TextureNode>();
        private readonly List<FontNode> _fontCache = new List<FontNode>();
        #endregion

        #region Properties
        public StateBlock StateBlock { get; private set; }
        public Sprite Sprite { get; private set; }

        /// <summary>
        /// Gets a font node from the cache
        /// </summary>
        public FontNode GetFontNode(int index)
        {
            return _fontCache[index];
        }

        /// <summary>
        /// Gets a texture node from the cache
        /// </summary>
        public TextureNode GetTextureNode(int index)
        {
            return _textureCache[index];
        }

        /// <summary>
        /// Gets the render engine
        /// </summary>
        public Engine Engine { get { return _engine; } }

        /// <summary>
        /// Gets the DirectX device
        /// </summary>
        public Device Device { get { return _engine.Device; } }

        /// <summary>
        /// Gets the render target control
        /// </summary>
        public System.Windows.Forms.Control Target { get { return _engine.Target; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Sets up the GUI system for usage with a rendering engine
        /// </summary>
        /// <param name="engine">The rendering engine conaining the Direct3D device</param>
        public DialogManager(Engine engine)
        {
            if (engine == null) throw new ArgumentNullException("engine");

            _engine = engine;
            OnCreateDevice();

            // Handle lost devices properly
            engine.DeviceLost += OnLostDevice;
            engine.DeviceReset += OnResetDevice;

            // Create the default MessageBox dialog
            MessageBox = new MessageBox(this);
            MessageBox.SetSize(400, 150);

            OnResetDevice();
        }
        #endregion

        //--------------------//

        #region Specialized dialogs
        /// <summary>
        /// Gets a Specialized MessageBox dialog
        /// </summary>
        public MessageBox MessageBox { get; private set; }
        #endregion

        #region Fonts
        /// <summary>
        /// Adds a font to the resource manager
        /// </summary>
        public int AddFont(string faceName, uint height, FontWeight weight)
        {
            // See if this font exists
            for (int i = 0; i < _fontCache.Count; i++)
            {
                FontNode fn = _fontCache[i];
                if ((string.Compare(fn.FaceName, faceName, StringComparison.OrdinalIgnoreCase) == 0) &&
                    fn.Height == height &&
                    fn.Weight == weight)
                {
                    // Found it
                    return i;
                }
            }

            // Doesn't exist, add a new one and try to create it
            var newNode = new FontNode {FaceName = faceName, Height = height, Weight = weight};
            _fontCache.Add(newNode);

            int fontIndex = _fontCache.Count - 1;
            // If a device is available, try to create immediately
            if (_engine != null)
                CreateFont(fontIndex);

            return fontIndex;
        }

        /// <summary>
        /// Creates a font
        /// </summary>
        public void CreateFont(int font)
        {
            // Get the font node here
            FontNode fn = GetFontNode(font);
            if (fn.Font != null)
                fn.Font.Dispose(); // Get rid of this

            // Create the new font
            fn.Font = new Font(_engine.Device, (int)fn.Height, 0, fn.Weight, 1, false, CharacterSet.Default,
                // ReSharper disable BitwiseOperatorOnEnumWihtoutFlags
                Precision.Default, FontQuality.Default, PitchAndFamily.Default | PitchAndFamily.DontCare, fn.FaceName);
            // ReSharper restore BitwiseOperatorOnEnumWihtoutFlags
        }
        #endregion

        #region Textures
        /// <summary>
        /// Adds a texture to the resource manager
        /// </summary>
        public int AddTexture(string filename)
        {
            // See if this texture exists
            for (int i = 0; i < _textureCache.Count; i++)
            {
                TextureNode tn = _textureCache[i];
                if (string.Compare(tn.Filename, filename, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // Found it
                    return i;
                }
            }

            // Doesn't exist, add a new one and try to create it
            var newNode = new TextureNode {Filename = filename};
            _textureCache.Add(newNode);

            int texIndex = _textureCache.Count - 1;

            // If a device is available, try to create immediately
            if (_engine != null)
                CreateTexture(texIndex);

            return texIndex;
        }

        /// <summary>
        /// Creates a texture
        /// </summary>
        public void CreateTexture(int tex)
        {
            // Get the texture node here
            TextureNode tn = GetTextureNode(tex);

            // Make sure there's a texture to create
            if (string.IsNullOrEmpty(tn.Filename)) return;

            // Create the new texture
            var info = new ImageInformation();
            using (var stream = ContentManager.GetFileStream("GUI/Textures", tn.Filename))
            {
                tn.Texture = Texture.FromStream(_engine.Device, stream, D3DX.Default, D3DX.Default, D3DX.Default, Usage.None,
                    Format.Unknown, Pool.Managed, Filter.Default, Filter.Default, 0);
            }

            // Store dimensions
            tn.Width = (uint)info.Width;
            tn.Height = (uint)info.Height;
        }
        #endregion

        #region Device event callbacks
        /// <summary>
        /// Called when the device is created
        /// </summary>
        public void OnCreateDevice()
        {
            // create fonts and textures
            for (int i = 0; i < _fontCache.Count; i++)
                CreateFont(i);

            for (int i = 0; i < _textureCache.Count; i++)
                CreateTexture(i);

            Sprite = new Sprite(_engine.Device); // Create the sprite
        }

        /// <summary>
        /// Called when the device has been lost.
        /// </summary>
        public void OnLostDevice()
        {
            if (!_engine.IsDisposed)
            {
                foreach (FontNode fn in _fontCache)
                {
                    if ((fn.Font != null) && (!fn.Font.Disposed))
                        fn.Font.OnLostDevice();
                }

                if (Sprite != null)
                    Sprite.OnLostDevice();

                if (StateBlock != null)
                {
                    StateBlock.Dispose();
                    StateBlock = null;
                }
            }
        }

        /// <summary>
        /// Called when the device has been reset.
        /// </summary>
        public void OnResetDevice()
        {
            _fontCache.ForEach(node => node.Font.OnResetDevice());

            if (Sprite != null)
                Sprite.OnResetDevice();

            // Create new state block
            StateBlock = new StateBlock(_engine.Device, StateBlockType.All);

            // Reposition the message box according to the new Viewport
            MessageBox.Location = new Point(
                (_engine.RenderSize.Width - MessageBox.Width) / 2,
                (_engine.RenderSize.Height - MessageBox.Height) / 2);
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <summary>
        /// Has this manager been disposed?
        /// </summary>
        [Browsable(false)]
        public bool Disposed { get; private set; }

        /// <summary>
        /// Unhooks the <see cref="OmegaGUI"/> system from the <see cref="Engine"/> and disposes its internal DirectX resources
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;

            // Unhook device events
            _engine.DeviceLost -= OnLostDevice;
            _engine.DeviceReset -= OnResetDevice;

            // Dispose DirectX objects
            Sprite.Dispose();
            StateBlock.Dispose();

            // Clear caches
            _textureCache.ForEach(node => node.Texture.Dispose());
            _fontCache.ForEach(node => node.Font.Dispose());

            GC.SuppressFinalize(this);
            Disposed = true;
        }

        /// <inheritdoc/>
        ~DialogManager()
        {
            // This block will only be executed on Garbage Collection, not by manual disposal
            Log.Error("Forgot to call Dispose on " + this);
#if DEBUG
            throw new InvalidOperationException("Forgot to call Dispose on " + this);
#endif
        }
        #endregion
    }

    /// <summary>
    /// Structure for shared textures
    /// </summary>
    [CLSCompliant(false)]
    public class TextureNode
    {
        public string Filename;
        public Texture Texture;
        public uint Width;
        public uint Height;
    }

    /// <summary>
    /// Structure for shared fonts
    /// </summary>
    [CLSCompliant(false)]
    public class FontNode
    {
        public string FaceName;
        public Font Font;
        public uint Height;
        public FontWeight Weight;
    }
}
