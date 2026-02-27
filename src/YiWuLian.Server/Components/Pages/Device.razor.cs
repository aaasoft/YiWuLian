using YiWuLian.Server.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Quick.EntityFrameworkCore.Plus;
using Quick.Utils;

namespace YiWuLian.Server.Components.Pages;

public partial class Device : ComponentBase
{
    [Inject]
    public IDialogService DialogService { get; set; }

    private IEnumerable<YIS_Device> Elements;

    protected override void OnInitialized()
    {
        Refresh();
    }

    private async Task Add()
    {
        var parameters = new DialogParameters<Controls.DeviceEdit>
        {
            { x => x.IsAdd, true },
            { x => x.Model, new YIS_Device() }
        };
        var dialogOptions = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true, BackdropClick = false };
        while (true)
        {
            var dialog = await DialogService.ShowAsync<Controls.DeviceEdit>("添加设备", parameters, dialogOptions);
            var result = await dialog.Result;
            if (result.Canceled)
                break;
            var model = (YIS_Device)result.Data;
            try
            {
                var existedModel = ConfigDbContext.CacheContext.Find(new YIS_Device(model.Id));
                if (existedModel != null)
                    throw new ArgumentException($"已经存在{existedModel}");
                ConfigDbContext.CacheContext.Add(model);
                Refresh();
                await InvokeAsync(StateHasChanged);
                break;
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessageBox(new MessageBoxOptions()
                {
                    Title = "错误",
                    Message = $"添加{model}时出错，原因：" + ExceptionUtils.GetExceptionMessage(ex)
                });
            }
        }
    }

    private void Refresh()
    {
        Elements = ConfigDbContext.CacheContext.Query<YIS_Device>()
            .OrderBy(t => t.ConnectionInfo.IsConnected);
    }

    private async Task ShowLogs(YIS_Device model)
    {
        var parameters = new DialogParameters<NoticeLog>
        {
            { x => x.DeviceId, model.Id }
        };
        var dialogOptions = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true, BackdropClick = false };
        await DialogService.ShowAsync<NoticeLog>($"{model} - 通知日志", parameters, dialogOptions);
    }

    private async Task Edit(YIS_Device model)
    {
        var parameters = new DialogParameters<Controls.DeviceEdit>
        {
            { x => x.IsAdd, false },
            { x => x.Model, new YIS_Device()
                {
                    Id=model.Id,
                    Name = model.Name,
                    ICCID = model.ICCID,
                    SimIMSI = model.SimIMSI,
                    SimPuk = model.SimPuk,
                    SimEnableDate = model.SimEnableDate
                }
            }
        };
        var dialogOptions = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true, BackdropClick = false };
        var dialog = await DialogService.ShowAsync<Controls.DeviceEdit>("编辑设备", parameters, dialogOptions);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            var edited_model = (YIS_Device)result.Data;
            model.Name = edited_model.Name;
            model.ICCID = edited_model.ICCID;
            model.SimIMSI = edited_model.SimIMSI;
            model.SimEnableDate = edited_model.SimEnableDate;
            try
            {
                ConfigDbContext.CacheContext.Update(edited_model);
                Core.Interfaces.Device.Manager.Instance.OnDeviceDeleted(model);
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessageBox(new MessageBoxOptions()
                {
                    Title = "错误",
                    Message = $"编辑{model}时出错，原因：" + ExceptionUtils.GetExceptionMessage(ex)
                });
            }
        }
    }

    private async Task Delete(YIS_Device model)
    {
        var result = await DialogService.ShowMessageBox(new()
        {
            Title = "删除确认",
            Message = $"将要删除{model}，确认要继续?",
            YesText = "确定",
            NoText = "取消"
        });
        if (result.HasValue && result.Value)
        {
            try
            {
                ConfigDbContext.CacheContext.Remove(model);
                Core.Interfaces.Device.Manager.Instance.OnDeviceDeleted(model);
                Refresh();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                await DialogService.ShowMessageBox(new MessageBoxOptions()
                {
                    Title = "错误",
                    Message = $"删除{model}时出错，原因：" + ExceptionUtils.GetExceptionMessage(ex)
                });
            }
        }
    }
}
