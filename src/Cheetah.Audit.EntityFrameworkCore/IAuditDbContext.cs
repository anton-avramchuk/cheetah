using Microsoft.EntityFrameworkCore;

namespace Cheetah.Audit.EntityFrameworkCore;

public interface IAuditDbContext
{
    DbSet<AuditEntry> AuditEntries { get; }
}
