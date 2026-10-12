/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Numerics;
using AwesomeAssertions;
using SharpGen.Runtime;
using Xunit;

namespace OmegaEngine.D3DX;

public class EffectTest : EngineTestBase
{
    private const string Source = """
        float Factor <string Description = "A test annotation";> = 1;
        float4x4 Transform;
        float4 Tint = float4(1, 1, 1, 1);

        float4 VS(float4 position : POSITION) : POSITION { return mul(position, Transform); }
        float4 PS() : COLOR { return Tint * Factor; }

        technique Main
        {
            pass P0
            {
                VertexShader = compile vs_3_0 VS();
                PixelShader = compile ps_3_0 PS();
            }
        }
        """;

    private Effect CompileEffect()
    {
        using var compiler = EffectCompiler.FromString(Source);
        return Effect.FromMemory(Engine.Device, compiler.CompileEffect());
    }

    [Fact]
    public void DescribesParametersTechniquesAndPasses()
    {
        using var effect = CompileEffect();

        effect.Description.Parameters.Should().Be(3);
        effect.Description.Techniques.Should().Be(1);

        var technique = effect.GetTechnique(0)!;
        effect.GetTechniqueDescription(technique).Should().Be(new TechniqueDescription("Main", Passes: 1, Annotations: 0));
        effect.GetTechnique("Main").Should().Be(technique);
        effect.GetPassDescription(effect.GetPass(technique, 0)!).Name.Should().Be("P0");

        var tint = effect.GetParameter(null, "Tint")!;
        var description = effect.GetParameterDescription(tint);
        description.Type.Should().Be(ParameterType.Float);
        description.Class.Should().Be(ParameterClass.Vector);
        description.Columns.Should().Be(4);

        effect.GetParameter(null, "Missing").Should().BeNull();
    }

    [Fact]
    public void ReadsAnnotations()
    {
        using var effect = CompileEffect();

        var annotation = effect.GetAnnotation(effect.GetParameter(null, "Factor")!, 0)!;
        effect.GetParameterDescription(annotation).Name.Should().Be("Description");
        effect.GetString(annotation).Should().Be("A test annotation");
    }

    [Fact]
    public void SetsAndGetsValues()
    {
        using var effect = CompileEffect();
        var factor = effect.GetParameter(null, "Factor")!;
        var tint = effect.GetParameter(null, "Tint")!;

        effect.SetValue(factor, 0.5f);
        effect.GetValue<float>(factor).Should().Be(0.5f);

        effect.SetValue(tint, new Vector4(0.1f, 0.2f, 0.3f, 0.4f));
        effect.GetValue<float>(tint, 4).Should().Equal(0.1f, 0.2f, 0.3f, 0.4f);

        effect.SetValue(effect.GetParameter(null, "Transform")!, Matrix4x4.CreateTranslation(1, 2, 3));
    }

    [Fact]
    public void AcceptsNamesAsHandles()
    {
        using var effect = CompileEffect();

        effect.Technique = "Main";
        effect.Technique.Should().Be(effect.GetTechnique(0));

        effect.SetValue("Factor", 2f);
        effect.GetValue<float>("Factor").Should().Be(2f);
    }

    [Fact]
    public void RunsPasses()
    {
        using var effect = CompileEffect();

        Engine.Device.BeginScene();
        effect.Begin().Should().Be(1);
        effect.BeginPass(0);
        effect.EndPass();
        effect.End();
        Engine.Device.EndScene();
    }

    [Fact]
    public void LoadsCompiledEffectFiles()
    {
        using var effect = Effect.FromFile(Engine.Device, Path.Combine(Engine.ShaderDir, "General.fxo"));

        effect.Description.Techniques.Should().BePositive();
    }

    [Fact]
    public void ReportsCompilerErrors()
    {
        var act = () => EffectCompiler.FromString("this is not HLSL");

        act.Should().Throw<SharpGenException>().Which.Message.Should().Contain("error");
    }

    [Fact]
    public void AppliesMacros()
    {
        using var compiler = EffectCompiler.FromString(Source.Replace("Tint * Factor", "Tint * FACTOR"), [new Macro("FACTOR", "2")]);

        compiler.CompileEffect().Should().NotBeEmpty();
    }
}
