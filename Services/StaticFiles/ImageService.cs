using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using vykuttolib.Configuration;

namespace vykuttolib.Services.StaticFiles
{
    public class ImageService : IImageService
    {
        private readonly FilePathConfiguration _config = new FilePathConfiguration();

        public ImageService(IConfiguration config)
        {
            config.GetSection("StaticFilePath").Bind(_config);
        }

        public async Task<string> OnPostUploadAsync(IFormFile file)
        {
            var filePath = GenerateRandomName();

            using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            // This way we don't pass the actual /wwwroot
            return filePath.Substring(filePath.IndexOf("/"));
        }

        public async Task<string> OnPostUploadAsync(byte[] file)
        {
            var filePath = GenerateRandomName();

            using var stream = File.Create(filePath);
            await stream.WriteAsync(file, 0, file.Length);

            // This way we don't pass the actual /wwwroot
            return filePath.Substring(filePath.IndexOf("/"));
        }

        public Uri GetUrl(HttpRequest request, string filename)
        {
            return new Uri(filename.Replace('\\', '/'), UriKind.Relative);
        }

        private string GenerateRandomName()
        {
            var imageIdentifier = Guid.NewGuid().ToString() + ".jpg";
            return Path.Combine(_config.Path, imageIdentifier);
        }

        public void RemoveFile(string filename)
        {
            filename = Path.GetFileName(filename);
            var resourcePath = Path.Combine(_config.Path, filename);
            File.Delete(resourcePath);
        }
    }
}
