namespace HallyuVault.Application.Dramas.GetDramas;

public sealed record DramasResponse(IReadOnlyCollection<DramaResponse> Dramas, int TotalCount);