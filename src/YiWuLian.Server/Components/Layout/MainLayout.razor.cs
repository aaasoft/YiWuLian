using System;
using YiWuLian.Server.Components.Controls;
using YiWuLian.Server.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace YiWuLian.Server.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    private string Title = "易物联服务端";
    [Inject]
    public IDialogService DialogService { get; set; }
    [Inject]
    public Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; }
    [Inject]
    public NavigationManager NavigationManager { get; set; }
    private string LoginTokenKey;
    private string loginToken;

    public bool IsLogin { get; private set; } = false;

    private bool _drawerOpen = true;
    private bool _isDarkMode = false;
    private MudTheme _theme = null;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        LoginTokenKey = NavigationManager.Uri + "_token";
        _theme = new()
        {
            PaletteLight = _lightPalette,
            PaletteDark = _darkPalette,
            LayoutProperties = new LayoutProperties()
        };
    }

    private void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private void DarkModeToggle()
    {
        _isDarkMode = !_isDarkMode;
    }

    private readonly PaletteLight _lightPalette = new()
    {
        Black = "#110e2d",
        AppbarText = "#424242",
        AppbarBackground = "rgba(255,255,255,0.8)",
        DrawerBackground = "#ffffff",
        GrayLight = "#e8e8e8",
        GrayLighter = "#f9f9f9",
    };

    private readonly PaletteDark _darkPalette = new()
    {
        Primary = "#7e6fff",
        Surface = "#1e1e2d",
        Background = "#1a1a27",
        BackgroundGray = "#151521",
        AppbarText = "#92929f",
        AppbarBackground = "rgba(26,26,39,0.8)",
        DrawerBackground = "#1a1a27",
        ActionDefault = "#74718e",
        ActionDisabled = "#9999994d",
        ActionDisabledBackground = "#605f6d4d",
        TextPrimary = "#b2b0bf",
        TextSecondary = "#92929f",
        TextDisabled = "#ffffff33",
        DrawerIcon = "#92929f",
        DrawerText = "#92929f",
        GrayLight = "#2a2833",
        GrayLighter = "#1e1e2d",
        Info = "#4a86ff",
        Success = "#3dcb6c",
        Warning = "#ffb545",
        Error = "#ff3f5f",
        LinesDefault = "#33323e",
        TableLines = "#33323e",
        Divider = "#292838",
        OverlayLight = "#1e1e2d80",
    };

    public string DarkLightModeButtonIcon => _isDarkMode switch
    {
        true => Icons.Material.Rounded.AutoMode,
        false => Icons.Material.Outlined.DarkMode,
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            loginToken = await LocalStorage.GetItemAsStringAsync(LoginTokenKey);
            if (!string.IsNullOrEmpty(loginToken))
            {
                IsLogin = LoginTokenManager.Instance.Verify(loginToken);
                if (IsLogin)
                    LoginTokenManager.Instance.UsingToken(loginToken);
                else
                    await LocalStorage.RemoveItemAsync(LoginTokenKey);
            }
            _ = InvokeAsync(StateHasChanged);
            if (!IsLogin)
                await showLoginWindow();
        }
    }

    private async Task showLoginWindow()
    {
        var dialog = await DialogService.ShowAsync<LoginControl>(Title, new DialogOptions() { BackdropClick = false, CloseOnEscapeKey = false });
        var result = await dialog.Result;
        if (result.Canceled)
            return;
        await Login();
    }

    private async Task Login()
    {
        IsLogin = true;
        var token = Guid.NewGuid().ToString("N");
        LoginTokenManager.Instance.UsingToken(token);
        await LocalStorage.SetItemAsStringAsync(LoginTokenKey, token);
        await InvokeAsync(StateHasChanged);
    }

    private async Task Logout()
    {
        var result = await DialogService.ShowMessageBox("退出", "是否要退出登录？");
        if (result == null || !result.Value)
            return;
        await LocalStorage.RemoveItemAsync(LoginTokenKey);
        LoginTokenManager.Instance.Logout(loginToken);
        IsLogin = false;
        await InvokeAsync(StateHasChanged);
        await showLoginWindow();
    }
}
