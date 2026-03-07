using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace YiWuLian.Server.Components.Controls;

public partial class DeviceEdit : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    [Inject]
    public IDialogService DialogService { get; set; }

    [Parameter]
    public bool IsAdd { get; set; }
    [Parameter]
    public Models.YIS_Device Model { get; set; }

    private void Submit()
    {
        if (string.IsNullOrEmpty(Model.Id))
        {
            DialogService.ShowMessageBoxAsync("错误", "请输入IMEI！");
            return;
        }
        if (string.IsNullOrEmpty(Model.Name))
        {
            DialogService.ShowMessageBoxAsync("错误", "请输入设备名称！");
            return;
        }
        if (string.IsNullOrEmpty(Model.ICCID))
        {
            DialogService.ShowMessageBoxAsync("错误", "请输入ICCID！");
            return;
        }
        if (string.IsNullOrEmpty(Model.SimIMSI))
        {
            DialogService.ShowMessageBoxAsync("错误", "请输入IMSI！");
            return;
        }
        if (string.IsNullOrEmpty(Model.SimPuk))
        {
            DialogService.ShowMessageBoxAsync("错误", "请输入PUK！");
            return;
        }
        if (Model.SimEnableDate==null)
        {
            DialogService.ShowMessageBoxAsync("错误", "请输入SIM卡启用日期！");
            return;
        }
        MudDialog.Close(DialogResult.Ok(Model));
    }
    private void Cancel() => MudDialog.Cancel();
}
