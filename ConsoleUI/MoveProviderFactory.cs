using GameEngine;
using GameEngine.Ai;

namespace ConsoleUI;

/// <summary>
/// Turns a saved player type into the object that plays it. The only place that knows every type;
/// A6 adds its minimax levels here and nowhere else (the game loop stays as it is).
/// The <see cref="Random"/> comes from dependency injection: one shared instance for the whole run.
/// </summary>
public class MoveProviderFactory(ConsoleMoveProvider human, Random random)
{
    public IMoveProvider Create(EPlayerType type) => type switch
    {
        EPlayerType.Human => human,
        EPlayerType.RandomAi => new RandomAiMoveProvider(random),
        EPlayerType.GreedyAi => new GreedyAiMoveProvider(random),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown player type."),
    };
}
