using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MroczwareFramework.Models
{
    public abstract class BaseModel
    {      
        public DateTime Created { get; set; }
        public int CreatedById { get; set; } = 1;
        public DateTime? Updated { get; set; }
        public int? UpdatedById { get; set; }
    }

    public class BaseModelConfiguration : IEntityTypeConfiguration<BaseModel>
    {
        public void Configure(EntityTypeBuilder<BaseModel> modelBuilder)
        {
            modelBuilder.Property(e => e.Created)
                .IsRequired()
                .ValueGeneratedOnAdd();

            modelBuilder.Property(e => e.Updated)
                .ValueGeneratedOnUpdate();
        }
    }
}
