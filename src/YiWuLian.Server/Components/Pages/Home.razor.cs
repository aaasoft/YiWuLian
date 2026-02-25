using System;
using Microsoft.AspNetCore.Components;
using Quick.EntityFrameworkCore.Plus;
using YiWuLian.Server.Core.Interfaces.Device;

namespace YiWuLian.Server.Components.Pages;

public partial class Home : ComponentBase
{
    private int GetDevicesCount()
    {
        using (var dbContext = new ConfigDbContext())
        {
            return dbContext.Set<Models.YIS_Device>().Count();
        }
    }

    private int GetConnectionCount()
    {
        return Manager.Instance.ConnectedDevices.Length;
    }
}
