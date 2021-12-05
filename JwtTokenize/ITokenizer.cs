using JWT;
using JWT.Exceptions;
using JWT.Serializers;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Mail;

namespace JwtTokenize
{
    public interface ITokenizer
    {
        string GenerateToken(IUser user, Dictionary<string, object> payload);

        bool TryVerifyingToken(string token, out IUser user, out Dictionary<string, object> payload);
    }

    public class JwtTokenizer : ITokenizer
    {
        private readonly IConfiguration _configuration;

        public JwtTokenizer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(IUser user, Dictionary<string, object> payload)
        {
            // classic
            //IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            //IJsonSerializer serializer = new JsonNetSerializer();
            //IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            //IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            //var token = encoder.Encode(payload, GetSecret());

            // fluent
            var token = JwtBuilder.Create()
                      .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                      .WithSecret(GetSecret())
                      .AddClaims(payload)
                      .AddClaim(ClaimName.OriginatingIdentityString, user.Id)
                      .AddClaim(ClaimName.FullName, user.Name)
                      .AddClaim(ClaimName.VerifiedEmail, user.Email)
                      .AddClaim(ClaimName.ExpirationTime, DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                      //.AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                      .Encode();

            return token;
        }

        public bool TryVerifyingToken(string token, out IUser user, out Dictionary<string, object> payload)
        {
            user = null;
            payload = null;
            try
            {
                // classic
                //IJsonSerializer serializer = new JsonNetSerializer();
                //IDateTimeProvider provider = new UtcDateTimeProvider();
                //IJwtValidator validator = new JwtValidator(serializer, provider);
                //IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                //IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
                //IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
                //var json = decoder.Decode(token, GetSecret(), verify: true);

                // fluent
                var json = JwtBuilder.Create()
                     .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                     .WithValidator(new JwtValidator(new JsonNetSerializer(), new UtcDateTimeProvider()))
                     .WithUrlEncoder(new JwtBase64UrlEncoder())
                     .WithAlgorithm(new HMACSHA256Algorithm())
                     .WithSecret(GetSecret())
                     .MustVerifySignature()
                     .Decode(token);

                var data = JObject.Parse(json);

                return true;
            }
            catch (TokenExpiredException e)
            {
                // log
                return false;
            }
            catch (SignatureVerificationException e)
            {
                // log
                return false;
            }
            catch (Exception e)
            {
                // log
                return false;
            }
        }

        private string GetSecret()
        {
            return _configuration.GetSection(JwtConstanst.SECRET_NAME).Get<string>();
        }
    }

    public class Member : IUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string MemberLang { get; set; }
        public bool IsDelete { get; set; }
        public bool IsActive { get; set; }
    }

    public interface IMembershipService
    {
        IUser ValidateUserByToken(string token);

        IUser ValidateUser(string email, string password);

        string GetToken(IUser user);
    }

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
            if (_tokenizer.TryVerifyingToken(token, out var user, out var data))
            {
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

        private void CheckEmailPasswordControl(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new UnauthorizedAccessException();
            }

            // encode the password and compare it with the encoded password in the database
        }
    }

    public interface IUserManager
    {
        IUser GetUser(int id);

        IUser GetUser(string email);

        Dictionary<string, object> GetUserPayload(IUser user);
    }

    public class UserManager : IUserManager
    {
        public IUser GetUser(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return new Member { Id = id, Email = $"{RandomString(10)}@foo.com", Name = RandomString(5) };
        }

        public IUser GetUser(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            try
            {
                MailAddress m = new(email);
            }
            catch (FormatException)
            {
                return null;
            }

            return new Member { Id = new Random().Next(1, 100), Email = email, Name = RandomString(5) };
        }

        public Dictionary<string, object> GetUserPayload(IUser user)
        {
            var data = new Dictionary<string, object>
            {
                { "CountryId", 1 },
                { "DefaultLang", !string.IsNullOrEmpty(user.MemberLang) ? user.MemberLang : CultureInfo.CurrentCulture.TwoLetterISOLanguageName }
            };

            return data;
        }

        private static readonly Random _random = new();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
