using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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
    /// <summary>
    /// Web服务地址
    /// </summary>
    public string Urls { get; set; } = "http://127.0.0.1:10066";
    /// <summary>
    /// 配置密码
    /// </summary>
    public string Password { get; set; } = "123456";

    /// <summary>
    /// 数据库类型
    /// </summary>
    public string AppDbType { get; set; } = "Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler";
    /// <summary>
    /// 数据库配置
    /// </summary>
    public JsonNode AppDbConfig { get; set; } = new JsonObject()
    {
        ["DataSource"] = "YIS.db"
    };

    /// <summary>
    /// 设备接口密码
    /// </summary>
    public string DeviceInterfacePassword { get; set; } = "123456";
    /// <summary>
    /// 设备接口是否启用WebSocket
    /// </summary>
    [JsonConverter(typeof(JsonBoolConverter))]
    public bool DeviceInterfaceWebSocketEnable { get; set; } = false;
    /// <summary>
    /// 设备接口是否启用管道
    /// </summary>
    [JsonConverter(typeof(JsonBoolConverter))]
    public bool DeviceInterfacePipeEnable { get; set; } = true;
    /// <summary>
    /// 设备接口管道名称
    /// </summary>
    public string DeviceInterfacePipeName { get; set; } = $"{nameof(YiWuLian.Server)}.ClientInterface";
    /// <summary>
    /// 设备接口是否启用TCP
    /// </summary>
    [JsonConverter(typeof(JsonBoolConverter))]
    public bool DeviceInterfaceTcpEnable { get; set; } = false;
    /// <summary>
    /// 设备接口TCP监听地址
    /// </summary>
    public string DeviceInterfaceTcpListenAddress { get; set; } = "0.0.0.0";
    /// <summary>
    /// 设备接口TCP监听端口
    /// </summary>
    [JsonConverter(typeof(JsonInt32Converter))]
    public int DeviceInterfaceTcpListenPort { get; set; } = 10067;
}