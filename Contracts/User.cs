namespace Contracts
{
    public record UserRegistered(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    System.DateTime CreatedAt);

    public record UserLoggedIn(
        int Id,
        string Email,
        System.DateTime At);

    public record UserLoggedOut(
        int Id,
        string Email,
        System.DateTime At);

    public record UserEmailConfirmed(
        int Id,
        string Email,
        System.DateTime At);
}
