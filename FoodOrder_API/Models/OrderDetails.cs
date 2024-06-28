using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrder_API.Models
{
    public class OrderDetails
    {
        [Key]
        public int OrderDetailId { get; set; }

        [Required]
        public int OrderHeaderId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey("MenuItemId")]
        public MenuItem MenuItem { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string ItemName { get; set; }

        // Price is listed here as sometimes the menu item price can be updated, so to differentiate menu price and order price.
        [Required]
        public double Price { get; set; }
    }
}
