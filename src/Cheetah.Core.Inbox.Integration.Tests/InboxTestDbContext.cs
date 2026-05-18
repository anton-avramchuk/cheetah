using Cheetah.Core.Inbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.Inbox.Integration.Tests;

public class InboxTestDbContext : DbContext, IInboxDbContext
{
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;

    public InboxTestDbContext(DbContextOptions<InboxTestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInbox();
    }
}
