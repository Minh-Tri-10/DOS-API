using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CategoriesAPI.Services.Interfaces;
using System.Security.Principal;

namespace CategoriesAPI.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var cloudName = config["CloudinarySettings:CloudName"];
            var apiKey = config["CloudinarySettings:ApiKey"];
            var apiSecret = config["CloudinarySettings:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, memoryStream),
                Folder = "products"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                return uploadResult.SecureUrl.ToString();

            throw new Exception("Upload ảnh thất bại.");
        }
    }
}
