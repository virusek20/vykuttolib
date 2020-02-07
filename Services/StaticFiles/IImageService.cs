using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace vykuttolib.Services.StaticFiles
{
    /// <summary>
    /// Stores images on local storage
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Saves an uploaded file to the static files directory under a random GUID name
        /// The name is not guranteed to be unique, but it "should" be fine
        /// </summary>
        /// <param name="file">Uploaded file</param>
        /// <returns>Relative path to file</returns>
        Task<string> OnPostUploadAsync(IFormFile file);

        /// <summary>
        /// Saves a file file to the static files directory under a random GUID name
        /// The name is not guranteed to be unique, but it "should" be fine
        /// </summary>
        /// <param name="file">File data</param>
        /// <returns>Relative path to file</returns>
        Task<string> OnPostUploadAsync(byte[] file);

        /// <summary>
        /// Removes a file from the static resource folder
        /// </summary>
        /// <param name="filename">File path as provided by OnPostUploadAsync to be removed</param>
        void RemoveFile(string filename);

        /// <summary>
        /// Generates an Uri pointing to a resources using the same schema as the original request.
        /// </summary>
        /// <param name="request">Currently served request</param>
        /// <param name="filename">Filename of requested path, relative to wwwroot</param>
        /// <returns>Uri pointing to requested resource</returns>
        Uri GetUrl(HttpRequest request, string filename);
    }
}
