/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.Serialization;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// Exception thrown when <see cref="DynamicShader"/> fails to compile FX code
    /// </summary>
    [Serializable]
    public class ShaderCompileException : Exception
    {
        /// <summary>
        /// The FX code that failed to compile
        /// </summary>
        public string FxCode { get; private set; }

        public ShaderCompileException() : this("Failed to compile shader")
        {}

        public ShaderCompileException(string message) : base(message)
        {}

        public ShaderCompileException(string message, Exception innerException) : base(message, innerException)
        {}

        public ShaderCompileException(string message, Exception innerException, string fxCode) : this(message, innerException)
        {
            FxCode = fxCode;
        }

        protected ShaderCompileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
    }
}
