using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MroczwareFramework.Models.Role
{
    public class RoleModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string StrongName { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    public class RoleModelConfiguration : IEntityTypeConfiguration<RoleModel>
    {
        public void Configure(EntityTypeBuilder<RoleModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            var seedDate = new DateTime(2025, 1, 1);

            builder.HasData(
                new RoleModel
                {
                    Id = 1,
                    Name = "Admin",
                    StrongName = "Admin",
                    Description = "Admin",
                    Created = seedDate,
                    CreatedById = 1
                },
                 new RoleModel
                 {
                     Id = 2,
                     Name = "User",
                     StrongName = "User",
                     Description = "User",
                     Created = seedDate,
                     CreatedById = 1
                 }
            );
        }
    }
}
