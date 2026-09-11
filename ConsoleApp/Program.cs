var running = true;
 
while (running)
{
    DrawMenu("TIC-TAC-TOE");
    Console.Write("Enter your choice: ");
    var userChoice = Console.ReadLine();
 
    switch (userChoice?.Trim().ToUpper())
    {
        case "N":
            PlayGame();
            break;
        case "X":
            running = false;
            break;
        default:
            Console.WriteLine("Unknown choice, press any key...");
            Console.ReadKey(true);
            break;
    }
}
 
return;
 
static void DrawMenu(string title)
{
    Console.Clear();
    Console.WriteLine($"=== {title} ===");
    Console.WriteLine("N) New game");
    Console.WriteLine("X) Exit");
    Console.WriteLine();
}
 
static void PlayGame()
{
    // 3x3 board, ' ' means empty
    var board = new char[3, 3];
    for (var r = 0; r < 3; r++)
        for (var c = 0; c < 3; c++)
            board[r, c] = ' ';
 
    var currentPlayer = 'X';
    var movesMade = 0;
 
    while (true)
    {
        DrawBoard(board);
        Console.Write($"Player {currentPlayer}, enter cell (1-9): ");
        var input = Console.ReadLine();
 
        if (!int.TryParse(input, out var cell) || cell < 1 || cell > 9)
        {
            Console.WriteLine("Please enter a number from 1 to 9.");
            Console.ReadKey(true);
            continue;
        }
 
        var row = (cell - 1) / 3;
        var col = (cell - 1) % 3;
 
        if (board[row, col] != ' ')
        {
            Console.WriteLine("That cell is already taken.");
            Console.ReadKey(true);
            continue;
        }
 
        board[row, col] = currentPlayer;
        movesMade++;
 
        if (HasWon(board, currentPlayer))
        {
            DrawBoard(board);
            Console.WriteLine($"Player {currentPlayer} wins!");
            break;
        }
 
        if (movesMade == 9)
        {
            DrawBoard(board);
            Console.WriteLine("It's a draw!");
            break;
        }
 
        currentPlayer = currentPlayer == 'X' ? 'O' : 'X';
    }
 
    Console.WriteLine("Press any key to return to the menu...");
    Console.ReadKey(true);
}
 
static void DrawBoard(char[,] board)
{
    Console.Clear();
    Console.WriteLine("Cells are numbered 1-9, left to right, top to bottom.");
    Console.WriteLine();
 
    for (var r = 0; r < 3; r++)
    {
        Console.WriteLine($" {board[r, 0]} | {board[r, 1]} | {board[r, 2]} ");
        if (r < 2)
            Console.WriteLine("---+---+---");
    }
 
    Console.WriteLine();
}
 
static bool HasWon(char[,] b, char p)
{
    // rows and columns
    for (var i = 0; i < 3; i++)
    {
        if (b[i, 0] == p && b[i, 1] == p && b[i, 2] == p) return true;
        if (b[0, i] == p && b[1, i] == p && b[2, i] == p) return true;
    }
 
    // diagonals
    if (b[0, 0] == p && b[1, 1] == p && b[2, 2] == p) return true;
    if (b[0, 2] == p && b[1, 1] == p && b[2, 0] == p) return true;
 
    return false;
}
