using FoodOrder_API.Data;
using FoodOrder_API.Models;
using FoodOrder_API.Models.Dto;
using FoodOrder_API.Services;
using FoodOrder_API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace FoodOrder_API.Controllers
{
    [Route("api")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobService _blobService;
        private ApiResponse _response;

        public MenuItemController(ApplicationDbContext context, IBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
            _response = new ApiResponse();
        }

        [Authorize]
        [HttpGet]
        [Route("menuitems")]
        public async Task<IActionResult> GetMenuItems()
        {
            _response.Result = await _context.MenuItems.ToListAsync();
            _response.HttpStatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpGet]
        [Route("menuitem/{id}", Name = "GetMenuItemById")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            if (id == 0)
            {
                _response.HttpStatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            MenuItem menuItem = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == id);

            if (menuItem == null)
            {
                _response.HttpStatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }

            _response.Result = menuItem;
            _response.HttpStatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpPost]
        [Route("menuitem")]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDTO menuItemCreateDto)
        {
            try
            {
                // IsValid will validate the model based on the attributes in the Model class.
                if (ModelState.IsValid)
                {
                    if (menuItemCreateDto.File == null || menuItemCreateDto.File.Length == 0)
                    {
                        _response.HttpStatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest(_response);
                    }
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemCreateDto.File.FileName)}";
                    MenuItem menuItemToCreate = new()
                    {
                        Name = menuItemCreateDto.Name,
                        Price = menuItemCreateDto.Price,
                        Category = menuItemCreateDto.Category,
                        SpecialTag = menuItemCreateDto.SpecialTag,
                        Description = menuItemCreateDto.Description,
                        Image = await _blobService.UploadBlob(fileName, SD.SD_Storage_Container, menuItemCreateDto.File)
                    };

                    _context.MenuItems.Add(menuItemToCreate);
                    _context.SaveChanges();
                    _response.Result = menuItemToCreate;
                    _response.HttpStatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetMenuItemById", new { id = menuItemToCreate.Id }, _response);
                }
                else
                {
                    _response.IsSuccess = false;
                }

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message.ToString() };
            }
            return _response;
        }

        [HttpPut]
        [Route("menuitem/{id}")]
        public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int id, [FromForm] MenuItemUpdateDto menuItemUpdateDto)
        {
            try
            {
                // IsValid will validate the model based on the attributes in the Model class.
                if (ModelState.IsValid)
                {
                    if (menuItemUpdateDto == null)
                    {
                        _response.HttpStatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest(_response);
                    }

                    MenuItem menuItemFromDb = await _context.MenuItems.FindAsync(id);
                    if (menuItemFromDb == null)
                    {
                        _response.HttpStatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest(_response);
                    }

                    menuItemFromDb.Name = menuItemUpdateDto.Name ?? menuItemFromDb.Name;
                    menuItemFromDb.Price = menuItemUpdateDto.Price ?? menuItemFromDb.Price;
                    menuItemFromDb.Category = menuItemUpdateDto.Category ?? menuItemFromDb.Category;
                    menuItemFromDb.SpecialTag = menuItemUpdateDto.SpecialTag ?? menuItemFromDb.SpecialTag;
                    menuItemFromDb.Description = menuItemUpdateDto.Description ?? menuItemFromDb.Description;

                    if (menuItemUpdateDto.File != null && menuItemUpdateDto.File.Length > 0)
                    {
                        // remove the old file from blob storage
                        await _blobService.DeleteBlob(menuItemFromDb.Image.Split('/').Last(), SD.SD_Storage_Container);

                        // upload the new image
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemUpdateDto.File.FileName)}";
                        menuItemFromDb.Image = await _blobService.UploadBlob(fileName, SD.SD_Storage_Container, menuItemUpdateDto.File);
                    }


                    _context.MenuItems.Update(menuItemFromDb);
                    _context.SaveChanges();
                    _response.HttpStatusCode = HttpStatusCode.NoContent;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                }

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message.ToString() };
            }
            return _response;
        }


        [HttpDelete]
        [Route("menuitem/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
        {
            try
            {

                if (id == 0)
                {
                    _response.HttpStatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }

                MenuItem menuItemFromDb = await _context.MenuItems.FindAsync(id);
                if (menuItemFromDb == null)
                {
                    _response.HttpStatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }

                // delete the file from blob storage
                await _blobService.DeleteBlob(menuItemFromDb.Image.Split('/').Last(), SD.SD_Storage_Container);

                _context.MenuItems.Remove(menuItemFromDb);
                _context.SaveChanges();
                _response.HttpStatusCode = HttpStatusCode.NoContent;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message.ToString() };
            }
            return _response;
        }

    }
}
