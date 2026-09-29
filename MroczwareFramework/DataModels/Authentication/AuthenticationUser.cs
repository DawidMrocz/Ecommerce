namespace MroczwareFramework.DataModels.Authentication
{
    public class AuthenticationUser : IAuthenticationUser
    {
        public int Id { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string AuthorizeToken { get; set; } = null!;
        public string Culture { get; set; } = null!;
    }
}
