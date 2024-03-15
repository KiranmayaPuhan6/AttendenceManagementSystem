using AMS.Services.Services.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AMS.Services.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<string> UploadImageAsync(IFormFile objfile)
        {
            try
            {
                string guid = Guid.NewGuid().ToString();
                string[] allowedExtension = new string[] { ".jpg", ".jpeg", ".png", ".jfif" };
                if (objfile.Length > 0)
                {
                    if (!Directory.Exists(_environment.WebRootPath + "\\Upload\\"))
                    {
                        Directory.CreateDirectory(_environment.WebRootPath + "\\Upload\\");
                    }
                    string extension = Path.GetExtension(objfile.FileName);
                    if (allowedExtension.Contains(extension))
                    {
                        using (FileStream fileStream = File.Create(_environment.WebRootPath + "\\Upload\\" + guid + objfile.FileName))
                        {
                            objfile.CopyTo(fileStream);
                            fileStream.Flush();
                            return "\\Upload\\" + guid + objfile.FileName;
                        }
                    }
                    else
                    {
                        return "Not a valid type";
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
        public async Task<string> DeleteImageAsync(string path)
        {
            FileInfo fileInfo = new FileInfo(_environment.WebRootPath + path);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
                return "Deleted";
            }
            else
            {
                return null;
            }
        }
    }
}
