using Microsoft.EntityFrameworkCore;
using ImbroglioCombatAPI.Models;

namespace ImbroglioCombatAPI.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        public DbSet<GameState> GameStates { get; set; }
        public DbSet<UnitState> UnitStates { get; set; }
        public DbSet<LeaderboardEntry> LeaderboardEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<GameState>()
                .HasMany(g => g.Units)
                .WithOne()
                .HasForeignKey(u => u.GameStateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            modelBuilder.Entity<GameState>()
                .HasIndex(g => g.PlayerId);

            modelBuilder.Entity<LeaderboardEntry>()
                .HasIndex(l => l.Score);
        }
    }
}

