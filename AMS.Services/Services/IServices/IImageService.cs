using Microsoft.AspNetCore.Http;

namespace AMS.Services.Services.IServices
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile objfile);
        Task<string> DeleteImageAsync(string path);
    }
}
