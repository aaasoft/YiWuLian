using YiWuLian.Server.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Quick.EntityFrameworkCore.Plus;

namespace YiWuLian.Server.Components.Pages;

public partial class NoticeLog : ComponentBase
{
    [Parameter]
    public string DeviceId { get; set; }

    private MudDataGrid<YIS_NoticeLog> dataGrid;
    private string searchString = null;

    private Task OnSearch(string text)
    {
        searchString = text;
        return dataGrid.ReloadServerData();
    }

    private async Task<GridData<YIS_NoticeLog>> LoadServerDataAsync(GridState<YIS_NoticeLog> state, CancellationToken cancellationToken)
    {
        using (var dbContext = new ConfigDbContext())
        {
            IQueryable<YIS_NoticeLog> query = query = dbContext.Set<YIS_NoticeLog>();
            if (!string.IsNullOrEmpty(DeviceId))
                query = query.Where(t => t.DeviceId == DeviceId).OrderByDescending(t => t.Time);
            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(t => t.DeviceId.Contains(searchString) || t.DeviceName.Contains(searchString));
            query = query.OrderByDescending(t => t.Time);

            var totalItems = query.Count();
            var pagedData = query.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray();
            return new GridData<YIS_NoticeLog>
            {
                TotalItems = totalItems,
                Items = pagedData
            };
        }
    }
}
