using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Quick.EntityFrameworkCore.Plus;
using Quick.EntityFrameworkCore.Plus.MySql;
using YiQiDong.Core.JsonConverters;

namespace YiWuLian.Server.Models;

[Table(nameof(YIS_Device))]
[MySqlCharSet(DbConsts.MYSQL_DEFAULT_CHARSET)]
[Comment("设备")]
public class YIS_Device : BaseModel
{
    public YIS_Device() { }

    public YIS_Device(string deviceId)
    {
        Id = deviceId;
    }

    [Comment("设备编号(IMEI)")]
    public override string Id { get; set; }
    /// <summary>
    /// 设备名称
    /// </summary>
    [Comment("设备名称")]
    public string Name { get; set; }
    [Comment("集成电路卡识别码(ICCID)")]
    public string ICCID { get; set; }
    [Comment("SIM卡国际移动用户识别号(IMSI)")]
    public string SimIMSI { get; set; }
    [Comment("SIM卡个人解锁码(PUK)")]
    public string SimPuk { get; set; }
    [Comment("SIM卡启用日期")]
    [JsonConverter(typeof(JsonNullableDateTimeConverter))]
    public DateTime? SimEnableDate { get; set; }
    /// <summary>
    /// 连接信息
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public DeviceConnectionInfo ConnectionInfo { get; } = new();

    public override string ToString()
    {
        return $"设备[编号:{Id},名称:{Name}]";
    }
}
