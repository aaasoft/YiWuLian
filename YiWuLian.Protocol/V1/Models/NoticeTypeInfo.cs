using System;

namespace YlIotProtocol.V1.Models;

/// <summary>
/// 通知类型信息
/// </summary>
public class NoticeTypeInfo
{
    /// <summary>
    /// 通知类型编号
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 通知类型名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 是否需要设定目标
    /// </summary>
    public bool HasTarget { get; set; }
    /// <summary>
    /// 目标类型编号
    /// </summary>
    public string TargetTypeId { get; set; }
    /// <summary>
    /// 目标类型名称
    /// </summary>
    public string TargetTypeName { get; set; }
}
