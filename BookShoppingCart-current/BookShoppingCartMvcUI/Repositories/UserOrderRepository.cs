using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMvcUI.Repositories
{
    //create a userorderRepository 
    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        // this is so you can see the order in user account 
        public UserOrderRepository(ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
             IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        // this is almost the same code as the cart just changed into 
        //order.status and details using the order 
        public async Task<IEnumerable<Order>> UserOrders()
        {
            // gets the userId function GetUserID()
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not logged-in");
            var orders = await _db.Orders
                //this includes both relationships and also the Book and Genre
                //if it is equal to the user Id the order will be place in the log-in user
                            .Include(x=>x.OrderStatus)
                            .Include(x=>x.OrderDetail)
                            .ThenInclude(x=>x.Book)
                            .ThenInclude(x=>x.Genre)
                            .Where(a=>a.UserId==userId)
                            .ToListAsync();
            return orders;
        }
        // this again copied from the CartRepo to confirm user 
        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
