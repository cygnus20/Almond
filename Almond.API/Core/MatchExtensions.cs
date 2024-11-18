using Almond.API.Models;

namespace Almond.API.Core;

public static class MatchExtensions
{
    public static Participant? GetWinner(this Match match)
    {
        int firstParticScore;
        bool ispart1ScoreInt = int.TryParse(match.Participant1?.Score, out firstParticScore);
        int secondParticScore;
        bool ispart2ScoreInt = int.TryParse(match.Participant2?.Score, out secondParticScore);

        if (!ispart1ScoreInt || !ispart2ScoreInt)
            return null;

        if (match.Participant2 is null || firstParticScore > secondParticScore)
        {
            return new Participant
            {
                Guid = match.Participant1?.Guid ?? Guid.Empty,
                Name = match.Participant1?.Name ?? ""

            };
        }

        else if (secondParticScore > firstParticScore) 
        {
            return new Participant
            {
                Guid = match.Participant2?.Guid ?? Guid.Empty,
                Name = match.Participant2?.Name ?? ""
            };
        }

        return null;
    }

    public static Participant? GetLoser(this Match match)
    {
        int firstParticScore = Convert.ToInt32(match.Participant1?.Score);
        int secondParticScore = Convert.ToInt32(match.Participant2?.Score);
        if (match.Participant2 is null || firstParticScore > secondParticScore)
        {
            return new Participant
            {
                Guid = match.Participant2?.Guid ?? Guid.Empty,
                Name = match?.Participant2?.Name ?? ""
            };
        }

        else if (secondParticScore > firstParticScore)
        {
            return new Participant
            {
                Guid = match.Participant1?.Guid ?? Guid.Empty,
                Name = match?.Participant1?.Name ?? ""
            };
        }

        return null;
    }
}
