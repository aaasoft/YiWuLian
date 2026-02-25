using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Quick.EntityFrameworkCore.Plus.MySql;
using YiQiDong.Core.JsonConverters;

namespace YiWuLian.Server.Models;

[Table(nameof(YIS_NoticeLog))]
[MySqlCharSet(DbConsts.MYSQL_DEFAULT_CHARSET)]
[Comment("通知日志")]
public class YIS_NoticeLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonConverter(typeof(JsonInt32Converter))]
    [Comment("事件编号")]
    public int OrderNo { get; set; }
    [Comment("设备编号")]
    public string DeviceId { get; set; }
    [Comment("设备名称")]
    public string DeviceName { get; set; }
    [JsonConverter(typeof(JsonNullableDateTimeConverter))]
    [Comment("时间")]
    public DateTime? Time { get; set; }
    [Comment("通知类型")]
    public string NoticeType { get; set; }
    [Comment("通知对象")]
    public string NoticeTarget { get; set; }
    [Comment("通知内容")]
    public string NoticeContent { get; set; }
    [Comment("通知结果")]
    public string NoticeResult { get; set; }
}
