using HallyuVault.Etl.MetadataEnrichment;
using HallyuVault.Etl.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HallyuVault.Etl.Infra
{
    public class EtlContext : DbContext
    {
        public EtlContext() { }

        public EtlContext(DbContextOptions<EtlContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer("Server=.;Database=HallyuFlow;Trusted_Connection=True;TrustServerCertificate=True;");

            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<ScrapedDrama> ScrapedDramas { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<MediaVersion> MediaVersions { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<StandardEpisode> StandardEpisodes { get; set; }
        public DbSet<BatchEpisode> BatchEpisodes { get; set; }
        public DbSet<SpecialEpisode> SpecialEpisodes { get; set; }
        public DbSet<UnknownEpisode> UnknownEpisodes { get; set; }
        public DbSet<ResolvedLink> ResolvedLinks { get; set; }
        public DbSet<LinkVersion> LinkVersions { get; set; }
        public DbSet<EpisodeMetadata> EpisodeMetadata { get; set; }
        public DbSet<FileNameMetadata> FileNameMetadata { get; set; }
        public DbSet<MediaMetadata> MediaMetadata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ScrapedDrama>(entity =>
            {
                entity.HasKey(e => e.ScrapedDramaId);

                entity.Property(e => e.ScrapedDramaId)
                        .ValueGeneratedNever();

                entity.Property(e => e.AddedOnUtc)
                        .IsRequired();

                entity.Property(e => e.UpdatedOnUtc)
                        .IsRequired();

                entity.Property(e => e.PulledOn)
                        .IsRequired();
            });

            // Media
            modelBuilder.Entity<Media>(entity =>
            {
                entity.HasKey(e => e.MediaId);

                entity.Property(e => e.MediaId)
                    .ValueGeneratedNever();

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

                // Configure Id to be generated on add
                entity.Property(ev => ev.Id)
                      .ValueGeneratedOnAdd(); // auto-increment
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

                // Configure Id to be generated on add
                entity.Property(ev => ev.Id)
                      .ValueGeneratedOnAdd(); // auto-increment
            });

            // LinkContainer → FileCryptLink (One-to-Many)
            modelBuilder.Entity<LinkContainer>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(x => x.ResolvedLink)
                    .WithOne()
                    .HasForeignKey<LinkContainer>(x => x.ResolvedLinkId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.FileCryptLinks)
                    .WithOne(x => x.FileCryptContainer)
                    .HasForeignKey(x => x.FileCryptContainerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ContainerScrapedLink>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ScrapedLink)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(x => x.ParentResolvedLink)
                    .WithMany()
                    .HasForeignKey(x => x.ParentResolvedLinkId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.FileCryptLink)
                    .WithOne(x => x.ContainerScrapedLink)
                    .HasForeignKey<ContainerScrapedLink>(x => x.FileCryptLinkId)
                    .OnDelete(DeleteBehavior.Cascade);

                /*
                 * (CASE WHEN TextContainerLinkId IS NOT NULL THEN 1 ELSE 0 END) +
                 * (CASE WHEN FileHostContainerId IS NOT NULL THEN 1 ELSE 0 END)
                 */
                entity.ToTable(b => b.HasCheckConstraint("CK_ContainerScrapedLink_OnlyOneFK", @"
                    (
                        (CASE WHEN FileCryptLinkId IS NOT NULL THEN 1 ELSE 0 END)
                    ) = 1
                "));
            });

            modelBuilder.Entity<FileCryptLink>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FileName)
                    .HasMaxLength(255);

                entity.Property(x => x.FileSize).IsRequired(false);
                entity.Property(x => x.FileUnit).IsRequired(false);

                entity.Property(x => x.Status)
                    .IsRequired();

                entity.HasOne(x => x.FileCryptContainer)
                    .WithMany(x => x.FileCryptLinks)
                    .HasForeignKey(x => x.FileCryptContainerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable(tb => tb.HasCheckConstraint("CK_FileHostContainer_FileSizeAndUnit", @"
                    (       
                        (FileSize IS NULL AND FileUnit IS NULL) OR
                        (FileSize IS NOT NULL AND FileUnit IS NOT NULL)
                    )
                "));
            });

            modelBuilder.Entity<LinkVersion>(entity =>
            {
                entity.HasKey(lv => lv.LinkVersionId);

                entity.Property(lv => lv.GroupId)
                       .IsRequired();

                entity.Property(lv => lv.ScrapedLinkId)
                       .IsRequired();

                entity.HasIndex(lv => lv.ScrapedLinkId).IsUnique();

                entity.HasOne(lv => lv.ScrapedLink)
                    .WithOne()  // One ContainerScrapedLink has only one LinkVersion
                    .HasForeignKey<LinkVersion>(lv => lv.ScrapedLinkId);
            });

            modelBuilder.Entity<EpisodeMetadata>(entity =>
            {
                entity.HasKey(e => e.EpisodeMetadataId);

                entity.Property(e => e.RawEpisodeVersionId).IsRequired(false);
                entity.Property(e => e.ContainerScrapedLinkVersionId).IsRequired(false);

                entity.HasOne(e => e.FileNameMetadata)
                       .WithOne()
                       .HasForeignKey<FileNameMetadata>(e => e.FileNameMetadataId)
                       .IsRequired(false);

                entity.HasOne(e => e.MediaMetadata)
                       .WithOne()
                       .HasForeignKey<MediaMetadata>(e => e.MediaMetadataId)
                       .IsRequired(false); // When no download link is available atm
            });

            modelBuilder.Entity<MediaMetadata>(entity =>
            {
                entity.HasKey(m => m.MediaMetadataId);

                entity.Property(m => m.Duration).IsRequired();
                entity.Property(m => m.Height).IsRequired();
                entity.Property(m => m.Width).IsRequired();
                entity.Property(m => m.VideoCodecName).IsRequired();
                entity.Property(m => m.AudioCodecName).IsRequired();
                entity.Property(m => m.AudioChannels).IsRequired();

                entity.HasMany(m => m.Subtitles)
                       .WithOne(s => s.MediaMetadata)
                       .HasForeignKey(s => s.MediaMetadataId)
                       .IsRequired();
            });

            modelBuilder.Entity<SubtitleMetadata>(entity =>
            {
                entity.HasKey(s => s.SubtitleMetadataId);

                entity.Property(s => s.Language).IsRequired();
                entity.Property(s => s.CodecName).IsRequired();
                entity.Property(s => s.Title).IsRequired(false);
            });

            modelBuilder.Entity<FileNameMetadata>(entity =>
            {
                entity.HasKey(f => f.FileNameMetadataId);

                entity.Property(f => f.FileName).IsRequired();
            });
        }
    }
}