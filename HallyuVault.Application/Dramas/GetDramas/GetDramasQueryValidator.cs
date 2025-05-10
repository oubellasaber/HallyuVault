using FluentValidation;

namespace HallyuVault.Application.Dramas.GetDramas;

internal class GetDramasQueryValidator : AbstractValidator<GetDramasQuery>
{
    public GetDramasQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 10);
    }
}
