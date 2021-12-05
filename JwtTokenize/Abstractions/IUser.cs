namespace JwtTokenize.Abstractions
{
    public interface IUser
    {
        int Id { get; set; }
        string Email { get; set; }
        string Name { get; set; }
        string MemberLang { get; set; }
        bool IsDelete { get; set; }
        bool IsActive { get; set; }
    }
}
