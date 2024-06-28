using FoodOrder_API.Data;
using FoodOrder_API.Models;
using FoodOrder_API.Models.Dto;
using FoodOrder_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace FoodOrder_API.Controllers
{
    [Route("api")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private ApiResponse _response;
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context)
        {
            _context = context;
            _response = new ApiResponse();
        }

        [HttpGet]
        [Route("orders")]
        public async Task<ActionResult<ApiResponse>> GetOrders(string? userId)
        {
            try
            {
                var orderHeaders = await _context.OrderHeaders.Include(x => x.OrderDetails)
                                        .ThenInclude(x => x.MenuItem)
                                        .OrderByDescending(x => x.OrderHeaderId).ToListAsync();

                if(!string.IsNullOrEmpty(userId))
                {
                    _response.Result = orderHeaders.Where(x => x.ApplicationUserId == userId);
                }
                else
                {
                    _response.Result = orderHeaders;
                }

                _response.HttpStatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.HttpStatusCode = HttpStatusCode.BadRequest;
            }
            return _response;
        }

        [HttpGet]
        [Route("order/{id}")]
        public async Task<ActionResult<ApiResponse>> GetOrderById(int id)
        {
            try
            {

                if(id == 0)
                {
                    _response.HttpStatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }

                var orderHeaders = await _context.OrderHeaders.Include(x => x.OrderDetails)
                                        .ThenInclude(x => x.MenuItem)
                                        .Where(x => x.OrderHeaderId == id).ToListAsync();

                if (orderHeaders == null)
                {
                    _response.HttpStatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess=false;
                    return NotFound(_response);   
                }
                _response.Result = orderHeaders;
                _response.HttpStatusCode = HttpStatusCode.OK;
                return Ok(_response);
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
        [Route("orders")]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDto orderHeaderCreateDTO)
        {
            try
            {
                OrderHeader order = new()
                {
                    ApplicationUserId = orderHeaderCreateDTO.ApplicationUserId,
                    PickupEmail = orderHeaderCreateDTO.PickupEmail,
                    PickupName = orderHeaderCreateDTO.PickupName,
                    PickupPhoneNumber = orderHeaderCreateDTO.PickupPhoneNumber,
                    OrderTotal = orderHeaderCreateDTO.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripPaymentIntentId = orderHeaderCreateDTO.StripPaymentIntentId,
                    TotalItems = orderHeaderCreateDTO.TotalItems,
                    Status = String.IsNullOrEmpty(orderHeaderCreateDTO.Status) ? SD.Status_Pending : orderHeaderCreateDTO.Status,
                };

                if(ModelState.IsValid)
                {
                    _context.OrderHeaders.Add(order);
                    _context.SaveChanges();
                    
                    // loop thru each order to add into order details

                    foreach(var orderDetailDTO in orderHeaderCreateDTO.OrderDetailsDTO)
                    {
                        OrderDetails orderDetails = new()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDTO.ItemName,
                            MenuItemId = orderDetailDTO.MenuItemId,
                            Price = orderDetailDTO.Price,
                            Quantity = orderDetailDTO.Quantity,
                        };
                        _context.OrderDetails.Add(orderDetails);
                    }
                    await _context.SaveChangesAsync();
                    _response.Result = order;
                    order.OrderDetails = null;
                    _response.HttpStatusCode = HttpStatusCode.Created;
                    return Ok(_response);

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

        [HttpPut]
        [Route("orders/{id}")]
        public async Task<ActionResult<ApiResponse>> UpdateOrder(int id, [FromBody] OrderHeaderUpdateDto orderHeaderUpdateDto)
        {
            try
            {
                if(orderHeaderUpdateDto == null || id != orderHeaderUpdateDto.OrderHeaderId)
                {
                    _response.IsSuccess = false;
                    _response.HttpStatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                OrderHeader orderHeaderFromDb = await _context.OrderHeaders.FirstOrDefaultAsync(x => x.OrderHeaderId == id);

                if(orderHeaderFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.HttpStatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                orderHeaderFromDb.PickupName = String.IsNullOrEmpty(orderHeaderUpdateDto.PickupName) 
                                                ? orderHeaderFromDb.PickupName : orderHeaderFromDb.PickupName;

                orderHeaderFromDb.PickupEmail = String.IsNullOrEmpty(orderHeaderUpdateDto.PickupEmail)
                                                 ? orderHeaderFromDb.PickupEmail : orderHeaderFromDb.PickupEmail;

                orderHeaderFromDb.PickupPhoneNumber = String.IsNullOrEmpty(orderHeaderUpdateDto.PickupPhoneNumber)
                                                       ? orderHeaderFromDb.PickupPhoneNumber : orderHeaderFromDb.PickupPhoneNumber;

                orderHeaderFromDb.StripPaymentIntentId = String.IsNullOrEmpty(orderHeaderUpdateDto.StripPaymentIntentId) 
                                                            ? orderHeaderFromDb.StripPaymentIntentId : orderHeaderFromDb.StripPaymentIntentId;

                orderHeaderFromDb.Status = String.IsNullOrEmpty(orderHeaderUpdateDto.Status)
                                            ? SD.Status_Pending : orderHeaderFromDb.Status;

                _context.OrderHeaders.Update(orderHeaderFromDb);
                await _context.SaveChangesAsync();
                _response.HttpStatusCode = HttpStatusCode.NoContent;
                return Ok(_response);

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
