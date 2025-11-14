using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class UserResetPasswordConfiguration : IEntityTypeConfiguration<UserPasswordReset>
{
    public void Configure(EntityTypeBuilder<UserPasswordReset> builder)
    {
        builder.HasKey(x => x.Id);
        builder
            .HasOne(x => x.User)
            .WithMany(x => x.PasswordResetRequests)
            .HasForeignKey(x => x.UserId);
    }
}
