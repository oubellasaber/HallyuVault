using HallyuVault.Etl.FileCryptExtractor.Entities.FileCryptHeader.ValueObjects;
using System.Globalization;

namespace HallyuVault.Etl.FileCryptExtractor.Entities.FileCryptHeader;

public class FileCryptHeader
{
    public FileCryptHeader(string phpSessionCookie)
    {
        PhpSessionCookie = new CookieHeader($"PHPSESSID={phpSessionCookie}");
    }

    public HttpHeader PhpSessionCookie { get; private set; }
}