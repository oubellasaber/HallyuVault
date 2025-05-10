using Dapper;
using HallyuVault.Application.Abstractions.Data;
using HallyuVault.Application.Abstractions.Messaging;
using HallyuVault.Core.Abstractions;

namespace HallyuVault.Application.Dramas.GetDramas;

internal sealed class GetDramasQueryHandler : IQueryHandler<GetDramasQuery, DramasResponse>
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ITmdbService _tmdbService;

    public GetDramasQueryHandler(
        ISqlConnectionFactory connectionFactory,
        ITmdbService tmdbService)
    {
        _connectionFactory = connectionFactory;
        _tmdbService = tmdbService;
    }

    public async Task<Result<DramasResponse>> Handle(GetDramasQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """  
            SELECT  
                md.Id AS DramaId,  
                md.TmdbId AS Name,  
                md.SeasonNumber AS SeasonNumber  
            FROM MediaDetails AS md  
            ORDER BY md.Id  
            OFFSET @Offset ROWS  
            FETCH NEXT @PageSize ROWS ONLY;  

            SELECT COUNT(*) FROM MediaDetails;
            """;

        using var multi = await connection.QueryMultipleAsync(sql, new
        {
            Offset = (request.Page - 1) * request.PageSize,
            PageSize = request.PageSize
        });

        var partialDramas = (await multi.ReadAsync<PartialDrama>()).ToList();
        var count = await multi.ReadFirstAsync<int>();

        // Parallel TMDB calls  
        var tasks = partialDramas.Select(partialDrama =>
            _tmdbService.GetDramaAsync(partialDrama.TmdbId, partialDrama.SeasonNumber));

        var tmdbDramas = await Task.WhenAll(tasks);

        // Map partial and tmdb dramas to DramaResponse  
        var mappedDramas = partialDramas.Zip(
            tmdbDramas, 
            (partialDrama, tmdbDrama) => partialDrama.ToResponse(tmdbDrama))
            .ToList();

        var dramas = new DramasResponse(mappedDramas, count);

        return dramas;
    }
}