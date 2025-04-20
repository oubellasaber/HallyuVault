namespace HallyuVault.Etl.FileCryptExtractor.Entities.FileCryptHeader.ValueObjects;

public record HttpHeader(string HeaderName, string Value)
{
    public string Value { set; get; } = Value;
}