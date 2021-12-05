namespace JwtTokenize.Models
{
    public class Member : IUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string MemberLang { get; set; }
        public bool IsDelete { get; set; }
        public bool IsActive { get; set; }
    }
}
