using System;
using System.Text.Json.Serialization.Metadata;
using Quick.Protocol;

namespace YlIotProtocol.V1.Commands.SendNotice;

public class Response : AbstractQpSerializer<Response>
{
    protected override JsonTypeInfo<Response> GetTypeInfo() => SendNoticeCommandSerializerContext.Default2.Response;
}
