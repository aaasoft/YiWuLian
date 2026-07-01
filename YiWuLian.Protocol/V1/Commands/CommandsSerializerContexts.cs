using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YlIotProtocol.V1.Commands;

[JsonSerializable(typeof(Register.Request))]
[JsonSerializable(typeof(Register.Response))]
public partial class RegisterCommandSerializerContext : JsonSerializerContext
{
    public static RegisterCommandSerializerContext Default2 { get; } = new RegisterCommandSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

[JsonSerializable(typeof(GetNoticeTypes.Request))]
[JsonSerializable(typeof(GetNoticeTypes.Response))]
public partial class GetNoticeTypesCommandSerializerContext : JsonSerializerContext
{
    public static GetNoticeTypesCommandSerializerContext Default2 { get; } = new GetNoticeTypesCommandSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

[JsonSerializable(typeof(SendNotice.Request))]
[JsonSerializable(typeof(SendNotice.Response))]
public partial class SendNoticeCommandSerializerContext : JsonSerializerContext
{
    public static SendNoticeCommandSerializerContext Default2 { get; } = new SendNoticeCommandSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}