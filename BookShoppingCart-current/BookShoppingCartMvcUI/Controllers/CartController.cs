using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookShoppingCartMvcUI.Models;
using BookShoppingCartMvcUI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;
        private readonly ApplicationDbContext _context;

        public CartController(ICartRepository cartRepo, ApplicationDbContext context)
        {
            
            _cartRepo = cartRepo;
            _context = context;
        }
        
        public async Task<IActionResult> AddItem(int bookId, int qty = 1, int redirect = 0)
        {
            var cartCount = await _cartRepo.AddItem(bookId, qty);
            if (redirect == 0)
                return Ok(cartCount);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> RemoveItem(int bookId)
        {
            var cartCount = await _cartRepo.RemoveItem(bookId);
            return RedirectToAction("GetUserCart");
        }
        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartRepo.GetUserCart();
            return View(cart);
        }

        public async Task<IActionResult> GetTotalItemInCart()
        {
            int cartItem = await _cartRepo.GetCartItemCount();
            return Ok(cartItem);
        }
        public async Task<IActionResult> Checkout()
        {
            var domain = "http://localhost:5049/"; // Adjust this to your domain
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + "Cart/OrderConfirmation",
                CancelUrl = domain + "Cart/GetUserCart",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment"
            };

            var shoppingCart = await _cartRepo.GetUserCart();

            foreach (var cartDetail in shoppingCart.CartDetails)
            {
                int priceInCents = (int)(cartDetail.UnitPrice * 100);

                var sessionListItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = priceInCents,
                        Currency = "cad",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = cartDetail.Book.BookName,
                        }
                    },
                    Quantity = cartDetail.Quantity
                };

                options.LineItems.Add(sessionListItem);
            }

            // This creates a new Stripe session using the provided options
            var service = new SessionService();
            Session session = service.Create(options);

            // This redirects to the Stripe page
            return Redirect(session.Url);

        }

        // Additional methods if required...

        public IActionResult OrderConfirmation()
        {
            // Handle order confirmation logic here, if needed
            return View();
        }
    }
}