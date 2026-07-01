using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Quick.EntityFrameworkCore.Plus.MySql;
using YiQiDong.Core.JsonConverters;

namespace YiWuLian.Server.Models;

[Table(nameof(YIS_ConnectionLog))]
[MySqlCharSet(DbConsts.MYSQL_DEFAULT_CHARSET)]
[Comment("连接日志")]
public class YIS_ConnectionLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonConverter(typeof(JsonInt32Converter))]
    [Comment("日志编号")]
    public int OrderNo { get; set; }
    [Comment("设备编号")]
    public string DeviceId { get; set; }
    [Comment("设备名称")]
    public string DeviceName { get; set; }
    [JsonConverter(typeof(JsonNullableDateTimeConverter))]
    [Comment("时间")]
    public DateTime? Time { get; set; }
    [Comment("内容")]
    public string Content { get; set; }
}
