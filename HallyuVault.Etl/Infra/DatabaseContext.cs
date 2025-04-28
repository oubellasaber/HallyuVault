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
            // Media
            modelBuilder.Entity<Media>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.KoreanTitle).IsRequired(false);
                entity.Property(m => m.EnglishTitle).IsRequired();

                entity.HasMany(m => m.Seasons)
                      .WithOne()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seasons
            modelBuilder.Entity<Season>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasMany(s => s.MediaVersions)
                      .WithOne()
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure Id to be generated on add
                entity.Property(s => s.Id)
                      .ValueGeneratedOnAdd(); // auto-increment
            });

            // MediaVersion
            modelBuilder.Entity<MediaVersion>(entity =>
            {
                entity.HasKey(mv => mv.Id);
                entity.HasMany(mv => mv.Episodes)
                      .WithOne()
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure Id to be generated on add
                entity.Property(mv => mv.Id)
                      .ValueGeneratedOnAdd(); // auto-increment
            });

            // Episode (TPH - Table Per Hierarchy)
            modelBuilder.Entity<Episode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasMany(e => e.EpisodeVersions)
                      .WithOne()
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure Id to be generated on add
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd(); // auto-increment
            });

            modelBuilder.Entity<StandardEpisode>().ToTable("StandardEpisodes");
            modelBuilder.Entity<SpecialEpisode>().ToTable("SpecialEpisodes");
            modelBuilder.Entity<UnknownEpisode>().ToTable("UnknownEpisodes");
            modelBuilder.Entity<BatchEpisode>().ToTable("BatchEpisodes");

            // EpVersion
            modelBuilder.Entity<EpisodeVersion>(entity =>
            {
                entity.HasKey(ev => ev.Id);
                entity.HasMany(ev => ev.Links)
                      .WithOne()
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure Id to be generated on add
                entity.Property(ev => ev.Id)
                      .ValueGeneratedOnAdd(); // auto-increment
            });

            // ShortLink
            modelBuilder.Entity<DramaDayLink>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Host)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString(),    // Convert Uri to string when saving
                        v => new Uri(v));     // Convert string to Uri when reading
            });

            modelBuilder.Entity<ResolvedLink>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ResolvedLinkUrl)
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString(),
                        v => new Uri(v));

                entity.Property(e => e.ResolvedAt)
                    .IsRequired();

                entity.HasOne(e => e.ParentRawLink)
                    .WithOne() // ONE-TO-ONE
                    .HasForeignKey<ResolvedLink>(e => e.ParentRawLinkId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                // Also make sure ParentRawLinkId is UNIQUE
                entity.HasIndex(e => e.ParentRawLinkId)
                    .IsUnique();
            });

        }
    }
}
