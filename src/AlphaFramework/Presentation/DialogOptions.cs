/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using NLua;

namespace AlphaFramework.Presentation;

/// <summary>
/// Options for loading a dialog with <see cref="GameBase.LoadDialog(string,AlphaFramework.Presentation.DialogOptions?,object?)"/>.
/// </summary>
public sealed class DialogOptions
{
    /// <summary>
    /// Exclusively focus the dialog, blocking input to all other dialogs.
    /// </summary>
    public bool Modal { get; init; }

    /// <summary>
    /// Center the dialog on the screen. Overrides <see cref="Location"/>.
    /// </summary>
    public bool Centered { get; init; }

    /// <summary>
    /// Close all other dialogs before showing this one.
    /// </summary>
    public bool Splash { get; init; }

    /// <summary>
    /// The location of the dialog on the screen. Leave unset for a small offset from the top-left corner.
    /// </summary>
    public Point? Location { get; init; }

    /// <summary>
    /// Interprets options passed in from Lua or .NET code.
    /// </summary>
    /// <param name="options">A <see cref="LuaTable"/>, a <see cref="DialogOptions"/> instance or <c>null</c> for the defaults.</param>
    /// <exception cref="ArgumentException"><paramref name="options"/> is of an unsupported type or contains unknown options.</exception>
    internal static DialogOptions Parse(object? options)
        => options switch
        {
            null => new(),
            DialogOptions dialogOptions => dialogOptions,
            LuaTable table => FromLua(table),
            _ => throw new ArgumentException($"Dialog options must be a Lua table or a {nameof(DialogOptions)} instance.", nameof(options))
        };

    private static DialogOptions FromLua(LuaTable table)
    {
        foreach (object key in table.Keys)
        {
            if (key is not (nameof(Modal) or nameof(Centered) or nameof(Splash) or nameof(Location)))
                throw new ArgumentException($"Unknown dialog option: {key}", nameof(table));
        }

        return new()
        {
            Modal = GetBool(table, nameof(Modal)),
            Centered = GetBool(table, nameof(Centered)),
            Splash = GetBool(table, nameof(Splash)),
            Location = table[nameof(Location)] switch
            {
                null => null,
                Point point => point,
                {} value => throw new ArgumentException($"Dialog option '{nameof(Location)}' must be a Point, not: {value}", nameof(table))
            }
        };
    }

    private static bool GetBool(LuaTable table, string name)
        => table[name] switch
        {
            null => false,
            bool value => value,
            {} value => throw new ArgumentException($"Dialog option '{name}' must be a boolean, not: {value}", nameof(table))
        };
}
