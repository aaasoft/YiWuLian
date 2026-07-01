using Quick.Protocol;

namespace YiWuLian.Server.Models;

public class DeviceConnectionInfo
{
    private CancellationTokenSource connectCts;
    public string ClientProgram { get; set; }
    public QpChannel Channel { get; set; }
    public bool IsConnected { get; private set; } = false;
    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime? ConnectTime { get; private set; }
    /// <summary>
    /// 断开时间
    /// </summary>
    public DateTime? DisconnectTime { get; private set; }
    /// <summary>
    /// 连接取消令牌
    /// </summary>
    public CancellationToken ConnectCancellationToken => connectCts.Token;
    /// <summary>
    /// 获取连接状态
    /// </summary>
    /// <returns></returns>
    public string GetDeviceConnectStatus() => IsConnected ? "已连接" : string.Empty;
    public int? GetTransportTimeout()
    {
        var options = Channel?.Options;
        if (options == null)
            return null;
        return options.InternalTransportTimeout;
    }

    /// <summary>
    /// 重新生成连接取消令牌
    /// </summary>
    public void RenewConnectCancellationToken()
    {
        connectCts?.Cancel();
        connectCts = new();
    }

    public void SetConnected(bool connected)
    {
        IsConnected = connected;
        if (connected)
            ConnectTime = DateTime.Now;
        else
            DisconnectTime = DateTime.Now;
    }
}
