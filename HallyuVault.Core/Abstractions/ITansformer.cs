namespace HallyuVault.Core.Abstractions
{
    public interface ITansformer<TInput, TOutput>
    {
        Result<TOutput> Tranform(TInput input);
    }
}