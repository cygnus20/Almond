namespace Almond.API.Models;

public class Round
{
    public List<Match> Matches { get; set; } = new();
    public bool Current { get; set; } = false;
    public bool Concluded { get; set; } = false;
}
