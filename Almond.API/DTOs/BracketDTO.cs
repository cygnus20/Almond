using Almond.API.Models;

namespace Almond.API.DTOs;

public record BracketDTO(
    int Id, 
    Guid Guid,
    string Type,
    List<Round> Rounds);
