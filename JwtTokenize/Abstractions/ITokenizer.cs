namespace JwtTokenize.Abstractions
{
    public interface ITokenizer
    {
        string GenerateToken(IUser user, Dictionary<string, object> payload);

        bool TryVerifyingToken(string token, out int userId, out Dictionary<string, object> payload);
    }
}
