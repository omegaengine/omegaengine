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
using System.Windows.Forms;
using Common;
using Hanoi.Logic;

namespace Hanoi
{
    /// <summary>
    /// Provides a control interface for the simulation
    /// </summary>
    public partial class MainForm : Form
    {
        private Session _currentSession;

        #region Constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        //--------------------//

        #region Setup & Solve
        private void setupButton_Click(object sender, EventArgs e)
        {
            byte pegs, discs;
            if (!byte.TryParse(pegsBox.Text, out pegs) || !byte.TryParse(discsBox.Text, out discs))
                return;
            if (pegs > 2 && discs > 0)
            {
                sourceBox.Items.Clear();
                targetBox.Items.Clear();
                for (int i = 1; i <= pegs; i++)
                {
                    sourceBox.Items.Add(i);
                    targetBox.Items.Add(i);
                }

                NewSession(new Universe(pegs, discs));
                ApplySpeed();
            }
        }

        private void NewSession(Universe universe)
        {
            throw new NotImplementedException();
        }

        private void solveButton_Click(object sender, EventArgs e)
        {
            if (_currentSession.Solving)
                _currentSession.StopSolving();
            else
                _currentSession.StartSolving();
        }
        #endregion

        #region Speed
        private void ApplySpeed()
        {
            _currentSession.TimeWarpFactor = (float)speedSlider.Value / 2;
        }

        private void speedSlider_Scroll(object sender, EventArgs e)
        {
            ApplySpeed();
        }
        #endregion

        #region Move
        private void moveButton_Click(object sender, EventArgs e)
        {
            if (sourceBox.SelectedItem != null && targetBox.SelectedItem != null)
            {
                int sourcePeg = sourceBox.SelectedIndex;
                int targetPeg = targetBox.SelectedIndex;

                Universe universe = _currentSession.CurrentUniverse;
                Peg[] pegs = universe.GetPegs();

                try
                {
                    universe.MoveDisc(pegs[sourcePeg], pegs[targetPeg]);
                }
                catch (ArgumentException)
                {
                    Msg.Inform(this, "Invalid action!", MsgSeverity.Error);
                }
            }
        }

        /// <summary>
        /// Externally selects a peg for source of movement
        /// </summary>
        /// <param name="index">The index of the peg</param>
        public void SelectSource(int index)
        {
            sourceBox.SelectedIndex = index;
        }

        /// <summary>
        /// Externally selects a peg for target of movement
        /// </summary>
        /// <param name="index">The index of the peg</param>
        public void SelectTarget(int index)
        {
            targetBox.SelectedIndex = index;
        }
        #endregion
    }
}
