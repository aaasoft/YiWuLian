using System.Text.Json.Serialization.Metadata;
using Quick.Protocol;
using YlIotProtocol.V1.Models;

namespace YlIotProtocol.V1.Commands.GetNoticeTypes;

public class Response : AbstractQpSerializer<Response>
{
    protected override JsonTypeInfo<Response> GetTypeInfo() => GetNoticeTypesCommandSerializerContext.Default2.Response;
    /// <summary>
    /// 通知类型信息
    /// </summary>
    public NoticeTypeInfo[] Data { get; set; }
}
