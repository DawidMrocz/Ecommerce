using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MroczwareFramework.Models.Country
{
    public class CountryModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
    }

    public class CountryModelConfiguration : IEntityTypeConfiguration<CountryModel>
    {
        public void Configure(EntityTypeBuilder<CountryModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            var seedDate = new DateTime(2025, 1, 1);

            builder.HasData(
                new CountryModel
                {
                    Id = 1,
                    Name = "Polska",
                    Code = "PL",
                    Created = seedDate,
                    CreatedById = 1
                },
                new CountryModel
                {
                    Id = 2,
                    Name = "Anglia",
                    Code = "EN",
                    Created = seedDate,
                    CreatedById = 1
                }
            );
        }
    }
}
