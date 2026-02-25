using System;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using Quick.Protocol;

namespace YlIotProtocol.V1.Commands.GetNoticeTypes;

[DisplayName("获取通知类型列表")]
public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
{
    protected override JsonTypeInfo<Request> GetTypeInfo() => GetNoticeTypesCommandSerializerContext.Default2.Request;
}
