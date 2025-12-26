/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using Xunit;

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// Contains test methods for <see cref="FreeFlyCamera"/>.
/// </summary>
public class FreeFlyCameraTest
{
    [Fact]
    public void TestNavigateTranslation()
    {
        var camera = new FreeFlyCamera();

        camera.Navigate(translation: new(0, 0, 10));

        camera.Position.X.Should().BeApproximately(0, precision: 0.1);
        camera.Position.Y.Should().BeApproximately(0, precision: 0.1);
        camera.Position.Z.Should().BeApproximately(10, precision: 0.1);
    }

    [Fact]
    public void TestNavigateRotatedTranslation()
    {
        var camera = new FreeFlyCamera();

        camera.Navigate(rotation: new(90, 0, 0));
        camera.Navigate(translation: new(0, 0, 10));

        camera.Position.X.Should().BeApproximately(-10, precision: 0.1);
        camera.Position.Y.Should().BeApproximately(0, precision: 0.1);
        camera.Position.Z.Should().BeApproximately(0, precision: 0.1);
    }
}
