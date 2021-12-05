using JWT;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JwtTokenize.Services
{
    public class JwtTokenizer : ITokenizer
    {
        private readonly IOptions<JwtOptions> _jwtOptions;

        public JwtTokenizer(IOptions<JwtOptions> options)
        {
            _jwtOptions = options;
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
                      .AddClaim("user-id", user.Id)
                      .AddClaim("user-name", user.Name)
                      .AddClaim("user-email", user.Email)
                      .AddClaim(ClaimName.ExpirationTime, DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.Value.ExpirationMinute).ToUnixTimeSeconds())
                      .Encode();

            return token;
        }

        public bool TryVerifyingToken(string token, out int userId, out Dictionary<string, object> payload)
        {
            userId = default;
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

                payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (payload == null)
                {
                    return false;
                }

                var expName = "exp";
                if (payload.ContainsKey(expName) && long.TryParse(payload[expName].ToString(), out var date) && date < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    throw new TokenExpiredException("Token is expired");
                }

                var userIdName = "user-id";
                if (payload.ContainsKey(userIdName) && int.TryParse(payload[userIdName].ToString(), out var id))
                {
                    userId = id;
                }

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
            return _jwtOptions.Value.SecretKey;
        }
    }
}
