using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AccountAPI.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}