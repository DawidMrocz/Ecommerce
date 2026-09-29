using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.ApiModels
{
    public class AddPhotoRequest
    {
        [FromForm(Name = "file")]
        public IFormFile Photo { get; set; }
    }
}
