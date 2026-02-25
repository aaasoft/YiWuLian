using Microsoft.AspNetCore.Components;
using Quick.Protocol;
using YiWuLian.Server.Core.Interfaces.Device;

namespace YiWuLian.Server.Components.Pages;

public partial class DeviceConnection : ComponentBase, IDisposable
{
    private IEnumerable<DeviceConnectionInfo> Elements;

    private Timer timer;

    protected override void OnInitialized()
    {
        refresh(null);
        timer = new Timer(refresh, null, 1000, 1000);
    }

    private void refresh(object _)
    {
        Elements = Manager.Instance.ConnectedDevices;
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }
}