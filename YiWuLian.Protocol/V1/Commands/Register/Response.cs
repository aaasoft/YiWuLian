using System;
using System.Text.Json.Serialization.Metadata;
using Quick.Protocol;

namespace YlIotProtocol.V1.Commands.Register;

    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => RegisterCommandSerializerContext.Default2.Response;
    }
