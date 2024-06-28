using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrder_API.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserId { get; set; }
    
        public ICollection<CartItem> CartItems { get; set; }

        // carttotal will not be in the db
        [NotMapped]
        public double CartTotal { get; set; }

        [NotMapped]
        public string StripPaymentIntentId { get; set; }

        [NotMapped]
        public string ClientSecret { get; set; }
    }
}
