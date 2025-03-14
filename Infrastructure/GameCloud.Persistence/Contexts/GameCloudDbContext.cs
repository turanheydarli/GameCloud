using GameCloud.Domain.Entities;
using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Entities.Rooms;
using GameCloud.Persistence.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Contexts
{
    public sealed class GameCloudDbContext(DbContextOptions<GameCloudDbContext> options)
        : IdentityDbContext<AppUser, AppRole, Guid>(options)
    {
        public DbSet<Developer> Developers { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameKey> GameKeys { get; set; }
        public DbSet<FirebaseProject> FirebaseProjects { get; set; }
        public DbSet<FunctionConfig> FunctionConfigs { get; set; }
        public DbSet<GameState> GameStates { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ImageDocument> ImageDocuments { get; set; }
        public DbSet<ImageVariant> ImageVariants { get; set; }
        public DbSet<GameActivity> GameActivities { get; set; }
        public DbSet<PlayerAttribute> Attributes { get; set; }
        public DbSet<MatchmakingQueue> MatchmakingQueues { get; set; }
        public DbSet<MatchTicket> MatchTickets { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchAction> MatchActions { get; set; }
        public DbSet<StoredMatch> StoredMatches { get; set; }
        public DbSet<StoredPlayer> StoredPlayers { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomConfig> RoomConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("gc");
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new AppUserEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AppRoleEntityConfiguration());
            modelBuilder.ApplyConfiguration(new GameEntityConfiguration());
            modelBuilder.ApplyConfiguration(new GameKeyEntityConfiguration());
            modelBuilder.ApplyConfiguration(new DeveloperEntityConfiguration());
            modelBuilder.ApplyConfiguration(new FirebaseProjectEntityConfiguration());
            modelBuilder.ApplyConfiguration(new FunctionConfigEntityConfiguration());
            modelBuilder.ApplyConfiguration(new GameStateEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ImageDocumentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ActionLogEntityConfiguration());
            modelBuilder.ApplyConfiguration(new StoredMatchEntityConfiguration());
            modelBuilder.ApplyConfiguration(new StoredPlayerEntityConfiguration());
            
            modelBuilder.ApplyConfiguration(new RoomEntityConfiguration());
            modelBuilder.ApplyConfiguration(new RoomConfigEntityConfiguration());
        }
    }
}