using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HallyuVault.Etl.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EpisodeMetadata",
                columns: table => new
                {
                    EpisodeMetadataId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContainerScrapedLinkVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RawEpisodeVersionId = table.Column<int>(type: "int", nullable: true),
                    FileNameMetadataId = table.Column<int>(type: "int", nullable: true),
                    MediaMetadataId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeMetadata", x => x.EpisodeMetadataId);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    MediaId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EnglishTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KoreanTitle = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.MediaId);
                });

            migrationBuilder.CreateTable(
                name: "ScrapedDramas",
                columns: table => new
                {
                    ScrapedDramaId = table.Column<int>(type: "int", nullable: false),
                    AddedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PulledOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapedDramas", x => x.ScrapedDramaId);
                });

            migrationBuilder.CreateTable(
                name: "FileNameMetadata",
                columns: table => new
                {
                    FileNameMetadataId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeasonNumber = table.Column<int>(type: "int", nullable: false),
                    RangeStart = table.Column<int>(type: "int", nullable: false),
                    RangeEnd = table.Column<int>(type: "int", nullable: true),
                    Quality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleaseGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Network = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RipType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileNameMetadata", x => x.FileNameMetadataId);
                    table.ForeignKey(
                        name: "FK_FileNameMetadata_EpisodeMetadata_FileNameMetadataId",
                        column: x => x.FileNameMetadataId,
                        principalTable: "EpisodeMetadata",
                        principalColumn: "EpisodeMetadataId");
                });

            migrationBuilder.CreateTable(
                name: "MediaMetadata",
                columns: table => new
                {
                    MediaMetadataId = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    VideoCodecName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AudioCodecName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AudioChannels = table.Column<int>(type: "int", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaMetadata", x => x.MediaMetadataId);
                    table.ForeignKey(
                        name: "FK_MediaMetadata_EpisodeMetadata_MediaMetadataId",
                        column: x => x.MediaMetadataId,
                        principalTable: "EpisodeMetadata",
                        principalColumn: "EpisodeMetadataId");
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonNumber = table.Column<int>(type: "int", nullable: true),
                    MediaId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "Media",
                        principalColumn: "MediaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubtitleMetadata",
                columns: table => new
                {
                    SubtitleMetadataId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodecName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaMetadataId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubtitleMetadata", x => x.SubtitleMetadataId);
                    table.ForeignKey(
                        name: "FK_SubtitleMetadata_MediaMetadata_MediaMetadataId",
                        column: x => x.MediaMetadataId,
                        principalTable: "MediaMetadata",
                        principalColumn: "MediaMetadataId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaVersions_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MediaVersionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Episodes_MediaVersions_MediaVersionId",
                        column: x => x.MediaVersionId,
                        principalTable: "MediaVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BatchEpisodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    EpisodeStart = table.Column<int>(type: "int", nullable: false),
                    EpisodeEnd = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchEpisodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchEpisodes_Episodes_Id",
                        column: x => x.Id,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EpisodeVersion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EpisodeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EpisodeVersion_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecialEpisodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialEpisodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialEpisodes_Episodes_Id",
                        column: x => x.Id,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StandardEpisodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardEpisodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StandardEpisodes_Episodes_Id",
                        column: x => x.Id,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnknownEpisodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    RawTitle = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnknownEpisodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnknownEpisodes_Episodes_Id",
                        column: x => x.Id,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DramaDayLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EpisodeVersionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DramaDayLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DramaDayLink_EpisodeVersion_EpisodeVersionId",
                        column: x => x.EpisodeVersionId,
                        principalTable: "EpisodeVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResolvedLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResolvedLinkUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParentRawLinkId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResolvedLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResolvedLinks_DramaDayLink_ParentRawLinkId",
                        column: x => x.ParentRawLinkId,
                        principalTable: "DramaDayLink",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LinkContainer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResolvedLinkId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkContainer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkContainer_ResolvedLinks_ResolvedLinkId",
                        column: x => x.ResolvedLinkId,
                        principalTable: "ResolvedLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileCryptLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileCryptContainerId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FileSize = table.Column<double>(type: "float", nullable: true),
                    FileUnit = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileCryptLink", x => x.Id);
                    table.CheckConstraint("CK_FileHostContainer_FileSizeAndUnit", "\r\n                    (       \r\n                        (FileSize IS NULL AND FileUnit IS NULL) OR\r\n                        (FileSize IS NOT NULL AND FileUnit IS NOT NULL)\r\n                    )\r\n                ");
                    table.ForeignKey(
                        name: "FK_FileCryptLink_LinkContainer_FileCryptContainerId",
                        column: x => x.FileCryptContainerId,
                        principalTable: "LinkContainer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerScrapedLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentResolvedLinkId = table.Column<int>(type: "int", nullable: true),
                    ScrapedLink = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileCryptLinkId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerScrapedLink", x => x.Id);
                    table.CheckConstraint("CK_ContainerScrapedLink_OnlyOneFK", "\r\n                    (\r\n                        (CASE WHEN FileCryptLinkId IS NOT NULL THEN 1 ELSE 0 END)\r\n                    ) = 1\r\n                ");
                    table.ForeignKey(
                        name: "FK_ContainerScrapedLink_ContainerScrapedLink_ParentResolvedLinkId",
                        column: x => x.ParentResolvedLinkId,
                        principalTable: "ContainerScrapedLink",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainerScrapedLink_FileCryptLink_FileCryptLinkId",
                        column: x => x.FileCryptLinkId,
                        principalTable: "FileCryptLink",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LinkVersions",
                columns: table => new
                {
                    LinkVersionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScrapedLinkId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkVersions", x => x.LinkVersionId);
                    table.ForeignKey(
                        name: "FK_LinkVersions_ContainerScrapedLink_ScrapedLinkId",
                        column: x => x.ScrapedLinkId,
                        principalTable: "ContainerScrapedLink",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContainerScrapedLink_FileCryptLinkId",
                table: "ContainerScrapedLink",
                column: "FileCryptLinkId",
                unique: true,
                filter: "[FileCryptLinkId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerScrapedLink_ParentResolvedLinkId",
                table: "ContainerScrapedLink",
                column: "ParentResolvedLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_DramaDayLink_EpisodeVersionId",
                table: "DramaDayLink",
                column: "EpisodeVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_MediaVersionId",
                table: "Episodes",
                column: "MediaVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeVersion_EpisodeId",
                table: "EpisodeVersion",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_FileCryptLink_FileCryptContainerId",
                table: "FileCryptLink",
                column: "FileCryptContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkContainer_ResolvedLinkId",
                table: "LinkContainer",
                column: "ResolvedLinkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LinkVersions_ScrapedLinkId",
                table: "LinkVersions",
                column: "ScrapedLinkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaVersions_SeasonId",
                table: "MediaVersions",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ResolvedLinks_ParentRawLinkId",
                table: "ResolvedLinks",
                column: "ParentRawLinkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_MediaId",
                table: "Seasons",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_SubtitleMetadata_MediaMetadataId",
                table: "SubtitleMetadata",
                column: "MediaMetadataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchEpisodes");

            migrationBuilder.DropTable(
                name: "FileNameMetadata");

            migrationBuilder.DropTable(
                name: "LinkVersions");

            migrationBuilder.DropTable(
                name: "ScrapedDramas");

            migrationBuilder.DropTable(
                name: "SpecialEpisodes");

            migrationBuilder.DropTable(
                name: "StandardEpisodes");

            migrationBuilder.DropTable(
                name: "SubtitleMetadata");

            migrationBuilder.DropTable(
                name: "UnknownEpisodes");

            migrationBuilder.DropTable(
                name: "ContainerScrapedLink");

            migrationBuilder.DropTable(
                name: "MediaMetadata");

            migrationBuilder.DropTable(
                name: "FileCryptLink");

            migrationBuilder.DropTable(
                name: "EpisodeMetadata");

            migrationBuilder.DropTable(
                name: "LinkContainer");

            migrationBuilder.DropTable(
                name: "ResolvedLinks");

            migrationBuilder.DropTable(
                name: "DramaDayLink");

            migrationBuilder.DropTable(
                name: "EpisodeVersion");

            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "MediaVersions");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Media");
        }
    }
}
