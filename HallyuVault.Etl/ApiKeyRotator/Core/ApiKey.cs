namespace HallyuVault.Etl.ApiKeyRotator.Core
{
    public abstract class ApiKey
    {
        public abstract string Key { get; }

        public override bool Equals(object? obj)
        {
            return obj is ApiKey key &&
                   Key == key.Key;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

    }
}
