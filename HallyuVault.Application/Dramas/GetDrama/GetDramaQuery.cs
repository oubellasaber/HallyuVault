using HallyuVault.Application.Abstractions.Messaging;

namespace HallyuVault.Application.Dramas.GetDrama;

public record GetDramaQuery(int DramaId) : IQuery<DramaResponse>;