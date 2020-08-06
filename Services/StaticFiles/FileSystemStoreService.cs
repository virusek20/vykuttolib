using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using vykuttolib.Configuration;
using FileInfo = vykuttolib.Services.StaticFiles.IFileStoreService.FileInfo;

namespace vykuttolib.Services.StaticFiles
{
    public class FileSystemStoreService : IFileStoreService
    {
        private readonly FilePathConfiguration _config = new FilePathConfiguration();

        private readonly ILogger<FileSystemStoreService> _logger;
        private readonly IMimeTypeService _mimeTypeService;

        public FileSystemStoreService(IConfiguration config, ILogger<FileSystemStoreService> logger, IMimeTypeService mimeTypeService)
        {
            config.GetSection("StaticFilePath").Bind(_config);

            _logger = logger;
            _mimeTypeService = mimeTypeService;
        }

        public Task<FileInfo> CopyFile(string sourceIdentifier, string destinationGroup)
        {
            var resourcePath = TranslateIdentifier(sourceIdentifier);
            if (!File.Exists(resourcePath)) throw new FileNotFoundException("Cannot find file to copy", resourcePath);
            var fileName = Path.GetFileName(resourcePath);

            var groupPath = TranslateIdentifier(destinationGroup);
            if (!Directory.Exists(groupPath)) Directory.CreateDirectory(groupPath);

            File.Copy(resourcePath, groupPath);
            var copyPath = Path.Combine(destinationGroup, fileName);

            _logger.LogInformation($"Copied file '{resourcePath}' to '{copyPath}'");
            return Task.FromResult(new FileInfo
            {
                Identifier = copyPath,
                Url = new Uri(copyPath, UriKind.Relative)
            });
        }

        public Task<FileInfo> CopyFile(string sourceIdentifier, string destinationGroup, string destinationIdentifier)
        {
            var resourcePath = TranslateIdentifier(sourceIdentifier);
            if (!File.Exists(resourcePath)) throw new FileNotFoundException("Cannot find file to copy", resourcePath);

            TranslateIdentifier(destinationIdentifier); // Just check if valid
            if (destinationIdentifier.Contains('/') || destinationIdentifier.Contains('\\')) throw new ArgumentException("Destination identifier cannot contain a directory name", nameof(destinationIdentifier));

            var groupPath = TranslateIdentifier(destinationGroup);
            if (!Directory.Exists(groupPath)) Directory.CreateDirectory(groupPath);

            File.Copy(resourcePath, groupPath);
            var copyPath = Path.Combine(destinationGroup, destinationIdentifier);

            _logger.LogInformation($"Copied file '{resourcePath}' to '{copyPath}'");
            return Task.FromResult(new FileInfo
            {
                Identifier = copyPath,
                Url = new Uri(copyPath, UriKind.Relative)
            });
        }

        public Task<FileInfo> CopyGroup(string sourceGroup, string destinationGroup)
        {
            var sourceGroupPath = TranslateIdentifier(sourceGroup);
            if (!Directory.Exists(sourceGroupPath)) throw new FileNotFoundException("Cannot find source folder", sourceGroupPath);

            var destinationGroupPath = TranslateIdentifier(destinationGroup);
            File.Copy(sourceGroupPath, destinationGroupPath);

            _logger.LogInformation($"Copied group '{sourceGroupPath}' to '{destinationGroupPath}'");
            return Task.FromResult(new FileInfo
            {
                Identifier = destinationGroup,
                Url = new Uri(destinationGroup, UriKind.Relative)
            });
        }

        public Task RemoveFile(string fileIdentifier)
        {
            var resourcePath = TranslateIdentifier(fileIdentifier);
            if (!File.Exists(resourcePath)) throw new FileNotFoundException("Cannot find file to delete", resourcePath);

            File.Delete(resourcePath);

            _logger.LogInformation($"Removed file '{resourcePath}'");
            return Task.CompletedTask;
        }

        public Task RemoveGroup(string groupIdentifier)
        {
            var resourcePath = TranslateIdentifier(groupIdentifier);
            if (!Directory.Exists(resourcePath)) throw new FileNotFoundException("Cannot find group to delete", resourcePath);

            Directory.Delete(resourcePath, true);

            _logger.LogInformation($"Removed group '{resourcePath}'");
            return Task.CompletedTask;
        }

        public async Task<FileInfo> StoreBytes(byte[] data, string groupIdentifier, string mimeType)
        {
            using var ms = new MemoryStream(data);
            return await Store(ms, groupIdentifier, mimeType);
        }

        public async Task<FileInfo> StoreFormFile(IFormFile data, string groupIdentifier, string mimeType)
        {
            using var s = data.OpenReadStream();
            return await Store(s, groupIdentifier, mimeType);
        }

        public async Task<FileInfo> StoreMultipartFile(MultipartSection data, string groupIdentifier, string mimeType)
        {
            return await Store(data.Body, groupIdentifier, mimeType);
        }

        private async Task<FileInfo> Store(Stream data, string groupIdentifier, string mimeType)
        {
            var identifier = GenerateRandomIdentifier(mimeType);
            var groupPath = TranslateIdentifier(groupIdentifier);
            var filePath = Path.Combine(groupPath, identifier);

            using var stream = File.Create(filePath);
            await data.CopyToAsync(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var mimeCorrect = _mimeTypeService.CheckSignature(mimeType, stream);
            if (!mimeCorrect)
            {
                stream.Close();
                File.Delete(filePath);
                throw new InvalidDataException("Supplied data does not match mime-type signature");
            }

            var fileIdentifier = Path.Combine(groupIdentifier, identifier);

            return new FileInfo
            {
                Identifier = fileIdentifier,
                Url = new Uri(fileIdentifier, UriKind.Relative)
            };
        }

        private string TranslateIdentifier(string identifier)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));
            if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentException("Identifier cannot be empty or whitespace", nameof(identifier));
            if (Path.IsPathRooted(identifier)) throw new ArgumentException("Identifier must be a relative path", nameof(identifier));
            if (identifier == ".." || identifier.Contains("../")) throw new ArgumentException("Identifier cannot traverse up the filesystem", nameof(identifier));

            return Path.Combine(_config.Path, identifier);
        }

        private string GenerateRandomIdentifier(string mimeType)
        {
           return $"{Guid.NewGuid()}.{_mimeTypeService.DetermineExtension(mimeType) ?? "unknown"}";
        }
    }
}
