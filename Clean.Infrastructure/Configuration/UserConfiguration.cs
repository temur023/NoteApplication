using Clean.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clean.Infrastructure.Configuration;

public class UserConfiguration:IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x=>x.Name).HasMaxLength(50).IsRequired();
        builder.HasMany(x=>x.Notes).WithOne(x=>x.User).HasForeignKey(x=>x.UserId);
        builder.Property(x=>x.PasswordHash).IsRequired();
    }
}