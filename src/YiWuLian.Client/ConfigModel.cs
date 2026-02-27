using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace YiWuLian.Client;

[JsonSerializable(typeof(ConfigModel))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class ConfigModelSerializerContext : JsonSerializerContext { }

/// <summary>
/// 易启动配置模型类
/// </summary>
public class ConfigModel
{
    /// <summary>
    /// 通讯端口
    /// </summary>
    public int ServiceListenPort { get; set; } = 20951;
    /// <summary>
    /// 易物联连接地址
    /// </summary>
    public string ConnectUrl { get; set; }
    /// <summary>
    /// 易物联连接密码
    /// </summary>
    public string ConnectPassword { get; set; } = "123456";
    /// <summary>
    /// 设备IMEI
    /// </summary>
    public string DeviceIMEI { get; set; }
    /// <summary>
    /// 设备ICCID
    /// </summary>
    public string DeviceICCID { get; set; }

    /// <summary>
    /// 加载
    /// </summary>
    /// <returns></returns>
    public static ConfigModel Load()
    {
        var configFile = Consts.CONFIG_JSON_FILENAME;
        if (File.Exists(configFile))
        {
            var content = File.ReadAllText(configFile);
            var model = JsonSerializer.Deserialize(content, ConfigModelSerializerContext.Default.ConfigModel);
            var isModelChanged = false;

            if (isModelChanged)
                model.Save();
            return model;
        }
        else
        {
            return new ConfigModel();
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    public void Save()
    {
        var content = JsonSerializer.Serialize(this, ConfigModelSerializerContext.Default.ConfigModel);
        var configFile = Consts.CONFIG_JSON_FILENAME;
        File.WriteAllText(configFile, content, Encoding.UTF8);
    }

    public ConfigModel Clone()
    {
        var content = JsonSerializer.Serialize(this, ConfigModelSerializerContext.Default.ConfigModel);
        return JsonSerializer.Deserialize(content, ConfigModelSerializerContext.Default.ConfigModel);
    }
}
