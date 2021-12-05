using System.Globalization;
using System.Net.Mail;

namespace JwtTokenize.Services
{
    public class MemberManager : IUserManager
    {
        public string TypeOfSupply => nameof(Member);

        public IUser GetUser(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return new Member { Id = id, Email = $"{RandomString(10)}@foo.com", Name = RandomString(5), IsActive = true, IsDelete = false };
        }

        public IUser GetUser(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            try
            {
                _ = new MailAddress(email);
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
