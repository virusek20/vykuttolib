using System.IO;

namespace vykuttolib.Services.StaticFiles
{
    public interface IMimeTypeService
    {
        /// <summary>
        /// Determines the correct extension for a given mime-type
        /// </summary>
        /// <param name="mimeType">Mime-type to be determined</param>
        /// <returns>File extension</returns>
        string DetermineExtension(string mimeType);

        /// <summary>
        /// Checks whether the provided date matches the signature associated with the mime-type
        /// </summary>
        /// <param name="mimeType">Assumed mime-type of data</param>
        /// <param name="data">Data to be checked</param>
        /// <returns>Whether data signature matches the provided mime-type</returns>
        bool CheckSignature(string mimeType, Stream data);
    }
}
