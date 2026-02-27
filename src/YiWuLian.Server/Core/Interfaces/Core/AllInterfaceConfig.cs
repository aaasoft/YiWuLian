using System.Text.Json.Serialization;
using Quick.Protocol;
using YiQiDong.Core.JsonConverters;

namespace YiWuLian.Server.Core.Interfaces.Core
{
    public class AllInterfaceConfig
    {
        [JsonIgnore]
        public string InterfaceName { get; set; }
        [JsonIgnore]
        public QpInstruction[] InstructionSet { get; set; }
        [JsonIgnore]
        public string WebSocketPath { get; set; }

        public string Password { get; set; } = "123456";

        [JsonConverter(typeof(JsonBoolConverter))]
        public bool WebSocketEnable { get; set; } = false;
        [JsonConverter(typeof(JsonBoolConverter))]
        public bool PipeEnable { get; set; } = false;
        public string PipeName { get; set; }
        [JsonConverter(typeof(JsonBoolConverter))]
        public bool TcpEnable { get; set; } = false;
        public string TcpListenAddress { get; set; } = "0.0.0.0";
        [JsonConverter(typeof(JsonInt32Converter))]
        public int TcpListenPort { get; set; }
    }
}
