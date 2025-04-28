using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing;
using HallyuVault.Etl.Models;
using Microsoft.EntityFrameworkCore;

namespace HallyuVault.Etl.Infra
{
    // for the media
    public class DatabaseContext : DbContext
    {
        public DbSet<ScrapedDrama> ScrapedDramas;
        public DbSet<Media> Media;
        public DbSet<Season> Seasons;
        public DbSet<MediaVersion> MediaVersions;
        public DbSet<Episode> Episodes;
        public DbSet<StandardEpisode> StandardEpisodes;
        public DbSet<BatchEpisode> BatchEpisodes;
        public DbSet<SpecialEpisode> SpecialEpisodes;
        public DbSet<UnknownEpisode> UnknownEpisodes;
        public DbSet<ResolvedLink> ResolvedLinks;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure your entities here
        }
    }
}
