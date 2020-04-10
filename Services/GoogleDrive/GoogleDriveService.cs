using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using vykuttolib.Configuration;

namespace vykuttolib.Services.GoogleDrive
{
    public class GoogleDriveService
    {
        private readonly GoogleDriveConfiguration _config = new GoogleDriveConfiguration();
        private readonly string[] _scopes = { DriveService.Scope.Drive };
        private readonly DriveService _service;
        
        public GoogleDriveService(IConfiguration config)
        {
            config.GetSection("GoogleDriveAPI").Bind(_config);
            GoogleCredential credential;
            using (var stream = new FileStream(_config.Credentials, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(_scopes);
            }
            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _config.ApplicationName,
            });
        }
        public async Task<Google.Apis.Drive.v3.Data.File> CreateFolderAsync(string folderName)
        {
            Google.Apis.Drive.v3.Data.File file = new Google.Apis.Drive.v3.Data.File();
            file.Name = folderName;
            file.MimeType = "application/vnd.google-apps.folder";
            FilesResource.CreateRequest request = _service.Files.Create(file);
            request.Fields = "id";
            return await request.ExecuteAsync();
        }

        [Obsolete("Use the version with explicit mimeType")]
        public async Task<Google.Apis.Upload.IUploadProgress> UploadAsync(Stream stream, string fileName, string folderId, Action<Google.Apis.Drive.v3.Data.File> UploadRequestResponseReceived)
        {
            return await UploadAsync(stream, fileName, folderId, "image/jpeg", UploadRequestResponseReceived);
        }

        public async Task<Google.Apis.Upload.IUploadProgress> UploadAsync(Stream stream, string fileName, string folderId, string mimeType, Action<Google.Apis.Drive.v3.Data.File> UploadRequestResponseReceived)
        {
            Google.Apis.Drive.v3.Data.File fileMetadata = new Google.Apis.Drive.v3.Data.File();
            fileMetadata.Name = fileName;
            fileMetadata.MimeType = mimeType;
            fileMetadata.Parents = new List<string> { folderId };
            FilesResource.CreateMediaUpload request;
            request = _service.Files.Create(fileMetadata, stream, mimeType);
            request.Fields = "id, webViewLink, thumbnailLink";
            request.ResponseReceived += UploadRequestResponseReceived;
            return await request.UploadAsync();
        }
        public void ShareFile(string fileId)
        {
            Permission newPermission = new Permission
            {
                Type = "anyone",
                Role = "reader"
            };

            _service.Permissions.Create(newPermission, fileId).Execute();
        }
        public async Task<Permission> AddPermissionAsync(string fileId, string type, string role)
        {
            Permission newPermission = new Permission();
            newPermission.Type = type;
            newPermission.Role = role;
            return await _service.Permissions.Create(newPermission, fileId).ExecuteAsync();
        }
        public async Task<string> DeleteFileAsync(string fileId)
        {
            return await _service.Files.Delete(fileId).ExecuteAsync();
        }
        public List<Google.Apis.Drive.v3.Data.File> List(string folderId)
        {
            List<Google.Apis.Drive.v3.Data.File> res = new List<Google.Apis.Drive.v3.Data.File>();
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(name, parents, webViewLink, thumbnailLink)";
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            foreach (var file in files)
            {
                if (file.Parents != null)
                {
                    if (file.Parents[0] == folderId) res.Add(file);
                }
            }
            return res;
        }
    }
}
