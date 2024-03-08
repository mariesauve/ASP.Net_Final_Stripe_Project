namespace BookShoppingCartMvcUI.Repositories
{
    public interface IUserOrderRepository
    {
        //just like we did for both interfaces home and cart 
        Task<IEnumerable<Order>> UserOrders();
    }
}