/*
 * Copyright 2006-2012 Bastian Eicher
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;
using Common.Utils;
using Common.Storage;
using Common.Values;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaGUI.Render;
using Resources = OmegaGUI.Properties.Resources;

namespace OmegaGUI.Model
{

    #region Delegates
    /// <summary>
    /// A delegate for executing a script
    /// </summary>
    /// <param name="script">The script to be executed</param>
    /// <param name="source">From where the script is being called</param>
    /// <seealso cref="Dialog.ScriptFired"/>
    public delegate void ScriptExecution(string script, string source);
    #endregion

    /// <summary>
    /// An XML-serializable dialog view
    /// </summary>
    [XmlInclude(typeof(Color4))]
    [CLSCompliant(false)]
    public class Dialog : ICloneable
    {
        #region Events
        /// <summary>
        /// Occurs whenever one of the controls fires a script
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event ScriptExecution ScriptFired;
        #endregion

        #region Variables
        /// <summary>
        /// The <see cref="Render.Dialog"/> used for actual rendering
        /// </summary>
        internal Render.Dialog DialogModel;

        /// <summary>
        /// ToDo: Document
        /// </summary>
        internal uint CustomTexture = 1;

        private IDictionary<string, string> _locale;
        #endregion

        #region Properties
        /// <summary>
        /// The culture used for loading the assembly resources
        /// </summary>
        public static CultureInfo ResourceCulture { get { return Resources.Culture; } set { Resources.Culture = value; } }

        public override string ToString()
        {
            return "Dialog";
        }

        /// <summary>
        /// A flag to determine whether the model counterpart to this dialog vuew needs to be recreated to reflect changes made to properties
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public bool NeedsUpdate { get; internal set; }

        #region Appearance
        private string _captionText = "";

        /// <summary>
        /// The caption text displayed at the top of this dialog
        /// </summary>
        [DefaultValue(""), Description("The caption text displayed at the top of this dialog"), Category("Appearance")]
        public string CaptionText
        {
            get { return _captionText; }
            set
            {
                _captionText = value;
                if (DialogModel != null) DialogModel.SetCaptionText(GetLocalized(_captionText));
            }
        }

        private int _captionHeight = 20;

        /// <summary>
        /// The height of caption displayed at the top of this dialog
        /// </summary>
        [DefaultValue(20), Description("The height of caption displayed at the top of this dialog"), Category("Appearance")]
        public int CaptionHeight
        {
            get { return _captionHeight; }
            set
            {
                _captionHeight = value;
                if (DialogModel != null) DialogModel.CaptionHeight = value;
            }
        }

        private bool _animate = true;

        /// <summary>
        /// Shall this dialog use animations?
        /// </summary>
        [XmlAttribute, DefaultValue(true), Description("Shall this dialog use animations?"), Category("Appearance")]
        public bool Animate { get { return _animate; } set { _animate = value; } }
        #endregion

        #region Colors
        /// <summary>Used for XML serialization.</summary>
        public XColor ColorBackground, ColorCaption = Color.FromArgb(128, 96, 128, 255), ColorText = Color.White;

        private void UpdateColors()
        {
            if (DialogModel != null)
            {
                DialogModel.SetBackgroundColors(ColorBackground.ToColorValue());
                DialogModel.SetCaptionColor(ColorCaption.ToColorValue());
            }
        }

        /// <summary>
        /// The dialog's background color
        /// </summary>
        [XmlIgnore, Description("The dialog's background color"), Category("Appearance")]
        public Color BackgroundColor
        {
            get { return (Color)ColorBackground; }
            set
            {
                ColorBackground = value;
                UpdateColors();
            }
        }

        /// <summary>
        /// The dialog's caption bar color
        /// </summary>
        [XmlIgnore, Description("The dialog's caption bar color"), Category("Appearance")]
        public Color CaptionColor
        {
            get { return (Color)ColorCaption; }
            set
            {
                ColorCaption = value;
                UpdateColors();
            }
        }

        /// <summary>
        /// The color of text on the dialog - no auto-update
        /// </summary>
        [XmlIgnore, Description("The color of text on the dialog"), Category("Appearance")]
        public Color TextColor
        {
            get { return (Color)ColorText; }
            set
            {
                ColorText = value;
                NeedsUpdate = true;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// A script to be run when the dialog is first shown
        /// </summary>
        [DefaultValue(""), Description("A script to be run when the dialog is first shown"), Category("Events"),
         Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string OnShow { get; set; }

        /// <summary>
        /// A script to be run after the dialog was first shown and whenever it needs to update its output
        /// </summary>
        [DefaultValue(""), Description("A script to be run after the dialog was first shown and whenever it needs to update its output"), Category("Events"),
         Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string OnUpdate { get; set; }
        #endregion

        #region Texture file
        private string _textureFile = "base.png";

        /// <summary>
        /// The file containing the texture for controls on this dialog - no auto-update
        /// </summary>
        [XmlAttribute, DefaultValue("base.png"), Description("The file containing the texture for controls on this dialog"), Category("Appearance")]
        public string TextureFile
        {
            get { return _textureFile; }
            set
            {
                _textureFile = value;
                NeedsUpdate = true;
            }
        }

        [Description("Is the specified texture file name valid?"), Category("Appearance")]
        public bool TextureFileValid { get { return !string.IsNullOrEmpty(_textureFile) && ContentManager.FileExists("GUI/Textures", _textureFile, true); } }
        #endregion

        #region Font
        private string _fontName = "Arial";

        /// <summary>
        /// The name of font for text on controls - no auto-update
        /// </summary>
        [DefaultValue("Arial"), Description("The name of font for text on controls"), Category("Appearance")]
        public string FontName
        {
            get { return _fontName; }
            set
            {
                _fontName = value;
                NeedsUpdate = true;
            }
        }

        private uint _fontSize = 16;

        /// <summary>
        /// The font size
        /// </summary>
        [DefaultValue((uint)16), Description("The font size"), Category("Appearance")]
        public uint FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                if (DialogModel != null) DialogModel.DefaultFontSize = (uint)(_fontSize * EffectiveScale);
            }
        }
        #endregion

        #region Layout
        private Size _size;

        /// <summary>
        /// The initial size of the dialog, used for auto-scaling fullscreen dialogs - no auto-update
        /// </summary>
        [Category("Layout"), Description("The initial size of the dialog, used for auto-scaling fullscreen dialogs")]
        public Size Size
        {
            get { return _size; }
            set
            {
                _size = value;
                NeedsUpdate = true;
            }
        }

        private Point _shift;

        /// <summary>
        /// A set of coordinates by which all control positions are shifted
        /// </summary>
        [DefaultValue(typeof(Point), "0; 0"), Category("Layout"), Description("A set of coordinates by which all control positions are shifted")]
        public Point Shift
        {
            get { return _shift; }
            set
            {
                _shift = value;
                if (DialogModel != null)
                {
                    foreach (Control control in _controls)
                        control.UpdateLayout();
                }
            }
        }

        private float _scale = 1;

        /// <summary>
        /// A factor by which all sizes are multiplied, is ignored in fullscreen mode
        /// </summary>
        [XmlAttribute, DefaultValue(1f), Category("Layout"), Description("A factor by which all sizes are multiplied, is ignored in fullscreen mode")]
        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                if (DialogModel != null)
                {
                    foreach (Control control in _controls)
                        control.UpdateLayout();
                    DialogModel.DefaultFontSize = (uint)(_fontSize * EffectiveScale);
                }
            }
        }

        private float _autoScale = 1;

        /// <summary>
        /// An automatically calculated scale factor that is multiplied with the manual value
        /// </summary>
        internal float AutoScale
        {
            get { return _autoScale; }
            set
            {
                _autoScale = value;
                if (DialogModel != null)
                {
                    foreach (Control control in _controls)
                        control.UpdateLayout();
                    DialogModel.DefaultFontSize = (uint)(_fontSize * EffectiveScale);
                }
            }
        }

        /// <summary>
        /// The effective scale resulting from the user-defined and automatic scaling to fullscreen
        /// </summary>
        [Browsable(false)]
        public float EffectiveScale { get { return _scale * _autoScale; } }

        private bool _fullscreen;

        /// <summary>
        /// Shall this dialog be auto-scaled to fill the complete screen? - no auto-update
        /// </summary>
        [XmlAttribute, DefaultValue(false), Category("Layout"), Description("Shall this dialog be auto-scaled to fill the complete screen?")]
        public bool Fullscreen
        {
            get { return _fullscreen; }
            set
            {
                _fullscreen = value;
                NeedsUpdate = true;
            }
        }
        #endregion

        private bool _visible = true;

        /// <summary>
        /// Is this dialog currently visible?
        /// </summary>
        [XmlAttribute, XmlIgnore, Browsable(false)]
        public bool Visible { get { return _visible; } set { _visible = value; } }

        #region Controls
        private Collection<ButtonStyle> _buttonStyles = new Collection<ButtonStyle>();
        // Note: Can not use ICollection<T> interface with XML Serialization
        /// <summary>
        /// A list of all custom button styles available in the dialog
        /// </summary>
        [Category("Design"), Description("A list of all custom button styles available in the dialog")]
        [XmlElement(typeof(ButtonStyle))]
        public Collection<ButtonStyle> ButtonStyles { get { return _buttonStyles; } }

        // Note: Can not use IBindingList<T> interface with XML Serialization
        private BindingList<Control> _controls = new BindingList<Control>();

        /// <summary>
        /// A list of all controls on the dialog
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(Button)), XmlElement(typeof(CheckBox)),
         XmlElement(typeof(DropdownList)), XmlElement(typeof(GroupBox)),
         XmlElement(typeof(TextBox)), XmlElement(typeof(ListBox)),
         XmlElement(typeof(PictureBox)), XmlElement(typeof(RadioButton)),
         XmlElement(typeof(ScrollBar)), XmlElement(typeof(Slider)),
         XmlElement(typeof(Label))]
        public BindingList<Control> Controls { get { return _controls; } }
        #endregion

        #endregion

        #region Localization
        /// <summary>
        /// Returns the localized version of a string if available
        /// </summary>
        /// <param name="value">The value to localize</param>
        /// <returns>The localized value or the original value</returns>
        internal string GetLocalized(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value == "[Version]") return "v" + Assembly.GetEntryAssembly().GetName().Version;
            if (_locale != null && value.StartsWith("[", StringComparison.Ordinal) && value.EndsWith("]", StringComparison.Ordinal))
            {
                string localeKey = value.Substring(1, value.Length - 2);
                string localeValue;
                if (_locale.TryGetValue(localeKey, out localeValue)) return localeValue;
            }
            return value;
        }
        #endregion

        //--------------------//

        #region Generate
        /// <summary>
        /// Generates a real dialog model from this XML-representation
        /// </summary>
        /// <param name="manager">The <see cref="DialogManager"/> instance that provides the resources for rendering of this dialog</param>
        /// <returns>The generated dialog model</returns>
        public Render.Dialog GenerateDialog(DialogManager manager)
        {
            // Load language XML
            string langName = StringUtils.GetLeftPartAtFirstOccurrence(Resources.Culture.EnglishName, ' ');
            _locale = LocaleFile.LoadLang(langName).ToDictionary();

            // Generate dialog model object
            DialogModel = TextureFileValid ?
                                               new Render.Dialog(manager, ColorText.ToColorValue(), _textureFile, _fontName, (uint)(_fontSize * EffectiveScale)) :
                                                                                                                                                                     new Render.Dialog(manager, ColorText.ToColorValue());

            // Set dialog properites
            DialogModel.SetCaptionText(GetLocalized(_captionText));
            DialogModel.CaptionHeight = _captionHeight;
            UpdateColors();

            // Load custom button styles
            foreach (ButtonStyle style in _buttonStyles)
            {
                style.Parent = this;
                style.Generate();
            }

            // Generate control models from their view counterparts
            foreach (Control control in _controls)
            {
                control.Parent = this;
                control.Generate();
            }

            // Update control positions
            DialogModel.Resize += delegate
            {
                foreach (Control control in _controls)
                    control.UpdateLayout();
            };

            NeedsUpdate = false;
            return DialogModel;
        }

        /// <summary>
        /// Executes a script to handle an event
        /// </summary>
        /// <param name="script">The script to be executed</param>
        /// <param name="source">From where the script is being called</param>
        /// <seealso cref="ScriptFired"/>
        internal void RaiseEvent(string script, string source)
        {
            if (ScriptFired != null && !string.IsNullOrEmpty(script))
                ScriptFired(script, source);
        }
        #endregion

        #region Access
        /// <summary>
        /// Gets the first <see cref="Control"/> in this <see cref="Dialog"/> with the specified <paramref name="name"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException">An element with the specified key does not exist in the dictionary.</exception>
        public Control this[string name]
        {
            get
            {
                #region Sanity checks
                if (name == null) throw new ArgumentNullException("name");
                #endregion

                foreach (Control control in _controls)
                    if (control.Name == name) return control;
                throw new KeyNotFoundException(Resources.ControlNotFound);
            }
        }

        /// <summary>
        /// Gets the <see cref="ButtonStyle"/> with the specified <paramref name="name"/>
        /// </summary>
        /// <returns>The requested <see cref="ButtonStyle"/> or <see langword="null"/> if it couldn't be found</returns>
        internal ButtonStyle GetButtonStyle(string name)
        {
            foreach (ButtonStyle style in _buttonStyles)
                if (style.Name == name) return style;
            return null;
        }

        /// <summary>
        /// Finds <see cref="Control"/>s within a certain area
        /// </summary>
        /// <param name="area">The coordinate area to look in</param>
        /// <returns>A list of all <see cref="Control"/>s with the <paramref name="area"/>.</returns>
        public ICollection<Control> PickControls(Rectangle area)
        {
            var pickedControls = new LinkedList<Control>();
            foreach (var control in Controls)
            {
                if (control.DrawBox.IntersectsWith(area))
                    pickedControls.AddLast(control);
            }
            return pickedControls;
        }
        #endregion

        #region MsgBoxes
        public void MsgBox(string text, MsgBoxType type, Action<MsgBoxResult> callback)
        {
            DialogModel.Refresh();
            DialogModel.DialogManager.MessageBox.SetFont(0, _fontName, _fontSize, FontWeight.Normal);
            DialogModel.DialogManager.MessageBox.Show(GetLocalized(text), type, callback);
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a dialog from an XML file via the <see cref="ContentManager"/>.
        /// </summary>
        /// <param name="id">The ID of the file to load from</param>
        /// <returns>The loaded dialog</returns>
        public static Dialog FromContent(string id)
        {
            using (var stream = ContentManager.GetFileStream("GUI", id))
                return XmlStorage.Load<Dialog>(stream);
        }

        /// <summary>
        /// Loads a dialog from an XML file
        /// </summary>
        /// <param name="path">The file to load from</param>
        /// <returns>The loaded dialog</returns>
        public static Dialog Load(string path)
        {
            return XmlStorage.Load<Dialog>(path);
        }

        /// <summary>
        /// Saves this dialog in an XML file
        /// </summary>
        /// <param name="path">The file to save in</param>
        public void Save(string path)
        {
            XmlStorage.Save(path, this);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this dialog.
        /// You need to call <see cref="GenerateDialog"/> on it before it can be used for rendering.
        /// </summary>
        /// <returns>The cloned dialog.</returns>
        public Dialog Clone()
        {
            var newDialog = (Dialog)MemberwiseClone();

            newDialog._controls = new BindingList<Control>();
            foreach (Control control in _controls)
                newDialog._controls.Add(control.Clone());

            newDialog._buttonStyles = new Collection<ButtonStyle>();
            foreach (ButtonStyle style in _buttonStyles)
                newDialog._buttonStyles.Add(style.Clone());

            newDialog.ScriptFired = null;

            return newDialog;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
