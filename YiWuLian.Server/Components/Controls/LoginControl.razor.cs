using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace YiWuLian.Server.Components.Controls;

public partial class LoginControl : ComponentBase
{
    [Inject]
    public IDialogService DialogService { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    public string Message { get; private set; }
    private string CorrectPassword => Agent.Instance.Config.Password;

    public string Password { get; set; }

    private void Submit()
    {
        if (Password == CorrectPassword)
        {
            MudDialog.Close(DialogResult.Ok((string)null));
            return;
        }
        DialogService.ShowMessageBoxAsync("错误", "密码错误");
    }
}
