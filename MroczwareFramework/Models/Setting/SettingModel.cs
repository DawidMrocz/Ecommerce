using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MroczwareFramework.Models.Setting;

namespace MroczwareFramework.Models.Setting
{
    public class SettingModel : BaseModel
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class SettingModelConfiguration : IEntityTypeConfiguration<SettingModel>
    {
        public void Configure(EntityTypeBuilder<SettingModel> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
