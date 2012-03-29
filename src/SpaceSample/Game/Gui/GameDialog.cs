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

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OmegaGUI.Controller;

namespace Core.Gui
{
    /// <summary>
    /// A GUI dialog used by the game
    /// </summary>
    public class GameDialog : Dialog
    {
        #region Constructor
        /// <summary>
        /// Creates a new dialog from an XML file
        /// </summary>
        /// <param name="game">The game this dialog belongs to</param>
        /// <param name="filename">The filename of the XML file to load</param>
        /// <param name="location">The location of the dialog on the screen</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Not possible due to usage in super-constructor call")]
        public GameDialog(Game game, string filename, Point location) : base(game.DialogManager, filename, location, true)
        {
            game.SetupLua(Lua);
        }
        #endregion
    }
}
