namespace Contracts
{
    public record ProductCreated(
        int Id,
        string Name,
        decimal Price,
        int Stock,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    public record ProductDeleted(
        int Id,
        string Name,
        decimal Price,
        int Stock,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    public record AddToBasket(
       int Id,
       int Quantity,
       int UserId);
}
