using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FoodOrder_API.Models.Dto
{
    public class OrderDetailsCreateDto
    {

        [Required]
        public int MenuItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string ItemName { get; set; }

        // Price is listed here as sometimes the menu item price can be updated, so to differentiate menu price and order price.
        [Required]
        public double Price { get; set; }
    }
}
