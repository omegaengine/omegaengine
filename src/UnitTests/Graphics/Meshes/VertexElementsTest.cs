/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Runtime.InteropServices;
using AwesomeAssertions;
using OmegaEngine.Graphics.VertexDecl;
using Vortice.Direct3D9;
using Xunit;

namespace OmegaEngine.Graphics.Meshes;

public class VertexElementsTest
{
    [Theory]
    [InlineData(PositionColored.Format)]
    [InlineData(PositionTextured.Format)]
    [InlineData(PositionNormalTextured.Format)]
    [InlineData(TransformedColoredTextured.Format)]
    public void RoundTripsFixedFunctionFormats(VertexFormat format)
        => VertexElements.FromFormat(format).ToFormat().Should().Be(format);

    [Fact]
    public void MatchesStructSizes()
    {
        VertexElements.FromFormat(PositionNormalTextured.Format).GetVertexSize().Should().Be(Marshal.SizeOf<PositionNormalTextured>());
        PositionNormalTangentBinormalTextured.GetVertexElements().GetVertexSize().Should().Be(Marshal.SizeOf<PositionNormalTangentBinormalTextured>());
    }

    [Fact]
    public void HasNoFormatForCustomDeclarations()
        => PositionNormalTangentBinormalTextured.GetVertexElements().ToFormat().Should().Be(VertexFormat.None);
}
