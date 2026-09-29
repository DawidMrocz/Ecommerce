namespace Contracts
{
    public record BasketItemQuantityChanged(int ProductId, int Quantity);
    public record BasketItemDeleted(int ProductId, int Quantity);
}
