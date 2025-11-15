using Domain.Aggregates.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(x => new { x.PermissionId, x.RoleId });

        builder.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId);
        builder.HasOne(x => x.Role).WithMany(x => x.Permissions).HasForeignKey(x => x.RoleId);
    }
}
