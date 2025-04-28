namespace HallyuVault.Etl.Domain.Entities.Episodes;

public record EpisodeId(Guid Value)
{
    public static EpisodeId New() => new(Guid.NewGuid());
}