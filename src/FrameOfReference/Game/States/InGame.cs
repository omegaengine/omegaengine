using System.Linq;
using FrameOfReference.World;
using FrameOfReference.World.Positionables;

namespace FrameOfReference.States;

/// <summary>
/// State while a game session is active.
/// </summary>
public class InGame(Game game, Session session) : InGameBase(game, session)
{
    protected override string HudDialog => "InGame/HUD";

    protected override void OnPresenterInitialized()
    {
        if (session.Universe.Positionables.OfType<Entity>().FirstOrDefault(x => x.IsPlayerControlled) is { Name: { } playerEntity })
            _presenter.LockOn(playerEntity);
    }
}
