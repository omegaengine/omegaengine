/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using AwesomeAssertions;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Xunit;

namespace OmegaEngine.Graphics.Shaders;

public class EffectCompilerTest
{
    private const string Source = """
        float4 PS() : COLOR { return FACTOR; }

        technique Main
        {
            pass P0 { PixelShader = compile ps_3_0 PS(); }
        }
        """;

    [Fact]
    public void CompilesToD3D9EffectBytecode()
    {
        var bytecode = EffectCompiler.Compile(Source, [new ShaderMacro("FACTOR", "1")]);

        BitConverter.ToUInt32(bytecode, 0).Should().Be(0xFEFF0901, because: "fx_2_0 effects start with this version tag");
    }

    [Fact]
    public void AppliesMacros()
    {
        var withoutMacro = () => EffectCompiler.Compile(Source);

        withoutMacro.Should().Throw<SharpGenException>().Which.Message.Should().Contain("FACTOR");
    }

    [Fact]
    public void ReportsCompilerErrors()
    {
        var act = () => EffectCompiler.Compile("this is not HLSL");

        act.Should().Throw<SharpGenException>().Which.Message.Should().Contain("error");
    }
}
