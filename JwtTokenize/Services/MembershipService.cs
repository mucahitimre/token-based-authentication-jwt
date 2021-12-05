namespace JwtTokenize.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly ITokenizer _tokenizer;
        private readonly IUserManager _userManager;

        public MembershipService(
            ITokenizer tokenizer,
            IUserManager userManager)
        {
            _tokenizer = tokenizer;
            _userManager = userManager;
        }

        public string GetToken(IUser user)
        {
            var payload = _userManager.GetUserPayload(user);
            var token = _tokenizer.GenerateToken(user, payload);

            return token;
        }

        public IUser ValidateUserByToken(string token)
        {
            if (_tokenizer.TryVerifyingToken(token, out var userId, out var data))
            {
                var user = _userManager.GetUser(userId);
                if (user.IsDelete)
                {
                    return null;
                }

                if (!user.IsActive)
                {
                    return null;
                }

                return user;
            }

            return null;
        }

        public IUser ValidateUser(string email, string password)
        {
            CheckEmailPasswordControl(email, password);

            var user = _userManager.GetUser(email);

            return user;
        }

        private static void CheckEmailPasswordControl(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new UnauthorizedAccessException();
            }

            // encode the password and compare it with the encoded password in the database
        }
    }
}
