using HallyuVault.Core.Abstractions;

namespace HallyuVault.Etl.DramaDayMediaParser.Abtractions;

public partial record HtmlParsingErrors
{
    public static readonly Error MismatchedParser = new("Error.MismatchedParser", "The provided HTML node does not match the expected format for the parser.");
    public static readonly Error ParsingFailed = new("Error.ParsingFailed", "All parsers failed to parse the input.");
}