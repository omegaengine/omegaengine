/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2014 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using System.Drawing;
using SlimDX;

namespace OmegaGUI.Render
{
    /// <summary>
    /// Stores data for a DropdownList or ListBox item
    /// </summary>
    public struct ListItem
    {
        public string ItemText;
        public string ItemTag;
        public object ItemData;
        public Rectangle ItemRect;
        public bool IsItemVisible;
        public bool IsItemSelected;
    }

    /// <summary>
    /// Blends colors
    /// </summary>
    public struct BlendColor
    {
        public Color4[] States; // Modulate colors for all possible control states
        public Color4 Current; // Current color

        /// <summary>Initialize the color blending</summary>
        public void Initialize(Color4 defaultColor, Color4 disabledColor, Color4 hiddenColor)
        {
            // Create the array
            States = new Color4[(int)ControlState.LastState];
            for (int i = 0; i < States.Length; i++)
                States[i] = defaultColor;

            // Store the data
            States[(int)ControlState.Disabled] = disabledColor;
            States[(int)ControlState.Hidden] = hiddenColor;
            Current = hiddenColor;
        }

        /// <summary>Initialize the color blending</summary>
        public void Initialize(Color4 defaultColor)
        {
            Initialize(defaultColor, new(0.75f, 0.5f, 0.5f, 0.5f), new());
        }

        /// <summary>Blend the colors together</summary>
        public void Blend(ControlState state, float elapsedTime, float rate)
        {
            if ((States == null) || (States.Length == 0))
                return; // Nothing to do

            Color4 destColor = States[(int)state];
            Current = Color4.Lerp(Current, destColor, 1.0f - (float)Math.Pow(rate, 30 * elapsedTime));
        }

        /// <summary>Blend the colors together</summary>
        public void Blend(ControlState state, float elapsedTime)
        {
            Blend(state, elapsedTime, 0.7f);
        }
    }

    /// <summary>
    /// Contains all the display information for a given control type
    /// </summary>
    public struct ElementHolder
    {
        public ControlType ControlType;
        public uint ElementIndex;
        public Element Element;
    }
}
