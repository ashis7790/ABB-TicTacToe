using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class GameService : IGameService
{
    private readonly Dictionary<Guid, Game> _games = new();

    private readonly Dictionary<Guid, Stack<GameSnapshot>> _undoHistory = new();

    private readonly Scoreboard _scoreboard = new();

    public GameResponse CreateGame(CreateGameRequest request)
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Mode = request.Mode
        };

        _games[game.Id] = game;
        _undoHistory[game.Id] = new Stack<GameSnapshot>();

        return ToResponse(game);
    }

    public GameResponse GetGame(Guid gameId)
    {
        var game = GetGameOrThrow(gameId);

        return ToResponse(game);
    }

    public GameResponse MakeMove(Guid gameId, MakeMoveRequest request)
    {
        var game = GetGameOrThrow(gameId);

        ValidateMove(game, request);

        SaveSnapshot(game);

        MakeMoveInternal(
            game,
            request.Row,
            request.Column,
            game.CurrentPlayer);

        if (game.Status == GameStatus.InProgress)
        {
            SwitchPlayer(game);
        }

        // Computer plays as O
        if (game.Mode == GameMode.PlayerVsComputer &&
            game.Status == GameStatus.InProgress &&
            game.CurrentPlayer == Player.O)
        {
            MakeComputerMove(game);
        }

        return ToResponse(game);
    }

    public GameResponse UndoLastMove(Guid gameId)
    {
        var game = GetGameOrThrow(gameId);

        if (game.Moves.Count == 0)
        {
            throw new InvalidOperationException(
                "There is no move available to undo.");
        }

        if (game.Mode == GameMode.PlayerVsPlayer)
        {
            RestorePreviousState(game);
        }
        else
        {
            RestorePreviousState(game);

            if (game.Moves.Count > 0)
            {
                RestorePreviousState(game);
            }
        }

        return ToResponse(game);
    }

    private void RestorePreviousState(Game game)
    {
        if (!_undoHistory.TryGetValue(game.Id, out var history) ||
            history.Count == 0)
        {
            return;
        }

        var snapshot = history.Pop();

        RestoreSnapshot(game, snapshot);
    }
    public GameResponse ResetGame(Guid gameId)
    {
        var game = GetGameOrThrow(gameId);

        game.Board = new Player?[9];
        game.CurrentPlayer = Player.X;
        game.Status = GameStatus.InProgress;
        game.Winner = null;
        game.WinningCells.Clear();
        game.Moves.Clear();

        _undoHistory[gameId].Clear();

        return ToResponse(game);
    }

    private static GameResponse ToResponse(Game game)
    {
        return new GameResponse
        {
            Id = game.Id,
            Board = game.Board.ToArray(),
            CurrentPlayer = game.CurrentPlayer,
            Mode = game.Mode,
            Status = game.Status,
            Winner = game.Winner,
            WinningCells = game.WinningCells.ToList(),

            Moves = game.Moves
                .Select(m => new Move
                {
                    MoveNumber = m.MoveNumber,
                    Player = m.Player,
                    Row = m.Row,
                    Column = m.Column
                })
                .ToList()
        };
    }

    private void ValidateMove(Game game, MakeMoveRequest request)
    {
        if (game.Status != GameStatus.InProgress)
        {
            throw new InvalidOperationException(
                "The game has already been completed.");
        }

        if (request.Row < 0 || request.Row > 2)
        {
            throw new ArgumentException(
                "Row must be between 0 and 2.");
        }

        if (request.Column < 0 || request.Column > 2)
        {
            throw new ArgumentException(
                "Column must be between 0 and 2.");
        }

        var index = ToIndex(request.Row, request.Column);

        if (game.Board[index] != null)
        {
            throw new InvalidOperationException(
                "The selected cell is already occupied.");
        }

        // In computer mode, the human can only make X moves.
        if (game.Mode == GameMode.PlayerVsComputer &&
            game.CurrentPlayer != Player.X)
        {
            throw new InvalidOperationException(
                "It is not the player's turn.");
        }
    }

    private void MakeMoveInternal(
        Game game,
        int row,
        int column,
        Player player)
    {
        var index = ToIndex(row, column);

        game.Board[index] = player;

        game.Moves.Add(new Move
        {
            MoveNumber = game.Moves.Count + 1,
            Player = player,
            Row = row,
            Column = column
        });

        CheckGameStatus(game, player);
    }

    private void CheckGameStatus(Game game, Player player)
    {
        var winningLine = GetWinningLine(game.Board, player);

        if (winningLine != null)
        {
            game.Status = player == Player.X
                ? GameStatus.XWon
                : GameStatus.OWon;

            game.Winner = player;
            game.WinningCells = winningLine.ToList();

            if (player == Player.X)
            {
                _scoreboard.XWins++;
            }
            else
            {
                _scoreboard.OWins++;
            }

            return;
        }

        if (game.Board.All(cell => cell != null))
        {
            game.Status = GameStatus.Draw;
            game.Winner = null;
            game.WinningCells.Clear();

            _scoreboard.Draws++;
        }
    }

    private int[]? GetWinningLine(Player?[] board, Player player)
    {
        int[][] winningLines =
        {
            new[] { 0, 1, 2 },
            new[] { 3, 4, 5 },
            new[] { 6, 7, 8 },

            new[] { 0, 3, 6 },
            new[] { 1, 4, 7 },
            new[] { 2, 5, 8 },

            new[] { 0, 4, 8 },
            new[] { 2, 4, 6 }
        };

        foreach (var line in winningLines)
        {
            if (board[line[0]] == player &&
                board[line[1]] == player &&
                board[line[2]] == player)
            {
                return line;
            }
        }

        return null;
    }

    private void SwitchPlayer(Game game)
    {
        game.CurrentPlayer =
            game.CurrentPlayer == Player.X
                ? Player.O
                : Player.X;
    }

    private void MakeComputerMove(Game game)
    {
        var move = FindBestComputerMove(game.Board);

        if (move == null)
        {
            return;
        }

        MakeMoveInternal(
            game,
            move.Value.row,
            move.Value.column,
            Player.O);

        if (game.Status == GameStatus.InProgress)
        {
            SwitchPlayer(game);
        }
    }

    private (int row, int column)? FindBestComputerMove(Player?[] board)
    {
        // 1. Try to win.
        var winningMove = FindWinningMove(board, Player.O);

        if (winningMove != null)
        {
            return winningMove;
        }

        // 2. Block the player.
        var blockingMove = FindWinningMove(board, Player.X);

        if (blockingMove != null)
        {
            return blockingMove;
        }

        // 3. Take the center.
        if (board[4] == null)
        {
            return (1, 1);
        }

        // 4. Take a corner.
        int[] corners = { 0, 2, 6, 8 };

        foreach (var corner in corners)
        {
            if (board[corner] == null)
            {
                return (corner / 3, corner % 3);
            }
        }

        // 5. Take any available cell.
        for (var index = 0; index < board.Length; index++)
        {
            if (board[index] == null)
            {
                return (index / 3, index % 3);
            }
        }

        return null;
    }

    private (int row, int column)? FindWinningMove(
        Player?[] board,
        Player player)
    {
        for (var index = 0; index < board.Length; index++)
        {
            if (board[index] != null)
            {
                continue;
            }

            board[index] = player;

            var winningLine = GetWinningLine(board, player);

            board[index] = null;

            if (winningLine != null)
            {
                return (index / 3, index % 3);
            }
        }

        return null;
    }

    private void SaveSnapshot(Game game)
    {
        var snapshot = new GameSnapshot
        {
            Board = game.Board.ToArray(),
            CurrentPlayer = game.CurrentPlayer,
            Status = game.Status,
            Moves = game.Moves
                .Select(m => new Move
                {
                    MoveNumber = m.MoveNumber,
                    Player = m.Player,
                    Row = m.Row,
                    Column = m.Column
                })
                .ToList(),
          
        };

        _undoHistory[game.Id].Push(snapshot);
    }

    private void RestoreSnapshot(
        Game game,
        GameSnapshot snapshot)
    {
        game.Board = snapshot.Board.ToArray();
        game.CurrentPlayer = snapshot.CurrentPlayer;
        game.Status = snapshot.Status;

        game.Moves = snapshot.Moves
            .Select(m => new Move
            {
                MoveNumber = m.MoveNumber,
                Player = m.Player,
                Row = m.Row,
                Column = m.Column
            })
            .ToList();

    }
    private Game GetGameOrThrow(Guid gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            throw new KeyNotFoundException(
                $"Game '{gameId}' was not found.");
        }

        return game;
    }

    private static int ToIndex(int row, int column)
    {
        return row * 3 + column;
    }



    private class GameSnapshot
    {
        public Player?[] Board { get; set; } = new Player?[9];

        public Player CurrentPlayer { get; set; }

        public GameStatus Status { get; set; }

        public List<Move> Moves { get; set; } = new();
    
    }

    public ScoreboardResponse GetScoreboard()
    {
        return new ScoreboardResponse
        {
            XWins = _scoreboard.XWins,
            OWins = _scoreboard.OWins,
            Draws = _scoreboard.Draws
        };
    }

    // 
    public ScoreboardResponse ResetScoreboard()
    {
        _scoreboard.XWins = 0 ; 
        _scoreboard.OWins = 0;
        _scoreboard.Draws = 0;

        return GetScoreboard();
    }
}