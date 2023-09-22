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
        optionsBuilder.UseSqlite("Filename=C:\\coding\\RaceTrack\\RaceTrack\\RaceTrack.Db\\RaceTrack.db"); // Name of your SQLite database file
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>()
            .HasMany(p => p.Laps)
            .WithOne(l => l.Player)
            .HasForeignKey(l => l.PlayerId);
        modelBuilder.Entity<Player>().HasKey(x => x.Id);
    
        modelBuilder.Entity<Race>().HasKey(x => x.Id);
        
        modelBuilder.Entity<Lap>().HasKey(x => x.Id);
        
        base.OnModelCreating(modelBuilder);
    }
}