namespace HallyuVault.Etl.FileCryptExtractor.Entities.FileCryptHeader.ValueObjects;

public record CookieHeader(string Value) : HttpHeader("Cookie", Value);