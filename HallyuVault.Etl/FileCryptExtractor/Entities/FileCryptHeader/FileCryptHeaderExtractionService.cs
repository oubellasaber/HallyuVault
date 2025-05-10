using Microsoft.Extensions.Options;

namespace HallyuVault.Etl.FileCryptExtractor.Entities.FileCryptHeader;

public class FileCryptHeaderExtractionService
{
    private readonly FileCryptOptions _settings;

    public FileCryptHeaderExtractionService(IOptions<FileCryptOptions> settings)
    {
        _settings = settings.Value;
    }

    public FileCryptHeader GetFileCryptHeader(HttpResponseMessage httpRequestMessage)
    {
        string requiredHeaders = httpRequestMessage.Headers.GetValues("Set-Cookie").First();

        var headersSeperated = requiredHeaders.Split(';');

        var phpSessid = headersSeperated[0].Split('=')[1];

        return new FileCryptHeader(phpSessid);
    }
}