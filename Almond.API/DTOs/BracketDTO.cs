using Almond.API.Models;

namespace Almond.API.DTOs;

public record BracketDTO(
    Guid Guid,
    List<Round> Rounds);
