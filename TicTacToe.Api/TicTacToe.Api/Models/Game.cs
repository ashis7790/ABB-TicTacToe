namespace TicTacToe.Api.Models;

public class Game
{
    public Guid Id { get; set; }

    public Player?[] Board { get; set; } = new Player?[9];

    public Player CurrentPlayer { get; set; } = Player.X;

    public GameStatus Status { get; set; } = GameStatus.InProgress;

    public GameMode Mode { get; set; } = GameMode.PlayerVsPlayer;

    public Player? Winner { get; set; }

    public List<int> WinningCells { get; set; } = new();

    public List<Move> Moves { get; set; } = new();
}