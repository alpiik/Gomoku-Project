namespace GameEngine;

/// <summary>Thrown by <see cref="GameBrain.MakeMove"/> when it is asked to make an illegal move.</summary>
public class InvalidMoveException : Exception
{
    public InvalidMoveException(int row, int col, string message)
        : base($"Move ({row}, {col}) rejected: {message}")
    {
        Row = row;
        Col = col;
    }

    public int Row { get; }
    public int Col { get; }
}
