using FrameOfReference.Presentation;
using FrameOfReference.Presentation.Config;
using FrameOfReference.World;
using NLua;
using NanoByte.Common;
using OmegaEngine;

namespace FrameOfReference.States;

/// <summary>
/// State while the main menu is shown.
/// </summary>
public class MainMenu(Game game, Universe universe) : IGameState
{
    private readonly MenuPresenter _presenter = new(game.Engine, universe);
    private readonly Savegames _savegames = new(game);

    /// <inheritdoc/>
    public void Enter()
    {
        using var _ = new TimedLogEvent($"Enter {nameof(MainMenu)} state");

        game.Loading = true;

        _presenter.Initialize();
        _presenter.HookIn();
        if (Settings.Current.Graphics.Fading) game.Engine.FadeIn();
        game.GuiManager.CloseAll();
        game.LoadModalDialog("MainMenu");
        game.Engine.Music.PlayTheme("MainMenu");

        game.Loading = false;
    }

    /// <inheritdoc/>
    public void Exit() => _presenter.HookOut();

    /// <inheritdoc/>
    public double GetElapsedGameTime(double elapsedTime) => elapsedTime;

    /// <inheritdoc/>
    public void BindLua(Lua lua)
    {
        lua["IsMainMenu"] = true;
        lua["Presenter"] = _presenter;
        lua["Savegames"] = _savegames;
    }

    /// <inheritdoc/>
    public void Dispose() => _presenter.Dispose();
}
