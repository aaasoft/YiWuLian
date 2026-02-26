using Microsoft.EntityFrameworkCore;

namespace YiWuLian.Server.Models;

public class ConfigDbContextProxy
{
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<YIS_Device>();
        var entity_LC_EventLog = modelBuilder.Entity<YIS_NoticeLog>();
        entity_LC_EventLog.HasIndex(t => new { t.DeviceId, t.Time }).IsDescending();
        entity_LC_EventLog.HasIndex(t => t.Time).IsDescending();
    }
}
