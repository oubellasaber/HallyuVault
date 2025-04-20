using HallyuVault.Etl.FileCryptExtractor.Entities.Rows.ValueObjects;

namespace HallyuVault.Etl.FileCryptExtractor.Entities.Rows;

public class Row
{
    public string? FileName { get; private set; }
    public FileSize? FileSize { get; private set; }
    public FcLink Link { get; private set; }

    public Row(string? fileName, FileSize? fileSize, FcLink link)
    {
        if (fileName == "n/a")
        {
            fileName = null;
        }

        FileName = fileName;
        FileSize = fileSize;
        Link = link;
    }
}