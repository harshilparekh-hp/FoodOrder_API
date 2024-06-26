namespace FoodOrder_API.Services
{
    public interface IBlobService
    {
        Task<string> GetBlob(string blobname, string container);
        Task<bool> DeleteBlob(string blobname, string container);
        Task<string> UploadBlob(string blobname, string containerm, IFormFile formFile);

    }
}
