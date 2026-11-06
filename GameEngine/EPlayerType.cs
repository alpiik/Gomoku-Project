namespace GameEngine;

/// <summary>
/// Who plays one side. Saved with the game, so a loaded game continues with the same players.
/// Stored as text ("RandomAi"), so adding a value later (A6: minimax levels) can't break old saves.
/// </summary>
public enum EPlayerType
{
    Human,

    /// <summary>A random legal move (lecture 09.1 "level 0").</summary>
    RandomAi,

    /// <summary>Takes an immediate win when there is one, otherwise random (A5 bonus).</summary>
    GreedyAi,
}
