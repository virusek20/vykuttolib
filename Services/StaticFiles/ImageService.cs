using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using vykuttolib.Configuration;
using static vykuttolib.Services.StaticFiles.IImageService;

namespace vykuttolib.Services.StaticFiles
{
    public class ImageService : IImageService
    {
        private readonly FilePathConfiguration _config = new FilePathConfiguration();

        public ImageService(IConfiguration config)
        {
            config.GetSection("StaticFilePath").Bind(_config);
        }

        public async Task<FileUpload> OnPostUploadAsync(IFormFile file, string folderName)
        {
            var filePath = GenerateRandomName(folderName);

            using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            // This way we don't pass the actual /wwwroot
            return new FileUpload
            {
                Url = filePath.Substring(filePath.IndexOf("/")),
                RelativePath = Path.Combine(folderName, Path.GetFileName(filePath))
            };
        }

        public async Task<FileUpload> OnPostUploadAsync(byte[] file, string folderName)
        {
            var filePath = GenerateRandomName(folderName);

            using var stream = File.Create(filePath);
            await stream.WriteAsync(file, 0, file.Length);

            // This way we don't pass the actual /wwwroot
            return new FileUpload
            {
                Url = filePath.Substring(filePath.IndexOf("/")),
                RelativePath = Path.Combine(folderName, Path.GetFileName(filePath))
            };
        }

        public Uri GetUrl(HttpRequest request, string filename)
        {
            return new Uri(filename.Replace('\\', '/'), UriKind.Relative);
        }

        private string GenerateRandomName(string folderName)
        {
            var imageIdentifier = Guid.NewGuid().ToString() + ".jpg";
            var folderPath = Path.Combine(_config.Path, folderName);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            return Path.Combine(_config.Path, folderName, imageIdentifier);
        }

        public void RemoveFile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename) || filename.StartsWith('/') || filename.Contains("..")) throw new InvalidOperationException("Cannot remove files higher than spcified path, please only use relative paths.");

            var resourcePath = Path.Combine(_config.Path, filename);
            if (Directory.Exists(resourcePath)) Directory.Delete(resourcePath, true);
            else File.Delete(resourcePath);
        }

        public Task<FileUpload> CopyFile(string sourceFile, string destFile)
        {
            var fileName = Path.GetFileName(sourceFile);

            var folderPath = Path.Combine(_config.Path, destFile);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var sourceFolderPath = Path.Combine(_config.Path, sourceFile);
            var filePath = Path.Combine(folderPath, fileName);

            File.Copy(sourceFolderPath, filePath);
            
            return Task.FromResult(new FileUpload
            {
                Url = filePath.Substring(filePath.IndexOf("/")),
                RelativePath = Path.Combine(destFile, fileName)
            });
        }
    }
}
