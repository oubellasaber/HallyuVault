namespace HallyuVault.Core.Abstractions
{
    public interface IValidator<TInput>
    {
        Result Validate(TInput input);
    }
}
