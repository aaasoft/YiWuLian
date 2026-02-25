using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YiWuLian.Server.Core.NoticeTypes.SmsNoticeType;


[JsonSerializable(typeof(SmsConfigModel))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true)]
public partial class SmsConfigModelSerializerContext : JsonSerializerContext
{
    public static SmsConfigModelSerializerContext Default2 { get; } = new SmsConfigModelSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

public class SmsConfigModel
{
    /// <summary>
    /// API地址
    /// </summary>
    public string ApiAddress { get; set; } = "";
    /// <summary>
    /// 企业名称
    /// </summary>
    public string EcName { get; set; } = "";
    /// <summary>
    /// 扩展码
    /// </summary>
    public string AddSerial { get; set; } = "";
    /// <summary>
    /// 接口账号用户名
    /// </summary>
    public string ApId { get; set; } = "";
    /// <summary>
    /// 接口账号密码
    /// </summary>
    public string SecretKey { get; set; } = "";
    /// <summary>
    /// 签名编码
    /// </summary>
    public string Sign { get; set; } = "";
    /// <summary>
    /// 模板ID
    /// </summary>
    public string TemplateId { get; set; } = "";
}