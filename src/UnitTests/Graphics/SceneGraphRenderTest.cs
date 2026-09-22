/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using AwesomeAssertions;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using Xunit;

namespace OmegaEngine.Graphics;

/// <summary>
/// Tests that nested <see cref="PositionableRenderable.Children"/> take part in a <see cref="View"/>'s rendering.
/// </summary>
/// <remarks><see cref="Engine.Render()"/> skips all work while the render target is invisible, so these tests show the window.</remarks>
public class SceneGraphRenderTest : EngineTestBase
{
    private View AddView(Scene scene)
    {
        Engine.Target.Show();

        var view = new View(scene, new ArcballCamera {Radius = 20, NearClip = 1, FarClip = 100})
        {
            Name = "Scene graph test",
            BackgroundColor = Color.CornflowerBlue
        };
        Engine.Views.Add(view);
        return view;
    }

    [Fact]
    public void RendersNestedNodesAndCullsThemIndividually()
    {
        var pivot = new Pivot();
        var near = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        var far = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        far.Position = new(0, 0, 10_000);
        pivot.Children.Add(near);
        pivot.Children.Add(far);

        using var scene = new Scene {Positionables = {pivot}};
        AddView(scene);

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        near.RenderCount.Should().BeGreaterThan(0, "descendants of a root must be traversed");
        far.RenderCount.Should().Be(0, "every node is culled on its own");
    }

    [Fact]
    public void HiddenParentDoesNotSuppressItsChildren()
    {
        var pivot = new Pivot {Visible = false};
        var child = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        pivot.Children.Add(child);

        using var scene = new Scene {Positionables = {pivot}};
        AddView(scene);

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        child.RenderCount.Should().BeGreaterThan(0, "there is intentionally no subtree visibility suppression");
    }

    [Fact]
    public void PicksNestedNodes()
    {
        var pivot = new Pivot();
        var child = Model.Box(Engine, XMaterial.Default, new(2, 2, 2));
        pivot.Children.Add(child);

        using var scene = new Scene {Positionables = {pivot}};
        var view = AddView(scene);

        Engine.Render(elapsedGameTime: 0, noPresent: true);

        view.Pick(view.ViewportCenter).Should().BeSameAs(child, "picking must consider nested nodes");
    }
}
