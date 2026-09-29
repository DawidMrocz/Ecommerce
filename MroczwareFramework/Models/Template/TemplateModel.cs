using MroczwareFramework.Models.Culture;

namespace MroczwareFramework.Models.Template
{
    public class TemplateModel : BaseModel
    {
        public int Id { get; set; }
        public string Body { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string StrongName { get; set; } = null!;
        public string? Description { get; set; }
        public string? DataStrongName { get; set; }
        public int CultureId { get; set; }
        public CultureModel Culture { get; set; } = null!;
    }
}
