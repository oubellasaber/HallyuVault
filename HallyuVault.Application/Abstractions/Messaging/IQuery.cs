using HallyuVault.Core.Abstractions;
using MediatR;

namespace HallyuVault.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
