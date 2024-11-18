using Almond.API.Models;

namespace Almond.API.Core;

public static class BracketGenExtensions
{
    public static void Generate(this Bracket bracket)
    {
        Queue<Participant> participQueue = new Queue<Participant>();
        Stack<Participant> participStack = new Stack<Participant>();
        int participantCount = bracket.Participants.Count();
        double partLog = Math.Log2(bracket.Participants.Count());
        int roundsCount;

        // Check whether number of participants is a power of 2^n e.g 2, 4, 8, 16, 32, ...
        if (partLog % 1 == 0)
        {
            roundsCount = (int)partLog; // Number of rounds equals log base 2 of the number of participants
        }

        // Otherwise add 1 to the log base 2 of the number of participants and truncate the fraction part
        else
        {
            roundsCount = (int)partLog + 1;
        }

        // Number of participants that automatically advance to the next round without playing an opponent
        int byesCount = (int)Math.Pow(2, roundsCount) - participantCount;
        // The sum of the number of byes and the half of number of non-byes
        int queueParticCount = ((participantCount - byesCount) / 2) + byesCount;

        for (int i = 0; i < participantCount; i++)
        {
            if (i < queueParticCount)
            {
                // Add the byes and first half of non-byes participant to particpants queue
                participQueue.Enqueue(bracket.Participants[i]);
            }
             
            else
            {
                // Add the second half of non-byes participant to participants stack
                participStack.Push(bracket.Participants[i]);
            }
                
        }

        for (int i = 0; i < roundsCount; i++) 
        {
            bracket.Rounds.Add(new Round());
        }

        bracket.Rounds[0].Current = true;

        List<List<Match>> matches = Enumerable.Range(0, roundsCount).Select(m => new List<Match>()).ToList();

        for (int i = 0; i < roundsCount; i++)
        {
            /* Calculate number of matches per round by dividing 2^number of rounds by 2^round index + 1 
             * For example, for 4 rounds: 2^4 / 2^1 = 8, 2^4 / 2^2 = 4, 2^4 / 2^3 = 2, 2^4 / 2^4 = 1
             */
            int matchCount = (int)Math.Pow(2, roundsCount) / (int)Math.Pow(2, i+1);

            // First round in the list of rounds
            if (i == 0)
            {
                // Add byes to matches list i.e. matches with only one participant
                matches[i].AddRange(Enumerable.Range(0, byesCount).Select(m => new Match
                {
                    Participant1 = participQueue.Dequeue()
                }).ToList());

                // Add matches with two participants to the matches list
                matches[i].AddRange(Enumerable.Range(0, (participantCount - byesCount)/2).Select(m => new Match 
                { 
                    Participant1 = participQueue.Dequeue(),
                    Participant2 = participStack.Pop()
                }).ToList());
            }

            // Rounds after the first round
            else
            {
                // Add matches with no participants since the participants will be determined from the previous round
                matches[i] = Enumerable.Range(0, matchCount).Select(m => new Match()).ToList();
            }
            
        }
        
        // Add each list of matches to there respective rounds
        for (int i = 0; i < bracket.Rounds.Count(); i++)
        {

            bracket.Rounds[i].Matches = matches[i];
        }

        Stack<Match> stack = new Stack<Match>();

        // Push every second matches to a stack and remove them for the list of matches
        for (int i = 1; i < matches[0].Count(); i+=2)
        {
            stack.Push(matches[0][i]);
            matches[0].Remove(matches[0][i]);
        }

        // Add the matches back to the list from the stack, this way the order will the reversed
        foreach (int i in Enumerable.Range(0, stack.Count))
        {
            matches[0].Add(stack.Pop());
        }
    }

    public static void Update(this Bracket bracket)
    {
        Round currentRound = new();
        Round? nextRound = null;

        // For each round in bracket
        for (int i = 0; i < bracket.Rounds.Count; i++)
        {
            // Check for the current round
            if (bracket.Rounds[i].Current)
            {
                // Check whether current round is not the last round
                if (i != bracket.Rounds.Count - 1)
                    nextRound = bracket.Rounds[i + 1]; // Next round is the round after the current round
                //else 
                    //nextRound = currentRound; // Otherwise current round is the next round

                currentRound = bracket.Rounds[i];
                //bracket.Rounds[i].Current = false;
                // If there is a change in participants' scores, conclude the round and move to the next round
                if (currentRound.Matches.TrueForAll(m => m?.Participant1?.Score != "" && m?.Participant2?.Score != ""))
                {
                    currentRound.Concluded = true;
                    if (nextRound is not null)
                    {
                        currentRound.Current = false;
                        nextRound.Current = true;
                    }
                }

                break;

            }
        }
        // Step through the number of matches in the current round, 2 at a time
        for (int i = 0; i < currentRound.Matches.Count; i+=2)
        {
            // Ensure that this is not the final round
            if (nextRound != null && currentRound.Matches.Count > 1)
            {
                // Advance match winners in the current round to the next round
                nextRound.Matches[i/2].Participant1 = currentRound.Matches[i].GetWinner();
                nextRound.Matches[i/2].Participant2 = currentRound.Matches[i + 1].GetWinner();
            }
            
        }
    }

}
