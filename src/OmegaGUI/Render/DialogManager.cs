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
using System.IO;
using FontStashSharp;
using NanoByte.Common.Native;
using OmegaEngine;
using OmegaEngine.Foundation.Storage;
using OmegaEngine.Graphics;
using Vortice.Direct3D9;
using SharpGen.Runtime;

namespace OmegaGUI.Render;

/// <summary>
/// Manages shared resources of DirectX-based dialogs
/// </summary>
public sealed class DialogManager : IDisposable
{
    #region Variables

    // Lists of textures/fonts
    private readonly List<TextureNode> _textureCache = [];
    private readonly List<FontNode> _fontCache = [];

    // Font faces shared by all sizes and the textures they rasterize glyphs into
    private readonly Dictionary<(string FaceName, FontWeight Weight), FontSystem?> _fontSystems = [];
    private GlyphTextureManager? _glyphTextures;
    #endregion

    #region Properties
    public IDirect3DStateBlock9? StateBlock { get; private set; }
    public SpriteBatch? Sprite { get; private set; }

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
    public Engine? Engine { get; }

    /// <summary>
    /// Gets the DirectX device
    /// </summary>
    public IDirect3DDevice9 Device => Engine.Device;

    /// <summary>
    /// Gets the render target control
    /// </summary>
    public System.Windows.Forms.Control Target => Engine.Target;

    /// <summary>
    /// Dialogs last refreshed before this <see cref="WindowsUtils.AbsoluteTime"/> are refreshed on their next render
    /// </summary>
    public double RefreshTime { get; set; }
    #endregion

    #region Focus
    /// <summary>
    /// The control which currently has keyboard focus (across all dialogs using this manager)
    /// </summary>
    public Control? FocusedControl { get; internal set; }

    /// <summary>
    /// The control the mouse is currently hovering over (across all dialogs using this manager)
    /// </summary>
    internal Control? MouseOverControl { get; set; }

    /// <summary>
    /// <see cref="WindowsUtils.AbsoluteTime"/> when <see cref="MouseOverControl"/> last changed
    /// </summary>
    internal double TooltipHoverStart { get; set; }

    /// <summary>
    /// Request that a control has focus
    /// </summary>
    public void RequestFocus(Control control)
    {
        if (control == null) throw new ArgumentNullException(nameof(control));

        if (FocusedControl == control)
            return; // Already does

        if (!control.CanHaveFocus)
            return; // Can't have focus

        if (FocusedControl != null)
            FocusedControl.OnFocusOut();

        // Set the control focus now
        control.OnFocusIn();
        FocusedControl = control;
    }

    /// <summary>
    /// Clears focus from the currently focused control
    /// </summary>
    public void ClearFocus()
    {
        if (FocusedControl != null)
        {
            FocusedControl.OnFocusOut();
            FocusedControl = null;
        }
    }
    #endregion

    #region Constructor
    /// <summary>
    /// Sets up the GUI system for usage with a rendering engine
    /// </summary>
    /// <param name="engine">The rendering engine containing the Direct3D device</param>
    public DialogManager(Engine engine)
    {
        Engine = engine ?? throw new ArgumentNullException(nameof(engine));
        OnCreateDevice();

        // Handle lost devices properly
        engine.DeviceLost += OnLostDevice;
        engine.DeviceReset += OnResetDevice;

        OnResetDevice();
    }
    #endregion

    //--------------------//

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
        if (Engine != null)
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
        _glyphTextures ??= new(Engine.Device);
        fn.Font = GetFontSystem(fn.FaceName, fn.Weight) is {} fontSystem
            ? new GuiFont(fontSystem, _glyphTextures, (int)fn.Height, GetCharWidths(fn))
            : null;
    }

    /// <summary>
    /// Gets GDI's hinted character widths for a font, so text takes up the same space as it always did.
    /// </summary>
    /// <returns>The widths; <c>null</c> to fall back to the unhinted outlines.</returns>
    private static int[]? GetCharWidths(FontNode fn)
    {
        try
        {
            return SystemFontData.GetCharWidths(fn.FaceName, fn.Weight, (int)fn.Height);
        }
        #region Error handling
        catch (IOException ex)
        {
            Log.Warn($"Getting character widths of font {fn.FaceName} failed; text may take up more space than intended", ex);
            return null;
        }
        #endregion
    }

    /// <summary>
    /// Gets the font system for a font face, loading it from the installed system fonts if necessary.
    /// </summary>
    /// <returns>The font system; <c>null</c> if the font could not be loaded.</returns>
    private FontSystem? GetFontSystem(string faceName, FontWeight weight)
    {
        var key = (faceName.ToLowerInvariant(), weight);
        if (_fontSystems.TryGetValue(key, out var fontSystem)) return fontSystem;

        try
        {
            fontSystem = new(new() {PremultiplyAlpha = false, TextureWidth = 512, TextureHeight = 512});
            fontSystem.AddFont(SystemFontData.Get(faceName, weight));
        }
        #region Error handling
        catch (Exception ex) when (ex is IOException or ArgumentException)
        {
            Log.Error($"Loading font {faceName} failed; text rendering will be disabled", ex);
            fontSystem?.Dispose();
            fontSystem = null;
        }
        #endregion

        _fontSystems.Add(key, fontSystem);
        return fontSystem;
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
        if (Engine != null)
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
        using (var stream = ContentManager.GetFileStream("GUI/Textures", tn.Filename))
        {
            try
            {
                tn.Texture = TextureLoader.FromStream(Engine.Device, stream, mipmaps: true, srgb: false);
            }
            catch (SharpGenException) when (stream.CanSeek)
            {
                Log.Warn("Loading texture with mip-maps failed; retrying without mip-map generation");
                stream.Position = 0;
                tn.Texture = TextureLoader.FromStream(Engine.Device, stream, mipmaps: false);
            }
        }

        // Store dimensions (read from the actual created texture)
        SurfaceDescription desc = tn.Texture.GetLevelDesc(0);
        tn.Width = (uint)desc.Width;
        tn.Height = (uint)desc.Height;
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

        Sprite = new(Engine.Device);

    }

    /// <summary>
    /// Called when the device has been lost.
    /// </summary>
    public void OnLostDevice()
    {
        if (!Engine.IsDisposed)
        {
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
        // Create new state block

        try
        {
            StateBlock = Engine.Device.CreateStateBlock(StateBlockType.All);
        }
        #region Error handling
        catch (SharpGenException ex)
        {
            Log.Error("Direct3D StateBlock creation failed; GUI render-state capture will be skipped", ex);
        }
        #endregion
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
        Engine.DeviceLost -= OnLostDevice;
        Engine.DeviceReset -= OnResetDevice;

        // Dispose DirectX objects
        StateBlock?.Dispose();

        // Clear caches
        _textureCache.ForEach(node => node.Texture?.Dispose());
        foreach (var fontSystem in _fontSystems.Values) fontSystem?.Dispose();
        _glyphTextures?.Dispose();

        GC.SuppressFinalize(this);
        Disposed = true;
    }

    /// <inheritdoc/>
    ~DialogManager()
    {
        // This block will only be executed on Garbage Collection, not by manual disposal
        Log.Error($"Forgot to call Dispose on {this}");
#if DEBUG
        throw new InvalidOperationException($"Forgot to call Dispose on {this}");
#endif
    }
    #endregion
}

/// <summary>
/// Structure for shared textures
/// </summary>
public class TextureNode
{
    public string Filename;
    public IDirect3DTexture9 Texture;
    public uint Width;
    public uint Height;
}

/// <summary>
/// Structure for shared fonts
/// </summary>
public class FontNode
{
    public string FaceName;
    public GuiFont? Font;
    public uint Height;
    public FontWeight Weight;
}
