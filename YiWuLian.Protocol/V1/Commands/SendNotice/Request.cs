using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using Quick.Protocol;

namespace YlIotProtocol.V1.Commands.SendNotice;

[DisplayName("发送通知")]
public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
{
    protected override JsonTypeInfo<Request> GetTypeInfo() => SendNoticeCommandSerializerContext.Default2.Request;
    /// <summary>
    /// 通知类型编号
    /// </summary>
    public string NoticeTypeId { get; set; }
    /// <summary>
    /// 通知目标
    /// </summary>
    public string Target { get; set; }
    /// <summary>
    /// 通知变量字典
    /// </summary>
    public Dictionary<string, string> VariableDict { get; set; }
    /// <summary>
    /// 通知内容
    /// </summary>
    public string Content { get; set; }
}
