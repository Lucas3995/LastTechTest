using LastTechTest.Dominio.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastTechTest.Persistencia.Configurations;

public class UserMfaConfiguration : IEntityTypeConfiguration<UserMfa>
{
    public void Configure(EntityTypeBuilder<UserMfa> builder)
    {
        builder.ToTable("UserMfa");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SecretKey)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Enabled)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}