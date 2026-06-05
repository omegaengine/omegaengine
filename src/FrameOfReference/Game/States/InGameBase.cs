using FrameOfReference.Presentation;
using FrameOfReference.Presentation.Config;
using FrameOfReference.World;
using JetBrains.Annotations;
using LuaInterface;
using NanoByte.Common;
using OmegaEngine;

namespace FrameOfReference.States;

/// <summary>
/// Common base for states with an active <see cref="InGamePresenter"/> and pause support.
/// </summary>
public abstract class InGameBase : SessionStateBase
{
    protected readonly InGamePresenter _presenter;
    private readonly Savegames _savegames;

    protected InGameBase(Game game, Session session) : base(game, session)
    {
        _presenter = new(game.Engine, session.Universe);
        _savegames = new(game, session, () => _presenter.PrepareSave());
    }

    private bool _isPaused;

    protected abstract string HudDialog { get; }

    /// <inheritdoc/>
    [LuaHide]
    public override void Enter()
    {
        game.Loading = true;

        using (new TimedLogEvent("Initialize game"))
        {
            _presenter.Initialize();
            OnPresenterInitialized();
            game.AddInputReceiver(_presenter);
            _presenter.HookIn();
            if (Settings.Current.Graphics.Fading) game.Engine.FadeIn();

            game.GuiManager.Reset();
            game.LoadDialog(HudDialog);
        }

        CleanCaches();

        game.Loading = false;
        _isPaused = false;
        InitializeLua();
    }

    protected virtual void OnPresenterInitialized() {}

    /// <inheritdoc/>
    [LuaHide]
    public override void Exit()
    {
        game.RemoveInputReceiver(_presenter);
        _presenter.HookOut();
        _savegames.SaveAsResume();
    }

    /// <summary>
    /// Toggles between gameplay and the pause menu.
    /// </summary>
    [UsedImplicitly]
    public void TogglePause()
    {
        if (_isPaused)
        {
            _isPaused = false;
            _presenter.DimUp();
            game.AddInputReceiver(_presenter);
            game.GuiManager.Reset();
            game.LoadDialog(HudDialog);
        }
        else
        {
            _isPaused = true;
            game.RemoveInputReceiver(_presenter);
            _presenter.DimDown();
            game.GuiManager.Reset();
            game.LoadModalDialog("PauseMenu");
        }
    }

    /// <inheritdoc/>
    [LuaHide]
    public override double GetElapsedGameTime(double elapsedTime)
        => _isPaused ? elapsedTime / 10 : base.GetElapsedGameTime(elapsedTime);

    /// <inheritdoc/>
    [LuaHide]
    public override void BindLua(Lua lua)
    {
        base.BindLua(lua);

        lua["InGame"] = this;
        lua["IsPaused"] = _isPaused;

        lua["Presenter"] = _presenter;
        lua["Savegames"] = _savegames;
    }

    /// <inheritdoc/>
    [LuaHide]
    public override void Dispose() => _presenter.Dispose();
}
