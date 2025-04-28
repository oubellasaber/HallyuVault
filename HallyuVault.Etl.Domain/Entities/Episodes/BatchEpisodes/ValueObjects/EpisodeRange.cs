namespace HallyuVault.Etl.Domain.Entities.Episodes.BatchEpisodes.ValueObjects;

public record EpisodeRange
{
    public int StartEpisode { get; }
    public int EndEpisode { get; }

    public EpisodeRange(int startEpisode, int endEpisode)
    {
        if (startEpisode <= 0 || endEpisode <= 0)
            throw new ArgumentException("Episode numbers must be positive integers.");

        if (endEpisode < startEpisode)
            throw new ArgumentException("End episode cannot be less than the start episode.");

        if (startEpisode > endEpisode)
            throw new ArgumentException("Start episode cannot be greater than the end episode.");

        StartEpisode = startEpisode;
        EndEpisode = endEpisode;
    }
}