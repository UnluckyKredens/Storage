using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class DocumentNumberSequenceConfiguration : IEntityTypeConfiguration<DocumentNumberSequence>
{
    public void Configure(EntityTypeBuilder<DocumentNumberSequence> builder)
    {
        builder.ToTable("DocumentNumberSequences");

        builder.HasKey(sequence => sequence.DocumentNumberSequenceId);

        builder.Property(sequence => sequence.DocumentNumberSequenceId)
            .ValueGeneratedOnAdd();

        builder.Property(sequence => sequence.DocumentType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(sequence => sequence.Year)
            .IsRequired();

        builder.Property(sequence => sequence.LastNumber)
            .IsRequired();

        builder.Property(sequence => sequence.UpdatedOnUtc)
            .IsRequired();

        builder.HasIndex(sequence => new { sequence.WarehouseId, sequence.DocumentType, sequence.Year })
            .IsUnique();

        builder.HasOne(sequence => sequence.Warehouse)
            .WithMany()
            .HasForeignKey(sequence => sequence.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
