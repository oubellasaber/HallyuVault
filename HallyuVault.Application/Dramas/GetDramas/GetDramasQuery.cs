using HallyuVault.Application.Abstractions.Messaging;
namespace HallyuVault.Application.Dramas.GetDramas;

public record GetDramasQuery(int Page, int PageSize) : IQuery<DramasResponse>;