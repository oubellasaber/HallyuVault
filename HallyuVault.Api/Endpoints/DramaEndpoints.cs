using HallyuVault.Api.Common;
using HallyuVault.Application.Dramas;
using HallyuVault.Application.Dramas.GetDrama;
using HallyuVault.Application.Dramas.GetDramas;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HallyuVault.Api.Endpoints
{
    public static class DramaEndpoints
    {
        public static IEndpointRouteBuilder MapDramaEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var routeGroupBuilder = endpoints.MapGroup("/dramas");

            routeGroupBuilder.MapGet("/", GetDramas);

            routeGroupBuilder.MapGet("/{id:int}", GetDrama);

            return routeGroupBuilder;
        }

        public static async Task<Ok<PaginationResult<DramaResponse>>> GetDramas(
            int page, 
            int pageSize, 
            ISender sender)
        {
            var query = new GetDramasQuery(page, pageSize);

            var result = await sender.Send(query);

            var dramasResponse = result.Value;

            var paginationResult = new PaginationResult<DramaResponse>
            {
                Items = dramasResponse.Dramas.ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = dramasResponse.TotalCount
            };

            return TypedResults.Ok(paginationResult);
        }

        public static async Task<Results<Ok<DramaResponse>, NotFound>> GetDrama(
            int id,
            ISender sender)
        {
            var query = new GetDramaQuery(id);

            var result = await sender.Send(query);

            return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.NotFound();
        }
    }
}
