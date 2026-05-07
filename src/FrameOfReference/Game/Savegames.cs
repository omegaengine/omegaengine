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
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;

namespace FrameOfReference;

/// <summary>
/// Provides helpers for saving and loading <see cref="Session"/>s as named savegames.
/// </summary>
public static class Savegames
{
    /// <summary>
    /// Saves <paramref name="session"/> under the specified <paramref name="name"/>.
    /// </summary>
    public static void SaveAs(this Session session, string name)
        => session.Save(GetPath(name));

    /// <summary>
    /// Saves <paramref name="session"/> as the auto-resume slot so it can be reloaded on next launch.
    /// </summary>
    public static void SaveAsResume(this Session session)
        => session.SaveAs("Resume");

    /// <summary>
    /// Loads the session saved under the specified <paramref name="name"/>.
    /// </summary>
    public static Session LoadFrom(string name)
        => Session.Load(GetPath(name));

    /// <summary>
    /// Loads the auto-resume save, or returns <see langword="null"/> if none exists or it cannot be read.
    /// </summary>
    public static Session? LoadFromResume()
    {
        try
        {
            return LoadFrom("Resume");
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
    /// Returns the names of all user-created savegames, excluding the auto-resume slot.
    /// </summary>
    public static IEnumerable<string> GetNames()
    {
        var dir = new DirectoryInfo(Locations.GetSaveDataPath(Constants.AppName, isFile: false));
        return dir.GetFiles($"*{Constants.SavegameFileExt}")
                  .Select(x => x.Name[..^Constants.SavegameFileExt.Length])
                  .Except("Resume");
    }

    private static string GetPath(string name)
        => Locations.GetSaveDataPath(Constants.AppName, isFile: true, name + Constants.SavegameFileExt);
}
