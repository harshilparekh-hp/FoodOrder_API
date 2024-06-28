using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FoodOrder_API.Models.Dto
{
    public class OrderHeaderCreateDto
    {

        [Required]
        public string PickupName { get; set; }

        [Required]
        public string PickupPhoneNumber { get; set; }

        [Required]
        public string PickupEmail { get; set; }
        public string ApplicationUserId { get; set; }
        public double OrderTotal { get; set; }
        public string StripPaymentIntentId { get; set; }
        public string Status { get; set; }
        public int TotalItems { get; set; }

        public ICollection<OrderDetailsCreateDto> OrderDetailsDTO { get; set; }
    }
}
