namespace MroczwareFramework.DataModels.Authentication
{
    public interface IAuthenticationUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string AuthorizeToken { get; set; }
        public string Culture { get; set; }
    }
}
