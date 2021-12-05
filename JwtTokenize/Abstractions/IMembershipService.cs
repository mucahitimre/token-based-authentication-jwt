namespace JwtTokenize.Abstractions
{
    public interface IMembershipService
    {
        IUser ValidateUserByToken(string token);

        IUser ValidateUser(string email, string password);

        string GetToken(IUser user);
    }
}
