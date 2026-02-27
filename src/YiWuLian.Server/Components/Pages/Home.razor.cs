using Microsoft.AspNetCore.Components;
using Quick.EntityFrameworkCore.Plus;
using YiWuLian.Server.Core.Interfaces.Device;

namespace YiWuLian.Server.Components.Pages;

public partial class Home : ComponentBase
{
    private int GetDevicesCount() => ConfigDbContext.CacheContext.Query<Models.YIS_Device>().Length;

    private int GetConnectionCount() => ConfigDbContext.CacheContext.Query<Models.YIS_Device>(t => t.ConnectionInfo.IsConnected).Length;
}
