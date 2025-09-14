namespace FrameOfReference;

/// <seealso cref="Game.CurrentState"/>
public enum GameState
{
    /// <summary>The game is starting up</summary>
    Init,

    /// <summary>The game is in the main menu</summary>
    Menu,

    /// <summary>The game is paused</summary>
    Pause,

    /// <summary>The game is running an automatic benchmark</summary>
    Benchmark,

    /// <summary>The game is normal playing mode</summary>
    InGame,

    /// <summary>The game is in a special live editing mode</summary>
    Modify
}
