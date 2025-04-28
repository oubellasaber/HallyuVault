namespace HallyuVault.Core.Abstractions
{
    public interface IParser<TInput, TOutput>
    {
        Result<TOutput> Parse(TInput input);
    }
}