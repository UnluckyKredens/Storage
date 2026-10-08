using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class ContractorConfiguration : IEntityTypeConfiguration<Contractor>
{
    public void Configure(EntityTypeBuilder<Contractor> builder)
    {
        builder.ToTable("Contractors");

        builder.HasKey(contractor => contractor.ContractorId);

        builder.Property(contractor => contractor.ContractorId)
            .HasColumnName("ContractorID")
            .ValueGeneratedOnAdd();

        builder.Property(contractor => contractor.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(contractor => contractor.TaxNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(contractor => contractor.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(contractor => contractor.Email)
            .HasMaxLength(255);

        builder.Property(contractor => contractor.Phone)
            .HasMaxLength(50);

        builder.Property(contractor => contractor.Address)
            .HasMaxLength(500);
    }
}
