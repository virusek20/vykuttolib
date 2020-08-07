using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace vykuttolib.Services.StaticFiles
{
    public interface IFileStoreService
    {
        /// <summary>
        /// Information of file in store
        /// </summary>
        public struct FileInfo
        {
            public string Identifier;
            public Uri Url;
        }

        /// <summary>
        /// Stores an array of bytes in the store under a group
        /// New group will be created in case it doesn't exist yet
        /// </summary>
        /// <param name="data">Stored data</param>
        /// <param name="groupIdentifier">Target group identifier</param>
        /// <param name="extension">MimeType of data</param>
        /// <returns></returns>
        Task<FileInfo> StoreBytes(byte[] data, string groupIdentifier, string mimeType);

        /// <summary>
        /// Stores a form file in the store under a group
        /// New group will be created in case it doesn't exist yet
        /// </summary>
        /// <param name="data">Stored data</param>
        /// <param name="groupIdentifier">Target group identifier</param>
        /// <param name="extension">MimeType of data</param>
        /// <returns></returns>
        Task<FileInfo> StoreFormFile(IFormFile data, string groupIdentifier, string mimeType);

        /// <summary>
        /// Stores a multipart section in the store under a group
        /// New group will be created in case it doesn't exist yet
        /// </summary>
        /// <param name="data">Stored data</param>
        /// <param name="groupIdentifier">Target group identifier</param>
        /// <param name="extension">MimeType of data</param>
        /// <returns></returns>
        Task<FileInfo> StoreMultipartFile(MultipartSection data, string groupIdentifier, string mimeType);

        /// <summary>
        /// Copies a file from one group to another, file identifier will be reused in target group
        /// </summary>
        /// <param name="sourceIdentifier">Source file identifier</param>
        /// <param name="destinationGroup">Destination group identifier</param>
        /// <returns>Info about created copy</returns>
        Task<FileInfo> CopyFile(string sourceIdentifier, string destinationGroup);

        /// <summary>
        /// Copies all group files to another, file identifier will be reused in target group
        /// </summary>
        /// <param name="sourceIdentifier">Source file identifier</param>
        /// <param name="destinationGroup">Destination group identifier</param>
        /// <returns>Info about created copy</returns>
        Task<FileInfo> CopyGroup(string sourceGroup, string destinationGroup);

        /// <summary>
        /// Copies a file from one group to another with a different file identifier
        /// </summary>
        /// <param name="sourceIdentifier">Source file identifier</param>
        /// <param name="destinationGroup">Destination group identifier</param>
        /// <param name="destinationIdentifier">Identifier of file in new group</param>
        /// <returns>Info about created copy</returns>
        Task<FileInfo> CopyFile(string sourceIdentifier, string destinationGroup, string destinationIdentifier);

        /// <summary>
        /// Removes a file from the store
        /// </summary>
        /// <param name="identifier">Identifier of file to be removed</param>
        Task RemoveFile(string fileIdentifier);

        /// <summary>
        /// Removes a group and all associated files from the store
        /// </summary>
        /// <param name="groupIdentifier">Identifier of group to be removed</param>
        Task RemoveGroup(string groupIdentifier);

        /// <summary>
        /// Resolves an identefier into an Url pointing to that resource.
        /// </summary>
        /// <param name="fileIdentifier">Requested file</param>
        /// <returns>Link to file</returns>
        Task<Uri> ResolveIdentifier(string fileIdentifier);
    }
}