using System.ComponentModel.DataAnnotations;

namespace CoolCardGames.WebApi.Endpoints.GameSession;

public class GameSessionPlayCardsRequest
{
    [Required] public List<int> IndexesOfCardsToPlay { get; set; } = [];
}