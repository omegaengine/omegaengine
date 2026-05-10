/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.IO;
using System.Linq;
using FrameOfReference.World;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;

namespace FrameOfReference;

/// <summary>
/// Saving and loading <see cref="Session"/>s as named savegames.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="session">The currently active session, if any.</param>
/// <param name="beforeSave">Called before saving, e.g. to sync presenter state back to the session.</param>
public class Savegames(Game game, Session? session = null, Action? beforeSave = null)
{
    private const string ResumeName = "Resume";

    /// <summary>
    /// Saves the currently active session under the specified <paramref name="name"/>.
    /// </summary>
    [UsedImplicitly]
    public void Save(string name)
    {
        if (string.IsNullOrEmpty(name) || session == null) return;
        beforeSave?.Invoke();
        session.Save(GetPath(name));
    }

    /// <summary>
    /// Saves the currently active session in the auto-resume slot.
    /// </summary>
    internal void SaveAsResume()
    {
        beforeSave?.Invoke();
        session?.Save(GetPath(ResumeName));
    }

    /// <summary>
    /// Loads the session saved under the specified <paramref name="name"/> and activates it.
    /// </summary>
    [UsedImplicitly]
    public void Load(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        game.SwitchToInGame(LoadFrom(name));
    }

    /// <summary>
    /// Loads the session saved in the auto-resume slot without activating it.
    /// </summary>
    internal static Session? LoadFromResume()
    {
        try
        {
            return LoadFrom(ResumeName);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to restore previous game session: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Loads the session saved under the specified <paramref name="name"/> without activating it.
    /// </summary>
    private static Session LoadFrom(string name)
        => Session.Load(GetPath(name));

    /// <summary>
    /// Returns the names of all user-created savegames, excluding the auto-resume slot.
    /// </summary>
    [UsedImplicitly]
    public IEnumerable<string> GetNames()
    {
        var dir = new DirectoryInfo(Locations.GetSaveDataPath(Constants.AppName, isFile: false));
        return dir.GetFiles($"*{Constants.SavegameFileExt}")
            .Select(x => x.Name[..^Constants.SavegameFileExt.Length])
            .Except(ResumeName);
    }

    private static string GetPath(string name)
        => Locations.GetSaveDataPath(Constants.AppName, isFile: true, name + Constants.SavegameFileExt);
}
