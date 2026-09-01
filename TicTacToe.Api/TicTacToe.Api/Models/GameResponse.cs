using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class GameResponse
{
    public Guid Id { get; set; }

    public Player?[] Board { get; set; } = new Player?[9];

    public Player CurrentPlayer { get; set; }

    public GameMode Mode { get; set; }

    public GameStatus Status { get; set; }

    public Player? Winner { get; set; }

    public List<int> WinningCells { get; set; } = new();

    public List<Move> Moves { get; set; } = new();
}