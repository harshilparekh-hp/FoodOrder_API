using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FoodOrder_API.Models.Dto
{
    public class OrderHeaderUpdateDto
    {
        [Key]
        public int OrderHeaderId { get; set; }
        public string PickupName { get; set; }
        public string PickupPhoneNumber { get; set; }
        public string PickupEmail { get; set; }
        public string StripPaymentIntentId { get; set; }
        public string Status { get; set; }

    }
}
