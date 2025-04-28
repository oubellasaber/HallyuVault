using HallyuVault.Etl.Models;
using System.Threading.Tasks.Dataflow;

namespace HallyuVault.Etl.Orchestration
{
    public class BuildingBlocks
    {
        public static BufferBlock<ScrapedDrama> buffer = new BufferBlock<ScrapedDrama>();

        public static TransformBlock<ScrapedDrama, Media> HtmlTransformer = new TransformBlock<ScrapedDrama, Media>(x => new Media("", "", ""));

        public static ActionBlock<Media> UploadMediaToDb = new ActionBlock<Media>(x =>
        {
            // Upload media to the database
            Console.WriteLine($"Uploading media to DB: {x}");
        });

        public static TransformManyBlock<Media, (int season, string mediaVersion, int epNumber, EpisodeVersion episodeVersion)> TransformManyBlock = new TransformManyBlock<Media, (int season, string mediaVersion, int epNumber, EpisodeVersion episodeVersion)>(media =>
        {
            // Transform media to episode versions
            var episodeVersions = new List<(int season, string mediaVersion, int epNumber, EpisodeVersion episodeVersion)>();

            foreach (var season in media.Seasons)
            {
                foreach (var mediaVersion in season.MediaVersions)
                {
                    foreach (var ep in mediaVersion.Episodes)
                    {
                        foreach (var item in ep.EpisodeVersions)
                        {
                            episodeVersions.Add((season.SeasonNumber ?? 1, mediaVersion.Name, ((StandardEpisode)ep).EpisodeNumber, item));
                        }
                    }
                }
            }
            return episodeVersions;
        });

        // resolution takes action

        // reoslved links get saved to db

        // after that passed to the next block

        public static void Execute(ScrapedDrama dramaPost)
        {
            // Execute the transformation
            var media = HtmlTransformer.SendAsync(dramaPost).Result;

            // Do something with the transformed media
            Console.WriteLine($"Transformed Media: {media}");
        }
    }
}
