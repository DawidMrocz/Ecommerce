using Microsoft.AspNetCore.Http;

namespace MroczwareFramework.Extensions
{
    public static class FormFileExtension
    {
        /// <summary>
        /// Przekształca IFormFile na tablice byte[]
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<byte[]> ConvertFileToByteArrayAsync(this IFormFile file)
        {
            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
