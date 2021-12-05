namespace JwtTokenize.Abstractions
{
    public class JwtOptions
    {
        public string SecretKey { get; set; }

        public string Issuer { get; set; }

        public int ExpirationMinute { get; set; }
    }
}
