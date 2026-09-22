/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using AwesomeAssertions;
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

    [Fact]
    public void PropagatesEngineToNestedRenderablesAndDisposesThem()
    {
        var pivot = new Pivot();
        var model = new Model(XMesh.Get(Engine, "Test/Box/Normal/Normal.x"));
        pivot.Children.Add(model);

        var scene = new Scene { Positionables = { pivot } };

        // The engine reaches renderables nested below a root
        scene.Engine = Engine;
        model.IsEngineSet.Should().BeTrue();

        // Disposing the scene disposes the whole hierarchy
        scene.Dispose();
        pivot.IsDisposed.Should().BeTrue();
        model.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void FailedAddLeavesThePreviousParentIntact()
    {
        var pivot = new Pivot();
        var child = new Pivot();
        pivot.Children.Add(child);
        using var scene = new Scene { Engine = Engine, Positionables = { pivot } };

        child.Dispose();
        Assert.Throws<ObjectDisposedException>(() => scene.Positionables.Add(child));

        pivot.Children.Should().Equal(child);
        scene.Positionables.Should().Equal(pivot);
    }
}
