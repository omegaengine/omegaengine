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
using NLua;

namespace OmegaGUI;

/// <summary>
/// Transfers values from one <see cref="Lua"/> instance to another.
/// </summary>
internal static class LuaArgs
{
    /// <summary>
    /// Prepares a value originating from one <see cref="Lua"/> instance for use in another one.
    /// </summary>
    /// <param name="value">The value to transfer. Lua tables are copied, .NET objects are passed by reference.</param>
    /// <param name="target">The <see cref="Lua"/> instance the value is to be used in.</param>
    /// <param name="depth">The current Lua table nesting level.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is or contains a Lua function or too deeply nested Lua tables.</exception>
    /// <remarks>Lua tables are bound to the <see cref="Lua"/> instance they were created in and can therefore not be shared.</remarks>
    public static object? Transfer(object? value, Lua target, int depth = 0)
        => value switch
        {
            LuaTable table => CopyTable(table, target, depth),
            LuaFunction => throw new ArgumentException("Lua functions can not be passed between Lua instances.", nameof(value)),
            _ => value
        };

    private static LuaTable CopyTable(LuaTable table, Lua target, int depth)
    {
        var copy = (LuaTable)target.DoString("return {}")[0];
        foreach (object key in table.Keys)
            copy[Transfer(key, target, depth + 1)] = Transfer(table[key], target, depth + 1);
        return copy;
    }
}
