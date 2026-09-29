using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MroczwareFramework.Models.Culture
{
    public class CultureModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string StrongName { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Icon { get; set; }
    }

    public class CultureModelConfiguration : IEntityTypeConfiguration<CultureModel>
    {
        public void Configure(EntityTypeBuilder<CultureModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            var seedDate = new DateTime(2025, 1, 1);

            builder.HasData(
                new CultureModel
                {
                    Id = 1,
                    Name = "Polska",
                    StrongName = "PL",
                    DisplayName = "Polska",
                    Created = seedDate,
                    CreatedById = 1
                },
                new CultureModel
                {
                    Id = 2,
                    Name = "Angielksa",
                    StrongName = "EN",
                    DisplayName = "Angielska",
                    Created = seedDate,
                    CreatedById = 1
                }
            );
        }
    }
}
