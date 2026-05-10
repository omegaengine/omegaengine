using FrameOfReference.World;

namespace FrameOfReference.States;

/// <summary>
/// State while a game session is being live-modified.
/// </summary>
public class Modify(Game game, Session session) : InGameBase(game, session)
{
    protected override string HudDialog => "InGame/HUD_Modify";
}
