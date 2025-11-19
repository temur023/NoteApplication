using Clean.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clean.Infrastructure.Configuration;

public class NoteConfiguration:IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.HasKey(x=>x.Id);
        builder.Property(x=>x.Title).HasMaxLength(50).IsRequired();
        builder.Property(x=>x.Content).HasMaxLength(500).IsRequired();
    }
}