using System.Text;
using Blazored.LocalStorage;
using YiWuLian.Server.Components;
using YiWuLian.Server.Models;
using YiWuLian.Server.Utils;
using MudBlazor.Services;
using MudBlazor.Translations;
using Quick.EntityFrameworkCore.Plus;
using YiQiDong.Agent;
using YiQiDong.Core;
using Quick.Utils;

namespace YiWuLian.Server;

public class Agent : AbstractAgent
{
    public static Agent Instance { get; private set; }
    public ConfigModel Config { get; private set; }
    private WebApplication app;
    private CancellationTokenSource cts;

    public Agent()
    {
        Instance = this;
    }

    public override void Init()
    {
#if DEBUG
        Environment.CurrentDirectory = AppContext.BaseDirectory;
#endif
        //支持GB18030编码
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        //初始化计算引擎
        //CalcEngine.Init();
        //初始化数据库辅助类
        DbUtils.Init();
        //初始化模型加载逻辑
        ConfigDbContext.ModelBuilderHandler = ConfigDbContextProxy.OnModelCreating;

        AddFunction(new Functions.Test());
        AddFunction(new Functions.DbInfoView(() => new ConfigDbContext()));
        AddFunction(new Functions.Config());

        Config = Functions.Config.Instance.ReadConfig();

        ConfigDbContext.ConfigHandler = DbUtils.AppDbUtils.GetDbContextConfigHandler(Config.AppDbType, Config.AppDbConfig);
    }

    public override void Start()
    {
        Config = Functions.Config.Instance.ReadConfig();
        base.Start();

        try
        {
            //初始化数据库连接
            ConfigDbContext.ConfigHandler = DbUtils.AppDbUtils.GetDbContextConfigHandler(Config.AppDbType, Config.AppDbConfig);

            AgentContext.LogInfo("确保数据库创建和更新...");
            ConfigDbContext.ConfigHandler.DatabaseEnsureCreatedAndUpdated(() => new ConfigDbContext());
            AgentContext.LogInfo("数据库连接初始化完成.");
        }
        catch (Exception ex)
        {
            AgentContext.LogWarn($"初始化数据库连接时出错，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
            throw new IOException($"初始化数据库连接时出错，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
        }
        Core.NoticeTypes.NoticeTypeManager.Instance.Start();

        cts?.Cancel();
        cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        try
        {
            AgentContext.LogInfo("正在启动Web服务...");
#if DEBUG
            var webApplicationOptions = new WebApplicationOptions();
#else
            var webApplicationOptions = new WebApplicationOptions()
            {
                ContentRootPath = AgentContext.Container.ImageFolder
            };
#endif
            var builder = WebApplication.CreateBuilder(webApplicationOptions);
            builder.Services.AddLocalization();

            // Add MudBlazor services
            builder.Services.AddMudServices();
            builder.Services.AddMudTranslations();            
            builder.Services.AddBlazoredLocalStorage();
            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddCircuitOptions(options => { options.DetailedErrors = true; });
            builder.WebHost
                .UseUrls(Config.Urls.Split([',', ';']))
                .ConfigureKestrel(options => options.AddServerHeader = false);
            app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
            }
            app.UseRequestLocalization(new RequestLocalizationOptions()
                .AddSupportedCultures(["en-US", "zh-CN"])
                .AddSupportedUICultures(["en-US", "zh-CN"]));
            app.UseAntiforgery();
            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
            Core.Interfaces.Device.Manager.Instance.Init(app, Config);
            app.Start();
            AgentContext.LogInfo("Web服务启动完成，URL：" + Config.Urls);
        }
        catch (Exception ex)
        {
            AgentContext.LogWarn($"启动Web服务时出错，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
        }
        //启动设备接口
        Core.Interfaces.Device.Manager.Instance.Start();
    }

    public override void Stop()
    {
        Core.Interfaces.Device.Manager.Instance.Stop();
        app.StopAsync();
        Core.NoticeTypes.NoticeTypeManager.Instance.Stop();
        base.Stop();
    }
}
