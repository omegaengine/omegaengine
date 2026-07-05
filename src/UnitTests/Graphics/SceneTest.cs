/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using FluentAssertions;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Renderables;
using Xunit;

namespace OmegaEngine.Graphics;

public class SceneTest : EngineTestBase
{
    [Fact]
    public void PropagatesEngineToPositionablesAndDisposesThem()
    {
        var model = new Model(XMesh.Get(Engine, "Test/Box/Normal/Normal.x"));
        model.IsEngineSet.Should().BeFalse();

        var scene = new Scene { Positionables = { model } };

        // Setting the scene's engine propagates it through the collection to the model
        scene.Engine = Engine;
        model.IsEngineSet.Should().BeTrue();

        // Disposing the scene disposes its contained renderables
        scene.Dispose();
        model.IsDisposed.Should().BeTrue();
    }
}
