using Almond.API.Core;
using Almond.API.Data;
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

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var brackets = await _context.Brackets.AsNoTracking().ToListAsync();
        return Ok(brackets);
    }

    [HttpGet("{guid}")]
    public IActionResult Get(Guid guid)
    {
        var bracket = _context.Brackets.AsNoTracking().SingleOrDefault(c => c.Guid == guid);

        if (bracket != null) 
        {
            return Ok(bracket);
        }
        return Problem(
            title: "Not found",
            detail: "Bracket does not exist",
            statusCode: StatusCodes.Status404NotFound);
    }

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

    [HttpPut("{guid}")]
    public async Task<IActionResult> Put(Guid guid, [FromBody] Round rounds) 
    {
        var bracket = _context.Brackets.SingleOrDefault(c => c.Guid == guid);
        //Round currentRound = new();
        // bracket = Brackets[id - 1];
        if (bracket != null)
        {
            
            var currentRound = bracket.Rounds.First(r => r.Current);
            int index = bracket.Rounds.IndexOf(currentRound);
            bracket.Rounds[index] = rounds;
            bracket.Update();

            await _context.SaveChangesAsync();
            return Ok(bracket);
        }

        return Problem(
            title: "Not found",
            detail: "Bracket does not exist",
            statusCode: StatusCodes.Status404NotFound);
    }

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
