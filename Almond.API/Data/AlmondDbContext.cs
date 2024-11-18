using Almond.API.Core;
using Almond.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Almond.API.Data;

public class AlmondDbContext(DbContextOptions<AlmondDbContext> options, IGetUserClaims claims) : IdentityDbContext(options)
{
    private readonly IGetUserClaims _claims = claims;
    public DbSet<Bracket> Brackets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.Entity<Bracket>()
            .HasQueryFilter(b => b.UserId == _claims.UserId)
            .OwnsMany(b => b.Participants, p => p.ToJson())
            .OwnsMany(b => b.Rounds, r => 
            {
                r.ToJson();
                r.OwnsMany(r => r.Matches, m =>
                {
                    m.OwnsOne(m => m.Participant1);
                    m.OwnsOne(m => m.Participant2);
                });
            })
            .HasIndex(_ => _.Guid)
            .IsUnique();
    }
}
