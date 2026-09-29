using MroczwareFramework.Models.Template;

namespace MroczwareFramework.Services.TemplateService
{
    public interface ITemplateService
    {
        Task<TemplateModel?> Get(string templateName, string culture);
    }
}
