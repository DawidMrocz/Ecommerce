namespace MroczwareFramework.Models.User
{
    public class UserSessionModel
    {
        public string Id { get; set; } = null!;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}