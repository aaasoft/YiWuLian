using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YiWuLian.Server.Models;

[JsonSerializable(typeof(YIS_Device))]
[JsonSerializable(typeof(YIS_NoticeLog))]
[JsonSerializable(typeof(YIS_ConnectionLog))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ModelsJsonSerializerContext : JsonSerializerContext
{
    public static ModelsJsonSerializerContext Default2 { get; } = new ModelsJsonSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}