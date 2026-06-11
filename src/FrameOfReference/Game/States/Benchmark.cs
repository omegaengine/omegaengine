using System;
using FrameOfReference.Presentation;
using FrameOfReference.Presentation.Config;
using FrameOfReference.World;
using NLua;
using NanoByte.Common;
using OmegaEngine;

namespace FrameOfReference.States;

/// <summary>
/// State while running an automatic performance benchmark.
/// </summary>
public class Benchmark : SessionStateBase
{
    private readonly BenchmarkPresenter _presenter;

    public Benchmark(Game game, Action<string> onComplete)
        : base(game, new Session(Universe.FromContent($"Benchmark{Constants.MapFileExt}")))
    {
        _presenter = new(game.Engine, session.Universe, onComplete);
    }

    /// <inheritdoc/>
    [LuaHide]
    public override void Enter()
    {
        using (new TimedLogEvent("Start benchmark"))
        {
            _presenter.Initialize();
            InitializeLua();
            _presenter.HookIn();
            if (Settings.Current.Graphics.Fading) game.Engine.FadeIn();

            game.GuiManager.Reset();
            game.LoadDialog("InGame/HUD_Benchmark");
        }

        CleanCaches();
    }

    /// <inheritdoc/>
    [LuaHide]
    public override void Exit() => _presenter.HookOut();

    /// <inheritdoc/>
    [LuaHide]
    public override void Dispose() => _presenter.Dispose();
}
