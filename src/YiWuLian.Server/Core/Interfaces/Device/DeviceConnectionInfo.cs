using Quick.Protocol;

namespace YiWuLian.Server.Core.Interfaces.Device;

public class DeviceConnectionInfo
{
    public Models.YIS_Device Device { get; set; }
    public string ClientProgram { get; set; }
    public QpChannel Channel { get; set; }
    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectTime { get; set; }
}
