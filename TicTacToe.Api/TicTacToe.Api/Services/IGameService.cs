using TicTacToe.Api.DTOs;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    GameResponse CreateGame(CreateGameRequest request);

    GameResponse GetGame(Guid gameId);

    GameResponse MakeMove(Guid gameId, MakeMoveRequest request);

    GameResponse UndoLastMove(Guid gameId);

    GameResponse ResetGame(Guid gameId);

    ScoreboardResponse GetScoreboard();

    ScoreboardResponse ResetScoreboard();
}