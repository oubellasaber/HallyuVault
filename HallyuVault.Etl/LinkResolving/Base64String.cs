using System.Text;

namespace HallyuVault.Etl.LinkResolving
{
    public readonly struct Base64String
    {
        public string Value { get; }

        private Base64String(string value)
        {
            Value = value;
        }

        public static Base64String Parse(string s)
        {
            byte[] bytes = Convert.FromBase64String(s);
            string decoded = Encoding.UTF8.GetString(bytes);
            return new Base64String(decoded);
        }

        public static bool TryParse(string? s,  out Base64String result)
        {
            result = default;

            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString() => Value;
    }
}
