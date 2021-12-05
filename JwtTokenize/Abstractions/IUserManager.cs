namespace JwtTokenize.Abstractions
{
    public interface IUserManager
    {
        string TypeOfSupply { get; }

        IUser GetUser(int id);

        IUser GetUser(string email);

        Dictionary<string, object> GetUserPayload(IUser user);
    }
}
