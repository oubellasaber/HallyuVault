using Dapper;
using HallyuVault.Application.Abstractions.Data;
using HallyuVault.Application.Abstractions.Messaging;
using HallyuVault.Core.Abstractions;
using HallyuVault.Domain.Entities.Drama;

namespace HallyuVault.Application.Dramas.GetDrama;

internal sealed class GetDramaQueryHandler : IQueryHandler<GetDramaQuery, DramaResponse>
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ITmdbService _tmdbService;

    public GetDramaQueryHandler(
        ISqlConnectionFactory connectionFactory,
        ITmdbService tmdbService)
    {
        _connectionFactory = connectionFactory;
        _tmdbService = tmdbService;
    }

    public async Task<Result<DramaResponse>> Handle(GetDramaQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """  
              SELECT  
                  md.Id AS DramaId,  
                  md.TmdbId AS Name,  
                  md.SeasonNumber AS SeasonNumber  
              FROM MediaDetails AS md  
              WHERE md.Id = @Id;
              """;

        var partialDrama = await connection.QueryFirstOrDefaultAsync<PartialDrama>(
            sql,
            new
            {
                Id = request.DramaId
            });

        if (partialDrama is null)
        {
            return Result.Failure<DramaResponse>(DramaErrors.NotFound);
        }

        var tmdbDrama = await _tmdbService.GetDramaAsync(partialDrama.TmdbId, partialDrama.SeasonNumber);
        var mappedDrama = partialDrama.ToResponse(tmdbDrama);

        return mappedDrama;
    }
}