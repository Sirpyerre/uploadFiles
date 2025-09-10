using System.Text.RegularExpressions;

namespace FileUploadApi.Application.helpers
{
    public static class FilePathHelper
    {
        /// <summary>
        /// Normalize filename to be used as a path
        /// </summary>
        public static string NormalizeFileName(string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(originalFileName));
                
            var name = originalFileName.Trim().ToLower();
            
            name = name.Replace(" ", "_");
            
            name = Regex.Replace(name, @"[^a-z0-9\.\-_]", "");
            
            return name;
        }

        public static string GetPhotoPath(Guid userId, string fileName)
        {
            var normalizedFileName = NormalizeFileName(fileName);
            return $"{userId}/photo/{normalizedFileName}";
        }
    }
}