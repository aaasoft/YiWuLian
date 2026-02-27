using Microsoft.AspNetCore.Components;
using Quick.EntityFrameworkCore.Plus;
using YiWuLian.Server.Models;

namespace YiWuLian.Server.Components.Pages;

public partial class DeviceConnection : ComponentBase, IDisposable
{
    private IEnumerable<YIS_Device> Elements;

    private Timer timer;

    protected override void OnInitialized()
    {
        refresh(null);
        timer = new Timer(refresh, null, 1000, 1000);
    }

    private void refresh(object _)
    {
        Elements = ConfigDbContext.CacheContext.Query<YIS_Device>()
            .OrderByDescending(t => t.ConnectionInfo.IsConnected);
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }
}