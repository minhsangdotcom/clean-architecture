using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class UserResetPasswordConfiguration : IEntityTypeConfiguration<UserResetPassword>
{
    public void Configure(EntityTypeBuilder<UserResetPassword> builder)
    {
        builder.HasKey(x => x.Id);
        builder
            .HasOne(x => x.User)
            .WithMany(x => x.UserResetPasswords)
            .HasForeignKey(x => x.UserId);
    }
}
