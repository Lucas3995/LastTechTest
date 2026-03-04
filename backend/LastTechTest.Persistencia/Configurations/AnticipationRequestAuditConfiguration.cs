using LastTechTest.Dominio.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastTechTest.Persistencia.Configurations;

public sealed class AnticipationRequestAuditConfiguration : IEntityTypeConfiguration<AnticipationRequestAudit>
{
    public void Configure(EntityTypeBuilder<AnticipationRequestAudit> builder)
    {
        builder.ToTable("AnticipationRequestAudits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RequestId).IsRequired();
        builder.Property(x => x.Action).IsRequired().HasMaxLength(32);
        builder.Property(x => x.AtUtc).IsRequired();
    }
}
