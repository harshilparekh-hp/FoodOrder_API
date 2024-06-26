using System.Net;

namespace FoodOrder_API.Models
{
    public class ApiResponse
    {
        public ApiResponse() 
        {
            ErrorMessages = new List<string>();
        }
        public HttpStatusCode HttpStatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<string> ErrorMessages { get; set; }
        public object Result { get; set; }
    }
}
