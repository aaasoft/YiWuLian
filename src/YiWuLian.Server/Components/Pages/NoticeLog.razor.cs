using YiWuLian.Server.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Quick.EntityFrameworkCore.Plus;
using YiQiDong.Core.Utils;

namespace YiWuLian.Server.Components.Pages;

public partial class NoticeLog : ComponentBase, IDisposable
{
    [Parameter]
    public string DeviceId { get; set; }

    private IEnumerable<YIS_NoticeLog> Elements;
    private ConfigDbContext dbContext;

    protected override void OnInitialized()
    {
        dbContext = new();
    }

    protected override void OnParametersSet()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (string.IsNullOrEmpty(DeviceId))
            Elements = dbContext.Set<YIS_NoticeLog>().OrderByDescending(t=>t.Time);
        else
            Elements = dbContext.Set<YIS_NoticeLog>().Where(t => t.DeviceId == DeviceId).OrderByDescending(t=>t.Time);
    }

    public void Dispose()
    {
        dbContext.Dispose();
    }
}
