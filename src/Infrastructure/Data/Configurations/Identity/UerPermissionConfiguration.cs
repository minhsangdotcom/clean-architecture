using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class UerPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.HasKey(x => new { x.PermissionId, x.UserId });

        builder.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId);
        builder.HasOne(x => x.User).WithMany(x => x.Permissions).HasForeignKey(x => x.UserId);
    }
}
