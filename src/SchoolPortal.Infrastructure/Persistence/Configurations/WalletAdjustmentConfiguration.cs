using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Infrastructure.Persistence.Configurations;

internal sealed class WalletAdjustmentConfiguration : IEntityTypeConfiguration<WalletAdjustment>
{
    public void Configure(EntityTypeBuilder<WalletAdjustment> builder)
    {
        builder.ToTable("WalletAdjustments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.BalanceAfter).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Reason).HasConversion<int>().IsRequired();
        builder.Property(x => x.Note).HasMaxLength(512);
        builder.Property(x => x.RequestId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.PerformedBy).HasMaxLength(128);

        builder.HasIndex(x => new { x.UserId, x.RequestId }).IsUnique();
        builder.HasIndex(x => x.OccurredAt);
    }
}
