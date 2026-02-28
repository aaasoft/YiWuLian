using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Quick.EntityFrameworkCore.Plus;
using Quick.Protocol.InterfaceService;
using YiQiDong.Core.JsonConverters;

namespace YiWuLian.Server;

[JsonSerializable(typeof(ConfigModel))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true)]
partial class ConfigModelSerializerContext : JsonSerializerContext
{
    public static ConfigModelSerializerContext Default2 { get; } = new ConfigModelSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

public class ConfigModel
{
    public static ConfigModel Default { get; } = new();

    /// <summary>
    /// Web服务地址
    /// </summary>
    public string Urls { get; set; } = "http://127.0.0.1:10066";
    /// <summary>
    /// 配置密码
    /// </summary>
    public string Password { get; set; } = "123456";
    /// <summary>
    /// 设备断开通知持续分钟数
    /// </summary>
    [JsonConverter(typeof(JsonInt32Converter))]
    public int NoticeConnectionChangedDurationMinutes { get; set; } = 1;

    /// <summary>
    /// 应用数据库
    /// </summary>
    public DbConfigInfo AppDb { get; set; } = new()
    {
#if DEBUG
        DbType = "Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler",
        DbConnectionParameter = new JsonObject()
        {
            ["DataSource"] = "YIS.db"
        }
#else
            DbType = "Quick.EntityFrameworkCore.Plus.MySql.MySqlDbContextConfigHandler",
#endif
    };

    /// <summary>
    /// 设备服务配置
    /// </summary>
    public QpInterfaceServiceConfig DeviceServiceConfig { get; set; } = new()
    {
        Password = "123456",
        WebSocketEnable = false,
        WebSocketPath = "/ws/device",
        PipeEnable = false,
        PipeName = $"{nameof(Server)}.ClientInterface",
        TcpEnable = true,
        TcpListenAddress = "0.0.0.0",
        TcpListenPort = 10067
    };
    /// <summary>
    /// 短信服务配置
    /// </summary>
    public Core.NoticeTypes.SmsNoticeType.SmsConfigModel SmsConfig { get; set; } = new();
}