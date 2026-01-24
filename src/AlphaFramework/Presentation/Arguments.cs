/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.CommandLine;

namespace AlphaFramework.Presentation;

/// <summary>
/// Helpers for interpreting the command-line arguments for the current process.
/// </summary>
public static class Arguments
{
    /// <summary>
    /// Gets the value associated with an option of a given name.
    /// </summary>
    /// <returns>The value associated with the option; <c>null</c> if the option wasn't set.</returns>
    public static T? GetOption<T>(string name)
    {
        var option = new Option<T>($"/{name}", $"--{name}");
        var rootCommand = new RootCommand { Options = { option } };
        return rootCommand.Parse(Environment.GetCommandLineArgs()).GetValue(option);
    }

    /// <summary>
    /// Gets the value associated with an option of a given name.
    /// </summary>
    /// <returns>The value associated with the option; <c>null</c> if the option wasn't set.</returns>
    public static string? GetOption(string name) => GetOption<string>(name);

    /// <summary>
    /// Determines whether an option with the given name was specified.
    /// </summary>
    public static bool HasOption(string name) => GetOption<bool>(name);
}
