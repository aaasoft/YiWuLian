using Microsoft.AspNetCore.Components;
using Quick.EntityFrameworkCore.Plus;

namespace YiWuLian.Server.Components.Pages;

public partial class Home : ComponentBase,IDisposable
{
    private int GetDevicesCount() => ConfigDbContext.CacheContext.Query<Models.YIS_Device>().Length;

    private int GetConnectionCount() => ConfigDbContext.CacheContext.Query<Models.YIS_Device>(t => t.ConnectionInfo.IsConnected).Length;

    private Timer timer;

    protected override void OnInitialized()
    {
        timer = new Timer(refresh, null, 1000, 1000);
    }
    
    private void refresh(object _)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }
}
