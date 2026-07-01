using Microsoft.EntityFrameworkCore;

namespace YiWuLian.Server.Models;

public class ConfigDbContextProxy
{
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<YIS_Device>();
        {
            var entity = modelBuilder.Entity<YIS_NoticeLog>();
            entity.HasIndex(t => new { t.DeviceId, t.Time }).IsDescending();
            entity.HasIndex(t => new { t.DeviceName, t.Time }).IsDescending();
            entity.HasIndex(t => t.Time).IsDescending();
        }
        {
            var entity = modelBuilder.Entity<YIS_ConnectionLog>();
            entity.HasIndex(t => new { t.DeviceId, t.Time }).IsDescending();
            entity.HasIndex(t => new { t.DeviceName, t.Time }).IsDescending();
            entity.HasIndex(t => t.Time).IsDescending();
        }
    }
}
