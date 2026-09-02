using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Api.Tests;

public class GameServiceTests
{
    // ============================================================
    // GAME CREATION
    // ============================================================

    [Fact]
    public void CreateGame_ShouldCreateNewGame()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(GameMode.PlayerVsPlayer, game.Mode);
        Assert.Empty(game.Moves);
        Assert.Equal(9, game.Board.Length);

        Assert.All(
            game.Board,
            cell => Assert.Null(cell));
    }


    [Fact]
    public void CreateGame_PlayerVsComputer_ShouldSetComputerMode()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsComputer
            });

        Assert.Equal(
            GameMode.PlayerVsComputer,
            game.Mode);

        Assert.Equal(
            Player.X,
            game.CurrentPlayer);

        Assert.Equal(
            GameStatus.InProgress,
            game.Status);
    }


    // ============================================================
    // VALID MOVE
    // ============================================================

    [Fact]
    public void MakeMove_ShouldPlacePlayerMove()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        var result = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        Assert.Equal(
            Player.X,
            result.Board[0]);

        Assert.Single(result.Moves);

        Assert.Equal(
            1,
            result.Moves[0].MoveNumber);

        Assert.Equal(
            Player.X,
            result.Moves[0].Player);

        Assert.Equal(
            0,
            result.Moves[0].Row);

        Assert.Equal(
            0,
            result.Moves[0].Column);
    }


    // ============================================================
    // TURN SWITCHING
    // ============================================================

    [Fact]
    public void MakeMove_ShouldSwitchPlayerFromXToO()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        var result = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        Assert.Equal(
            Player.O,
            result.CurrentPlayer);
    }


    [Fact]
    public void MakeMove_ShouldAlternateBetweenXAndO()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        var afterX = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        var afterO = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 0
            });

        Assert.Equal(
            Player.O,
            afterX.CurrentPlayer);

        Assert.Equal(
            Player.X,
            afterO.CurrentPlayer);

        Assert.Equal(
            2,
            afterO.Moves.Count);

        Assert.Equal(
            Player.X,
            afterO.Moves[0].Player);

        Assert.Equal(
            Player.O,
            afterO.Moves[1].Player);
    }


    // ============================================================
    // INVALID MOVES
    // ============================================================

    [Fact]
    public void MakeMove_OnOccupiedCell_ShouldThrowException()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        var exception = Assert.Throws<InvalidOperationException>(
            () =>
                service.MakeMove(
                    game.Id,
                    new MakeMoveRequest
                    {
                        Row = 0,
                        Column = 0
                    }));

        Assert.Equal(
            "The selected cell is already occupied.",
            exception.Message);
    }


    [Fact]
    public void MakeMove_WithInvalidRow_ShouldThrowException()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        var exception = Assert.Throws<ArgumentException>(
            () =>
                service.MakeMove(
                    game.Id,
                    new MakeMoveRequest
                    {
                        Row = 3,
                        Column = 0
                    }));

        Assert.Equal(
            "Row must be between 0 and 2.",
            exception.Message);
    }


    [Fact]
    public void MakeMove_WithNegativeRow_ShouldThrowException()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        var exception = Assert.Throws<ArgumentException>(
            () =>
                service.MakeMove(
                    game.Id,
                    new MakeMoveRequest
                    {
                        Row = -1,
                        Column = 0
                    }));

        Assert.Equal(
            "Row must be between 0 and 2.",
            exception.Message);
    }


    [Fact]
    public void MakeMove_WithInvalidColumn_ShouldThrowException()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        var exception = Assert.Throws<ArgumentException>(
            () =>
                service.MakeMove(
                    game.Id,
                    new MakeMoveRequest
                    {
                        Row = 0,
                        Column = 3
                    }));

        Assert.Equal(
            "Column must be between 0 and 2.",
            exception.Message);
    }


    [Fact]
    public void MakeMove_WithNegativeColumn_ShouldThrowException()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        var exception = Assert.Throws<ArgumentException>(
            () =>
                service.MakeMove(
                    game.Id,
                    new MakeMoveRequest
                    {
                        Row = 0,
                        Column = -1
                    }));

        Assert.Equal(
            "Column must be between 0 and 2.",
            exception.Message);
    }


    // ============================================================
    // ROW WIN
    // ============================================================

    [Fact]
    public void RowWin_ShouldSetWinnerAndWinningCells()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        // X -> 0
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        // O -> 3
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 0
            });

        // X -> 1
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 1
            });

        // O -> 4
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 1
            });

        // X -> 2
        var result = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 2
            });

        Assert.Equal(
            GameStatus.XWon,
            result.Status);

        Assert.Equal(
            Player.X,
            result.Winner);

        Assert.Equal(
            new[] { 0, 1, 2 },
            result.WinningCells);
    }


    // ============================================================
    // COLUMN WIN
    // ============================================================

    [Fact]
    public void ColumnWin_ShouldSetWinnerAndWinningCells()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        // X -> 0
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        // O -> 1
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 1
            });

        // X -> 3
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 0
            });

        // O -> 4
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 1
            });

        // X -> 6
        var result = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 2,
                Column = 0
            });

        Assert.Equal(
            GameStatus.XWon,
            result.Status);

        Assert.Equal(
            Player.X,
            result.Winner);

        Assert.Equal(
            new[] { 0, 3, 6 },
            result.WinningCells);
    }


    // ============================================================
    // DIAGONAL WIN
    // ============================================================

    [Fact]
    public void DiagonalWin_ShouldSetWinnerAndWinningCells()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        // X -> 0
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        // O -> 1
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 1
            });

        // X -> 4
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 1
            });

        // O -> 2
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 2
            });

        // X -> 8
        var result = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 2,
                Column = 2
            });

        Assert.Equal(
            GameStatus.XWon,
            result.Status);

        Assert.Equal(
            Player.X,
            result.Winner);

        Assert.Equal(
            new[] { 0, 4, 8 },
            result.WinningCells);
    }


    // ============================================================
    // DRAW
    // ============================================================

    [Fact]
    public void FullBoardWithoutWinner_ShouldResultInDraw()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        // 0 X
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        // 1 O
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 1
            });

        // 2 X
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 2
            });

        // 4 O
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 1
            });

        // 3 X
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 0
            });

        // 5 O
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 2
            });

        // 7 X
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 2,
                Column = 1
            });

        // 6 O
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 2,
                Column = 0
            });

        // 8 X
        var result = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 2,
                Column = 2
            });

        Assert.Equal(
            GameStatus.Draw,
            result.Status);

        Assert.Null(result.Winner);

        Assert.Empty(
            result.WinningCells);

        Assert.Equal(
            9,
            result.Moves.Count);
    }


    // ============================================================
    // MOVE AFTER COMPLETION
    // ============================================================

    [Fact]
    public void MoveAfterGameCompletion_ShouldThrowException()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 0
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 1
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 1
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 2
            });

        var exception = Assert.Throws<InvalidOperationException>(
            () =>
                service.MakeMove(
                    game.Id,
                    new MakeMoveRequest
                    {
                        Row = 2,
                        Column = 2
                    }));

        Assert.Equal(
            "The game has already been completed.",
            exception.Message);
    }


    // ============================================================
    // RESET GAME
    // ============================================================

    [Fact]
    public void ResetGame_ShouldClearBoardAndHistory()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 1
            });

        var result =
            service.ResetGame(game.Id);

        Assert.All(
            result.Board,
            cell => Assert.Null(cell));

        Assert.Empty(
            result.Moves);

        Assert.Equal(
            Player.X,
            result.CurrentPlayer);

        Assert.Equal(
            GameStatus.InProgress,
            result.Status);

        Assert.Null(
            result.Winner);

        Assert.Empty(
            result.WinningCells);
    }


    // ============================================================
    // UNDO - TWO PLAYER
    // ============================================================

    [Fact]
    public void UndoInTwoPlayerMode_ShouldRemoveLastMove()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        // X
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        // O
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 1
            });

        var result =
            service.UndoLastMove(game.Id);

        Assert.Single(
            result.Moves);

        Assert.Equal(
            Player.X,
            result.Board[0]);

        Assert.Null(
            result.Board[4]);

        Assert.Equal(
            Player.O,
            result.CurrentPlayer);
    }


    [Fact]
    public void UndoWithNoMoves_ShouldThrowException()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => service.UndoLastMove(game.Id));

        Assert.Equal(
            "There is no move available to undo.",
            exception.Message);
    }


    // ============================================================
    // COMPUTER MODE
    // ============================================================

    [Fact]
    public void ComputerMode_ShouldAutomaticallyMakeOMove()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsComputer
            });

        var result = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        Assert.Equal(
            Player.X,
            result.Board[0]);

        Assert.Contains(
            Player.O,
            result.Board);

        Assert.Equal(
            Player.X,
            result.CurrentPlayer);

        Assert.Equal(
            2,
            result.Moves.Count);
    }


    [Fact]
    public void ComputerMode_ShouldTakeCenterWhenAvailable()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsComputer
            });

        var result = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        Assert.Equal(
            Player.X,
            result.Board[0]);

        Assert.Equal(
            Player.O,
            result.Board[4]);
    }


    [Fact]
    public void ComputerMode_ShouldBlockXWinningMove()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsComputer
            });

        // X -> 0
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        // X -> 1
        // Computer should block position 2.
        var result = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 1
            });

        Assert.Equal(
            Player.X,
            result.Board[0]);

        Assert.Equal(
            Player.X,
            result.Board[1]);

        Assert.Equal(
            Player.O,
            result.Board[2]);
    }


    [Fact]
    public void ComputerMode_ShouldMakeOnlyValidMoves()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsComputer
            });

        var result = service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        var occupiedCells =
            result.Board.Count(
                cell => cell != null);

        Assert.Equal(
            2,
            occupiedCells);

        Assert.Equal(
            2,
            result.Moves.Count);
    }


    [Fact]
    public void UndoInComputerMode_ShouldRemoveHumanAndComputerMoves()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsComputer
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        var result =
            service.UndoLastMove(game.Id);

        Assert.Empty(
            result.Moves);

        Assert.All(
            result.Board,
            cell => Assert.Null(cell));

        Assert.Equal(
            Player.X,
            result.CurrentPlayer);

        Assert.Equal(
            GameStatus.InProgress,
            result.Status);
    }


    // ============================================================
    // SCOREBOARD
    // ============================================================

    [Fact]
    public void Scoreboard_ShouldIncreaseWhenXWins()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        // X wins.
        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 0
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 0
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 1
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 1,
                Column = 1
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest
            {
                Row = 0,
                Column = 2
            });

        var scoreboard =
            service.GetScoreboard();

        Assert.Equal(
            1,
            scoreboard.XWins);

        Assert.Equal(
            0,
            scoreboard.OWins);

        Assert.Equal(
            0,
            scoreboard.Draws);
    }


    [Fact]
    public void Scoreboard_ShouldIncreaseForDraw()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 0, Column = 0 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 0, Column = 1 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 0, Column = 2 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 1, Column = 1 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 1, Column = 0 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 1, Column = 2 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 2, Column = 1 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 2, Column = 0 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 2, Column = 2 });

        var scoreboard =
            service.GetScoreboard();

        Assert.Equal(
            0,
            scoreboard.XWins);

        Assert.Equal(
            0,
            scoreboard.OWins);

        Assert.Equal(
            1,
            scoreboard.Draws);
    }


    [Fact]
    public void ResetScoreboard_ShouldResetAllScores()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        // X wins.
        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 0, Column = 0 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 1, Column = 0 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 0, Column = 1 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 1, Column = 1 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 0, Column = 2 });

        var result =
            service.ResetScoreboard();

        Assert.Equal(
            0,
            result.XWins);

        Assert.Equal(
            0,
            result.OWins);

        Assert.Equal(
            0,
            result.Draws);
    }


    // ============================================================
    // RESET GAME SHOULD NOT RESET SCOREBOARD
    // ============================================================

    [Fact]
    public void ResetGame_ShouldKeepScoreboardUnchanged()
    {
        var service = new GameService();

        var game = service.CreateGame(
            new CreateGameRequest
            {
                Mode = GameMode.PlayerVsPlayer
            });

        // X wins.
        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 0, Column = 0 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 1, Column = 0 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 0, Column = 1 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 1, Column = 1 });

        service.MakeMove(
            game.Id,
            new MakeMoveRequest { Row = 0, Column = 2 });

        var beforeReset =
            service.GetScoreboard();

        service.ResetGame(
            game.Id);

        var afterReset =
            service.GetScoreboard();

        Assert.Equal(
            beforeReset.XWins,
            afterReset.XWins);

        Assert.Equal(
            beforeReset.OWins,
            afterReset.OWins);

        Assert.Equal(
            beforeReset.Draws,
            afterReset.Draws);
    }
}