using Almond.API.DTOs;
using Almond.API.Models;

namespace Almond.API.Core;

public static class ModelDTOMapExtension
{
    public static BracketDTO ToDTO(this Bracket bracket)
    {
        return new BracketDTO(bracket.Guid, bracket.Rounds);
    }

    public static RoundDTO ToDTO(this Round round)
    {
        return new RoundDTO(round.Matches);
    }
}
