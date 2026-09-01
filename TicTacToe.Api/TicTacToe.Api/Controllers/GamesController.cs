using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    public ActionResult<GameResponse> CreateGame(
        [FromBody] CreateGameRequest request)
    {
        var game = _gameService.CreateGame(request);

        return Ok(game);
    }

    [HttpGet("{gameId:guid}")]
    public ActionResult<GameResponse> GetGame(Guid gameId)
    {
        var game = _gameService.GetGame(gameId);

        return Ok(game);
    }

    [HttpPost("{gameId}/moves")]
    public ActionResult<GameResponse> MakeMove(
        Guid gameId,
        MakeMoveRequest request)
    {
        try
        {
            var game = _gameService.MakeMove(gameId, request);

            return Ok(game);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{gameId:guid}/undo")]
    public ActionResult<GameResponse> UndoLastMove(Guid gameId)
    {
        var game = _gameService.UndoLastMove(gameId);

        return Ok(game);
    }

    [HttpPost("{gameId:guid}/reset")]
    public ActionResult<GameResponse> ResetGame(Guid gameId)
    {
        var game = _gameService.ResetGame(gameId);

        return Ok(game);
    }
}