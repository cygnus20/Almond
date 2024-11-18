using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Almond.API.Models;

public class Bracket
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? UserId { get; set; }
    public Guid Guid { get; set; }
    public string Type { get; set; } = "";
    public List<Participant> Participants { get; set; } = new();
    public List<Round> Rounds { get; set; } = new();
}
