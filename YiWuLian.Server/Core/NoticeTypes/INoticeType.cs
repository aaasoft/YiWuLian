namespace YiWuLian.Server.Core.NoticeTypes;

public interface INoticeType
{
    /// <summary>
    /// 通知类型编号
    /// </summary>
    public string Id { get; }
    /// <summary>
    /// 通知类型名称
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// 是否需要设定目标
    /// </summary>
    public bool HasTarget { get; }
    /// <summary>
    /// 目标类型编号
    /// </summary>
    public string TargetTypeId { get; }
    /// <summary>
    /// 目标类型名称
    /// </summary>
    public string TargetTypeName { get; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; }

    /// <summary>
    /// 启动
    /// </summary>
    public void Start();
    /// <summary>
    /// 停止
    /// </summary>
    public void Stop();
    /// <summary>
    /// 发送通知
    /// </summary>
    /// <param name="device"></param>
    /// <param name="request"></param>
    public void SendNotice(Models.YIS_Device device, YlIotProtocol.V1.Commands.SendNotice.Request request);
}
