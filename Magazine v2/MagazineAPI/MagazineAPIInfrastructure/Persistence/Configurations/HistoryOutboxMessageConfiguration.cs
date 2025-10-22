using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public sealed class HistoryOutboxMessageConfiguration : IEntityTypeConfiguration<HistoryOutboxMessage>
{
    public void Configure(EntityTypeBuilder<HistoryOutboxMessage> builder)
    {
        builder.HasKey(message => message.Id);
        builder.Property(message => message.EntityType).HasMaxLength(80).IsRequired();
        builder.Property(message => message.Payload).IsRequired();
        builder.HasIndex(message => new { message.ProcessedOnUtc, message.CreatedOnUtc });
    }
}
