using Microsoft.EntityFrameworkCore;
using RaceTrack.Db.Entities;

namespace RaceTrack.Db;

public class RaceTrackDbContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Lap> Laps { get; set; }
    public DbSet<Race> Races { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Filename=RaceTrack.db"); // Name of your SQLite database file
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Player>()
            .HasMany(p => p.Laps)
            .WithOne(l => l.Player)
            .HasForeignKey(l => l.PlayerId);
    
        modelBuilder.Entity<RacePlayer>()
            .HasKey(rp => new { rp.RaceId, rp.PlayerId }); // Composite primary key
    
        modelBuilder.Entity<RacePlayer>()
            .HasOne(rp => rp.Race)
            .WithMany(r => r.RacePlayers)
            .HasForeignKey(rp => rp.RaceId);
    
        modelBuilder.Entity<RacePlayer>()
            .HasOne(rp => rp.Player)
            .WithMany(p => p.RacePlayers)
            .HasForeignKey(rp => rp.PlayerId);
    }
}