using Clean.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clean.Application.Abstractions;

public interface IDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}