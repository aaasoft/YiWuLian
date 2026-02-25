using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using Quick.Protocol;

namespace YlIotProtocol.V1.Commands.Register;

[DisplayName("注册物联网客户端")]
public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
{
    protected override JsonTypeInfo<Request> GetTypeInfo() => RegisterCommandSerializerContext.Default2.Request;
    /// <summary>
    /// 设备编号
    /// </summary>
    public string DeviceId { get; set; }
    /// <summary>
    /// 客户端程序
    /// </summary>
    public string ClientProgram { get; set; }
    /// <summary>
    /// 认证答案
    /// </summary>
    public string AuthAnswer { get; set; }
}
