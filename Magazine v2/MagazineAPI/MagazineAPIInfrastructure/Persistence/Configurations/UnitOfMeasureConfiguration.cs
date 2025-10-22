using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitsOfMeasure");

        builder.HasKey(unitOfMeasure => unitOfMeasure.Id);

        builder.Property(unitOfMeasure => unitOfMeasure.Id)
            .ValueGeneratedOnAdd();

        builder.Property(unitOfMeasure => unitOfMeasure.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(unitOfMeasure => unitOfMeasure.Symbol)
            .HasMaxLength(20)
            .IsRequired();
    }
}
