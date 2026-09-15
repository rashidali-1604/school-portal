using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Infrastructure.Context.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.FirstName).HasMaxLength(128).IsRequired();
            builder.Property(x => x.LastName).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Role).HasConversion<int>().IsRequired();
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();
            builder.Property(x => x.WalletBalance).HasColumnType("decimal(18,2)").IsRequired();

            builder.Property(x => x.Version).IsConcurrencyToken().IsRequired();

            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.LastName);
            builder.HasIndex(x => x.Status);

            builder.HasMany(x => x.Adjustments)
                .WithOne()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.FindNavigation(nameof(User.Adjustments))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
