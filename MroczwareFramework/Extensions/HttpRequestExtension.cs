using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Text;

namespace MroczwareFramework.Extensions
{
    public static class HttpRequestExtension
    {
        public static string JsonSerializeRequest(this HttpRequest request)
        {
            object body;
            try
            {
                body = request.GetRequestBody(checkIfHasFromContentType: true);
            }
            catch (Exception ex)
            {
                body = "Body serialization error - " + ex.Message;
            }

            object form;
            try
            {
                form = request.GetRequestForm();
            }
            catch (Exception ex2)
            {
                form = "Form serialization error - " + ex2.Message;
            }

            var value = new
            {
                Method = request.Method,
                Url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                Body = body,
                Form = form
            };
            return JsonConvert.SerializeObject(value);
        }

        public static object? GetRequestBody(this HttpRequest request, bool checkIfHasFromContentType = false)
        {
            if (request?.Body == null || (checkIfHasFromContentType && request.HasFormContentType))
            {
                return null;
            }

            request.Body.Position = 0L;
            byte[] array = new byte[Convert.ToInt32(request.ContentLength)];
            request.Body.Read(array, 0, array.Length);
            string @string = Encoding.UTF8.GetString(array);
            request.Body.Position = 0L;
            return JsonConvert.DeserializeObject(@string);
        }

        public static Dictionary<string, object>? GetRequestForm(this HttpRequest request)
        {
            if (!request.HasFormContentType || request.Form == null || !request.Form.Any())
            {
                return null;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (KeyValuePair<string, StringValues> item in request.Form)
            {
                dictionary[item.Key] = item.Value;
            }

            if (request.Form.Files != null && request.Form.Files.Any())
            {
                IEnumerable<IGrouping<string, IFormFile>> enumerable = from f in request.Form.Files
                                                                       group f by f.Name;
                foreach (IGrouping<string, IFormFile> item2 in enumerable)
                {
                    dictionary[item2.Key] = item2.Select((IFormFile f) => new
                    {
                        FileName = f.FileName,
                        FileLength = f.Length,
                        FileContentType = f.ContentType
                    });
                }
            }

            return dictionary;
        }
    }
}
