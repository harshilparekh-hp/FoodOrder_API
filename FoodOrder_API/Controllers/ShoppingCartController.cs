using FoodOrder_API.Data;
using FoodOrder_API.Models;
using FoodOrder_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace FoodOrder_API.Controllers
{
    [Route("api")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private ApiResponse _response;
        private readonly ApplicationDbContext _context;
        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
            _response = new ApiResponse();
        }

        [HttpGet]
        [Route("shoppingcarts")]
        public async Task<ActionResult<ApiResponse>> GetShoppingCarts(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _response.IsSuccess = false;
                    _response.HttpStatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                ShoppingCart shoppingCart = await _context.ShoppingCarts.Include(x => x.CartItems)
                                                .ThenInclude(x => x.MenuItem)
                                                .FirstOrDefaultAsync(x => x.UserId == userId);

                if(shoppingCart.CartItems != null && shoppingCart.CartItems.Count > 0)
                    shoppingCart.CartTotal = Math.Round(shoppingCart.CartItems.Sum(x => x.Quantity * x.MenuItem.Price), 2);

                _response.Result = shoppingCart;
                _response.HttpStatusCode = HttpStatusCode.OK;
                return _response;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.HttpStatusCode = HttpStatusCode.BadRequest;
                
            }
            return _response;
        }

        [HttpPost]
        [Route("shoppingcarts")]
        public async Task<ActionResult<ApiResponse>> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQuantityBy)
        {
            // Scenarios to handle:
            // 1. when a user adds a new item to a new shopping cart for the first time.
            // 2. when a user adds a new item to an existing shopping cart (basically user has other items in cart).
            // 3. when a user updates an existing item count.
            // 4. when a user removes an existing item.

            CartItem cartItem = new CartItem();

            ShoppingCart shoppingCart = _context.ShoppingCarts.Include(x => x.CartItems).FirstOrDefault(x => x.UserId == userId);

            MenuItem menuItem = _context.MenuItems.FirstOrDefault(x => x.Id == menuItemId);

            if (menuItem == null)
            {
                _response.HttpStatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            try
            {

                if (shoppingCart == null && updateQuantityBy > 0)
                {
                    // create a shopping cart and add cart item.

                    ShoppingCart newCart = new ShoppingCart() { UserId = userId };
                    _context.ShoppingCarts.Add(newCart);
                    await _context.SaveChangesAsync();

                    cartItem.MenuItemId = menuItemId;
                    cartItem.Quantity = updateQuantityBy;
                    cartItem.ShoppingCartId = newCart.Id;
                    cartItem.MenuItem = null;

                    _context.CartItems.Add(cartItem);
                    await _context.SaveChangesAsync();
                }

                if (shoppingCart != null)
                {
                    var cartMenuItem = _context.CartItems.FirstOrDefault(x => x.MenuItemId == menuItemId);

                    if (cartMenuItem == null && updateQuantityBy > 0)
                    {
                        cartItem.MenuItemId = menuItemId;
                        cartItem.Quantity = updateQuantityBy;
                        cartItem.ShoppingCartId = shoppingCart.Id;
                        cartItem.MenuItem = null;

                        _context.CartItems.Add(cartItem);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        cartMenuItem.Quantity += updateQuantityBy;
                        if (updateQuantityBy == 0 || cartMenuItem.Quantity <= 0)
                        {
                            // remove cart item from cart and its the only item then remove the entire shopping cart
                            _context.CartItems.Remove(cartMenuItem);
                            if (shoppingCart.CartItems.Count == 1)
                                _context.ShoppingCarts.Remove(shoppingCart);

                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            _context.CartItems.Update(cartMenuItem);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.HttpStatusCode = HttpStatusCode.BadRequest;
            }
            return _response;

        }

    }
}
