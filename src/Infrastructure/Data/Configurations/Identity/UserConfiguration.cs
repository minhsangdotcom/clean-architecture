using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DayOfBirth).HasColumnType("date");
        builder.Property(x => x.Username).HasColumnType("citext");
        builder.HasIndex(x => x.Username).IsUnique();
        builder.Property(x => x.Email).HasColumnType("citext");
        builder.HasIndex(x => x.Email).IsUnique();

        builder.HasIndex(x => x.CreatedAt);

        builder.OwnsOne(
            x => x.Address,
            address =>
            {
                address.Property(x => x.Street).IsRequired();

                address.Property(x => x.Province).IsRequired();
                address.Property(x => x.ProvinceId).IsRequired();

                address.Property(x => x.District).IsRequired();
                address.Property(x => x.DistrictId).IsRequired();

                address.Property(x => x.CommuneId);
                address.Property(x => x.Commune);
            }
        );
    }
}
