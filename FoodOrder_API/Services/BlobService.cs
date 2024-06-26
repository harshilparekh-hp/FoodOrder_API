
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.ComponentModel;

namespace FoodOrder_API.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobService;

        public BlobService(BlobServiceClient blobService)
        {
            _blobService = blobService;
        }
        public async Task<bool> DeleteBlob(string blobname, string container)
        {
            BlobContainerClient blobContainerClient = _blobService.GetBlobContainerClient(container);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobname);

            return await blobClient.DeleteIfExistsAsync();
        }

        public async Task<string> GetBlob(string blobname, string container)
        {
            BlobContainerClient blobContainerClient = _blobService.GetBlobContainerClient(container);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobname);

            return blobClient.Uri.AbsoluteUri;
        }

        public async Task<string> UploadBlob(string blobname, string container, IFormFile formFile)
        {
            BlobContainerClient blobContainerClient = _blobService.GetBlobContainerClient(container);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobname);

            var httpHeaders = new BlobHttpHeaders()
            {
                ContentType = formFile.ContentType,
            };
            var result = await blobClient.UploadAsync(formFile.OpenReadStream(), httpHeaders);

            if(result != null)
            {
                return await GetBlob(blobname, container);
            }
            return "";
        }
    }
}
