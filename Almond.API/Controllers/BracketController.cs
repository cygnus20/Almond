using Almond.API.Core;
using Almond.API.Data;
using Almond.API.DTOs;
using Almond.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Almond.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BracketController : ControllerBase
{
    private readonly AlmondDbContext _context;
    private readonly IGetUserClaims _userClaims;

    public BracketController(AlmondDbContext context, IGetUserClaims userClaims)
    {
        _context = context;
        _userClaims = userClaims;
    }
    public static List<Bracket> Brackets = new List<Bracket>();
    /// <summary>
    /// Get bracktets
    /// </summary>
    /// <returns>A list of bracktets</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var brackets = await _context.Brackets.AsNoTracking().Select(b => b.ToDTO()).ToListAsync();
        return Ok(brackets);
    }
    /// <summary>
    /// Get a bracket of the specified guid
    /// </summary>
    /// <param name="guid">The guid of the bracket</param>
    /// <returns>A bracket of the guid provided</returns>
    /// <remarks>
    /// Sample request:
    ///     GET /api/bracket/3fa85f64-5717-4562-b3fc-2c963f66afa4
    /// </remarks>
    /// <response code="200">Bracket has been returned successfully</response>
    /// <response code="404">Bracket does not exist</response>
    [HttpGet("{guid}")]
    public IActionResult Get(Guid guid)
    {
        var bracket = _context.Brackets.AsNoTracking().SingleOrDefault(c => c.Guid == guid)?.ToDTO();

        if (bracket != null) 
        {
            return Ok(bracket);
        }
        return Problem(
            title: "Not found",
            detail: "Bracket does not exist",
            statusCode: StatusCodes.Status404NotFound);
    }
    /// <summary>
    /// Creates a new bracket from names of participants provided
    /// </summary>
    /// <param name="names">A list of names of participant in the bracket</param>
    /// <returns>A newly created bracket</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/bracket
    ///     [
    ///         "name 1", "name 2", "name 3", "name 4"
    ///     ]
    /// </remarks>
    /// <response code="201">Returns the newly created bracket</response>
    [HttpPost]
    public async Task<IActionResult> Post(List<string> names)
    {
        Bracket bracket = new Bracket 
        { 
            Guid = Guid.NewGuid(),
            UserId = _userClaims.UserId,
            Participants = Enumerable.Range(0, names.Count)
            .Select(p => new Participant { Guid = Guid.NewGuid(), Name = names[p] }).ToList()
        };
        bracket.Generate();

        _context.Add(bracket);
        await _context.SaveChangesAsync();

        return Created(nameof(BracketController), bracket);
    }

    /// <summary>
    /// Updates the current round of the bracket guid specified and progress to the next round
    /// </summary>
    /// <param name="guid">The guid of the bracket to be updated</param>
    /// <param name="round">Current round to be updated</param>
    /// <returns></returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/bracket/3fa85f64-5717-4562-b3fc-2c963f66afa4
    ///     {
    ///         "matches": [
    ///             {
    ///                 "participant1": {
    ///                     "guid": "3fa85f64-5717-4562-b3fc-2c963f66afa5",
    ///                     "name": "name 1",
    ///                     "score": "3"
    ///                 },
    ///                 "participant2": {
    ///                     "guid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///                     "name": "name 2",
    ///                     "score": "1"
    ///                 }
    ///             }
    ///         ]
    ///     }
    /// </remarks>
    /// <response code="200">Bracket has been updated successfully</response>
    /// <response code="404">Bracket does not exist</response>
    [HttpPut("{guid}")]
    public async Task<IActionResult> Put(Guid guid, [FromBody] RoundDTO round) 
    {
        var bracket = _context.Brackets.SingleOrDefault(c => c.Guid == guid);
        //Round currentRound = new();
        // bracket = Brackets[id - 1];
        if (bracket != null)
        {
            
            var currentRound = bracket.Rounds.First(r => r.Current);
            int index = bracket.Rounds.IndexOf(currentRound);
            bracket.Rounds[index].Matches = round.Matches;
            bracket.Update();

            _context.Update(bracket);
            await _context.SaveChangesAsync();
            return Ok(bracket);
        }

        return Problem(
            title: "Not found",
            detail: "Bracket does not exist",
            statusCode: StatusCodes.Status404NotFound);
    }
    /// <summary>
    /// Deletes the bracket of the specified guid
    /// </summary>
    /// <param name="guid">The guid of the bracket to be deleted</param>
    /// <returns>No content</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     DELETE /api/bracket/3fa85f64-5717-4562-b3fc-2c963f66afa4
    /// </remarks>
    /// <response code="200">Bracket has been deleted successfully</response>
    /// <response code="404">Bracket does not exist</response>
    [HttpDelete("{guid}")]
    public async Task<IActionResult> Delete(Guid guid)
    {
        if (await _context.Brackets.FirstOrDefaultAsync(b => b.Guid == guid) is Bracket bracket)
        {
            _context.Brackets.Remove(bracket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        return Problem(
            title: "Not found", 
            detail: "Bracket does not exist", 
            statusCode: StatusCodes.Status404NotFound);
    }

}
