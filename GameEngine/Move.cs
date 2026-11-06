namespace GameEngine;

/// <summary>
/// A cell on the board, 0-based. A named struct instead of an (int Row, int Col) tuple, because
/// System.Text.Json skips tuple fields and the move history would be saved as empty objects.
/// </summary>
public readonly record struct Move(int Row, int Col);
