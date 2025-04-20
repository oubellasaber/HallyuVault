using HallyuVault.Etl.FileCryptExtractor.Entities.Rows.Enums;

namespace HallyuVault.Etl.FileCryptExtractor.Entities.Rows.ValueObjects;

public record FcLink(Uri Url, Status Status);