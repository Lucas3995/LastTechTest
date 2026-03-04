using LastTechTest.Dominio.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastTechTest.Persistencia.Configurations;

public sealed class AnticipationRequestConfiguration : IEntityTypeConfiguration<AnticipationRequest>
{
    public void Configure(EntityTypeBuilder<AnticipationRequest> builder)
    {
        builder.ToTable("AnticipationRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.RequestedAmount).HasPrecision(18, 4);
        builder.Property(x => x.GrossAmount).HasPrecision(18, 4);
        builder.Property(x => x.FeesAmount).HasPrecision(18, 4);
        builder.Property(x => x.NetAmount).HasPrecision(18, 4);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.RequestedAtUtc).IsRequired();
    }
}