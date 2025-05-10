namespace HallyuVault.Api.Common;

public interface ICollectionResponse<T>
{
    List<T> Items { get; init; }
}
