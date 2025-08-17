/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using NanoByte.Common;

namespace OmegaEngine.Graphics.Shaders;

partial class DynamicShader
{
    private abstract class Counter(string id)
    {
        public string ID { get; } = id;

        public abstract string GetValue(int run);
    }

    private sealed class IntCounter : Counter
    {
        private readonly int _min, _max;
        private readonly float _step = 1;

        public IntCounter(string id, int min, int max) : base(id)
        {
            _min = min;
            _max = max;
        }

        public IntCounter(string id, int min, int max, float step) : base(id)
        {
            _min = min;
            _max = max;
            _step = step;
        }

        public override string GetValue(int run)
        {
            var num = (int)Math.Ceiling(run * _step);
            return num.Clamp(_min, _max).ToString(CultureInfo.InvariantCulture);
        }
    }

    private sealed class CharCounter : Counter
    {
        private readonly char[] _chars;

        public CharCounter(string id, ICollection<char> chars) : base(id)
        {
            _chars = new char[chars.Count];
            chars.CopyTo(_chars, 0);
        }

        public override string GetValue(int run)
        {
            run--;
            while (run >= _chars.Length)
                run -= _chars.Length;
            return _chars[run].ToString(CultureInfo.InvariantCulture);
        }
    }
}
