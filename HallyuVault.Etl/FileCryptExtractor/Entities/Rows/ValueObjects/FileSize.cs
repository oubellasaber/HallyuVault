using HallyuVault.Etl.FileCryptExtractor.Entities.Rows.Enums;

namespace HallyuVault.Etl.FileCryptExtractor.Entities.Rows.ValueObjects
{
    public class FileSize
    {
        public double Size { get; }
        public DataMeasurement Unit { get; }

        public FileSize(double size, DataMeasurement unit)
        {
            if (size < 0)
                throw new ArgumentException("File size cannot be negative.", nameof(size));

            Size = size;
            Unit = unit;
        }

        public override string ToString() => $"{Size:F2} {Unit}";
    }
}