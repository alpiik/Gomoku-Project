namespace GameEngine;

/// <summary>
/// How a game is set up. A record: immutable, compared by value, changed by copying with <c>with</c>.
/// </summary>
public record GameConfiguration(string Name, int BoardWidth, int BoardHeight, int WinLength)
{
    public const int MinBoardSize = 9;
    public const int MaxBoardSize = 19;
    public const int MinWinLength = 4;
    public const int MaxWinLength = 6;
    public const int MaxNameLength = 64;

    /// <summary>
    /// Overline rule. True: only a line of exactly <see cref="WinLength"/> wins (standard Gomoku).
    /// False: a line of <see cref="WinLength"/> or more wins (free-style).
    /// </summary>
    public bool ExactWinLength { get; init; }

    /// <summary>
    /// Pro opening. The first move must be in the centre and the first player's second move
    /// must be at least <see cref="ProOpeningDistance"/> cells away from the centre.
    /// </summary>
    public bool ProOpening { get; init; }

    public const int ProOpeningDistance = 3;

    /// <summary>Returns every problem at once, so the UI can show them all. Empty list = valid.</summary>
    public List<string> Validate()
    {
        List<string> errors = [];
        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Name is required.");
        }
        else if (Name.Length > MaxNameLength)
        {
            errors.Add($"Name can be at most {MaxNameLength} characters.");
        }
        else if (!IsValidName(Name))
        {
            errors.Add("Name can only contain letters, digits, spaces, '-' and '_', and cannot start or end with a space.");
        }

        if (BoardWidth is < MinBoardSize or > MaxBoardSize)
        {
            errors.Add($"Board width must be {MinBoardSize}..{MaxBoardSize}.");
        }

        if (BoardHeight is < MinBoardSize or > MaxBoardSize)
        {
            errors.Add($"Board height must be {MinBoardSize}..{MaxBoardSize}.");
        }

        if (WinLength is < MinWinLength or > MaxWinLength)
        {
            errors.Add($"Winning length must be {MinWinLength}..{MaxWinLength}.");
        }

        return errors;
    }

    /// <summary>
    /// Letters, digits, spaces, '-' and '_', no space at either end. Such a name is also a valid
    /// file name on Windows, macOS and Linux, so the JSON storage can use it as the file name.
    /// </summary>
    public static bool IsValidName(string name) =>
        name.Length is > 0 and <= MaxNameLength
        && name.Trim() == name
        && name.All(c => char.IsLetterOrDigit(c) || c is ' ' or '-' or '_');
}
